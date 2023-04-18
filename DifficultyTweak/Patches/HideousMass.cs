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
        static void Postfix(Projectile __instance)
        {
            HideousMassProjectile flag = __instance.gameObject.GetComponent<HideousMassProjectile>();
            if (flag == null)
                return;

            GameObject createInsignia(float size, int damage)
            {
                GameObject insignia = GameObject.Instantiate(Plugin.virtueInsignia, __instance.transform.position, Quaternion.identity);
                insignia.transform.localScale = new Vector3(size, 1f, size);
                VirtueInsignia comp = insignia.GetComponent<VirtueInsignia>();
                comp.windUpSpeedMultiplier = ConfigManager.hideousMassInsigniaSpeed.value;
                comp.damage = damage;
                comp.predictive = false;
                comp.hadParent = false;
                comp.noTracking = true;

                return insignia;
            }

            if (ConfigManager.hideousMassInsigniaXtoggle.value)
            {
                GameObject insignia = createInsignia(ConfigManager.hideousMassInsigniaXsize.value, ConfigManager.hideousMassInsigniaXdamage.value);
                insignia.transform.Rotate(new Vector3(0, 0, 90f));
            }
            if (ConfigManager.hideousMassInsigniaYtoggle.value)
            {
                GameObject insignia = createInsignia(ConfigManager.hideousMassInsigniaYsize.value, ConfigManager.hideousMassInsigniaYdamage.value);
            }
            if (ConfigManager.hideousMassInsigniaZtoggle.value)
            {
                GameObject insignia = createInsignia(ConfigManager.hideousMassInsigniaZsize.value, ConfigManager.hideousMassInsigniaZdamage.value);
                insignia.transform.Rotate(new Vector3(90f, 0, 0));
            }
        }
    }

    [HarmonyPatch(typeof(Mass), "ShootExplosive")]
    public class HideousMassHoming
    {
        static bool Prefix(Mass __instance)
        {
            __instance.explosiveProjectile = GameObject.Instantiate(Plugin.hideousMassProjectile);
            __instance.explosiveProjectile.AddComponent<HideousMassProjectile>();
            return true;
        }

        static void Postfix(Mass __instance)
        {
            GameObject.Destroy(__instance.explosiveProjectile);
            __instance.explosiveProjectile = Plugin.hideousMassProjectile;
        }
    }
}
