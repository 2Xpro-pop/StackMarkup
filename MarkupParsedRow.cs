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

            if(row.IndexOf(' ')!= -1)
            {
                var defination = row.Substring(0, row.IndexOf(' '));
                
                SyntaxException.CheckIsEmptyName(defination);

                if(_elementWithProperties.IsMatch(defination))
                {
                    SetNameAndProperty(defination);
                    Content = row.Replace($"{MarkupElementName}[{PropertiesString}]", "");
                }
            }
            else if (_elementWithProperties.IsMatch(row))
            {
                SetNameAndProperty(row);
            }
            else 
            {
                SyntaxException.CheckIsEmptyName(row);
                MarkupElementName = row;
            }
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

        private void SetNameAndProperty(string defination)
        {
            MarkupElementName = defination.Substring(0, defination.IndexOf('['));
            PropertiesString = defination.Substring(
                defination.IndexOf('[') + 1,
                defination.IndexOf(']') - defination.IndexOf('[') -1
            );
        }

    }
}