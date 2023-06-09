﻿using HarmonyLib;
using UnityEngine;

namespace Ultrapain.Patches
{
    class SwingCheck2_CheckCollision_Patch2
    {
        static bool Prefix(SwingCheck2 __instance, Collider __0, EnemyIdentifier ___eid)
        {
            if (__0.gameObject.tag != "Player")
                return true;

            if (__instance.transform.parent == null)
                return true;

            EnemyIdentifier eid = __instance.transform.parent.gameObject.GetComponent<EnemyIdentifier>();
            if (eid == null || eid.enemyType != EnemyType.Filth)
                return true;

            GameObject expObj = GameObject.Instantiate(Plugin.explosion, eid.transform.position, Quaternion.identity);
            foreach(Explosion exp in expObj.GetComponentsInChildren<Explosion>())
            {
                exp.enemy = true;
                exp.damage = (int)(ConfigManager.filthExplosionDamage.value * ___eid.totalDamageModifier);
                exp.maxSize *= ConfigManager.filthExplosionSize.value;
                exp.speed *= ConfigManager.filthExplosionSize.value;
                exp.toIgnore.Add(EnemyType.Filth);
            }

            if (ConfigManager.filthExplodeKills.value)
            {
                eid.Death();
            }

            return false;
        }
    }
}
