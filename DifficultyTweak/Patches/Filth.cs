using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(SwingCheck2), "CheckCollision")]
    class SwingCheck2_CheckCollision_Patch2
    {
        public static int explosionDamage = 30;

        static bool Prefix(SwingCheck2 __instance, Collider __0)
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
                exp.damage = explosionDamage;
                exp.maxSize /= 2;
                exp.speed /= 2;
                exp.toIgnore.Add(EnemyType.Filth);
            }

            return false;
        }
    }
}
