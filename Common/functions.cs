using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public static class functions
    {
        public static string[] SplitStringOnNewlines(string input, int maxLength)
        {
            List<string> lines = new List<string>();
            StringBuilder currentLine = new StringBuilder();

            string[] splited = input.Split(new[] { "\n", "\r\n", "\r", "\u2028", "\u2029" }, StringSplitOptions.RemoveEmptyEntries);
            string needsWork = "";
            if (splited.Length == 2 )
            {
                if (splited[0] == String.Empty)
                {
                    needsWork = splited[1];
                }
            } else if (splited.Length == 1){
                needsWork = input;
            }
            if ( needsWork != "")
            {
                splited = Regex.Split(needsWork, @"[\r\n=]+");
            }
            foreach (string line in splited)
            {
                string l = RemoveColorPrefixes(line);
                l = RemoveAnsiEscapeCodes(l);
                if (currentLine.Length + l.Length + 1 <= maxLength)
                {
                    currentLine.AppendLine(l);
                }
                else
                {
                    lines.Add(currentLine.ToString().TrimEnd());
                    currentLine.Clear().AppendLine(l);
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().TrimEnd());
            }

            return lines.ToArray();
        }

        public static string RemoveColorPrefixes(string input)
        {
            string pattern = @"\u001b\[[\d;]+m";
            string cleaned = Regex.Replace(input, pattern, string.Empty);
            return cleaned;
        }

        public static string RemoveAnsiEscapeCodes(string input)
        {
            string pattern = @"\x1B\[[0-?]*[ -/]*[@-~]";
            return Regex.Replace(input, pattern, string.Empty);
        }
    }
}
