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
        public static List<T> Convert<T>(object obj) where T : new() {
            if (typeof(IList).IsAssignableFrom(obj.GetType()))
            {
                return (List<T>)Convert((IList)obj, typeof(T));
            }

            return (List<T>)Convert(new List<object> { obj }, typeof(T));
        }
        public static List<T> Convert<T>(IList list) where T : new() => (List<T>)Convert(list, typeof(T));

        static IList Convert(IList list, Type type)
        {
            var genericListType = typeof(List<>).MakeGenericType(type);
            IList objs = Activator.CreateInstance(genericListType) as IList;

            foreach (var dict in list)
            {
                objs.Add(dict.GetType() == type ? dict : Convert(dict as IDictionary<string, object>, type));
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
                    var match = names.FirstOrDefault(e => 
                        string.Equals(StringUtils.ToConstantCase(e), item.Value.ToString(), StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(e, item.Value.ToString(), StringComparison.OrdinalIgnoreCase)
                    );
                    
                    if ((!IsNullableType(tPropertyType) && match is not null) || IsNullableType(tPropertyType)) {
                        t.GetType().GetProperty(property.Name).SetValue(t, Enum.Parse(newT, match), null);
                    }
                    continue;
                }

                // String to List<String>
                /*if (typeof(IList).IsAssignableFrom(newT) && item.Value is string) {
                    t.GetType().GetProperty(property.Name).SetValue(t, new List<string> { item.Value as string }, null);
                    continue;
                }*/

                if (typeof(IList).IsAssignableFrom(newT) && !item.Value.GetType().IsGenericType)
                {
                    Type childType = IsCollectionType(newT.BaseType) ?
                        newT.BaseType.GetGenericArguments().FirstOrDefault() :
                        newT.GetGenericArguments().FirstOrDefault();

                    if (childType.IsPrimitive || childType.IsAssignableFrom(typeof(string)))
                    {
                        var genericListType = typeof(List<>).MakeGenericType(childType);
                        IList objs = Activator.CreateInstance(genericListType) as IList;

                        objs.Add(System.Convert.ChangeType(item.Value, childType));

                        var listPrimitive = ConvertCollectionToType(newT, objs);
                        t.GetType().GetProperty(property.Name).SetValue(t, listPrimitive, null);
                        continue;
                    }

                    IList listObjects = Activator.CreateInstance(typeof(List<object>)) as IList;
                    listObjects.Add(item.Value);

                    var listObjectsTransform = ConvertCollectionToType(newT, listObjects);
                    t.GetType().GetProperty(property.Name).SetValue(t, listObjectsTransform, null);
                    /*t.GetType().GetProperty(property.Name).SetValue(t, Convert(classes as List<object>, childType), null);*/
                    continue;
                }

                // List<object> to List
                if (typeof(IList).IsAssignableFrom(newT) && typeof(IList).IsAssignableFrom(item.Value.GetType()))
                {
                    Type childType = IsCollectionType(newT.BaseType) ? 
                        newT.BaseType.GetGenericArguments().FirstOrDefault() : 
                        newT.GetGenericArguments().FirstOrDefault();
                    
                    if (childType.IsPrimitive || childType.IsAssignableFrom(typeof(string))) {
                        var genericListType = typeof(List<>).MakeGenericType(childType);
                        IList objs = Activator.CreateInstance(genericListType) as IList;

                        foreach (var obj in item.Value as IList)
                        {
                            objs.Add(System.Convert.ChangeType(obj, childType));
                        }

                        var listPrimitive = ConvertCollectionToType(newT, objs);
                        t.GetType().GetProperty(property.Name).SetValue(t, listPrimitive, null);

                        continue;
                    }

                    var listObjects = ConvertCollectionToType(newT, Convert(item.Value as List<object>, childType));
                    t.GetType().GetProperty(property.Name).SetValue(t, listObjects, null);

                    continue;
                }

                t.GetType().GetProperty(property.Name).SetValue(t, System.Convert.ChangeType(item.Value, newT), null);
            }
            return t;
        }

        private static object ConvertCollectionToType(Type to, object source) {
            if (IsCollectionType(to.BaseType))
            {
                return Activator.CreateInstance(to, source);
            }

            return source;
        }
        private static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private static bool IsCollectionType(Type type)
        {
            return (type.GetInterface(nameof(ICollection)) != null);
        }
    }
}
