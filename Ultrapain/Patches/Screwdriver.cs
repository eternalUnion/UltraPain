using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.UIR;

namespace Ultrapain.Patches
{
    class Harpoon_OnTriggerEnter_Patch
    {
        public static float forwardForce = 10f;
        public static float upwardForce = 10f;
        static LayerMask envLayer = new LayerMask() { m_Mask = 16777472 };

        private static Harpoon lastHarpoon;
        static bool Prefix(Harpoon __instance, Collider __0)
        {
            if (!__instance.drill || __instance == lastHarpoon)
                return true;

            Coin sourceCoin = __0.gameObject.GetComponent<Coin>();
            if (sourceCoin != null)
            {
                Quaternion currentRotation = Quaternion.Euler(0, __0.transform.eulerAngles.y, 0);
                int totalCoinCount = ConfigManager.screwDriverCoinSplitCount.value;
                float rotationPerIteration = 360f / totalCoinCount;
            
                for(int i = 0; i < totalCoinCount; i++)
                {
                    GameObject coinClone = GameObject.Instantiate(Plugin.coin, __instance.transform.position, currentRotation);
                    Coin comp = coinClone.GetComponent<Coin>();
                    comp.sourceWeapon = sourceCoin.sourceWeapon;
                    comp.power = sourceCoin.power;
                    Rigidbody rb = coinClone.GetComponent<Rigidbody>();

                    rb.AddForce(coinClone.transform.forward * forwardForce + Vector3.up * upwardForce, ForceMode.VelocityChange);
                    currentRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y + rotationPerIteration, 0);
                }

                GameObject.Destroy(__0.gameObject);
                GameObject.Destroy(__instance.gameObject);
                lastHarpoon = __instance;
                return false;
            }

            Grenade sourceGrn = __0.GetComponent<Grenade>();
            if(sourceGrn != null)
            {
                Quaternion currentRotation = Quaternion.Euler(0, __0.transform.eulerAngles.y, 0);
                int totalGrenadeCount = ConfigManager.screwDriverCoinSplitCount.value;
                float rotationPerIteration = 360f / totalGrenadeCount;
                List<Tuple<EnemyIdentifier , float>> targetEnemies = new List<Tuple<EnemyIdentifier, float>>();

                foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                {
                    float sqrMagnitude = (enemy.transform.position - __0.transform.position).sqrMagnitude;
                    if (targetEnemies.Count < totalGrenadeCount || sqrMagnitude < targetEnemies.Last().Item2)
                    {
                        EnemyIdentifier eid = enemy.GetComponent<EnemyIdentifier>();
                        if (eid == null || eid.dead || eid.blessed)
                            continue;

                        if (Physics.Raycast(__0.transform.position, enemy.transform.position - __0.transform.position, out RaycastHit hit, Vector3.Distance(__0.transform.position, enemy.transform.position) - 0.5f, envLayer))
                            continue;

                        if(targetEnemies.Count == 0)
                        {
                            targetEnemies.Add(new Tuple<EnemyIdentifier, float>(eid, sqrMagnitude));
                            continue;
                        }

                        int insertionPoint = targetEnemies.Count;
                        while (insertionPoint != 0 && targetEnemies[insertionPoint - 1].Item2 > sqrMagnitude)
                            insertionPoint -= 1;

                        targetEnemies.Insert(insertionPoint, new Tuple<EnemyIdentifier, float>(eid, sqrMagnitude));
                        if (targetEnemies.Count > totalGrenadeCount)
                            targetEnemies.RemoveAt(totalGrenadeCount);
                    }
                }

                for (int i = 0; i < totalGrenadeCount; i++)
                {
                    Grenade grenadeClone = GameObject.Instantiate(sourceGrn, __instance.transform.position, currentRotation);
                    Rigidbody rb = grenadeClone.GetComponent<Rigidbody>();

                    rb.velocity = Vector3.zero;
                    if(i <= targetEnemies.Count - 1 || targetEnemies.Count != 0)
                    {
                        grenadeClone.transform.LookAt(targetEnemies[i <= targetEnemies.Count - 1 ? i : 0].Item1.transform);
                        if (!grenadeClone.rocket)
                        {
                            rb.AddForce(grenadeClone.transform.forward * 50f, ForceMode.VelocityChange);
                            rb.useGravity = false;
                        }
                        else
                        {
                            grenadeClone.rocketSpeed = 150f;
                        }
                    }
                    else
                    {
                        rb.AddForce(grenadeClone.transform.forward * forwardForce + Vector3.up * upwardForce, ForceMode.VelocityChange);
                    }

                    currentRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y + rotationPerIteration, 0);
                }

                GameObject.Destroy(__instance.gameObject);
                GameObject.Destroy(sourceGrn.gameObject);
                lastHarpoon = __instance;
                return false;
            }

            return true;
        }
    }
}
