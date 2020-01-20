using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ytm.Models;

namespace ytm.Services
{
    public interface IConfigProvider
    {
        MainConfig GetConfig(bool skipValidate = false);
    }

    public class ConfigProvider : IConfigProvider
    {
        private const string NoConfigParamValue = "NOCONFIG";
        private const string ConfigDefaultName = "main_config.json";
        private readonly string _configFileName;
        private MainConfig _config;

        public ConfigProvider(string configFileName)
        {
            _configFileName = configFileName ?? throw new ArgumentNullException(nameof(configFileName));
            _configFileName = FixConfigPath(_configFileName);
        }

        public MainConfig GetConfig(bool skipValidate = false)
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
            if (!skipValidate)
            {
                var validateResult = Validate(config);
                if (!string.IsNullOrEmpty(validateResult))
                {
                    throw new Exception($"Файл {_configFileName} имеет проблемы!\r\n{validateResult}");
                }
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

        private static string FixConfigPath(string configFileName)
        {
            if (!string.Equals(configFileName, NoConfigParamValue, StringComparison.InvariantCultureIgnoreCase))
            {
                return configFileName;
            }

            var dir = AssemblyDirectory;

            var paths = new[]
            {
                Path.Join(dir, ConfigDefaultName),
                Path.Join(Path.GetDirectoryName(dir), ConfigDefaultName),
                Path.Join(Path.GetDirectoryName(Path.GetDirectoryName(dir)), ConfigDefaultName)
            };
            var result = paths.FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(result))
            {
                result = paths[0];
            }

            if (File.Exists(result))
            {
                return result;
            }

            var emptyConfig = new MainConfig();
            File.WriteAllText(result, JsonConvert.SerializeObject(emptyConfig, Formatting.Indented));
            return result;
        }

        private static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}