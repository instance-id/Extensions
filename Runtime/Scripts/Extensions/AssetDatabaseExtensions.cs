// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Extensions                    --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace instance.id.Extensions
{
    public static class AssetDatabaseExtensions
    {
        public static string GetExtensionForType<T>() where T : Object
        {
            string extension = "";
            if (typeof(T) == typeof(Material)) extension = ".mat";
            else if (typeof(T) == typeof(Cubemap)) extension = ".cubemap";
            else if (typeof(T) == typeof(GUISkin)) extension = ".GUISkin";
            else if (typeof(T) == typeof(Animation)) extension = ".anim";
            else extension = ".asset";
            return extension;
        }

        // -- Path Related ------------------------------------------------------------------------

        #region Path Related

        public static string GetAssetGuid(FileInfo fileInfo)
        {
            return AssetDatabase.AssetPathToGUID(GetPathRelativeToProjectPath(fileInfo.FullName));
        }

        public static string GetAssetGuid(this Object obj)
        {
            return AssetDatabase.AssetPathToGUID(GetPathRelativeToProjectPath(obj.GetType().FullName));
        }

        /// <summary>
        /// Returns the `Unity` asset path for the given full path.
        /// </summary>
        public static string GetPathRelativeToProjectPath(string path)
        {
            // @TODO FIXME
            var packageName = "Packages/com.unity.tiny";
            var assetPath = Path.GetFullPath(path).ToForwardSlash();

            // check if the given path is a package path (relative or installed)
            // assumption: true if the path contains the package name
            var packagePartIndex = assetPath.LastIndexOf(packageName, StringComparison.Ordinal);
            if (packagePartIndex >= 0)
            {
                var localPath = packageName + assetPath.Substring(assetPath.IndexOf('/', packagePartIndex));
                return localPath;
            }

            // otherwise, we assume it can be any path, and attempt normalization
            var projectPath = new DirectoryInfo(".").FullName.ToForwardSlash() + "/";
            assetPath = assetPath.Replace(projectPath, string.Empty);
            return assetPath;
        }

        public static string ToAssetPath(this FileInfo fileInfo)
        {
            return GetPathRelativeToProjectPath(fileInfo.FullName);
        }

        public static string ToAssetGuid(this FileInfo fileInfo)
        {
            return GetAssetGuid(fileInfo);
        }

        public static string GetAssetPath(Object obj)
        {
            string path = "";
#if UNITY_EDITOR
            path = AssetDatabase.GetAssetPath(obj).GetRange("/Assets".Length);
#endif
            return path;
        }

        public static string GetResourcesPath(Object obj)
        {
            string resourcesPath = "";
#if UNITY_EDITOR
            resourcesPath = GetResourcesPath(AssetDatabase.GetAssetPath(obj));
#endif
            return resourcesPath;
        }

        public static string GetResourcesPath(string path)
        {
            string resourcesPath = "";
            resourcesPath = GetPathWithoutExtension(path.Substring(path.IndexOf("Resources/") + "Resources/".Length));
            return resourcesPath;
        }

        public static string GetPathWithoutExtension(string path)
        {
            return path.Substring(0, path.Length - Path.GetExtension(path).Length);
        }

        public static string GetFolderPath(string folderName)
        {
            string folderPath = "";
#if UNITY_EDITOR
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (path.EndsWith(folderName))
                {
                    folderPath = path;
                    break;
                }
            }
#endif
            return folderPath;
        }

        public static string[] GetFolderPaths(string folderName)
        {
            List<string> folderPaths = new List<string>();
#if UNITY_EDITOR
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (path.EndsWith(folderName))
                {
                    folderPaths.Add(path);
                    break;
                }
            }
#endif
            return folderPaths.ToArray();
        }

        public static string GetAssetPath(string assetName)
        {
            string assetPath = "";
#if UNITY_EDITOR
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (path.EndsWith(assetName))
                {
                    assetPath = path;
                    break;
                }
            }
#endif
            return assetPath;
        }

        public static bool PathIsRelativeTo(string path, string relativeTo)
        {
            return path.StartsWith(relativeTo);
        }

        /// <summary>
        /// Return a unique path for the file <i>name</i>.
        /// If a path is provided, it will use it as a root.
        /// If no path is provided, it will look for currently selected item.
        /// </summary>
        /// <param name="name">Name of the asset.</param>
        /// <param name="extension">Extension of the asset.</param>
        /// <param name="path">Path of the asset. If none is provided, the selected asset's path will be used.</param>
        public static string GenerateUniqueAssetPath(string name, string extension = "asset", string path = "")
        {
            string uniquePath = "";
#if UNITY_EDITOR
            string assetDirectory = "";
            if (!string.IsNullOrEmpty(path)) assetDirectory = Path.GetDirectoryName(path);
            else if (Selection.activeObject == null) assetDirectory = "Assets";
            else if (Selection.activeObject is DefaultAsset) assetDirectory = AssetDatabase.GetAssetPath(Selection.activeObject);
            else assetDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
            uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetDirectory + "/" + name + "." + extension);
