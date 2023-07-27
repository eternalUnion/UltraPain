using HarmonyLib;
using ULTRAKILL.Cheats;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class Stalker_SandExplode_Patch
    {
        static bool Prefix(Stalker __instance, ref int ___difficulty, ref EnemyIdentifier ___eid, int __0,
            ref bool ___exploding, ref float ___countDownAmount, ref float ___explosionCharge,
            ref Color ___currentColor, Color[] ___lightColors, AudioSource ___lightAud, AudioClip[] ___lightSounds,
            ref bool ___blinking, Machine ___mach, ref bool ___exploded, Transform ___target)
        {
            bool removeStalker = true;
            if (!(StockMapInfo.Instance != null && StockMapInfo.Instance.levelName == "GOD DAMN THE SUN"
                && __instance.transform.parent != null && __instance.transform.parent.name == "Wave 1"
                && __instance.transform.parent.parent != null && __instance.transform.parent.parent.name.StartsWith("5 Stuff")))
            {
                removeStalker = false;
            }

            GameObject explosion = Object.Instantiate<GameObject>(__instance.explosion, __instance.transform.position + Vector3.up * 2.5f, Quaternion.identity);
            if (__0 != 1)
            {
                explosion.transform.localScale *= 1.5f;
            }
            if (___eid.stuckMagnets.Count > 0)
            {
                float num = 0.75f;
                if (___eid.stuckMagnets.Count > 1)
                {
                    num -= 0.125f * (float)(___eid.stuckMagnets.Count - 1);
                }
                explosion.transform.localScale *= num;
            }

            SandificationZone zone = explosion.GetComponentInChildren<SandificationZone>();
            zone.buffDamage = zone.buffHealth = zone.buffSpeed = false;

            if (ConfigManager.stalkerSpreadHealthRad.value)
                zone.healthBuff = ___eid.healthBuffModifier + ConfigManager.stalkerSpreadHealthAddition.value;
            else
                zone.healthBuff = 0;

            if (ConfigManager.stalkerSpreadDamageRad.value)
                zone.damageBuff = ___eid.damageBuffModifier + ConfigManager.stalkerSpreadDamageAddition.value;
            else
                zone.damageBuff = 0;

            if (ConfigManager.stalkerSpreadSpeedRad.value)
                zone.speedBuff = ___eid.speedBuffModifier + ConfigManager.stalkerSpreadSpeedAddition.value;
            else
                zone.speedBuff = 0;

            if ((!removeStalker || ___eid.blessed || InvincibleEnemies.Enabled) && __0 != 1)
            {
                ___exploding = false;
                ___countDownAmount = 0f;
                ___explosionCharge = 0f;
                ___currentColor = ___lightColors[0];
                ___lightAud.clip = ___lightSounds[0];
                ___blinking = false;
                return false;
            }

            ___exploded = true;
            if (!___mach.limp)
            {
                ___mach.GoLimp();
                ___eid.Death();
            }
            if (___target != null)
            {
                if (MonoSingleton<StalkerController>.Instance.CheckIfTargetTaken(___target))
                {
                    MonoSingleton<StalkerController>.Instance.targets.Remove(___target);
                }
                EnemyIdentifier enemyIdentifier;
                if (___target.TryGetComponent<EnemyIdentifier>(out enemyIdentifier) && enemyIdentifier.buffTargeter == ___eid)
                {
                    enemyIdentifier.buffTargeter = null;
                }
            }
            if (___eid.drillers.Count != 0)
            {
                for (int i = ___eid.drillers.Count - 1; i >= 0; i--)
                {
                    Object.Destroy(___eid.drillers[i].gameObject);
                }
            }
            __instance.gameObject.SetActive(false);
            Object.Destroy(__instance.gameObject);

            return false;
        }
    }

    public class SandificationZone_Enter_Patch
    {
        static void Postfix(SandificationZone __instance, Collider __0)
        {
            if (__0.gameObject.layer == 10 || __0.gameObject.layer == 11)
            {
                EnemyIdentifierIdentifier component = __0.gameObject.GetComponent<EnemyIdentifierIdentifier>();
                if (component && component.eid && !component.eid.dead && component.eid.enemyType != EnemyType.Stalker)
                {
                    EnemyIdentifier eid = component.eid;
                    if (eid.damageBuffModifier < __instance.damageBuff)
                        eid.DamageBuff(__instance.damageBuff);
                    if (eid.speedBuffModifier < __instance.speedBuff)
                        eid.SpeedBuff(__instance.speedBuff);
                    if (eid.healthBuffModifier < __instance.healthBuff)
                        eid.HealthBuff(__instance.healthBuff);
                }
            }
        }
    }
}
