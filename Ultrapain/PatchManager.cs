using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ultrapain.Patches;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain
{
	public class UltrapainPatchInfo
	{
		public Type patcher;

		public MethodInfo target;
		public MethodInfo patch;
		public HarmonyPatchType patchType;
		public MethodInfo checker;

		private bool patched = false;
		internal void Patch()
		{
			if (patched)
				return;

			if (patchType == HarmonyPatchType.Prefix)
				PatchManager.harmony.Patch(target, prefix: new HarmonyMethod(patch));
			else if (patchType == HarmonyPatchType.Postfix)
				PatchManager.harmony.Patch(target, postfix: new HarmonyMethod(patch));
			else if (patchType == HarmonyPatchType.Transpiler)
				PatchManager.harmony.Patch(target, transpiler: new HarmonyMethod(patch));
			else if (patchType == HarmonyPatchType.Finalizer)
				PatchManager.harmony.Patch(target, finalizer: new HarmonyMethod(patch));
			patched = true;
		}
		internal void Unpatch()
		{
			if (!patched)
				return;

			PatchManager.harmony.Unpatch(target, patch);
			patched = false;
		}

		public void TryPatch()
		{
			bool result = (bool)checker.Invoke(null, new object[0]);
			if (result)
				Patch();
			else
				Unpatch();
		}
	}

	public static class PatchManager
	{
		public static Harmony harmony;
		private static Dictionary<Type, List<UltrapainPatchInfo>> patches = new Dictionary<Type, List<UltrapainPatchInfo>>();

		public static void AddPatchesFromType(Type type)
		{
			if (patches.ContainsKey(type))
				return;

			Debug.Log($"Adding patch info for {type}");

			if (type.GetCustomAttribute<UltrapainPatchAttribute>() == null)
				throw new ArgumentException($"Type {type} has no UltrapainPatch attribute!");

			HarmonyPatch patchInfo = type.GetCustomAttribute<HarmonyPatch>();
			if (patchInfo == null)
				throw new ArgumentException($"Type {type} has no HarmonyPatch attribute!");

			if (patchInfo.info.declaringType == null)
				throw new ArgumentException($"Type {type}'s HarmonyPatch has no declaring type! Use [HarmonyPatch(Type)]");

			Type targetType = patchInfo.info.declaringType;

			List<UltrapainPatchInfo> patchList = new List<UltrapainPatchInfo>();
			foreach (MethodInfo patchMethod in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Where(method => method.GetCustomAttribute<UltrapainPatchAttribute>() != null))
			{
				if (!HarmonyUtils.TryGetPatchType(patchMethod, out HarmonyPatchType patchType))
					throw new ArgumentException($"Method {patchMethod} has no harmony patch type attribute!");

				HarmonyPatch patchHarmonyInfo = patchMethod.GetCustomAttribute<HarmonyPatch>();
				if (patchHarmonyInfo == null)
					throw new ArgumentException($"Method {patchMethod} has no HarmonyPatch attribute!");

				MethodInfo targetMethod = null;
				if (patchHarmonyInfo.info.argumentTypes != null)
				{
					targetMethod = targetType.GetMethod(patchHarmonyInfo.info.methodName, patchHarmonyInfo.info.argumentTypes);
					if (targetMethod == null)
						throw new ArgumentException($"Method {patchMethod} has invalid target method with name {patchHarmonyInfo.info.methodName} and arguments {string.Join(", ", patchHarmonyInfo.info.argumentTypes.AsEnumerable())}");
				}
				else
				{
					MethodInfo[] validMethods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(method => method.Name == patchHarmonyInfo.info.methodName).ToArray();
					
					if (validMethods.Length == 0)
					{
						throw new Exception($"Method {patchMethod} has invalid target method with name {patchHarmonyInfo.info.methodName}");
					}
					if (validMethods.Length != 1)
					{
						throw new Exception($"Method {patchMethod} has overloaded target method, argument types must be specified");
					}

					targetMethod = validMethods[0];
				}

				string checkerMethodName = patchMethod.Name + "Check";
				MethodInfo checkMethod = type.GetMethod(checkerMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				if (checkMethod == null)
				{
					throw new ArgumentException($"Method {patchMethod} must have a static checker method with signature '[any access modifier] static bool {patchMethod.Name + "Check"}()' in the same class");
				}

				if (checkMethod.ReturnType != typeof(bool))
				{
					throw new ArgumentException($"Method {checkMethod} must have a return type of bool");
				}

				patchList.Add(new UltrapainPatchInfo()
				{
					patcher = type,
					patch = patchMethod,
					checker = checkMethod,
					patchType = patchType,
					target = targetMethod
				});
			}
			
			patches[type] = patchList;
		}

		public static void AddPatchesFromType<T>()
		{
			AddPatchesFromType(typeof(T));
		}
	
		public static void AddPatchesFromAssembly(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes().Where(type => type.GetCustomAttribute<UltrapainPatchAttribute>() != null))
			{
				try
				{
					AddPatchesFromType(type);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
		}
	
		internal static void Init()
		{
			if (harmony == null)
				harmony = new Harmony(Plugin.PLUGIN_GUID);

			AddPatchesFromAssembly(Assembly.GetExecutingAssembly());
		}

		internal static void Reload()
		{
			foreach (var patchList in patches.Values)
			{
				foreach (var patch in patchList)
				{
					if (Plugin.ultrapainDifficulty)
					{
						patch.TryPatch();
					}
					else
					{
						patch.Unpatch();
					}
				}
			}
		}
	}
}
