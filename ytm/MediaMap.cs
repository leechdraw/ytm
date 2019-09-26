using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ytm.Models;

namespace ytm
{
    internal class MediaMap
    {
        public MediaMap()
        {
            UnmappedMp3 = new List<MFile>();
            UnmappedMp4 = new List<MFile>();
            Mapped = new Dictionary<MFile, MFile>();
        }

        public List<MFile> UnmappedMp3 { get; private set; }
        public List<MFile> UnmappedMp4 { get; private set; }
        public Dictionary<MFile, MFile> Mapped { get; private set; }

        public static MediaMap Generate(MainConfig config)
        {
            // получаем список всех mp3
            // получаем список всех mp4
            // пытаемся сопоставить имена
            var mp3 = GetMFiles(config.Mp3Root, "*.mp3")
                .Select(x =>
                {
                    x.ImageFile = FindImage(x.FullPath);
                    return x;
                });
            var mp4 = GetMFiles(config.VideoRoot, "*.mp4");
            var result = new MediaMap();
            foreach (var audio in mp3)
            {
                var video = mp4.FirstOrDefault(vid => vid.IsSame(audio));
                if (video == null)
                {
                    result.UnmappedMp3.Add(audio);
                    continue;
                }

                result.Mapped.Add(audio, video);
            }

            result.UnmappedMp4.AddRange(mp4.Where(vid => !result.Mapped.Values.Contains(vid)));
            return result;
        }

        private static string FindImage(string mp3FullPath)
        {
            var dirFiles =
                new DirectoryInfo(Path.GetDirectoryName(mp3FullPath)).GetFiles("*.*", SearchOption.TopDirectoryOnly);
            var image = dirFiles.FirstOrDefault(x =>
                x.Extension.ToLower() == ".jpg" ||
                x.Extension.ToLower() == ".png"
            );
            return image?.FullName;
        }

        private static MFile[] GetMFiles(string path, string mask)
        {
            return new DirectoryInfo(path)
                .GetFiles(mask, SearchOption.AllDirectories)
                .Select(x => new MFile
                {
                    FullPath = x.FullName,
                    Album = GetAlBum(x.FullName),
                    Name = Path.GetFileNameWithoutExtension(x.Name)
                }).ToArray();
        }

        private static string GetAlBum(string fullPath)
        {
            return Path.GetDirectoryName(fullPath).Split(new[]
            {
                Path.DirectorySeparatorChar
            }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        
    }
}