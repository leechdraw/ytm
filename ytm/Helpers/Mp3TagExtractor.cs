using System.IO;
using ytm.Models;
using TagLib.Flac;

namespace ytm.Helpers
{
    public static class Mp3TagExtractor
    {
        public static MetaData GetMeta(this MFile mp3File)
        {
            var tFile = TagLib.File.Create(mp3File.FullPath);
            return new MetaData
            {
                Title = tFile.Tag.Title,
                Album = tFile.Tag.Album,
                Length = tFile.Properties.Duration,
                Performer = tFile.Tag.JoinedPerformers,
                Composer = tFile.Tag.JoinedComposers,
                Comment = tFile.Tag.Comment,
                FileName = Path.GetFileNameWithoutExtension(mp3File.FullPath)
            };
        }
    }
}