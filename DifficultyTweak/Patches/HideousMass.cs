using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    public class HideousMassProjectile : MonoBehaviour
    {

    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Explode))]
    public class Projectile_Explode_Patch 
    {
        public static float windupSpeedMultiplier = 2.5f;
        public static float beamSize = 2f;

        static void Postfix(Projectile __instance)
        {
            HideousMassProjectile flag = __instance.gameObject.GetComponent<HideousMassProjectile>();
            if (flag == null)
                return;

            GameObject insignia = GameObject.Instantiate(Plugin.virtueInsignia, __instance.transform.position, Quaternion.identity);
            insignia.transform.localScale = new Vector3(beamSize, 1f, beamSize);
            VirtueInsignia comp = insignia.GetComponent<VirtueInsignia>();
            comp.windUpSpeedMultiplier = windupSpeedMultiplier;
            comp.damage = 20;
            comp.predictive = false;
            comp.hadParent = false;
            comp.noTracking = true;
        }
    }
}
