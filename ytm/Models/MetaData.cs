using System;
using System.Collections.Generic;
using ytm.Helpers;

namespace ytm.Models
{
    public class MetaData
    {
        public string FileName { get; set; }
        public string Title { get; set; }

        public string Album { get; set; }

        public TimeSpan Length { get; set; }

        public string Performer { get; set; }

        public string Composer { get; set; }
        public string Comment { get; set; }

        public string ToDescription()
        {
            return $"{Title}\r\n" +
                   $"Album: {Album}\r\n" +
                   $"Performer: {Performer}\r\n" +
                   $"Composer: {Composer}";
        }

        public static string GetHelp()
        {
            return "Доступные поля:\r\n"
                   + $"{nameof(Title).ToPlaceHolder()} - Название песни\r\n" +
                   $"{nameof(FileName).ToPlaceHolder()} - Название файл (удобно для сортировки - рекомендуется использовать в заголовке)" +
                   $"{nameof(Album).ToPlaceHolder()} - Название альбома\r\n" +
                   $"{nameof(Length).ToPlaceHolder()} - Длительность песни\r\n" +
                   $"{nameof(Performer).ToPlaceHolder()} - Исполнитель\r\n" +
                   $"{nameof(Composer).ToPlaceHolder()} - Автор песни\r\n" +
                   $"{nameof(Comment).ToPlaceHolder()} - Комментарий\r\n";
        }

        public static List<Func<string, MetaData, string>> GetReplacers()
        {
            var result = new List<Func<string, MetaData, string>>
            {
                (t, m) => t.Replace(nameof(Title).ToPlaceHolder(), m.Title),
                (t, m) => t.Replace(nameof(Album).ToPlaceHolder(), m.Album),
                (t, m) => t.Replace(nameof(Length).ToPlaceHolder(), m.Length.ToString("g")),
                (t, m) => t.Replace(nameof(Performer).ToPlaceHolder(), m.Performer),
                (t, m) => t.Replace(nameof(Composer).ToPlaceHolder(), m.Composer),
                (t, m) => t.Replace(nameof(Comment).ToPlaceHolder(), m.Comment),
                (t, m) => t.Replace(nameof(FileName).ToPlaceHolder(), m.FileName),
            };
            return result;
        }
    }
}