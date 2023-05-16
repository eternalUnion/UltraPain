using HarmonyLib;
using PluginConfig.API;
using UnityEngine;

namespace Ultrapain.Patches
{
    class StreetCleaner_Start_Patch
    {
        static void Postfix(Streetcleaner __instance, ref EnemyIdentifier ___eid)
        {
            if (ConfigManager.streetCleanerCoinsIgnoreWeakPointToggle.value)
                ___eid.weakPoint = null;

            if(ConfigManager.streetCleanerFireRemainToggle.value)
                __instance.gameObject.AddComponent<StreetCleanerFireZoneSpawner>();
        }
    }

    /*[HarmonyPatch(typeof(Streetcleaner))]
    [HarmonyPatch("StartFire")]
    class StreetCleaner_StartFire_Patch
    {
        static void Postfix(Streetcleaner __instance, ref EnemyIdentifier ___eid)
        {
            __instance.CancelInvoke("StartDamaging");
            __instance.CancelInvoke("StopFire");
            __instance.Invoke("StartDamaging", 0.1f);
        }
    }*/

    /*[HarmonyPatch(typeof(Streetcleaner))]
    [HarmonyPatch("Update")]
    class StreetCleaner_Update_Patch
    {
        static bool cancelStartFireInvoke = false;

        static bool Prefix(Streetcleaner __instance, ref bool ___attacking)
        {
            cancelStartFireInvoke = !___attacking;
            return true;
        }

        static void Postfix(Streetcleaner __instance, ref EnemyIdentifier ___eid)
        {
            if(__instance.IsInvoking("StartFire") && cancelStartFireInvoke)
            {
                __instance.CancelInvoke("StartFire");
                __instance.Invoke("StartFire", 0.1f);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(Streetcleaner))]
    [HarmonyPatch("Dodge")]
    class StreetCleaner_Dodge_Patch
    {
        static bool didDodge = false;

        static bool Prefix(Streetcleaner __instance, ref float ___dodgeCooldown)
        {
            didDodge = !__instance.dead && ___dodgeCooldown == 0;
            return true;
        }

        static void Postfix(Streetcleaner __instance, ref float ___dodgeCooldown)
        {
            if(didDodge)
                ___dodgeCooldown = UnityEngine.Random.Range(0f, 1f);
        }
    }*/

    class BulletCheck_OnTriggerEnter_Patch
    {
        static void Postfix(BulletCheck __instance, Collider __0/*, EnemyIdentifier ___eid*/)
        {
            if (!(__instance.type == CheckerType.Streetcleaner && __0.gameObject.layer == 14))
                return;

            Grenade grn = __0.GetComponent<Grenade>();
            if (grn != null)
            {
                grn.enemy = true;
                grn.CanCollideWithPlayer(true);

                // OLD PREDICTION
                /*Rigidbody rb = __0.GetComponent<Rigidbody>();
                float magnitude = rb.velocity.magnitude;

                //float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position, __0.transform.position);
                float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position, __0.transform.position);
                Vector3 predictedPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(1.0f);

                float velocity = Mathf.Clamp(distance, Mathf.Max(magnitude - 5.0f, 0), magnitude + 5);

                __0.transform.LookAt(predictedPosition);
                rb.velocity = __0.transform.forward * velocity;*/

                // NEW PREDICTION
                Vector3 predictedPosition = Tools.PredictPlayerPosition(1);
                __0.transform.LookAt(predictedPosition);
                Rigidbody rb = __0.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.AddForce(__0.transform.forward * 20000f /* * ___eid.totalSpeedModifier */);
            }
        }
    }

    class CustomFizeZoneComp : MonoBehaviour
    {
        float playerHurtCooldown = 0f;
        public float totalDamageModifier;

