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
            this.aliases = new List<string>(aliases).AsReadOnly();
        }
    }
}