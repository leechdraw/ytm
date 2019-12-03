using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using ytm.Helpers;
using ytm.Models;
using ytm.Services;

namespace ytm
{
    class Program
    {
        private const string MainConfigFile = "main_config.json";

        static void Main(string[] args)
        {
            try
            {
                Lg("START");
                if (args.Length != 1)
                {
                    ShowUsage();
                    return;
                }

                var manager = new YtManager();
                switch (args[0].ToLower())
                {
                    case "template:create":
                        TemplateHelper.SaveDefaultTemplateContent(DefaultTemplateFileName);
                        break;
                    case "check:config":
                        ValidateConfig(out _);
                        break;
                    case "convert:default":
                        ConvertNotExisted();
                        break;
                    case "yt:fetch":
                        FetchYoutubeData(manager);
                        break;
                    case "yt:upload:soft":
                        SoftUpload(manager);
                        break;
                    default:
                        ShowUsage();
                        return;
                        break;
                }
            }
            finally
            {
                Lg("FINISH");
            }
        }

        private static void SoftUpload(YtManager manager)
        {
            FetchYoutubeData(manager);
            manager.SoftUpload(a => Lg(a, false));
        }

        private static void FetchYoutubeData(YtManager manager)
        {
            if (!ValidateConfig(out var config))
            {
                Lg("Невозможно работать с Youtube при неисправном конфиге!", true);
                return;
            }

            try
            {
                if (!manager.IsInitialized)
                {
                    manager.Init(config);
                }

                manager.DumpCurrentState(Path.GetFullPath("cur_state.json"));
            }
            catch (Exception e)
            {
                Lg($"Ошибка во время получения состояния аккаунта \r\n{e}", true);
            }
        }

        private static void ConvertNotExisted()
        {
            if (!ValidateConfig(out var config))
            {
                Lg("Есть ошибки не совместимые с конвертацией :(");
                return;
            }

            var converter = new Mp3Converter(config, Lg);
            try
            {
                converter.ConvertSoft();
            }
            catch (Exception e)
            {
                Lg("Ошибка во время конвертации: \r\n" + e, true);
            }
        }

        private static bool ValidateConfig(out MainConfig outConfig)
        {
            outConfig = null;
            if (!File.Exists(MainConfigFile))
            {
                Lg($"Файл {MainConfigFile} не найден!", true);
                return false;
            }

            MainConfig config;
            try
            {
                config = new ConfigProvider(MainConfigFile).GetConfig();
            }
            catch (Exception e)
            {
                Lg($"Ошбка при разборе конфига {e}", true);
                return false;
            }


            var templateFileName = config.VideoDescriptionTemplate;
            var (isValid, errorText) = new FileInfo(templateFileName).IsValidTemplate();
            if (!isValid)
            {
                Lg(errorText, true);
                return false;
            }

            ValidateMap(config);


            Lg("Все проверки прошли. Критичных ошибок не найдено!");
            Lg("Если выше есть ошибки - часть функционала не будет работать :)");
            outConfig = config;
            return true;
        }

        private static void ValidateMap(MainConfig config)
        {
            var map = MediaMap.Generate(config);
            if (map.UnmappedMp3.Count > 0)
            {
                var report = string.Empty;
                foreach (var file in map.UnmappedMp3.GroupBy(f => f.Album))
                {
                    report += $"\r\n Album: {file.Key}:\r\n";
                    foreach (var mFile in file)
                    {
                        report += $"\t - {mFile.Name}.mp3\r\n";
                    }
                }

                if (report.Trim().Length > 0)
                {
                    Lg($"Не смогли найти видео для: {report}");
                }
            }

            if (map.UnmappedMp4.Count > 0)
            {
                var report = string.Empty;
                foreach (var file in map.UnmappedMp4.GroupBy(f => f.Album))
                {
                    report += $"\r\n Album: {file.Key}:\r\n";
                    foreach (var mFile in file)
                    {
                        report += $"\t - {mFile.Name}.mp4\r\n";
                    }
                }

                if (report.Trim().Length > 0)
                {
                    Lg($"Не смогли найти исходный mp3 для: {report}");
                }
            }

            if (map.Mapped.Count + map.UnmappedMp3.Count + map.UnmappedMp4.Count == 0)
            {
                Lg("В папках видео и аудио не обнаружено.", true);
            }
            else if (map.Mapped.Count == 0)
            {
                Lg("В итоге ни одного сопоставленного файла!", true);
                return;
            }

            ValidateImages(map.UnmappedMp3);
        }

        private static void ValidateImages(List<MFile> mp3s)
        {
            var noImage = mp3s.Where(x => string.IsNullOrEmpty(x.ImageFile)).GroupBy(x => x.Album);
            var resultText = string.Empty;
            foreach (var nmk in noImage)
            {
                resultText += $"\r\nАльбом {nmk.Key} не имеет картинки в известных форматах!\r\n";
            }

            resultText = resultText.Trim();
            if (string.IsNullOrEmpty(resultText))
            {
                return;
            }

            Lg(resultText, true);
        }

        private static void ShowUsage()
        {
            Lg("Params:");
            Lg($"template:create - записать шаблон по умолчанию в файл {DefaultTemplateFileName}");
            Lg("check:config - проверка конфига и его содержимого");
            Lg("convert:default - сконвертировать несконвертированное");
            Lg("yt:fetch - вытащить текущее состояние аккаунта на Youtube.com");
        }

        private static string DefaultTemplateFileName => "template.txt";

        private static void Lg(string text, bool isError = false)
        {
            var tText = $"[{DateTime.Now:G}]{(isError ? "[ERROR]" : string.Empty)} {text}\r\n";
            File.AppendAllText("run.log", tText);
            if (!isError)
            {
                Console.WriteLine(tText.Trim());
            }
            else
            {
                var c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(tText);
                Console.ForegroundColor = c;
            }
        }
    }
}