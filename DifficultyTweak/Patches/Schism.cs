using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.ShootProjectile))]
    class ZombieProjectile_ShootProjectile_Patch
    {
        public static float speedMultiplier = 2;
        public static float turningSpeedMultiplier = 1f;
        public static int damage = 10;

        static void Postfix(ZombieProjectiles __instance, ref GameObject ___currentProjectile)
        {
            Projectile proj = ___currentProjectile.GetComponent<Projectile>();
            proj.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            proj.speed *= speedMultiplier;
            proj.turningSpeedMultiplier = turningSpeedMultiplier;
            proj.damage = damage;
        }
    }

    [HarmonyPatch(typeof(ZombieProjectiles), "Start")]
    class ZombieProjectile_Start_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Schism)
                return;

            __instance.projectile = Plugin.homingProjectile;
            __instance.decProjectile = Plugin.decorativeProjectile2;
        }
    }
}
