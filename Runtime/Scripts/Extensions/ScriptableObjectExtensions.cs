using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace instance.id.Extensions
{
    public static class ScriptableObjectExtensions
    {
#if UNITY_EDITOR
        //**** Modifiers ****//
        public static T CreateTypeAsset<T>(this T type, string filePath = null, string[] label = null) where T : ScriptableObject
        {
            return CreateAsset<T>(filePath, label);
        }

        public static T CreateAsset<T>(string filePath = null, string[] label = null) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (filePath == null || filePath.Length <= 0)
            {
                filePath = "Assets/" + typeof(T).ToString() + ".asset";
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(filePath);

            if (assetPathAndName != filePath)
            {
                Debug.LogWarning("Asset file has been changed from " + filePath + " to " + assetPathAndName);
            }

            SaveAsset(asset, assetPathAndName);

            if (label != null && !string.IsNullOrEmpty(label[0])) AssetDatabase.SetLabels(asset, label);

            return asset;
        }

        public static void CreateAsset(string classString, string filePath = null)
        {
            var asset = ScriptableObject.CreateInstance(classString);

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            Debug.Log($"Save Path: {directory}");

            if (filePath == null || filePath.Length <= 0)
            {
                filePath = "Assets/" + Type.GetType(classString) + ".asset";
            }

            Debug.Log($"filePath: {filePath}");

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(filePath);

            if (assetPathAndName != filePath)
            {
                Debug.LogWarning("Asset file has been changed from " + filePath + " to " + assetPathAndName);
            }

            Debug.Log($"assetPathAndName: {assetPathAndName}");
            SaveAsset(asset, assetPathAndName);
        }

        public static void RenameAsset<T>(T asset, string newName) where T : ScriptableObject
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), newName);
        }

        public static void SaveAsset<T>(T asset, string path) where T : ScriptableObject
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static T DuplicateAsset<T>(T asset, string newPath = null) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (newPath == null)
                newPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CopyAsset(path, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<T>(newPath);
        }

        public static void ReplaceAsset<T>(T asset, string path) where T : ScriptableObject
        {
            T oldAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (oldAsset == null)
            {
                SaveAsset<T>(asset, path);
            }
            else
            {
                EditorUtility.CopySerialized(asset, oldAsset);
                EditorUtility.SetDirty(oldAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        // -- DeleteAsset Extension method (ex. scriptableObject.DeleteAsset(); )
        public static void DeleteAsset(this ScriptableObject scriptableObject)
        {
            DeleteAsset<ScriptableObject>(scriptableObject);
        }

        public static void DeleteAsset<T>(T asset) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ReloadAsset<T>(ref T asset) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);

            Resources.UnloadAsset(asset);
            asset = AssetDatabase.LoadAssetAtPath(path, asset.GetType()) as T;
        }

        //**** Queries ****//
        public static bool AssetExists<T>(string path) where T : ScriptableObject
        {
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(T));

            return obj != null;
        }

        public static string GetAssetNameWithoutExtension<T>(T asset) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);

            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetAssetName<T>(T asset) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(asset);

            return Path.GetFileName(path);
        }

        public static List<ObjectType> GetAllScriptableObjectsOfType<ObjectType>() where ObjectType : ScriptableObject
        {
            List<ObjectType> res = new List<ObjectType>();
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ObjectType).FullName, new string[] {"Assets"});

            foreach (string guid in guids)
            {
                res.Add(AssetDatabase.LoadAssetAtPath<ObjectType>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return res;
        }

        public static List<string> GenerateGameDatabaseOfAllScriptableObjectsOfType<ObjectType>() where ObjectType : ScriptableObject
        {
            List<string> res = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(ObjectType).FullName, new string[] {"Assets"});

            foreach (string guid in guids)
            {
                res.Add(AssetDatabase.GUIDToAssetPath(guid).Replace("Assets/Resources", ""));
            }

            return res;
        }
#endif
    }
}
