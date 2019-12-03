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
    }
}