#endif
            return uniquePath;
        }

        public static string GetSelectedAssetPath()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(Selection.activeObject);
#else
			return "";
#endif
        }

        public static string GetSelectedAssetExtention()
        {
#if UNITY_EDITOR
            return Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
#else
			return "";
#endif
        }

        #endregion

        // -- Loading Related ---------------------------------------------------------------------

        #region Loading Related

        public static T[] LoadAllAssetsOfType<T>(this T obj, string path, string extension) where T : Object
        {
            List<T> assets = new List<T>();
#if UNITY_EDITOR
            string[] paths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < paths.Length; i++)
            {
                string assetPath = AssetDatabase.GetAllAssetPaths()[i];
                if (assetPath.StartsWith(path) && assetPath.EndsWith(extension)) assets.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)) as T);
            }
#endif
            return assets.ToArray();
        }

        public static T[] LoadAllAssetsOfType<T>(this T obj, string path) where T : Object
        {
            return LoadAllAssetsOfType<T>(obj, path, GetExtensionForType<T>());
        }

        public static T[] LoadAllAssetsOfType<T>(this T obj) where T : Object
        {
            return LoadAllAssetsOfType<T>(obj,"", GetExtensionForType<T>());
        }

        public static T GetDefaultAssetOfType<T>(string assetName) where T : Object
        {
            T defaultAsset = default(T);
            foreach (Object asset in GetDefaultAssetsOfType<T>())
            {
                if (asset.name == assetName) defaultAsset = asset as T;
            }

            return defaultAsset;
        }

        public static Object GetDefaultAsset(string assetName)
        {
            return GetDefaultAssetOfType<Object>(assetName);
        }

        public static T[] GetDefaultAssetsOfType<T>() where T : Object
        {
            List<T> defaultAssets = new List<T>();
#if UNITY_EDITOR
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath("Library/unity default resources"))
            {
                if (asset is T)
                {
                    defaultAssets.Add(asset as T);
                }
            }
#endif
            return defaultAssets.ToArray();
        }


        public static T LoadAssetInFolder<T>(string assetFileName, string folderName) where T : Object
        {
            T asset = default(T);
#if UNITY_EDITOR
            asset = AssetDatabase.LoadAssetAtPath(GetFolderPath(folderName) + Path.AltDirectorySeparatorChar + assetFileName, typeof(T)) as T;
#endif
            return asset;
        }

        public static Object[] LoadAssetsInFolder<T>(string assetFileName, string folderName) where T : Object
        {
            Object[] assets = null;
#if UNITY_EDITOR
            assets = AssetDatabase.LoadAllAssetsAtPath(GetFolderPath(folderName) + Path.AltDirectorySeparatorChar + assetFileName);
#endif
            return assets;
        }

        public static T GetOrAddAssetOfType<T>(string name, string path) where T : ScriptableObject
        {
            T asset = GetAssetOfType<T>(path);
#if UNITY_EDITOR
            if (asset == null)
            {
                Object[] existingAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                if (existingAssets == null || existingAssets.Length == 0) asset = CreateAssetOfType<T>(name, path);
                else asset = AddAssetOfType<T>(name, path);
            }
#endif
            return asset;
        }

        public static T GetAssetOfType<T>(string path) where T : ScriptableObject
        {
            T asset = null;
#if UNITY_EDITOR
            Object[] existingAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            asset = System.Array.Find(existingAssets, s => s is T) as T;
#endif
            return asset;
        }

        #endregion


        // -- Creation Related --------------------------------------------------------------------

        #region Creation Related

        public static T CreateAssetOfType<T>(string name, string path) where T : ScriptableObject
        {
            T asset = null;
#if UNITY_EDITOR
            asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            AssetDatabase.CreateAsset(asset, path);
#endif
            return asset;
        }

        public static T AddAssetOfType<T>(string name, string path) where T : ScriptableObject
        {
            T asset = null;
#if UNITY_EDITOR
            asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            AssetDatabase.AddObjectToAsset(asset, path);
#endif
            return asset;
        }

        #endregion


        // -- Save Related ------------------------------------------------------------------------

        #region Save Related

        public static void SaveAssets()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        #endregion
    }
}
