using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using ytm.Helpers;
using ytm.Models;
using Newtonsoft.Json;

namespace ytm
{
    public class YtManager
    {
        private MainConfig _mainConfig;
        private UserCredential _creds;
        private YouTubeService _service;
        private string _chanelId;

        private readonly Dictionary<YtPlayList, List<YtVideo>> _actualState =
            new Dictionary<YtPlayList, List<YtVideo>>();

        private MediaMap _map;

        public bool IsInitialized { get; private set; }

        public void Init(MainConfig config)
        {
            _mainConfig = config;
            _map = MediaMap.Generate(config);
            if (IsInitialized)
            {
                return;
            }

            InternalInit();
        }

        private void InternalInit()
        {
            try
            {
                IsInitialized = true;
                GetCreds();
            }
            catch (Exception)
            {
                IsInitialized = false;
                throw;
            }
        }

        private void GetCreds()
        {
            using (var stream = new FileStream(_mainConfig.YtSecret, FileMode.Open, FileAccess.Read))
            {
                _creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[]
                    {
                        YouTubeService.Scope.YoutubeUpload,
                        YouTubeService.Scope.Youtube
                    },
                    "user",
                    CancellationToken.None).Result;
                _service = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _creds,
                    ApplicationName = "dotNetAppForYtUpload4Skadi"
                });
                _chanelId = GetChanel(_service, _mainConfig.YtUserName);
                if (string.IsNullOrEmpty(_chanelId))
                {
                    throw new Exception($"Не удалось получить id канала пользователя {_mainConfig.YtUserName}");
                }
            }
        }

        public void DumpCurrentState(string outputFileName = null)
        {
            _actualState.Clear();
            var pls = GetPlaylists();
            foreach (var list in pls)
            {
                var videos = GetVideoFromPlaylist(list);
                _actualState.Add(list, videos);
            }

            if (!string.IsNullOrEmpty(outputFileName))
            {
                DumpState(outputFileName);
            }
        }

        private void DumpState(string outputFileName)
        {
            var rawData = from kv in _actualState
                select new
                {
                    ListId = kv.Key.Id,
                    ListName = kv.Key.Name,
                    ListChanelId = kv.Key.ChannelId,
                    Videos = kv.Value
                };
            var data = JsonConvert.SerializeObject(rawData, Formatting.Indented);
            File.WriteAllText(outputFileName, data);
        }


        private List<YtVideo> GetVideoFromPlaylist(YtPlayList list)
        {
            var request = _service.PlaylistItems.List("contentDetails,snippet");
            request.PlaylistId = list.Id;
            var data = request.Execute();
            return data.Items.Select(li => new YtVideo
            {
                Id = li.Id,
                Title = li.Snippet.Title
            }).ToList();
        }

        private List<YtPlayList> GetPlaylists()
        {
            var request = _service.Playlists.List("snippet,contentDetails");
            request.ChannelId = _chanelId;
            var playLists = request.Execute();

            return playLists.Items.Select(list =>
                new YtPlayList
                {
                    Id = list?.Id,
                    Name = list?.Snippet?.Title,
                    ChannelId = list?.Snippet?.ChannelId
                }).ToList();
        }

        private static string GetChanel(YouTubeService service, string userName)
        {
            var request = service.Channels.List("id");
            request.ForUsername = userName;
            var channels = request.Execute().Items;
            return channels?.FirstOrDefault()?.Id;
        }

        public void SoftUpload(Action<string> log)
        {
            log("Начинало загрузки видео");
            try
            {
                var templateSections =
                    VideoTemplateHelper.SplitToSections(File.ReadLines(_mainConfig.VideoDescriptionTemplate));
                var titleSection = templateSections.First(t =>
                    string.Equals(t.Key.Name, "Title", StringComparison.InvariantCultureIgnoreCase));
                var descriptionSection = templateSections.First(t =>
                    string.Equals(t.Key.Name, "Description", StringComparison.InvariantCultureIgnoreCase));

                foreach (var mp3 in _map.Mapped.OrderBy(x => x.Key.Album).ThenBy(x => x.Key.Name))
                {
                    log($"Обработка: [{mp3.Key.Album}]:[{mp3.Key.Name}]");
                    var fileName = mp3.Value.FullPath;
                    var albumName = mp3.Key.Album;
                    var title = titleSection.Value.ReplaceWithData(mp3.Key).Trim();
                    var description = descriptionSection.Value.ReplaceWithData(mp3.Key).Trim();
                    // ищем в загруженных
                    var uploadedAlbum = _actualState.Keys.FirstOrDefault(x => x.Name.Equals(albumName));
                    if (uploadedAlbum == null)
                    {
                        uploadedAlbum = CreateAlbum(mp3.Key.Album);
                        _actualState.Add(uploadedAlbum, new List<YtVideo>());
                    }

                    var wasUpload = _actualState[uploadedAlbum].FirstOrDefault(x =>
                                        x.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase)) != null;
                    if (wasUpload)
                    {
                        log("Уже загружен!");
                        continue;
                    }

                    log("Запускаем загрузку");
                    var v = UploadSingeVideo(title, description, fileName, uploadedAlbum.Id, log);
                    _actualState[uploadedAlbum].Add(v);
                    log("Загрузка и добавление альбом завершены");
                }
            }
            finally
            {
                log("Окончание загрузки видео");
            }
        }

        private YtVideo UploadSingeVideo(
            string title,
            string description,
            string fileName,
            string albumId,
            Action<string> log)
        {
            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = title,
                    Description = description,
                    CategoryId = "10"
                },
                Status = new VideoStatus
                {
                    PrivacyStatus = "public"
                }
            };
            var videosInsertRequestResponseReceived = new Action<Video>(v => { video = v; });

            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                var videosInsertRequest = _service.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ResponseReceived += videosInsertRequestResponseReceived;
                videosInsertRequest.ProgressChanged += VideosInsertRequestOnProgressChanged;
                var d = videosInsertRequest.Upload();
            }
            
            log($"Файл ID={video.Id} залит, добавляем в плейлист");
            var videoData = new YtVideo
            {
                Id = video.Id,
                Title = title
            };
            var fileFound = false;
            while (!fileFound)
            {
                log("проверяем что ютуб видит загруженное видео");
                fileFound = GetVideoById(videoData.Id);
                if (fileFound)
                {
                    break;
                }

                log("пока нет. ждем 5 секунд");
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            log("нашлось!");
            var request = _service.PlaylistItems.Insert(new PlaylistItem
            {
                Snippet = new PlaylistItemSnippet
                {
                    PlaylistId = albumId,
                    ResourceId = new ResourceId
                    {
                        Kind = "youtube#video",
                        VideoId = videoData.Id
                    }
                }
            }, "id,snippet,status,contentDetails");
            request.Execute();
            return videoData;
        }

        private void VideosInsertRequestOnProgressChanged(IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine("{0} bytes sent.", progress.BytesSent);
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }

        private bool GetVideoById(string videoId)
        {
            var request = _service.Videos.List("id");
            request.Id = videoId;
            var r = request.Execute();
            return r.Items.Count == 1;
        }

        private YtPlayList CreateAlbum(string album)
        {
            var pl = new Playlist
            {
                Snippet = new PlaylistSnippet
                {
                    Title = album
                },
                Status = new PlaylistStatus
                {
                    PrivacyStatus = "public"
                }
            };
            pl = _service.Playlists.Insert(pl, "snippet,status").Execute();
            return new YtPlayList
            {
                Id = pl.Id,
                Name = album,
                ChannelId = pl.Snippet?.ChannelId
            };
        }
    }

    internal class YtVideo
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }

    internal class YtPlayList
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string ChannelId { get; set; }
    }
}