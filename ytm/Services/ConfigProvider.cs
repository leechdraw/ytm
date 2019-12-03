using System;
using System.IO;
using Newtonsoft.Json;
using ytm.Models;

namespace ytm.Services
{
    public interface IConfigProvider
    {
        MainConfig GetConfig();
    }

    public class ConfigProvider : IConfigProvider
    {
        private readonly string _configFileName;
        private MainConfig _config;

        public ConfigProvider(string configFileName)
        {
            _configFileName = configFileName ?? throw new ArgumentNullException(nameof(configFileName));
        }

        public MainConfig GetConfig()
        {
            if (_config != null)
            {
                return _config;
            }

            if (!File.Exists(_configFileName))
            {
                throw new Exception($"File {_configFileName} does not exists!");
            }

            var config = Load(_configFileName);
            var validateResult = Validate(config);
            if (!string.IsNullOrEmpty(validateResult))
            {
                throw new Exception($"Файл {_configFileName} имеет проблемы!\r\n{validateResult}");
            }

            _config = config;
            return config;
        }

        private string Validate(MainConfig config)
        {
            return (
                $"{ExistsD(config.Mp3Root)}" +
                $"{ExistsD(config.VideoRoot)}" +
                $"{ExistsF(config.VideoDescriptionTemplate)}" +
                $"{ExistsF(config.FfMpeg)}" +
                $"{ExistsF(config.YtSecret)}").Trim();
        }


        private static string ExistsD(string dName)
        {
            return Directory.Exists(dName) ? string.Empty : $"{dName} не существует!\r\n";
        }

        private static string ExistsF(string dName)
        {
            return File.Exists(dName) ? string.Empty : $"{dName} не существует!\r\n";
        }

        private static MainConfig Load(string fileName)
        {
            var tmp = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(fileName));
            tmp.Mp3Root = Path.GetFullPath(tmp.Mp3Root);
            tmp.VideoRoot = Path.GetFullPath(tmp.VideoRoot);
            tmp.VideoDescriptionTemplate = Path.GetFullPath(tmp.VideoDescriptionTemplate);
            tmp.YtSecret = Path.GetFullPath(tmp.YtSecret);
            return tmp;
        }
    }
}