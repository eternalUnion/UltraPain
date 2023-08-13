using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UltrapainExtensions;
using UnityEngine;
using UnityEngine.Audio;

namespace Ultrapain.Patches
{
    [UltrapainPatch]
    [HarmonyPatch(typeof(Mandalore))]
    public static class DruidKnightMemePatch
    {
		public class FullBurstStateInfo
		{
			public GameObject oldProj;
			public GameObject tempProj;
		}

        [HarmonyPatch(nameof(Mandalore.FullBurst))]
        [HarmonyPrefix]
        [UltrapainPatch]
		public static bool CreateFullBurstSound(Mandalore __instance, out FullBurstStateInfo __state)
		{
			__state = new FullBurstStateInfo() { oldProj = __instance.fullAutoProjectile };

			if (__instance.GetComponent<NonUltrapainEnemy>() != null)
				return true;

			GameObject obj = new GameObject();
			obj.transform.position = __instance.transform.position;
			AudioSource aud = obj.AddComponent<AudioSource>();
			aud.playOnAwake = false;
			aud.clip = Plugin.druidKnightFullAutoAud;
			aud.time = 0.205f;
			aud.Play();

			GameObject proj = GameObject.Instantiate(__instance.fullAutoProjectile, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
			proj.GetComponent<AudioSource>().enabled = false;
			__state.tempProj = __instance.fullAutoProjectile = proj;

			return true;
		}

        public static bool CreateFullBurstSoundCheck()
        {
            return ConfigManager.funnyDruidKnightSFXToggle.value;
        }

		[HarmonyPatch(nameof(Mandalore.FullBurst))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ClearFullBurstSound(Mandalore __instance, FullBurstStateInfo __state)
		{
			__instance.fullAutoProjectile = __state.oldProj;
			if (__state.tempProj != null)
				GameObject.Destroy(__state.tempProj);
		}

		public static bool ClearFullBurstSoundCheck()
		{
			return ConfigManager.funnyDruidKnightSFXToggle.value;
		}

		[HarmonyPatch(nameof(Mandalore.FullerBurst))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool FullerBurstSound(Mandalore __instance)
		{
			if (__instance.GetComponent<NonUltrapainEnemy>() != null)
				return true;

			if (__instance.shotsLeft != 40)
				return true;

			GameObject obj = new GameObject();
			obj.transform.position = __instance.transform.position;
			AudioSource aud = obj.AddComponent<AudioSource>();
			aud.playOnAwake = false;
			aud.clip = Plugin.druidKnightFullerAutoAud;
			aud.time = 0.5f;
			aud.Play();
			return true;
		}

		public static bool FullerBurstSoundCheck()
		{
			return ConfigManager.funnyDruidKnightSFXToggle.value;
		}
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(Drone))]
	public static class DruidKnightMemeDronePatch
	{
		[HarmonyPatch(nameof(Drone.Explode))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool ExplosionSound(Drone __instance, out bool __state)
		{
			__state = __instance.exploded;
			return true;
		}

		public static bool ExplosionSoundCheck()
		{
			return ConfigManager.funnyDruidKnightSFXToggle.value;
		}

		[HarmonyPatch(nameof(Drone.Explode))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ExplosionSoundClear(Drone __instance, bool __state)
		{
			if (__state)
				return;

			if (!__instance.exploded || __instance.gameObject.GetComponent<Mandalore>() == null || __instance.gameObject.GetComponent<NonUltrapainEnemy>() != null)
				return;

			GameObject obj = new GameObject();
			obj.transform.position = __instance.transform.position;
			AudioSource aud = obj.AddComponent<AudioSource>();
			aud.playOnAwake = false;
			aud.clip = Plugin.druidKnightDeathAud;
			aud.time = 0.2f;
			aud.Play();
		}

		public static bool ExplosionSoundClearCheck()
		{
			return ConfigManager.funnyDruidKnightSFXToggle.value;
		}
	}
}
