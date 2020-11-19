using System;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace instance.id.Extensions
{
    public static class GetScriptLocation
    {
        public static string GetMonoScriptPathFor(this Type type, bool getName = false)
        {
            var asset = "";
            var guids = AssetDatabase.FindAssets(string.Format("{0} t:script", type.Name));
            if (guids.Length > 1)
            {
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Debug.Log($"Path {assetPath}");
                    var filename = Path.GetFileNameWithoutExtension(assetPath);
                    var filePath = Path.ChangeExtension(assetPath, null);

                    if (filename == type.Name)
                    {
                        if (getName)
                        {
                            return filePath;
                        }

                        asset = guid;
                        break;
                    }
                }
            }
            else if (guids.Length == 1)
            {
                asset = guids[0];
            }
            else
            {
                Debug.LogErrorFormat("Unable to locate {0}", type.Name);
                return null;
            }

            return AssetDatabase.GUIDToAssetPath(asset);
        }
    }
}
#endif
