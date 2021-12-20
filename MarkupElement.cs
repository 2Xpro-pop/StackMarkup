using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.ComponentModel;

namespace StackMarkup
{
    [Serializable]
    public class MarkupElement
    {
        [NonSerialized]
        private readonly Type _elementType;
        [NonSerialized]
        private readonly Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();
        [NonSerialized]
        private readonly MarkupConfiguration _configuration;
        [NonSerialized]
        private readonly PropertyInfo _content = null;
        
        public MarkupElement(Type elementType, MarkupConfiguration configuration)
        {
            _elementType = elementType;
            _configuration = configuration;

            foreach(var property in elementType.GetProperties())
            {
                if(property.GetCustomAttribute<MarkupIgnoreAttribute>() != null)
                {
                    break;
                }
                foreach(var attribute in property.GetCustomAttributes())
                {
                    if(attribute is MarkupAliasesAttribute aliasesAtr)
                    {
                        SettingAliases(property, aliasesAtr.aliases);
                    }
                    if(attribute is MarkupContentAttribute)
                    {
                        if(property.PropertyType != typeof(string))
                        {
                            throw new InvalidOperationException("Content всегда должен быть строкового типа!");
                        }
                        else
                        {
                            CanContainContent = true;
                            _content = property;
                        }
                    }
                }
                _properties.Add(property.Name.Trim().ToLower(), property);
            }
        }

        public bool CanContainContent { get; } 
        public object BuildedElement { get; private set; }

        public object BuildElement(MarkupParsedRow row)
        {
            var element  = Activator.CreateInstance(_elementType);
            var properties = new DbConnectionStringBuilder(); 

            if(!string.IsNullOrWhiteSpace(row.PropertiesString))
            {
                try
                {
                    properties.ConnectionString = row.PropertiesString;
                }
                catch (ArgumentException)
                {
                    throw new SyntaxException("Некорректный синтаксис свойств");
                }
            }

            foreach(var propertyName in properties.Keys)
            {
                var propName = (string)propertyName;
                if(_properties.ContainsKey(propName))
                {
                    var value = GetConvertedTypeFromString(
                        _properties[propName].PropertyType,
                        (string)properties[propName]
                    );
                    _properties[propName].SetValue(element, value);
                }
                else
                {
                    throw new InvalidCastException($"Не найдено свойство {propName}");
                }
            }

            if(CanContainContent)
            {
                _content.SetValue(element, row.Content);
            }

            return element;
        }

        private void SettingAliases(PropertyInfo property, IReadOnlyList<string> aliases)
        {
            foreach(var alias in aliases)
            {
                AliasException.CheckNaming(alias, _configuration);
                try
                {
                    _properties.Add(alias, property);
                }
                catch(ArgumentException exc)
                {
                    throw new AliasException($"Имя {alias} уже занято!");
                }
            }
        }

        private static object GetConvertedTypeFromString(Type type, string value)
        {
            var converter = TypeDescriptor.GetConverter(type);
            return converter.ConvertToInvariantString(value);
        }
    }
}