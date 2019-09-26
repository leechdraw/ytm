using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ytm.Models;

namespace ytm.Helpers
{
    public static class TemplateHelper
    {
        public static void SaveDefaultTemplateContent(string templateFileName)
        {
            if (File.Exists(templateFileName))
            {
                CreateFileBackup(templateFileName);
            }

            var sections = new List<TemplateSection>
            {
                TemplateSection.GetDefaultTemplate().FirstOrDefault(x => x.IsHelp)
            };
            sections.AddRange(TemplateSection.GetDefaultTemplate().Where(x => !x.IsHelp));
            var data = sections.Select(section => section.ToTemplatePart()).ToList();
            File.WriteAllLines(templateFileName, data);
        }

        public static string ReplaceWithData(this string template, MFile mFile)
        {
            var meta = mFile.GetMeta();
            var replacers = MetaData.GetReplacers();

            return replacers.Aggregate(template, (current, replacer) => replacer(current, meta));
        }

        private static string ToTemplatePart(this TemplateSection section)
        {
            return $"<# start of name={section.Name} descr='{section.RusName}'#>\r\n" +
                   $"{section.DefaultContent}\r\n" +
                   $"<# end of name={section.Name} #>\r\n";
        }


        private static void CreateFileBackup(string templateFileName)
        {
            if (!File.Exists(templateFileName))
            {
                return;
            }

            var buFileName = templateFileName + $".{DateTime.Now:yy-MM-dd_HHmmss}.bak";
            File.Copy(templateFileName, buFileName);
        }
    }
}