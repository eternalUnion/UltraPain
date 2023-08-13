using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
    [UltrapainPatch]
    [HarmonyPatch(typeof(EnrageEffect))]
    public static class EnrageEffectPatch
    {
        [HarmonyPatch(nameof(EnrageEffect.Start))]
        [HarmonyPostfix]
        [UltrapainPatch]
        public static void CreateEnrageSFX(EnrageEffect __instance)
        {
            AudioSource enrageAud = __instance.gameObject.GetComponents<AudioSource>().Where(src => src.loop).First();
            if (enrageAud.isPlaying)
                enrageAud.Stop();
            enrageAud.clip = Plugin.enrageAudioCustom;
            enrageAud.Play();
        }

        public static bool CreateEnrageSFXCheck()
        {
            return ConfigManager.enrageSfxToggle.value;
        }
	}
}
