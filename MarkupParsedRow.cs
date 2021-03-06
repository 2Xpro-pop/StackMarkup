using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace StackMarkup
{
    public class MarkupParsedRow
    {
        private static readonly Regex _elementWithProperties = new Regex(
            @"[a-zA-Z]+\[[^\]]*\](\.[^\]]*\])?", 
            RegexOptions.Compiled
        );

        public MarkupParsedRow(string row, MarkupConfiguration configuration)
        {
            if(configuration.BeforeCharacter != null)
            {
                SetCountBeforeCharacter(row, configuration);
            }
            
            row = row.Remove(0, CountBeforeCharacter);

            int i = 0;
            while (row.IndexOf(' ', i) < row.IndexOf(']'))
            {
                i = row.IndexOf(' ', i+1);
                if (i == -1)
                {
                    break;
                }
            }

            if (i!= -1)
            {
                var defination = row.Substring(0, row.IndexOf(' ', i));
                
                SyntaxException.CheckIsEmptyName(defination);

                if(_elementWithProperties.IsMatch(defination))
                {
                    SetNameAndProperty(defination, configuration);
                    Content = row.Replace($"{MarkupElementName}[{PropertiesString}] ", "");
                }
                else
                {
                    MarkupElementName = defination;
                    AliasException.CheckNaming(MarkupElementName, configuration);
                    Content = row.Replace($"{MarkupElementName} ", "");
                }
            }
            else if (_elementWithProperties.IsMatch(row))
            {
                SetNameAndProperty(row, configuration);
            }
            else 
            {
                SyntaxException.CheckIsEmptyName(row);
                MarkupElementName = row;
            }

            MarkupElementName = MarkupElementName.ToLower().Trim();
        }

        public int CountBeforeCharacter { get; private set; } = 0;
        public string MarkupElementName { get; private set; } 
        public string? PropertiesString { get; private set; }
        public string? Content { get; private set; }

        private void SetCountBeforeCharacter(string row, MarkupConfiguration configuration)
        {
            foreach(var character in row)
            {
                if(character == configuration.BeforeCharacter)
                {
                    CountBeforeCharacter += 1;
                    continue;
                }
                break;
            }
        }

        private void SetNameAndProperty(string defination, MarkupConfiguration configuration)
        {
            MarkupElementName = defination.Substring(0, defination.IndexOf('['));
            PropertiesString = defination.Substring(
                defination.IndexOf('[') + 1,
                defination.IndexOf(']') - defination.IndexOf('[') -1
            );
            AliasException.CheckNaming(MarkupElementName, configuration);
        }

    }
}