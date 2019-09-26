using System;
using System.Collections.Generic;
using System.IO;
using ytm.Helpers;

namespace ytm.Models
{
    public class TemplateSection
    {
        public string Name { get; set; }
        public string RusName { get; set; }
        public bool IsHelp { get; set; }
        public string DefaultContent { get; set; }

        public static TemplateSection[] GetDefaultTemplate() => new[]
        {
            new TemplateSection
            {
                Name = "Help",
                IsHelp = true,
                RusName = "Помощь",
                DefaultContent = CreateHelpText()
            },
            new TemplateSection
            {
                Name = "Title",
                RusName = "Заголовок",
                DefaultContent = nameof(MetaData.FileName).ToPlaceHolder()
            },
            new TemplateSection
            {
                Name = "Description",
                RusName = "Описание",
                DefaultContent =
                    $"{nameof(MetaData.Title).ToPlaceHolder()}\r\n" +
                    $"Album: {nameof(MetaData.Album).ToPlaceHolder()}\r\n" +
                    $"Performer: {nameof(MetaData.Performer).ToPlaceHolder()}\r\n" +
                    $"Composer: {nameof(MetaData.Composer).ToPlaceHolder()}"
            },
        };

        private static string CreateHelpText()
        {
            return "Во всех секциях можно использовать следующие плейс-холдеры:\r\n" +
                   $"{MetaData.GetHelp()}\r\n" +
                   $"Например: Автор: {nameof(MetaData.Composer).ToPlaceHolder()}\r\n" +
                   $"Если какого-то нужного плейс-холдер нет, напишите мне - добавим!\r\n";
        }
    }
}