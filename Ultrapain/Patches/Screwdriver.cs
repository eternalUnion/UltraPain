using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.UIR;

namespace Ultrapain.Patches
{
    class DrillFlag : MonoBehaviour
    {
        public Harpoon drill;
        public Rigidbody rb;
        public List<Tuple<EnemyIdentifier, float>> targetEids = new List<Tuple<EnemyIdentifier, float>>();
        public List<EnemyIdentifier> piercedEids = new List<EnemyIdentifier>();
        public Transform currentTargetTrans;
        public Collider currentTargetCol;
        public EnemyIdentifier currentTargetEid;

        void Awake()
        {
            if (drill == null)
                drill = GetComponent<Harpoon>();
            if (rb == null)
                rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if(targetEids != null)
            {
                if (currentTargetEid == null || currentTargetEid.dead || currentTargetEid.blessed || currentTargetEid.stuckMagnets.Count == 0)
                {
                    currentTargetEid = null;
                    foreach (Tuple<EnemyIdentifier, float> item in targetEids)
                    {
                        EnemyIdentifier eid = item.Item1;
                        if (eid == null || eid.dead || eid.blessed || eid.stuckMagnets.Count == 0)
                            continue;
                        currentTargetEid = eid;
                        currentTargetTrans = eid.transform;
                        if (currentTargetEid.gameObject.TryGetComponent(out Collider col))
                            currentTargetCol = col;
                        break;
                    }
                }

                if(currentTargetEid != null)
                {
                    transform.LookAt(currentTargetCol == null ? currentTargetTrans.position : currentTargetCol.bounds.center);
                    rb.velocity = transform.forward * 150f;
                }
                else
                {
                    targetEids.Clear();
                }
            }
        }
    }

    class Harpoon_Start
    {
        static void Postfix(Harpoon __instance)
        {
            if (!__instance.drill)
                return;
            DrillFlag flag = __instance.gameObject.AddComponent<DrillFlag>();
            flag.drill = __instance;
        }
    }

    class Harpoon_Punched
    {
        static void Postfix(Harpoon __instance, EnemyIdentifierIdentifier ___target)
        {
            if (!__instance.drill)
                return;

            DrillFlag flag = __instance.GetComponent<DrillFlag>();
            if (flag == null)
                return;

            if(___target != null && ___target.eid != null)
                flag.targetEids = UnityUtils.GetClosestEnemies(__instance.transform.position, 3, enemy =>
                {
                    if (enemy == ___target.eid)
                        return false;

                    foreach (Magnet m in enemy.stuckMagnets)
                    {
                        if (m != null)
                            return true;
                    }

                    return false;
                });
            else
                flag.targetEids = UnityUtils.GetClosestEnemies(__instance.transform.position, 3, enemy =>
                {
                    foreach(Magnet m in enemy.stuckMagnets)
                    {
                        if (m != null)
                            return true;
                    }

                    return false;
                });
        }
    }

    class Harpoon_OnTriggerEnter_Patch
    {
        public static float forwardForce = 10f;
        public static float upwardForce = 10f;
        static LayerMask envLayer = new LayerMask() { m_Mask = 16777472 };

        private static Harpoon lastHarpoon;
        static bool Prefix(Harpoon __instance, Collider __0)
        {
            if (!__instance.drill)
                return true;

            if(__0.TryGetComponent(out EnemyIdentifierIdentifier eii))
            {
                if (eii.eid == null)
                    return true;
                EnemyIdentifier eid = eii.eid;

                DrillFlag flag = __instance.GetComponent<DrillFlag>();
                if (flag == null)
                    return true;

                if(flag.currentTargetEid != null)
                {
                    if(flag.currentTargetEid == eid)
                    {
                        flag.targetEids.Clear();
                        flag.piercedEids.Clear();
                        flag.currentTargetEid = null;
                        flag.currentTargetTrans = null;
                        flag.currentTargetCol = null;

                        if(ConfigManager.screwDriverHomeDestroyMagnets.value)
                        {
                            foreach (Magnet h in eid.stuckMagnets)
                                if (h != null)
                                    GameObject.Destroy(h.gameObject);
                            eid.stuckMagnets.Clear();
                        }
                        return true;
                    }
                    else if (!flag.piercedEids.Contains(eid))
                    {
                        if (ConfigManager.screwDriverHomePierceDamage.value > 0)
                        {
                            eid.hitter = "harpoon";
                            eid.DeliverDamage(__0.gameObject, __instance.transform.forward, __instance.transform.position, ConfigManager.screwDriverHomePierceDamage.value, false, 0, null, false);
                            flag.piercedEids.Add(eid);
                        }
                        return false;
                    }

                    return false;
                }
            }

            Coin sourceCoin = __0.gameObject.GetComponent<Coin>();
            if (sourceCoin != null)
            {
                if (__instance == lastHarpoon)
                    return true;

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
                if (__instance == lastHarpoon)
                    return true;

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
