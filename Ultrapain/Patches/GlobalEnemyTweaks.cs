using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
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
