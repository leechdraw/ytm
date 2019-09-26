using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ytm.Models;

namespace ytm
{
    public class Mp3Converter
    {
        private readonly MainConfig _config;
        private readonly Action<string, bool> _logFunc;
        private readonly MediaMap _map;
        private const string ImagePathPlaceholder = "61135eee07614ec3b16d04a24c7590d8";
        private const string Mp3PathPlaceholder = "20b9253535fa44ec9fb4fab7a1d2db94";
        private const string Mp4PathPlaceholder = "023f2cd3e63a431a843c0503b6c12070";

        private readonly string[] ConvertString = new[]
        {
            "-loop",
            "1",
            "-i",
            ImagePathPlaceholder,
            "-i",
            Mp3PathPlaceholder,
            "-c:a",
            "copy",
            "-c:v",
            "libx264",
            "-shortest",
            Mp4PathPlaceholder
        };

        public Mp3Converter(MainConfig config, Action<string, bool> logFunc)
        {
            _config = config;
            _logFunc = logFunc;
            _map = MediaMap.Generate(config);
        }

        public void ConvertSoft()
        {
            var listToConvert = _map.UnmappedMp3;
            var targetDir = _config.VideoRoot;
            var replaceFunc = new Func<string, string>(
                mp3Name =>
                    mp3Name.Replace(_config.Mp3Root, targetDir)
                        .Replace(Path.GetExtension(mp3Name), ".mp4"));
            var ffmpegExeName = _config.FfMpeg;
            foreach (var mp3 in listToConvert)
            {
                if (string.IsNullOrEmpty(mp3.ImageFile) || !File.Exists(mp3.ImageFile))
                {
                    _logFunc($"Изображение альбома {mp3.Album} не найдено - пропускаем конвертацию для {mp3.Name}",
                        true);
                    continue;
                }
                var outputFileName = replaceFunc(mp3.FullPath);
                var parameters = ConvertString
                    .Select(x =>
                        x.Replace(ImagePathPlaceholder, mp3.ImageFile)
                            .Replace(Mp3PathPlaceholder, mp3.FullPath)
                            .Replace(Mp4PathPlaceholder, outputFileName)
                    ).ToArray();
                var cmd = new Process {StartInfo = {FileName = ffmpegExeName}};
                foreach (var parameter in parameters)
                {
                    cmd.StartInfo.ArgumentList.Add(parameter);
                }

                var dirName = Path.GetDirectoryName(outputFileName);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                _logFunc($"Start converting {mp3.FullPath} into {outputFileName}", false);
                cmd.Start();
                cmd.WaitForExit();
                _logFunc("Done!", false);
            }
        }
    }
}