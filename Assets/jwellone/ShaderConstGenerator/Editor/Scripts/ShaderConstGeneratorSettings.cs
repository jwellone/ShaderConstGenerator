using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

#nullable enable

namespace jwelloneEditor
{
    public class ShaderConstGeneratorSettings : ScriptableObject
    {
        static readonly string _fileName = "ShaderConst.cs";

        [SerializeField] bool _isRunWithImportShader = true;
        [SerializeField] string _assetPath = $"Scripts/Const";

        static ShaderConstGeneratorSettings? _cacheInstance;

        static ShaderConstGeneratorSettings _instance
        {
            get
            {
                if (_cacheInstance != null)
                {
                    return _cacheInstance;
                }

                var guids = AssetDatabase.FindAssets("t:scriptableobject ShaderConstGeneratorSettings");
                if (guids.Length > 0)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _cacheInstance = AssetDatabase.LoadAssetAtPath<ShaderConstGeneratorSettings>(assetPath);
                    if (_cacheInstance != null)
                    {
                        return _cacheInstance;
                    }
                }

                var path = $"Assets/ShaderConstGeneratorSettings.asset";
                _cacheInstance = CreateInstance<ShaderConstGeneratorSettings>();
                AssetDatabase.CreateAsset(_cacheInstance, path);

                return _cacheInstance;
            }
        }

        public class ImportProcessor : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (!_instance._isRunWithImportShader)
                {
                    return;
                }

                foreach (var asset in importedAssets)
                {
                    if (Path.GetExtension(asset) == ".shader")
                    {
                        GenerateOrUpdate();
                        return;
                    }
                }

                foreach (var asset in deletedAssets)
                {
                    if (Path.GetExtension(asset) == ".shader")
                    {
                        GenerateOrUpdate();
                        return;
                    }
                }
            }
        }

        [MenuItem("Tools/jwellone/Generate ShaderConst.cs")]
        static void GenerateOrUpdate()
        {
            Debug.Log($"### GenerateOrUpdate");
            var list = new List<Shader>();
            var infos = ShaderUtil.GetAllShaderInfo();
            foreach (var n in infos)
            {
                list.Add(Shader.Find(n.name));
            }

            var path = Path.Combine(Application.dataPath, _instance._assetPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var fullPath = Path.Combine(path, _fileName);
            ShaderConstGenerator.Save(fullPath, list.ToArray());
            AssetDatabase.ImportAsset(fullPath.Replace(Application.dataPath, "Assets"), ImportAssetOptions.ForceUpdate);
        }
    }
}