using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class Leviathan_FixedUpdate
    {
        static bool Roll(float chancePercent)
        {
            return UnityEngine.Random.Range(0, 99.9f) <= chancePercent;
        }

        struct StateInfo
        {
            public GameObject oldProjectile;
            public GameObject currentProjectile;

            public StateInfo(GameObject oldProjectile, GameObject currentProjectile)
            {
                this.oldProjectile = oldProjectile;
                this.currentProjectile = currentProjectile;
            }
        }

        static bool Prefix(LeviathanHead __instance, LeviathanController ___lcon, bool ___projectileBursting, float ___projectileBurstCooldown, out StateInfo __state)
        {
            __state = new StateInfo(null, null);
            if(___projectileBursting && ___projectileBurstCooldown == 0)
            {
                __state.oldProjectile = MonoSingleton<DefaultReferenceManager>.Instance.projectile;

                if (Roll(ConfigManager.leviathanProjectileYellowChance.value))
                {
                    GameObject proj = MonoSingleton<DefaultReferenceManager>.Instance.projectile = GameObject.Instantiate(Plugin.hideousMassProjectile);
                    __state.currentProjectile = proj;
                    Projectile comp = proj.GetComponent<Projectile>();
                    comp.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                    comp.safeEnemyType = EnemyType.Leviathan;

                    // values from p2 flesh prison
                    comp.turnSpeed *= 4f;
                    comp.turningSpeedMultiplier *= 4f;
                    comp.predictiveHomingMultiplier = 1.25f;

                    comp.speed *= ___lcon.eid.totalSpeedModifier;
                    comp.damage *= ___lcon.eid.totalDamageModifier;
                }
                else if (Roll(ConfigManager.leviathanProjectileBlueChance.value))
                {
                    GameObject proj = MonoSingleton<DefaultReferenceManager>.Instance.projectile = GameObject.Instantiate(Plugin.homingProjectile);
                    __state.currentProjectile = proj;
                    Projectile comp = proj.GetComponent<Projectile>();
                    comp.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                    comp.safeEnemyType = EnemyType.Leviathan;

                    // values from mindflayer
                    comp.turningSpeedMultiplier = 0.5f;
                    comp.speed *= ___lcon.eid.totalSpeedModifier;
                    comp.damage *= ___lcon.eid.totalDamageModifier;
                }
                else
                    __state.oldProjectile = null;
            }

            return true;
        }

        static void Postfix(StateInfo __state)
        {
            if (__state.oldProjectile != null)
            {
                MonoSingleton<DefaultReferenceManager>.Instance.projectile = __state.oldProjectile;
                GameObject.Destroy(__state.currentProjectile);
            }
        }
    }
}
