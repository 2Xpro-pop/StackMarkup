using System;
using System.Text.RegularExpressions;

namespace StackMarkup
{
    public class AliasException: Exception
    {
        private static readonly Regex _isLetterOnly = new Regex(@"^[a-zA-Z]+$", RegexOptions.Compiled);
        public static void CheckNaming(string alias, MarkupConfiguration configuration)
        {
            if(alias.Contains(' '))
            {
                throw new AliasException($"Имя {alias} не должно содержать пробелы!");
            }
            if(!_isLetterOnly.IsMatch(alias))
            {
                throw new AliasException($"Имя {alias} может соддержать только латинские буквы!");
            }
        }
        public AliasException(string message):base(message){}

    }
}