using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ultrapain.Patches
{
    class SomethingWickedFlag : MonoBehaviour
    {
        public GameObject spear;
        public MassSpear spearComp;
        public EnemyIdentifier eid;
        public Transform spearOrigin;
        public Rigidbody spearRb;

        public static float SpearTriggerDistance = 80f;
        public static LayerMask envMask = new LayerMask() { value = (1 << 8) | (1 << 24) };

        void Awake()
        {
            if (eid == null)
                eid = GetComponent<EnemyIdentifier>();
            if (spearOrigin == null)
            {
                GameObject obj = new GameObject();
                obj.transform.parent = transform;
                obj.transform.position = GetComponent<Collider>().bounds.center;

                obj.SetActive(false);
                spearOrigin = obj.transform;
            }
        }

        void Update()
        {
            if(spear == null)
            {
                Vector3 playerCenter = NewMovement.Instance.playerCollider.bounds.center;
                float distanceFromPlayer = Vector3.Distance(spearOrigin.position, playerCenter);

                if (distanceFromPlayer < SpearTriggerDistance)
                {
                    if(!Physics.Raycast(transform.position, playerCenter - transform.position, distanceFromPlayer, envMask))
                    {
                        spear = GameObject.Instantiate(Plugin.hideousMassSpear, transform);
                        spear.transform.position = spearOrigin.position;
                        spear.transform.LookAt(playerCenter);
                        spear.transform.position += spear.transform.forward * 5;
                        spearComp = spear.GetComponent<MassSpear>();
                        spearRb = spearComp.GetComponent<Rigidbody>();

                        spearComp.originPoint = spearOrigin;
                        spearComp.damageMultiplier = 0f;
                        spearComp.speedMultiplier = 2;
                    }
                }
            }
            else if(spearComp.beenStopped)
            {
                if (!spearComp.transform.parent || spearComp.transform.parent.tag != "Player")
                    if(spearRb.isKinematic == true)
                        GameObject.Destroy(spear);
            }
        }
    }

    class SomethingWicked_Start
    {
        static void Postfix(Wicked __instance)
        {
            SomethingWickedFlag flag = __instance.gameObject.AddComponent<SomethingWickedFlag>();
        }
    }

    class SomethingWicked_GetHit
    {
        static void Postfix(Wicked __instance)
        {
            SomethingWickedFlag flag = __instance.GetComponent<SomethingWickedFlag>();
            if (flag == null)
                return;

            if (flag.spear != null)
                GameObject.Destroy(flag.spear);
        }
    }

    class JokeWicked : MonoBehaviour
    {
        void OnDestroy()
        {
            MusicManager.Instance.ForceStartMusic();
        }
    }

    class JokeWicked_GetHit
    {
        static void Postfix(Wicked __instance)
        {
            if (__instance.GetComponent<JokeWicked>() == null)
                return;

            GameObject.Destroy(__instance.gameObject);
        }
    }

    class ObjectActivator_Activate
    {
        static bool Prefix(ObjectActivator __instance)
        {
            if (SceneManager.GetActiveScene().name != "38748a67bc9e67a43956a92f87d1e742")
                return true;

            if(__instance.name == "Scream")
            {
                GameObject goreZone = new GameObject();
                goreZone.AddComponent<GoreZone>();

                Vector3 spawnPos = new Vector3(86.7637f, -39.9667f, 635.7572f);
                if (Physics.Raycast(spawnPos + Vector3.up, Vector3.down, out RaycastHit hit, 100f, new LayerMask() { value = (1 << 8) | (1 << 24) }, QueryTriggerInteraction.Ignore))
                    spawnPos = hit.point;

                GameObject wicked = GameObject.Instantiate(Plugin.somethingWicked, spawnPos, Quaternion.identity, goreZone.transform); ;
                wicked.AddComponent<JokeWicked>();

                Wicked comp = wicked.GetComponent<Wicked>();
                comp.patrolPoints = new Transform[] { __instance.transform };
                wicked.AddComponent<JokeWicked>();
            }
            else if(__instance.name == "Hint 1")
            {
                return false;
            }

            return true;
        }
    }
}
