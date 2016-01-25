using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DefinitelyTypedNet
{
    public static class TypeScriptBuilder
    {
        public static string GetTypescriptContracts(params Assembly[] assemblies)
        {
            return GetTypescriptContracts(false, assemblies);
        }
        public static string GetTypescriptContractsCamelCase(params Assembly[] assemblies)
        {
            return GetTypescriptContracts(true, assemblies);
        }
        private static string GetTypescriptContracts(bool camelCase, params Assembly[] assemblies)
        {
            var allTypes = assemblies.SelectMany(x => x.GetTypes());
            var typescriptTypes = allTypes.Where(x => x.GetCustomAttributes<TypeScriptAttribute>().Any()).GroupBy(x => x.FullName).Select(x => x.First());
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("module TypeScriptBuilder {");
            stringBuilder.AppendLine("\texport interface IDictionaryString<TValue> {");
            stringBuilder.AppendLine("\t\t[key: string]: TValue;");
            stringBuilder.AppendLine("\t}");
            stringBuilder.AppendLine("}");
            foreach (var namespaceItems in typescriptTypes.GroupBy(x => x.Namespace))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat("module {0} {{", namespaceItems.Key);
                namespaceItems.SelectMany(x => x.GetCustomAttributes<TypeScriptAttribute>(), (type, attribute) => new
                {
                    type,
                    attribute
                }).ToList().ForEach(x => GenerateTypescript(stringBuilder, x.type, x.attribute, camelCase));
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("}");
            }
            return stringBuilder.ToString();
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(Type type, bool inheritedTypescriptContracts)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var propertyInfo in properties)
            {
                yield return propertyInfo;
            }
            if (inheritedTypescriptContracts)
            {
                var baseAttribute = type.BaseType.GetCustomAttributes<TypeScriptAttribute>().OfType<TypeScriptAttribute>().FirstOrDefault();
                if (baseAttribute != null)
                {
                    foreach (var propertyInfo in GetAllProperties(type.BaseType, true))
                    {
                        yield return propertyInfo;
                    }
                }
            }
        }
        private static void GenerateTypescript(StringBuilder stringBuilder, Type type, TypeScriptAttribute attribute, bool camelCase)
        {
            if (type.IsEnum)
            {
                GenerateEnum(stringBuilder, type);
            }
            else
            {
                if (type.IsInterface)
                {
                    GenerateInterface(stringBuilder, type, attribute, camelCase);
                }
                if (type.IsClass)
                {
                    GenerateClass(stringBuilder, type, attribute, camelCase);
                }
            }
        }

        private static void GenerateEnum(StringBuilder stringBuilder, Type type)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("\texport enum {0}", type.Name);
            stringBuilder.AppendLine(" {");
            foreach (int enumValue in type.GetEnumValues())
            {
                var name = type.GetEnumName(enumValue);
                stringBuilder.AppendLine(string.Format("\t\t{0} = {1},", name, enumValue));
            }
            stringBuilder.Append("\t}");
        }

        private static void GenerateClass(StringBuilder stringBuilder, Type type, TypeScriptAttribute attribute, bool camelCase)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("\texport class {0}", type.Name);

            var interfaces = type.GetInterfaces().Where(x => x.GetCustomAttribute<TypeScriptAttribute>() != null);
            if(interfaces.Any())
            {
                stringBuilder.AppendFormat(" implements {0}", string.Join(",", interfaces.Select(x => x.Name)));
            }
            stringBuilder.Append(" {");
            foreach (var propertyInfo in GetAllProperties(type, true))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat("\t\tpublic {0}: {1};", camelCase ? ToCamelCase(propertyInfo.Name) : propertyInfo.Name,
                    TypeScriptConvert.ToScriptType(propertyInfo.PropertyType));
            }
            if (attribute.GenerateTypeProperty)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat("\t\tpublic type: string = \"{0}\";", type.Name);
            }
            stringBuilder.AppendLine();
            stringBuilder.Append("\t}");
        }

        private static void GenerateInterface(StringBuilder stringBuilder, Type type, TypeScriptAttribute attribute,
            bool camelCase)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("\texport interface {0}", type.Name);
            var extendingInterfaces = new List<string>();
            if (type.BaseType != null)
            {
                var baseAttribute =
                    type.BaseType.GetCustomAttributes<TypeScriptAttribute>().OfType<TypeScriptAttribute>().FirstOrDefault();
                if (baseAttribute != null)
                {
                    extendingInterfaces.Add(string.Format("{0}.{1}", type.BaseType.Namespace, type.BaseType.Name));
                }
            }
            if (extendingInterfaces.Any())
            {
                stringBuilder.AppendFormat(" extends {0}", string.Join(", ", extendingInterfaces));
            }
            stringBuilder.Append(" {");
            foreach (var propertyInfo in GetAllProperties(type, false))
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat("\t\t{0}: {1};", camelCase ? ToCamelCase(propertyInfo.Name) : propertyInfo.Name,
                    TypeScriptConvert.ToScriptType(propertyInfo.PropertyType));
            }
            
            stringBuilder.AppendLine();
            stringBuilder.Append("\t}");
        }

        private static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
                return s;
            char[] chArray = s.ToCharArray();
            for (int index = 0; index < chArray.Length; ++index)
            {
                bool flag = index + 1 < chArray.Length;
                if (index <= 0 || !flag || char.IsUpper(chArray[index + 1]))
                    chArray[index] = char.ToLower(chArray[index], CultureInfo.InvariantCulture);
                else
                    break;
            }
            return new string(chArray);
        }
    }
}