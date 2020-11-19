using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace instance.id.Extensions
{
    public static class PathTools
    {


        /// <summary>
        /// Checks for existence of specified path, if it does not exist,
        /// it is created and the full path is returned
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CreateIfNotExists(this string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"InstanceId failed to create data folder: {path} : {ex}");
            }

            return path;
        }

        /// <summary>
        /// Pass SubFolder to receive full AppData Path in return
        /// </summary>
        /// <param name="subFolder">The subfolder in which to create if it does not exist</param>
        /// <returns>The full AppData path including subfolder</returns>
        public static string GetAppData(this string subFolder)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return CreateIfNotExists(Path.Combine(appData, subFolder));
        }

        /// <summary>
        /// Return the current project's name as text string
        /// </summary>
        /// <returns>The full AppData path including subfolder</returns>
        public static string GetProjectName()
        {
            return Directory.GetParent(Application.dataPath).Name;
        }

        /// <summary>
        /// Return the current project's name as text string
        /// </summary>
        /// <returns>The full AppData path including subfolder</returns>
        public static string GetProjectRoot(string subFolders = null, string fileName = null)
        {
            return string.IsNullOrEmpty(subFolders)
                ? fileName != null
                    ? Path.Combine(Directory.GetParent(Application.dataPath).ToString(), fileName)
                    : Directory.GetParent(Application.dataPath).ToString()
                : fileName != null
                    ? Path.Combine(Directory.GetParent(Application.dataPath).ToString(), subFolders, fileName)
                    : Path.Combine(Directory.GetParent(Application.dataPath).ToString(), subFolders);
        }

        public static string GetAssetPath(string subFolders)
        {
            return Path.Combine(Application.dataPath, subFolders);
        }

        /// <summary>
        /// Locate the local path of a script file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="getName"></param>
        /// <param name="getFolder"></param>
        /// <returns></returns>
        public static string GetScriptPath(this Type type, bool getName = false, bool getFolder = false)
        {
            string guidValue = "";
            var guidArray = AssetDatabase.FindAssets($"t:Script {type.Name}");
            if (guidArray.Length > 1)
            {
                foreach (var guid in guidArray)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var folderPath = Directory.GetParent(assetPath);
                    var filePath = Path.ChangeExtension(assetPath, null);
                    var filename = Path.GetFileNameWithoutExtension(assetPath);

                    if (filename != type.Name) continue;
                    if (getFolder) return folderPath.ToString();
                    if (getName) return filePath;

                    guidValue = guid;
                    break;
                }
            }
            else if (guidArray.Length == 1)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guidArray[0]);
                var folderPath = Directory.GetParent(assetPath);
                var filePath = Path.ChangeExtension(assetPath, null);
                var filename = Path.GetFileNameWithoutExtension(assetPath);

                if (filename != type.Name) return AssetDatabase.GUIDToAssetPath(guidValue);

                if (getFolder) return folderPath.ToString();
                if (getName) return filePath;
                guidValue = guidArray[0];
            }
            else
            {
                Debug.LogErrorFormat("Unable to locate {0}", type.Name);
                return null;
            }


            return AssetDatabase.GUIDToAssetPath(guidValue);
        }

        /// <summary>
        /// Takes supplied path and replaces backslahes(\) with forwardslashes(/)
        /// </summary>
        /// <param name="path">The path in which to replaces slashes</param>
        /// <returns>Path containing only forwardslashes(/)</returns>
        public static string ReplaceBackWithForwardSlashes(string path)
        {
            return path.Replace("\\", "/");
        }

        public static void EnsureFolderExists(string folder, bool refreshAfterCreation = false)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
#if UNITY_EDITOR
                if (refreshAfterCreation)
                    AssetDatabase.Refresh();
#endif
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="callerFilePath"></param>
        /// <returns></returns>
        public static string GetScriptDirectory(this object obj, [CallerFilePath] string callerFilePath = null)
        {
            var folder = Path.GetDirectoryName(callerFilePath);

#if UNITY_EDITOR_WIN
            if (folder != null)
            {
                folder = folder.Substring(folder.LastIndexOf(@"\Assets\", StringComparison.Ordinal) + 1);
#else
                folder = folder.Substring(folder.LastIndexOf("/Assets/", StringComparison.Ordinal) + 1);
#endif
            }
            else Debug.LogWarning("PathTools: Directory could not be found via GetAssetPath()");

            return folder + @"\";
        }

        /// <summary>
        /// Obtains the script path of the originating caller of this method
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sourceFilePath"></param>
        /// <returns>Full path of method caller </returns>
        public static string GetScriptFullPath(this object obj, [CallerFilePath] string sourceFilePath = "")
        {
            return sourceFilePath;
        }

        /// <summary>
        /// Get the caller's path (from Assets)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sourceFilePath"></param>
        /// <returns>Path of method caller using /Assets folder as the root directory</returns>
        public static string GetScriptPath(this object obj, [CallerFilePath] string sourceFilePath = "")
        {
            return new Uri(Application.dataPath).MakeRelativeUri(new Uri(sourceFilePath)).ToString();
        }
    }
}
