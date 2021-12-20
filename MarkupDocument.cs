using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace StackMarkup
{
    public class MarkupDocument: IEnumerable
    {
        private readonly List<Type> _registered = new List<Type>();
        private readonly Dictionary<string, MarkupElement> _elements = new Dictionary<string, MarkupElement>();

        public event MarkupElementBuildedHandler OnMarkupElemenBuilded;

        public MarkupDocument() { }

        public MarkupDocument(MarkupConfiguration configuration)
        {
            Configuration = configuration;
        }

        public MarkupConfiguration Configuration { get; set; } = new MarkupConfiguration()
        {
            BeforeCharacter = '/'
        };

        public List<object> Elements { get; } = new List<object>();
        
        public void Register<T>()
        {
            var type = typeof(T);

            if(_registered.Contains(type))
            {
                throw new ArgumentException("Тип уже зарегистрирован!");
            }

            var markupElement = new MarkupElement(type, Configuration);

            if (type.GetCustomAttribute<MarkupAliasesAttribute>() != null)
            {
                var attr = type.GetCustomAttribute<MarkupAliasesAttribute>();
                foreach (var alias in attr.aliases)
                {
                    AliasException.CheckNaming(alias.Trim().ToLower(), Configuration);

                    if(_elements.ContainsKey(alias.Trim().ToLower()))
                    {
                        throw new AliasException($"Имя {alias.Trim().ToLower()} уже занято!");
                    }

                    _elements.Add(alias.Trim().ToLower(), markupElement);
                }
            }

            _elements.Add(type.Name.Trim().ToLower(), markupElement);
        }

        public void Load(string path)
        {
            using(var reader = new StreamReader(path))
            {
                while (reader.Peek() >= 0)
                {
                    var row = new MarkupParsedRow(reader.ReadLine(), Configuration);

                    if(!_elements.ContainsKey(row.MarkupElementName))
                    {
                        throw new AliasException($"Имя {row.MarkupElementName} не существует");
                    }

                    OnMarkupElemenBuilded?.Invoke(_elements[row.MarkupElementName].BuildElement(row), row);
                }
            }
        }

        public async void LoadAsync(string path)
        {
            using (var reader = new StreamReader(path))
            {
                while (reader.Peek() >= 0)
                {
                    var row = new MarkupParsedRow(await reader.ReadLineAsync(), Configuration);

                    if (!_elements.ContainsKey(row.MarkupElementName))
                    {
                        throw new AliasException($"Имя {row.MarkupElementName} не существует");
                    }

                    OnMarkupElemenBuilded?.Invoke(_elements[row.MarkupElementName].BuildElement(row), row);
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
    }
}
