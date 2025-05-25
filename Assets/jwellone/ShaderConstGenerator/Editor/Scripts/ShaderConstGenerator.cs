using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
    public static class ShaderConstGenerator
    {
        public static void Save(string path, Shader[] shaders)
        {
            var properties = GetProperties(shaders);
            var keywords = GetKeywords(shaders);
            var sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();

            sb.AppendLine("public static class ShaderConst");
            sb.AppendLine("{");

            sb.AppendLine("#region ID");
            foreach (var property in properties)
            {
                var variableName = $"{property.TrimStart('_').ToUpper()}_ID";
                sb.Append("\tpublic static readonly int ").Append(variableName).Append(" = Shader.PropertyToID(\"").Append(property).AppendLine("\");");
            }
            sb.AppendLine("#endregion");

            sb.AppendLine();

            sb.AppendLine("#region Keyword");
            foreach (var keyword in keywords)
            {
                sb.Append("\t").Append("public static readonly string ").Append(keyword).Append("_KEYWORD").Append(" = \"").Append(keyword).AppendLine("\";");
            }
            sb.AppendLine("#endregion");

            sb.AppendLine("}");

            File.WriteAllText(path, sb.ToString());
        }

        static HashSet<string> GetProperties(Shader[] shaders)
        {
            var properties = new HashSet<string>();
            foreach (var shader in shaders)
            {
                var count = ShaderUtil.GetPropertyCount(shader);
                for (var i = 0; i < count; ++i)
                {
                    var name = ShaderUtil.GetPropertyName(shader, i);
                    if (!properties.Contains(name))
                    {
                        properties.Add(name);
                    }
                }
            }

            return properties;
        }

        static HashSet<string> GetKeywords(Shader[] shaders)
        {
            var keywords = new HashSet<string>();
            foreach (var shader in shaders)
            {
                var count = ShaderUtil.GetPropertyCount(shader);
                for (var i = 0; i < count; ++i)
                {
                    var attribs = shader.GetPropertyAttributes(i);
                    if (attribs.Length == 0)
                    {
                        continue;
                    }

                    foreach (var attribute in attribs)
                    {
                        if (attribute.IndexOf("Toggle(") != 0)
                        {
                            continue;
                        }

                        var name = attribute.TrimEnd(')').Replace("Toggle(", "");
                        if (!keywords.Contains(name))
                        {
                            keywords.Add(name);
                        }
                    }
                }
            }
            return keywords;
        }
    }
}