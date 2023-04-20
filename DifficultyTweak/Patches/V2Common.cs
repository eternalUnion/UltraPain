using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    public static class V2Utils
    {
        public static Transform GetClosestGrenade()
        {
            Transform closestTransform = null;
            float closestDistance = 1000000;

            foreach(Grenade g in GrenadeList.Instance.grenadeList)
            {
                float dist = Vector3.Distance(g.transform.position, PlayerTracker.Instance.GetTarget().position);
                if(dist < closestDistance)
                {
                    closestTransform = g.transform;
                    closestDistance = dist;
                }
            }

            foreach (Cannonball c in GrenadeList.Instance.cannonballList)
            {
                float dist = Vector3.Distance(c.transform.position, PlayerTracker.Instance.GetTarget().position);
                if (dist < closestDistance)
                {
                    closestTransform = c.transform;
                    closestDistance = dist;
                }
            }

            return closestTransform;
        }

        public static Vector3 GetDirectionAwayFromTarget(Vector3 center, Vector3 target)
        {
            // Calculate the direction vector from the center to the target
            Vector3 direction = target - center;

            // Set the Y component of the direction vector to 0
            direction.y = 0;

            // Normalize the direction vector
            direction.Normalize();

            // Reverse the direction vector to face away from the target
            direction = -direction;

            return direction;
        }
    }

    class V2CommonExplosion
    {
        static void Postfix(Explosion __instance)
        {
            if (__instance.sourceWeapon == null)
                return;

            V2MaliciousCannon malCanComp = __instance.sourceWeapon.GetComponent<V2MaliciousCannon>();
            if(malCanComp != null)
            {
                Debug.Log("Grenade explosion triggered by V2 malicious cannon");
                __instance.toIgnore.Add(EnemyType.V2);
                __instance.toIgnore.Add(EnemyType.V2Second);
                return;
            }

            EnemyRevolver revComp = __instance.sourceWeapon.GetComponentInChildren<EnemyRevolver>();
            if(revComp != null)
            {
                Debug.Log("Grenade explosion triggered by V2 revolver");
                __instance.toIgnore.Add(EnemyType.V2);
                __instance.toIgnore.Add(EnemyType.V2Second);
                return;
            }
        }
    }
}
