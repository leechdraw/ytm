using System.IO;
using Newtonsoft.Json;

namespace ytm.Models
{
    public class MainConfig
    {
        public string Mp3Root { get; set; }
        public string VideoRoot { get; set; }
        public string VideoDescriptionTemplate { get; set; }
        public string FfMpeg { get; set; }

        public string YtSecret { get; set; }
        public bool CreatePlaylistAsPublic { get; set; }
        public bool UploadVideoAsPublic { get; set; }
        public string YtUserName { get; set; }

        public string Validate()
        {
            return (
                $"{ExistsD(Mp3Root)}" +
                $"{ExistsD(VideoRoot)}" +
                $"{ExistsF(VideoDescriptionTemplate)}" +
                $"{ExistsF(FfMpeg)}" +
                $"{ExistsF(YtSecret)}").Trim();
        }


        private static string ExistsD(string dName)
        {
            return Directory.Exists(dName) ? string.Empty : $"{dName} не существует!\r\n";
        }

        private static string ExistsF(string dName)
        {
            return File.Exists(dName) ? string.Empty : $"{dName} не существует!\r\n";
        }

        public static MainConfig Load(string fileName)
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