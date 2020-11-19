// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/Extensions                    --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace instance.id.Extensions
{
    public static class InstanceIdExtensions
    {
        /// <summary>
        /// Check if component exists on a GameObject, if so, it can be used as an out parameter in a containing scope.
        /// if (gameObject.TryGetComponent<Component>(out var component))
        /// {
        ///     Use the component within this scope.
        /// }
        /// </summary>
        /// <returns>bool</returns>
        public static bool TryGetAddComponent<T>(this GameObject obj, out T result) where T : Component
        {
            return (result = obj.GetOrAddComponent<T>()) != null;
        }

        /// <summary>
        /// Remove Component from Object.
        /// </summary>
        /// <param name="Component"></param>
        /// <returns></returns>
        public static void RemoveComponent<T>(this Component self) where T : Component
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(self.GetComponent<T>());
#else
            GameObject.Destroy(self.GetComponent<T>());
#endif
        }

        /// <summary>
        /// Remove Component from GameObject.
        /// </summary>
        /// <param name="GameObject"></param>
        /// <returns></returns>
        public static void RemoveComponent<T>(this GameObject self) where T : Component
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(self.GetComponent<T>());
#else
            GameObject.Destroy(self.GetComponent<T>());
#endif
        }

        /// <summary>
        /// Returns all child objects of the transform.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<GameObject> GetChildren(this Transform obj)
        {
            return from Transform transform in obj.transform select transform.gameObject;
        }

        /// <summary>
        /// If the gameobject already has the given component, it is returned. 
        /// Otherwise a new component is attached and then returned.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject component) where T : Component
        {
            return component.GetComponent<T>() ?? component.gameObject.AddComponent<T>();
        }

        // public static T GetOrAddComponent<T>(this GameObject gameObject)
        // 	where T : T {
        // 	return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        // }

        /// <summary>
        /// Checks if provided component type is present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        public static bool HasComponent<T>(this Component component) where T : Component
        {
            return component.GetComponent<T>() != null;
        }

        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }

        // public static bool HasComponent<T>(this GameObject gameObject, out T component) where T : Component
        // {
        //     if (gameObject.GetComponent<T>() != null)
        //     {
        //         component = gameObject.GetComponent<T>();
        //         return true;
        //     }
        //
        //     component = null;
        //     return false;
        // }

        /// <summary>
        /// Checks if the child rectangle is completely inside this rect.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static bool Contains(this Rect parent, Rect child)
        {
            return parent.Contains(child.min) && parent.Contains(child.max);
        }

        /// <summary>
        /// Checks if the child bounds are completely inside this bounds.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static bool Contains(this Bounds parent, Bounds child)
        {
            return parent.Contains(child.min) && parent.Contains(child.max);
        }
    }
}
