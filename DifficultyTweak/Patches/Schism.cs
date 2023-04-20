﻿using HarmonyLib;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.ShootProjectile))]
    class ZombieProjectile_ShootProjectile_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref GameObject ___currentProjectile)
        {
            /*Projectile proj = ___currentProjectile.GetComponent<Projectile>();
            proj.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            proj.speed *= speedMultiplier;
            proj.turningSpeedMultiplier = turningSpeedMultiplier;
            proj.damage = damage;*/

            float degreePerIteration = ConfigManager.schismSpreadAttackAngle.value / ConfigManager.schismSpreadAttackCount.value;
            float currentDegree = degreePerIteration;
            for (int i = 0; i < ConfigManager.schismSpreadAttackCount.value; i++)
            {
                GameObject leftProj = GameObject.Instantiate(___currentProjectile);
                leftProj.transform.position += -leftProj.transform.right;
                leftProj.transform.Rotate(new Vector3(0, -currentDegree, 0), Space.Self);

                GameObject rightProj = GameObject.Instantiate(___currentProjectile);
                rightProj.transform.position += leftProj.transform.right;
                rightProj.transform.Rotate(new Vector3(0, currentDegree, 0), Space.Self);

                currentDegree += degreePerIteration;
            }
        }
    }

    /*[HarmonyPatch(typeof(ZombieProjectiles), "Start")]
    class ZombieProjectile_Start_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Schism)
                return;

            __instance.projectile = Plugin.homingProjectile;
            __instance.decProjectile = Plugin.decorativeProjectile2;
        }
    }*/
}
