using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ultrapain
{
	public static class HarmonyUtils
	{
		public class HarmonyTypePatch
		{
			public MethodInfo targetMethod;
			public MethodInfo prefix;
			public MethodInfo postfix;
			public MethodInfo transpiler;
			public MethodInfo finalizer;
		}

		/// <summary>
		/// Returns all patch methods for a type with a [<see cref="HarmonyPatch.HarmonyPatch(Type)"/>] attribute
		/// </summary>
		/// <param name="type">Type of the class with the attribute</param>
		/// <returns>All patches the class has</returns>
		public static List<HarmonyTypePatch> GetAllPatches(Type type)
		{
			HarmonyPatch typePatchInfo = type.GetCustomAttribute<HarmonyPatch>();
			if (typePatchInfo == null)
				return new List<HarmonyTypePatch>();

			Type targetType = typePatchInfo.info.declaringType;
			if (typePatchInfo == null)
				return new List<HarmonyTypePatch>();

			List<HarmonyTypePatch> currentPatches = new List<HarmonyTypePatch>();
			foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				HarmonyPatch methodInfo = method.GetCustomAttribute<HarmonyPatch>();
				if (methodInfo == null)
					continue;

				MethodInfo targetMethod = null;

				if (methodInfo.info.argumentTypes != null)
					targetMethod = targetType.GetMethod(methodInfo.info.methodName, methodInfo.info.argumentTypes);
				else
					targetMethod = targetType.GetMethod(methodInfo.info.methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

				if (targetMethod == null)
					continue;

				HarmonyTypePatch patchInfo = currentPatches.Where(info => info.targetMethod == targetMethod).FirstOrDefault();
				if (patchInfo == null)
				{
					patchInfo = new HarmonyTypePatch() { targetMethod = targetMethod };
					currentPatches.Add(patchInfo);
				}

				if (method.GetCustomAttribute<HarmonyPrefix>() != null)
					patchInfo.prefix = method;
				else if (method.GetCustomAttribute<HarmonyPostfix>() != null)
					patchInfo.postfix = method;
				else if (method.GetCustomAttribute<HarmonyTranspiler>() != null)
					patchInfo.transpiler = method;
				else if (method.GetCustomAttribute<HarmonyFinalizer>() != null)
					patchInfo.finalizer = method;
			}

			return currentPatches;
		}

		/// <summary>
		/// Returns patches for a single method in a type with a [<see cref="HarmonyPatch.HarmonyPatch(Type)"/>] attribute
		/// </summary>
		/// <param name="type">Type of the class with the attribute</param>
		/// <param name="methodName">Name of the method which is patched</param>
		/// <param name="argumentTypes">Optional arguments of the method</param>
		/// <returns></returns>
		public static HarmonyTypePatch GetPatches(Type type, string methodName, Type[] argumentTypes = null)
		{
			HarmonyTypePatch patchInfo = new HarmonyTypePatch();

			HarmonyPatch typePatchInfo = type.GetCustomAttribute<HarmonyPatch>();
			if (typePatchInfo == null)
				return null;

			Type targetType = typePatchInfo.info.declaringType;
			if (typePatchInfo == null)
				return null;

			MethodInfo targetMethod = null;
			if (argumentTypes != null)
				targetMethod = targetType.GetMethod(methodName, argumentTypes);
			else
				targetMethod = targetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

			if (targetMethod == null)
				return null;
			patchInfo.targetMethod = targetMethod;

			foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				HarmonyPatch methodInfo = method.GetCustomAttribute<HarmonyPatch>();
				if (methodInfo == null)
					continue;
				if (methodInfo.info.methodName != methodName)
					continue;

				if (methodInfo.info.argumentTypes == null ^ argumentTypes == null)
					continue;
				if (argumentTypes != null && !Enumerable.SequenceEqual(methodInfo.info.argumentTypes, argumentTypes))
					continue;

				if (method.GetCustomAttribute<HarmonyPrefix>() != null)
					patchInfo.prefix = method;
				else if (method.GetCustomAttribute<HarmonyPostfix>() != null)
					patchInfo.postfix = method;
				else if (method.GetCustomAttribute<HarmonyTranspiler>() != null)
					patchInfo.transpiler = method;
				else if (method.GetCustomAttribute<HarmonyFinalizer>() != null)
					patchInfo.finalizer = method;
			}

			return patchInfo;
		}
	
		public static bool TryGetPatchType(MethodInfo method, out HarmonyPatchType type)
		{
			type = HarmonyPatchType.All;

			if (method.GetCustomAttribute<HarmonyPrefix>() != null)
			{
				type = HarmonyPatchType.Prefix;
				return true;
			}
			else if (method.GetCustomAttribute<HarmonyPostfix>() != null)
			{
				type = HarmonyPatchType.Postfix;
				return true;
			}
			else if (method.GetCustomAttribute<HarmonyTranspiler>() != null)
			{
				type = HarmonyPatchType.Transpiler;
				return true;
			}
			else if (method.GetCustomAttribute<HarmonyFinalizer>() != null)
			{
				type = HarmonyPatchType.Finalizer;
				return true;
			}

			return false;
		}
	}
}
