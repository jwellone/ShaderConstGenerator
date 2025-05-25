using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
    public static class ShaderConstGeneratorMenu
    {
        [MenuItem("Tools/jwellone/Generate ShaderConst.cs")]
        static void Generate()
        {
            var list = new List<Shader>();
            var infos = ShaderUtil.GetAllShaderInfo();
            foreach (var n in infos)
            {
                list.Add(Shader.Find(n.name));
            }

            ShaderConstGenerator.Save(Application.dataPath + "/ShaderConst.cs", list.ToArray());
        }
    }
}