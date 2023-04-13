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
        static bool Prefix(SwingCheck2 __instance, Collider __0)
        {
            if (__0.gameObject.tag != "Player")
                return true;

            if (__instance.transform.parent == null)
                return true;

            EnemyIdentifier eid = __instance.transform.parent.gameObject.GetComponent<EnemyIdentifier>();
            if (eid == null || eid.enemyType != EnemyType.Filth)
                return true;

            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value || !ConfigManager.filthExplodeToggle.value)
                return true;

            GameObject expObj = GameObject.Instantiate(Plugin.explosion, eid.transform.position, Quaternion.identity);
            foreach(Explosion exp in expObj.GetComponentsInChildren<Explosion>())
            {
                exp.enemy = true;
                exp.damage = ConfigManager.filthExplosionDamage.value;
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
