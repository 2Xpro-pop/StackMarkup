using System;

namespace StackMarkup
{
    public class SyntaxException: Exception
    {
        public static void CheckIsEmptyName(string substring)
        {
            if(string.IsNullOrWhiteSpace(substring))
            {
                throw new SyntaxException("Имя элемента не может быть пустым!");
            }
        }
        public SyntaxException(string message): base(message) {}
    }
}