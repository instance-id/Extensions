using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace instance.id.Extensions
{
    public static class ReflectionExtensions
    {
        // -- Type Flags --------------------------------------------------------------------------
        public const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public const BindingFlags PublicFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public const BindingFlags NonPublicFlags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        public const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static readonly object[] EmptyArguments = new object[0];
        // -- Type Flags --------------------------------------------------------------------------

        /// <summary>
        /// Cast the object to a given type and return
        /// </summary>
        /// <param name="myobj">The object in which to cast</param>
        /// <param name="type">Type passed via parameter</param>
        /// <typeparam name="T">Type as attribute</typeparam>
        /// <returns>The object but now as the desired type</returns>
        public static T CastTo<T>(this object myobj) where T : Object
        {
            Type objectType = myobj.GetType();
            Type target = typeof(T);

            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                where source.MemberType == MemberTypes.Property select source ;
            var d = from source in target.GetMembers().ToList()
                where source.MemberType == MemberTypes.Property select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
                .ToList().Contains(memberInfo.Name)).ToList();

            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                value = myobj.GetType().GetProperty(memberInfo.Name)?.GetValue(myobj,null);

                propertyInfo?.SetValue(x,value,null);
            }
            return (T)x;
        }

        /// <summary>
        /// Cast the object to a given type and return
        /// </summary>
        /// <param name="myobj">The object in which to cast</param>
        /// <param name="type">Type passed via parameter</param>
        /// <typeparam name="T">Type as attribute</typeparam>
        /// <returns>The object but now as the desired type</returns>
        public static object CastTo(this object myobj, Type type = null)
        {
            Type objectType = myobj.GetType();
            Type target = type;
            // if (type != null) target = type;

            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                where source.MemberType == MemberTypes.Property select source ;
            var d = from source in target.GetMembers().ToList()
                where source.MemberType == MemberTypes.Property select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
                .ToList().Contains(memberInfo.Name)).ToList();

            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = type.GetProperty(memberInfo.Name);
                value = myobj.GetType().GetProperty(memberInfo.Name)?.GetValue(myobj,null);
                propertyInfo?.SetValue(x,value,null);
            }
            return x;
        }

        // -------------------------------------------------------------- GetFieldViaPath
        // -- GetFieldViaPath -----------------------------------------------------------
        public static FieldInfo GetFieldViaPath(this Type type, string path)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var parent = type;
            var fieldInfo = parent.GetField(path, flags);
            var paths = path.Split('.');
            for (var i = 0; i < paths.Length; i++)
            {
                if (parent is null) continue;
                fieldInfo = parent.GetField(paths[i], flags);
                switch (fieldInfo)
                {
                    // there are only two container field type that can be serialized:
                    // Array and List<T>
                    case FieldInfo fi when fi.FieldType.IsArray:
                        parent = fi.FieldType.GetElementType();
                        i += 2;
                        continue;
                    case FieldInfo fi when fi.FieldType.IsGenericType:
                        parent = fi.FieldType.GetGenericArguments()[0];
                        i += 2;
                        continue;
                }

                if (fieldInfo != null)
                {
                    parent = fieldInfo.FieldType;
                }
                else
                {
                    return null;
                }
            }

            return fieldInfo;
        }

                // @formatter:off --------------------------------------- GetValueDerivedFields()
        // -- If you want to find derived fields and properties when fetching the value -
        // -- GetValueDerivedFields() ---------------------------------------------------
        public static Tuple<List<FieldInfo>, List<PropertyInfo>> GetValueDerivedFields(
            this object source, bool isType = false) // @formatter:on
        {
            if (source == null) return null;
            var fieldInfoList = new List<FieldInfo>();
            var propertyInfoList = new List<PropertyInfo>();
            var type = isType ? source as Type : source.GetType();
            var allFields = type.GetFields();
            if (allFields.Length == 0)
            {
                Debug.LogWarning($"Type: {source.ToString()} has no results!");
                return null;
            }

            for (var i = 0; i < allFields.Length; i++)
            {
                var fieldInfo = FindFieldInTypeHierarchy(type, allFields[i].Name);
                if (!(fieldInfo is null)) fieldInfoList.Add(fieldInfo);
                else
                {
                    var propertyInfo = FindPropertyInTypeHierarchy(type, allFields[i].Name);
                    if (!(propertyInfo is null)) propertyInfoList.Add(propertyInfo);
                }
            }

            return new Tuple<List<FieldInfo>, List<PropertyInfo>>(fieldInfoList, propertyInfoList);
        }

                // @formatter:off ---------------------------------------- GetValueDerivedField()
        // -- If you want to find derived fields and properties when fetching the value -
        // -- GetValueDerivedField() ----------------------------------------------------
        #region GetValues
        public static object GetValueDerivedField(this object source, string name) // @formatter:on
        {
            if (source == null) return null;
            var type = source.GetType();
            var f = FindFieldInTypeHierarchy(type, name);
            if (f == null)
            {
                var p = FindPropertyInTypeHierarchy(type, name);
                if (p == null) return null;
                return p.GetValue(source, null);
            }

            return f.GetValue(source);
        }

        // ------------------------
        public static FieldInfo FindFieldInTypeHierarchy(Type providedType, string fieldName)
        {
            var field = providedType.GetField(fieldName, (BindingFlags) (-1));
            while (field == null && providedType.BaseType != null)
            {
                providedType = providedType.BaseType;
                field = providedType.GetField(fieldName, (BindingFlags) (-1));
            }

            return field;
        }

        // ------------------------
        public static PropertyInfo FindPropertyInTypeHierarchy(Type providedType, string propertyName)
        {
            var property = providedType.GetProperty(propertyName, (BindingFlags) (-1));
            while (property == null && providedType.BaseType != null)
            {
                providedType = providedType.BaseType;
                property = providedType.GetProperty(propertyName, (BindingFlags) (-1));
            }

            return property;
        }

        #endregion

        // @formatter:off -------------------------------------------- GetInspectedType()
        // -- Get all fields and related data from within a particular class           --
        // -- GetInspectedType() --------------------------------------------------------
        private static Lazy<FieldInfo> m_InspectedType = new Lazy<FieldInfo>(() => typeof(CustomEditor)
                .GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.NonPublic));

        public static Type GetInspectedType(this CustomEditor attribute)
        {
            return (Type) m_InspectedType.Value.GetValue(attribute);
        } // @formatter:on

                // @formatter:off ------------------------------------------------ GetAttribute()
        // -- GetAttribute() ------------------------------------------------------------
        public static T GetAttribute<T>(Type rObjectType)
        {
#if !UNITY_EDITOR && (NETFX_CORE || WINDOWS_UWP || UNITY_WP8 || UNITY_WP_8_1 || UNITY_WSA || UNITY_WSA_8_0 || UNITY_WSA_8_1 || UNITY_WSA_10_0)
            System.Collections.Generic.IEnumerable<System.Attribute> lInitialAttributes = rObjectType.GetTypeInfo().GetCustomAttributes(typeof(T), true);
            object[] lAttributes = lInitialAttributes.ToArray();
#else
            var lAttributes = rObjectType.GetCustomAttributes(typeof(T), true);
#endif
            if (lAttributes == null || lAttributes.Length == 0)
            {
                return default(T);
            }

            return (T) lAttributes[0];
        } // @formatter:on

        // ------------------------------------------------------------ GetRealTypeName()
        // -- GetRealTypeName() ---------------------------------------------------------
        public static string GetRealTypeName(this Type t, bool parameters = false)
        {
            if (!t.IsGenericType) return t.Name;
            var sb = new StringBuilder();
            sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
            if (!parameters) return sb.ToString();
            sb.Append('<');
            var appendComma = false;
            foreach (var arg in t.GetGenericArguments())
            {
                if (appendComma) sb.Append(',');
                sb.Append(GetRealTypeName(arg));
                appendComma = true;
            }

            sb.Append('>');
            return sb.ToString();
        }

        public static MemberInfo GetFieldOrPropertyInfo(this object obj, string memberName)
        {
            var field = obj.GetType().GetField(memberName, AllFlags);
            if (field != null) return field;
            var property = obj.GetType().GetProperty(memberName, AllFlags);
            if (property != null) return property;
            return null;
        }

        public static T GetMemberValue<T>(this MemberInfo member, object obj)
        {
            return (T) member.GetMemberValue(obj);
        }

        public static object GetMemberValue(this MemberInfo member, object obj)
        {
            var field = member as FieldInfo;
            if (field != null) return field.GetValue(obj);
            var property = member as PropertyInfo;
            if (property != null) return property.GetValue(obj, null);
            return null;
        }

        public static void SetMemberValue(this MemberInfo member, object obj, object value)
        {
            var field = member as FieldInfo;
            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }

            var property = member as PropertyInfo;
            if (property != null)
            {
                property.SetValue(obj, value, null);
                return;
            }
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            var field = member as FieldInfo;
            if (field != null) return field.FieldType;
            var property = member as PropertyInfo;
            if (property != null) return property.PropertyType;
            return null;
        }

        public static MemberInfo GetMemberInfoAtPath(this object obj, string memberPath)
        {
            int offset = 0;
            int separatorIndex = memberPath.IndexOf('.');
            if (separatorIndex == -1) return obj.GetFieldOrPropertyInfo(memberPath);
            var value = obj.GetValueFromMember(memberPath.Substring(offset, separatorIndex - offset));
            offset = separatorIndex + 1;
            separatorIndex = memberPath.IndexOf('.', offset);
            while (separatorIndex != -1)
            {
                value = value.GetValueFromMember(memberPath.Substring(offset, separatorIndex - offset));
                offset = separatorIndex + 1;
                separatorIndex = memberPath.IndexOf('.', offset);
            }

            return value.GetFieldOrPropertyInfo(memberPath.Substring(offset));
        }

        public static T GetValueFromMember<T>(this object obj, string memberName)
        {
            return (T) obj.GetValueFromMember(memberName);
        }

        public static object GetValueFromMember(this object obj, string memberName)
        {
            if (obj is IList) return ((IList) obj)[int.Parse(memberName)];
            var member = obj.GetFieldOrPropertyInfo(memberName);
            return member.GetMemberValue(obj);
        }

        public static T GetValueFromMemberAtPath<T>(this object obj, string memberPath)
        {
            return (T) obj.GetValueFromMemberAtPath(memberPath);
        }

        public static object GetValueFromMemberAtPath(this object obj, string memberPath)
        {
            var member = obj.GetMemberInfoAtPath(memberPath);
            var pathSplit = memberPath.Split('.');
            if (pathSplit.Length <= 1)
            {
                return obj.GetValueFromMember(pathSplit.Pop(out pathSplit));
            }

            int index;
            if (int.TryParse(pathSplit.Last(), out index))
            {
                Array.Resize(ref pathSplit, pathSplit.Length - 1);
                return ((IList) obj.GetValueFromMemberAtPath(pathSplit.Concat(".")))[index];
            }

            Array.Resize(ref pathSplit, pathSplit.Length - 1);
            object container = obj.GetValueFromMemberAtPath(pathSplit.Concat("."));
            return member.GetMemberValue(container);
        }

        public static void SetValueToMember(this object obj, string memberName, object value)
        {
            var member = obj.GetFieldOrPropertyInfo(memberName);
            member.SetMemberValue(obj, value);
        }

        public static void SetValueToMemberAtPath(this object obj, string memberPath, object value)
        {
            var member = obj.GetMemberInfoAtPath(memberPath);
            var pathSplit = memberPath.Split('.');
            if (pathSplit.Length <= 1)
            {
                obj.SetValueToMember(memberPath, value);
                return;
            }

            Array.Resize(ref pathSplit, pathSplit.Length - 1);
            var container = obj.GetValueFromMemberAtPath(pathSplit.Concat("."));
            member.SetMemberValue(container, value);
        }

        public static object InvokeMethod(this object obj, string methodName, params object[] arguments)
        {
            var methods = obj.GetType().GetMethods(AllFlags);
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name == methodName && method.GetParameters().Length == arguments.Length) return method.Invoke(obj, arguments);
            }

            return null;
        }

        public static string[] GetFieldsPropertiesNames(this object obj, BindingFlags flags, params Type[] filter)
        {
            return obj.GetType().GetFieldsAndPropertiesNames(flags, filter);
        }

        public static string[] GetFieldsPropertiesNames(this object obj, params Type[] filter)
        {
            return obj.GetType().GetFieldsAndPropertiesNames(AllFlags, filter);
        }

        public static string GetTypeName(this object obj)
        {
            return obj.GetType().GetName();
        }

        public static string[] GetFieldsAndPropertiesNames(this Type type, BindingFlags flags, params Type[] filter)
        {
            var names = new List<string>();
            var fields = type.GetFields(flags);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (filter == null || filter.Length == 0 || filter.Any(t => t.IsAssignableFrom(field.FieldType))) names.Add(field.Name);
            }

            var properties = type.GetProperties(flags);
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (filter == null || filter.Length == 0 || filter.Any(t => t.IsAssignableFrom(property.PropertyType))) names.Add(property.Name);
            }

            return names.ToArray();
        }

        public static string[] GetFieldsAndPropertiesNames(this Type type, params Type[] filter)
        {
            return GetFieldsAndPropertiesNames(type, AllFlags, filter);
        }

        public static object GetValueFromField(this object obj, string fieldName)
        {
            if (obj is IList) return ((IList) obj)[int.Parse(fieldName)];
            else return obj.GetType().GetField(fieldName, AllFlags).GetValue(obj);
        }

        public static object GetValueFromFieldAtPath(this object obj, string path)
        {
            int separatorIndex = path.IndexOf('.');
            if (separatorIndex == -1) return obj.GetValueFromField(path);
            var value = obj.GetValueFromField(path.Substring(0, separatorIndex));
            int offset = 0;
            do
            {
                offset = separatorIndex + 1;
                separatorIndex = path.IndexOf('.', offset);
                string currentPath;
                if (separatorIndex == -1) currentPath = path.Substring(offset);
                else currentPath = path.Substring(offset, separatorIndex - offset);
                value = value.GetValueFromField(currentPath);
            } while (separatorIndex != -1);

            return value;
        }

        public static void SetValueToField(this object obj, string fieldName, object value)
        {
            if (obj is IList) ((IList) obj)[int.Parse(fieldName)] = value;
            else obj.GetType().GetField(fieldName, AllFlags).SetValue(obj, value);
        }

        public static void SetValueToFieldAtPath(this object obj, string path, object value)
        {
            int separatorIndex = path.IndexOf('.');
            if (separatorIndex == -1)
            {
                obj.SetValueToField(path, value);
                return;
            }

            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '.')
                {
                    separatorIndex = i;
                    break;
                }
            }

            var parent = obj.GetValueFromFieldAtPath(path.Substring(0, separatorIndex));
            var fieldName = path.Substring(separatorIndex + 1);
            parent.SetValueToField(fieldName, value);
        }

        public static object GetValueFromFieldAtPath(this object obj, string[] path)
        {
            var parent = obj;
            for (int i = 0; i < path.Length - 1; i++) parent = parent.GetValueFromField(path[i]);
            return parent.GetValueFromField(path.Last());
        }

        public static void SetValueToFieldAtPath(this object obj, string[] path, object value)
        {
            var parent = obj;
            for (int i = 0; i < path.Length - 1; i++) parent = parent.GetValueFromField(path[i]);
            parent.SetValueToField(path.Last(), value);
        }

        public static bool IsBackingField(this FieldInfo field)
        {
            return field.IsDefined(typeof(CompilerGeneratedAttribute), true) && field.Name.Contains(">k__BackingField");
        }

        public static PropertyInfo GetAutoProperty(this FieldInfo field)
        {
            return field.DeclaringType.GetProperty(field.Name.GetRange(1, '>'), AllFlags);
        }

        public static bool IsStatic(this PropertyInfo property)
        {
            if (property.CanRead) return property.GetGetMethod(true).IsStatic;
            else return property.GetSetMethod(true).IsStatic;
        }

        public static bool IsAutoProperty(this PropertyInfo property)
        {
            return property.GetBackingField() != null;
        }

        public static bool IsPrivate(this PropertyInfo property)
        {
            return property.GetGetMethod(false) == null && property.GetSetMethod(false) == null;
        }

        public static bool IsAbstract(this PropertyInfo property)
        {
            if (property.CanRead) return property.GetGetMethod(true).IsAbstract;
            else return property.GetSetMethod(true).IsAbstract;
        }

        public static bool IsVirtual(this PropertyInfo property)
        {
            if (property.CanRead) return property.GetGetMethod(true).IsVirtual;
            else return property.GetSetMethod(true).IsVirtual;
        }

        public static FieldInfo GetBackingField(this PropertyInfo property)
        {
            return property.DeclaringType.GetField("<" + property.Name + ">k__BackingField", AllFlags);
        }

        public static bool IsDefined<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            return provider.IsDefined(typeof(T), inherit);
        }

        public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            return (T) provider.GetCustomAttributes(typeof(T), inherit).First();
        }

        public static T[] GetAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
        }

        // ---------------------------------------------------------- GetAssemblyByName()
        // -- GetAssemblyByName() -------------------------------------------------------
        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == name);
        }

        private static bool IsDynamic(Assembly assembly)
        {
            return assembly.GetType().FullName == "System.Reflection.Emit.AssemblyBuilder" ||
                   assembly.GetType().FullName == "System.Reflection.Emit.InternalAssemblyBuilder";
        }

        // ----------------------------------------------------------- GetExportedTypes()
        // -- GetExportedTypes() --------------------------------------------------------
        private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types;
            }
            catch (NotSupportedException)
            {
                return new Type[0];
            }
        }

        // ------------------------------------------------------------------- AllTypes()
        // -- AllTypes() ----------------------------------------------------------------
        public static IEnumerable<Type> AllTypes
        {
            get { return AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsDynamic(a)).SelectMany(GetExportedTypes).ToArray(); }
        }

        // ---------------------------------------------------------------- GetAllTypes()
        // -- GetAllTypes() -------------------------------------------------------------
        private static IEnumerable<Type> GetAllTypes(Type inspectedType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsDynamic(a)).SelectMany(GetExportedTypes).ToArray();
        }

        // ---------------------------------------------------------------- GetSubtypes()
        // -- GetSubtypes() -------------------------------------------------------------
        public static IEnumerable<Type> GetSubtypes(Type inspectedType, IEnumerable<Assembly> assemblies) // @formatter:off
        {
            var assemblyTypes = assemblies
                .SelectMany(asm => asm.GetExportedTypes())
                .Where(t => { try { return t != null && t != inspectedType; }catch{ return false; } });

            if (inspectedType.IsInterface) // @formatter:on
            {
                var implementers = assemblyTypes.Where(t => inspectedType.IsAssignableFrom(t));
                foreach (var implementer in implementers)
                {
                    if (implementer.BaseType == null) yield return implementer;
                    else if (!implementer.BaseType.IsAssignableFrom(inspectedType)) yield return implementer;
                }
            }
            else
            {
                var subTypes = assemblyTypes.Where(asmType => asmType.BaseType == inspectedType);
                foreach (var type in subTypes) yield return type;
            }
        }
    }
}
