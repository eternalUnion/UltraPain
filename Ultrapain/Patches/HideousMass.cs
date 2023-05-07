using HarmonyLib;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class HideousMassProjectile : MonoBehaviour
    {
        public float damageBuf = 1f;
        public float speedBuf = 1f;
    }

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
                comp.windUpSpeedMultiplier = ConfigManager.hideousMassInsigniaSpeed.value * flag.speedBuf;
                comp.damage = (int)(damage * flag.damageBuf);
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

    public class HideousMassHoming
    {
        static bool Prefix(Mass __instance, EnemyIdentifier ___eid)
        {
            __instance.explosiveProjectile = GameObject.Instantiate(Plugin.hideousMassProjectile);
            HideousMassProjectile flag = __instance.explosiveProjectile.AddComponent<HideousMassProjectile>();
            flag.damageBuf = ___eid.totalDamageModifier;
            flag.speedBuf = ___eid.totalSpeedModifier;
            return true;
        }

        static void Postfix(Mass __instance)
        {
            GameObject.Destroy(__instance.explosiveProjectile);
            __instance.explosiveProjectile = Plugin.hideousMassProjectile;
        }
    }
}
