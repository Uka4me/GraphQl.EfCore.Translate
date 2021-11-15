using GraphQl.EfCore.Translate.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.Converters
{
    public static class DictionaryToObjectConverter
    {
        public static List<T> Convert<T>(List<object> list) where T : new() => (List<T>)Convert(list, typeof(T));

        static IList Convert(List<object> list, Type type)
        {
            var genericListType = typeof(List<>).MakeGenericType(type);
            IList objs = Activator.CreateInstance(genericListType) as IList;

            foreach (var dict in list)
            {
                objs.Add(Convert(dict as IDictionary<string, object>, type));
            }
            return objs;
        }

        static object Convert(IDictionary<string, object> dict, Type type)
        {
            var t = Activator.CreateInstance(type);
            PropertyInfo[] properties = t.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (!dict.Any(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                KeyValuePair<string, object> item = dict.First(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                // Find which property type (int, string, double? etc) the CURRENT property is...
                Type tPropertyType = t.GetType().GetProperty(property.Name).PropertyType;

                // Fix nullables...
                Type newT = Nullable.GetUnderlyingType(tPropertyType) ?? tPropertyType;

                // String to Enum
                if (newT.IsEnum)
                {
                    var names = Enum.GetNames(newT);
                    var match = names.FirstOrDefault(e => string.Equals(StringUtils.ToConstantCase(e), item.Value.ToString(), StringComparison.OrdinalIgnoreCase));
                    
                    if ((!IsNullableType(tPropertyType) && match is not null) || IsNullableType(tPropertyType)) {
                        t.GetType().GetProperty(property.Name).SetValue(t, Enum.Parse(newT, match), null);
                    }
                    continue;
                }

                // String to List<String>
                if (typeof(IList).IsAssignableFrom(newT) && item.Value is string) {
                    t.GetType().GetProperty(property.Name).SetValue(t, new List<string> { item.Value as string }, null);
                    continue;
                }

                // List<string> to List
                if (typeof(IList<string>).IsAssignableFrom(newT) && (typeof(IList<string>).IsAssignableFrom(item.Value.GetType()) || typeof(IList<object>).IsAssignableFrom(item.Value.GetType())))
                {
                    t.GetType().GetProperty(property.Name).SetValue(t, (item.Value as List<object>).Cast<string>().ToList(), null);
                    continue;
                }

                // List<object> to List or Array
                if (typeof(IList).IsAssignableFrom(newT) && typeof(IList<object>).IsAssignableFrom(item.Value.GetType()))
                {
                    Type childType = newT.GetGenericArguments().FirstOrDefault();
                    t.GetType().GetProperty(property.Name).SetValue(t, Convert(item.Value as List<object>, childType), null);
                    continue;
                }

                object newA = System.Convert.ChangeType(item.Value, newT);
                t.GetType().GetProperty(property.Name).SetValue(t, newA, null);
            }
            return t;
        }

        private static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
