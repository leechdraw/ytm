using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ytm.Models;

namespace ytm.Helpers
{
    public static class VideoTemplateHelper
    {
        private static readonly Regex StartLine = new Regex(@"\<\# start of name\=(.+?)\ des",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EndLine = new Regex(@"\<\# end of name\=(.+?)\ ",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Tuple<bool, string> IsValidTemplate(this FileInfo template)
        {
            try
            {
                var content = File.ReadLines(template.FullName).ToArray();
                var sections = SplitToSections(content);
                var missed = TemplateSection.GetDefaultTemplate().Where(x => !x.IsHelp).Where(t =>
                        sections.Keys.Any(sk => sk.Name.Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)))
                    .ToArray();
                if (!missed.Any())
                {
                    return new Tuple<bool, string>(false,
                        $"Потерялись следующие секции: {string.Join("\r\n", missed.Select(m => m.RusName))}");
                }

                return new Tuple<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, $"Ошибка разбора шаблона {e}");
            }
        }

        public static Dictionary<TemplateSection, string> SplitToSections(IEnumerable<string> content)
        {
            var result = new Dictionary<TemplateSection, string>();
            var curSection = (TemplateSection) null;
            var curContent = string.Empty;
            foreach (var line in content)
            {
                var m = StartLine.Match(line);
                if (m.Success)
                {
                    if (curSection != null)
                    {
                        throw new Exception($"Неожиданное начало секции {m.Groups[1].Value}");
                    }

                    curContent = string.Empty;
                    curSection = TemplateSection.GetDefaultTemplate().FirstOrDefault(x =>
                        x.Name.Equals(m.Groups[1].Value, StringComparison.InvariantCultureIgnoreCase));
                    continue;
                }

                m = EndLine.Match(line);
                if (m.Success)
                {
                    if (curSection == null)
                    {
                        throw new Exception($"Неожиданный конец секции {m.Groups[1].Value}");
                    }

                    result.Add(curSection, curContent);
                    curContent = string.Empty;
                    curSection = null;
                    continue;
                }

                if (curSection == null)
                {
                    continue;
                }

                curContent += $"{line}\r\n";
            }

            return result;
        }

        public static string ToPlaceHolder(this string fieldName)
        {
            return $"<#{fieldName}#>";
        }
    }
}