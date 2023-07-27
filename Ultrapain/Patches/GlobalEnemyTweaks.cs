using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Ultrapain.ConfigManager;

namespace Ultrapain.Patches
{
    // EID
    class EnemyIdentifier_UpdateModifiers
    {
        static void Postfix(EnemyIdentifier __instance)
        {
            EidStatContainer container = ConfigManager.enemyStats[__instance.enemyType];
            
            if(__instance.enemyType == EnemyType.V2)
            {
                V2 comp = __instance.GetComponent<V2>();
                if(comp != null && comp.secondEncounter)
                {
                    container = ConfigManager.enemyStats[EnemyType.V2Second];
                }
            }

            __instance.totalHealthModifier *= container.health.value;
            __instance.totalDamageModifier *= container.damage.value;
            __instance.totalSpeedModifier *= container.speed.value;

            List<string> weakness = new List<string>();
            List<float> weaknessMulti = new List<float>();
            foreach(KeyValuePair<string, float> weaknessPair in container.resistanceDict)
            {
                weakness.Add(weaknessPair.Key);

                int index = Array.IndexOf(__instance.weaknesses, weaknessPair.Key);
                if(index >= 0)
                {
                    float defaultResistance = 1f / __instance.weaknessMultipliers[index];
                    if (defaultResistance > weaknessPair.Value)
                        weaknessMulti.Add(1f / defaultResistance);
                    else
                        weaknessMulti.Add(1f / weaknessPair.Value);
                }
                else
                    weaknessMulti.Add(1f / weaknessPair.Value);
            }
            for(int i = 0; i < __instance.weaknessMultipliers.Length; i++)
            {
                if (container.resistanceDict.ContainsKey(__instance.weaknesses[i]))
                    continue;
                weakness.Add(__instance.weaknesses[i]);
                weaknessMulti.Add(__instance.weaknessMultipliers[i]);
            }

            __instance.weaknesses = weakness.ToArray();
            __instance.weaknessMultipliers = weaknessMulti.ToArray();
        }
    }

    // DETECT DAMAGE TYPE
    class Explosion_Collide_FF
    {
        static bool Prefix(Explosion __instance)
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Explosion;
            if ((__instance.enemy || __instance.friendlyFire) && __instance.canHit != AffectedSubjects.PlayerOnly)
                EnemyIdentifier_DeliverDamage_FF.friendlyBurn = true;
            return true;
        }

        static void Postfix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Unknown;
            EnemyIdentifier_DeliverDamage_FF.friendlyBurn = false;
        }
    }

    class PhysicalShockwave_CheckCollision_FF
    {
        static bool Prefix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Explosion;
            return true;
        }

        static void Postfix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Unknown;
        }
    }

    class VirtueInsignia_OnTriggerEnter_FF
    {
        static bool Prefix(VirtueInsignia __instance)
        {
            if (__instance.gameObject.name == "PlayerSpawned")
                return true;

            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Fire;
            EnemyIdentifier_DeliverDamage_FF.friendlyBurn = true;
            return true;
        }

        static void Postfix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Unknown;
            EnemyIdentifier_DeliverDamage_FF.friendlyBurn = false;
        }
    }

    class SwingCheck2_CheckCollision_FF
    {
        static bool Prefix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Melee;
            return true;
        }

        static void Postfix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Unknown;
        }
    }

    class Projectile_Collided_FF
    {
        static bool Prefix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Projectile;
            return true;
        }

        static void Postfix()
        {
            EnemyIdentifier_DeliverDamage_FF.currentCause = EnemyIdentifier_DeliverDamage_FF.DamageCause.Unknown;
        }
    }

    class EnemyIdentifier_DeliverDamage_FF
    {
        public enum DamageCause
        {
            Explosion,
            Projectile,
            Fire,
            Melee,
            Unknown
        }

        public static DamageCause currentCause = DamageCause.Unknown;
        public static bool friendlyBurn = false;

        [HarmonyBefore]
        static bool Prefix(EnemyIdentifier __instance, ref float __3)
        {
            if (currentCause != DamageCause.Unknown && (__instance.hitter == "enemy" || __instance.hitter == "ffexplosion"))
            {
                switch(currentCause)
                {
                    case DamageCause.Projectile:
                        //if (ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue == 0)
                        //    return false;
                        __3 *= ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue;
                        break;
                    case DamageCause.Explosion:
                        //if (ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue == 0)
                        //    return false;
                        __3 *= ConfigManager.friendlyFireDamageOverrideExplosion.normalizedValue;
                        break;
                    case DamageCause.Melee:
                        //if (ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue == 0)
                        //    return false;
                        __3 *= ConfigManager.friendlyFireDamageOverrideMelee.normalizedValue;
                        break;
                }
            }

            return true;
        }
    }

    class Flammable_Burn_FF
    {
        static bool Prefix(Flammable __instance, ref float __0)
        {
            if (EnemyIdentifier_DeliverDamage_FF.friendlyBurn)
            {
                if (ConfigManager.friendlyFireDamageOverrideFire.normalizedValue == 0)
                    return false;
                __0 *= ConfigManager.friendlyFireDamageOverrideFire.normalizedValue;
            }
            return true;
        }
    }

    class StreetCleaner_Fire_FF
    {
        static bool Prefix(FireZone __instance)
        {
            if (__instance.source != FlameSource.Streetcleaner)
                return true;

            EnemyIdentifier_DeliverDamage_FF.friendlyBurn = true;
            return true;
        }

        static void Postfix(FireZone __instance)
        {
            if (__instance.source == FlameSource.Streetcleaner)
                EnemyIdentifier_DeliverDamage_FF.friendlyBurn = false;
        }
    }
}
