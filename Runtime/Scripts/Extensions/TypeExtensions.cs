using System;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;

namespace instance.id.Extensions
{
	public static class TypeExtensions
	{
		public static bool HasConstructor(this Type type)
		{
			return type.GetConstructors().Length > 0;
		}

		public static bool HasEmptyConstructor(this Type type)
		{
			return type.GetConstructor(Type.EmptyTypes) != null;
		}

		public static bool HasDefaultConstructor(this Type type)
		{
			return type.IsValueType || type.HasEmptyConstructor();
		}

		public static bool HasInterface(this Type type, Type interfaceType)
		{
			return interfaceType.IsAssignableFrom(type) || Array.Exists(type.GetInterfaces(), t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType);
		}

		public static bool Is<T>(this Type type)
		{
			return type.Is(typeof(T));
		}

		public static bool Is(this Type type, Type otherType, params Type[] genericArguments)
		{
			if (genericArguments.Length > 0 && otherType.IsGenericType)
				return type.Is(otherType.MakeGenericType(genericArguments));
			else
				return type.Is(otherType);
		}

		public static bool Is(this Type type, Type otherType)
		{
			return otherType.IsAssignableFrom(type);
		}

		public static bool IsNumerical(this Type type)
		{
			return
				type == typeof(sbyte) ||
				type == typeof(byte) ||
				type == typeof(short) ||
				type == typeof(ushort) ||
				type == typeof(int) ||
				type == typeof(uint) ||
				type == typeof(long) ||
				type == typeof(ulong) ||
				type == typeof(float) ||
				type == typeof(double) ||
				type == typeof(decimal);
		}

		public static bool IsVector(this Type type)
		{
			return
				type == typeof(Vector2) ||
				type == typeof(Vector3) ||
				type == typeof(Vector4);
		}

		public static bool IsConcrete(this Type type)
		{
			return
				!type.IsAbstract &&
				!type.IsInterface &&
				!type.IsGenericTypeDefinition;
		}

		public static bool IsGeneric(this Type type)
		{
			return
				type.IsGenericType ||
				type.IsGenericTypeDefinition ||
				type.IsGenericParameter;
		}

		public static bool IsMetadata(this Type type)
		{
			return
				type == typeof(Type) ||
				type == typeof(Assembly) ||
				type == typeof(AppDomain) ||
				type == typeof(ParameterInfo) ||
				type.Is<MemberInfo>();
		}

		public static bool IsImmutable(this Type type)
		{
			return
				type == typeof(string) ||
				type.IsPrimitive ||
				type.IsEnum ||
				type.IsMetadata() ||
				(type.IsDefined(typeof(ImmutableObjectAttribute), true) && type.GetAttribute<ImmutableObjectAttribute>(true).Immutable);
		}

		/// <summary>
		/// A Pure type is a type that can be deeply copied by assignment.
		/// </summary>
		/// <param name="type">The type to analyse.</param>
		/// <returns>The result of the analysis.</returns>
		public static bool IsPure(this Type type)
		{
			return
				type.IsImmutable() ||
				(type.IsValueType && Array.TrueForAll(type.GetFields(ReflectionExtensions.InstanceFlags), f => f.FieldType.IsPure()));
		}

		public static string GetName(this Type type)
		{
			return type.Name.Split('.').Last().GetRange('`');
		}
	}
}
