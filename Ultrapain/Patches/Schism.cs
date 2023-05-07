using HarmonyLib;
using System.ComponentModel;
using UnityEngine;

namespace Ultrapain.Patches
{
    class ZombieProjectile_ShootProjectile_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref GameObject ___currentProjectile, Animator ___anim, EnemyIdentifier ___eid)
        {
            /*Projectile proj = ___currentProjectile.GetComponent<Projectile>();
            proj.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            proj.speed *= speedMultiplier;
            proj.turningSpeedMultiplier = turningSpeedMultiplier;
            proj.damage = damage;*/

            bool horizontal = ___anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "ShootHorizontal";
            void AddProperties(GameObject obj)
            {
                Projectile component = obj.GetComponent<Projectile>();
                component.safeEnemyType = EnemyType.Schism;
                component.speed *= 1.25f;
                component.speed *= ___eid.totalSpeedModifier;
                component.damage *= ___eid.totalDamageModifier;
            }

            if (horizontal)
            {
                float degreePerIteration = ConfigManager.schismSpreadAttackAngle.value / ConfigManager.schismSpreadAttackCount.value;
                float currentDegree = degreePerIteration;
                for (int i = 0; i < ConfigManager.schismSpreadAttackCount.value; i++)
                {
                    GameObject downProj = GameObject.Instantiate(___currentProjectile);
                    downProj.transform.position += -downProj.transform.up;
                    downProj.transform.Rotate(new Vector3(-currentDegree, 0, 0), Space.Self);

                    GameObject upProj = GameObject.Instantiate(___currentProjectile);
                    upProj.transform.position += upProj.transform.up;
                    upProj.transform.Rotate(new Vector3(currentDegree, 0, 0), Space.Self);

                    currentDegree += degreePerIteration;
                    AddProperties(downProj);
                    AddProperties(upProj);
                }
            }
            else
            {
                float degreePerIteration = ConfigManager.schismSpreadAttackAngle.value / ConfigManager.schismSpreadAttackCount.value;
                float currentDegree = degreePerIteration;
                for (int i = 0; i < ConfigManager.schismSpreadAttackCount.value; i++)
                {
                    GameObject leftProj = GameObject.Instantiate(___currentProjectile);
                    leftProj.transform.position += -leftProj.transform.right;
                    leftProj.transform.Rotate(new Vector3(0, -currentDegree, 0), Space.Self);

                    GameObject rightProj = GameObject.Instantiate(___currentProjectile);
                    rightProj.transform.position += rightProj.transform.right;
                    rightProj.transform.Rotate(new Vector3(0, currentDegree, 0), Space.Self);

                    currentDegree += degreePerIteration;
                    AddProperties(leftProj);
                    AddProperties(rightProj);
                }
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
