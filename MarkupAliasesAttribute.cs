using System;
using System.Collections.Generic;

namespace StackMarkup
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class MarkupAliasesAttribute: Attribute 
    {
        public readonly IReadOnlyList<string> aliases;
        public MarkupAliasesAttribute(params string[] aliases)
        {
            var list = new List<string>();
            foreach(var alias in aliases)
            {
                list.Add(alias.Trim().ToLower());
            }
            this.aliases = list.AsReadOnly();

        }
    }
}