        void OnTriggerStay(Collider other)
        {
            if (playerHurtCooldown == 0f && other.gameObject.tag == "Player")
            {
                playerHurtCooldown = 0.5f;
                if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
                {
                    MonoSingleton<PlatformerMovement>.Instance.Burn(false);
                    return;
                }
                MonoSingleton<NewMovement>.Instance.GetHurt((int)(20f * totalDamageModifier), true, 1f, false, false);
                return;
            }
            else
            {
                Flammable component = other.GetComponent<Flammable>();
                if (component != null && !component.playerOnly)
                {
                    component.Burn(10f, false);
                }
            }

            playerHurtCooldown = Mathf.MoveTowards(playerHurtCooldown, 0f, Time.deltaTime);
        }
    }

    class StreetCleanerFireZoneSpawner : MonoBehaviour
    {
        float timeSinceLastSpawn = 0f;
        public Streetcleaner sc;
        public EnemyIdentifier eid;

        void Start()
        {
            if (sc == null)
                sc = GetComponent<Streetcleaner>();
            if (eid == null)
                eid = GetComponent<EnemyIdentifier>();
        }

        static Vector3[] localFireSpawnPoses = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(2f, 0, 2f),
            new Vector3(2f, 0, -2f),
            new Vector3(-2f, 0, 2f),
            new Vector3(-2f, 0, -2f),
            new Vector3(3f, 0, 0),
            new Vector3(-3f, 0, 0),
            new Vector3(0, 0, 3f),
            new Vector3(0, 0, -3f),
        };
        static float horizontalVariation = 0.25f;
        static LayerMask envMask = new LayerMask() { value = (1 << 8) | (1 << 24) };

        static GameObject _customFlame;
        static GameObject customFlame
        {
            get
            {
                if(_customFlame == null)
                {
                    _customFlame = GameObject.Instantiate(Plugin.fireParticle, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
                    _customFlame.SetActive(false);

                    GameObject.Destroy(_customFlame.GetComponent<ExplosionController>());
                }

                return _customFlame;
            }
        }

        void Update()
        {
            if(!sc.damaging)
            {
                timeSinceLastSpawn = 0f;
            }
            else
            {
                if (timeSinceLastSpawn == 0f)
                {
                    GameObject fireZone = new GameObject();
                    fireZone.transform.position = transform.position + transform.forward * 10f;

                    BoxCollider fireCol = fireZone.AddComponent<BoxCollider>();
                    fireCol.size = new Vector3(8f, 2f, 8f);
                    fireCol.isTrigger = true;

                    Rigidbody rb = fireZone.AddComponent<Rigidbody>();
                    rb.isKinematic = true;

                    foreach(Vector3 pos in localFireSpawnPoses)
                    {
                        GameObject flame = GameObject.Instantiate(customFlame, fireZone.transform);
                        flame.transform.localPosition = pos + UnityUtils.RandomVector(horizontalVariation);
                        flame.SetActive(true);

                        RaycastHit hit;
                        if (Physics.Raycast(flame.transform.position, Vector3.down, out hit, 5f, envMask))
                            flame.transform.position = new Vector3(flame.transform.position.x, hit.point.y + 0.5f, flame.transform.position.z);
                        else if (Physics.Raycast(flame.transform.position, Vector3.up, out hit, 5f, envMask))
                            flame.transform.position = new Vector3(flame.transform.position.x, hit.point.y + 0.5f, flame.transform.position.z);
                        else if(pos == Vector3.zero)
                        {
                            GameObject.Destroy(fireZone);
                            return;
                        }
                        else
                        {
                            GameObject.Destroy(flame);
                        }
                    }

                    fireZone.AddComponent<RemoveOnTime>().time = ConfigManager.streetCleanerFireRemainDuration.value;

                    CustomFizeZoneComp comp = fireZone.AddComponent<CustomFizeZoneComp>();
                    comp.totalDamageModifier = eid.totalDamageModifier;

                    timeSinceLastSpawn = 1f / ConfigManager.streetCleanerFireRemainFrequency.value;
                }
                else
                    timeSinceLastSpawn = Mathf.MoveTowards(timeSinceLastSpawn, 0f, Time.deltaTime);
            }
        }
    }
}
