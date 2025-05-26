using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        [SerializeField] List<string> _targetFolders = new List<string>(new[] { "Assets" });
        [SerializeField] List<Shader> _ignoreShaders = null!;

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

                var targetFolders = _instance._targetFolders;
                if (CheckGenerateOrUpdate(targetFolders, importedAssets) || CheckGenerateOrUpdate(targetFolders, deletedAssets))
                {
                    GenerateOrUpdate();
                }
            }

            static bool CheckGenerateOrUpdate(IList<string> targetFolders, string[] assetPaths)
            {
                foreach (var asset in assetPaths)
                {
                    if (targetFolders.Any(x => asset.StartsWith(x)))
                    {
                        if (Path.GetExtension(asset) == ".shader")
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [MenuItem("Tools/jwellone/Generate ShaderConst.cs")]
        static void GenerateOrUpdate()
        {
            var list = new List<Shader>();
            var infos = ShaderUtil.GetAllShaderInfo();
            var targetFolders = _instance._targetFolders;
            var ignores = _instance._ignoreShaders;
            foreach (var info in infos)
            {
                var shader = Shader.Find(info.name);
                var assetPath = AssetDatabase.GetAssetPath(shader);
                if (!ignores.Any(x => x.name == shader.name) && targetFolders.Any(x => assetPath.StartsWith(x)))
                {
                    list.Add(shader);
                }
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