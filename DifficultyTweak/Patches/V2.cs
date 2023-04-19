using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;
using UnityEngine.UIElements;

namespace DifficultyTweak.Patches
{
    class V2FirstFlag : MonoBehaviour
    {
        public V2RocketLauncher rocketLauncher;
        public V2MaliciousCannon maliciousCannon;
        public Collider v2collider;

        public float punchCooldown = 0f;
        public int punchDamage = 3;
        public int punchExplosionDamage = 5;

        void Update()
        {
            if (punchCooldown > 0)
                punchCooldown = Mathf.MoveTowards(punchCooldown, 0f, Time.deltaTime);
        }

        public void PunchShockwave()
        {
            /*float distanceToPlayer = Vector3.Distance(transform.position, PlayerTracker.Instance.GetTarget().transform.position);
            if (distanceToPlayer > 10)
                return;*/

            GameObject blast = Instantiate(Plugin.blastwave, v2collider.bounds.center, Quaternion.identity);
            blast.transform.LookAt(PlayerTracker.Instance.GetTarget());
            blast.transform.position += blast.transform.forward * 2f;

            Explosion exp = blast.GetComponentInChildren<Explosion>();
            if (exp != null)
            {
                exp.enemy = true;
                exp.damage = punchExplosionDamage;
                exp.maxSize = 15;
                exp.speed = 15f / 6;
                exp.hitterWeapon = "";
                exp.harmless = false;
                exp.playerDamageOverride = -1;
                exp.canHit = AffectedSubjects.All;
                exp.toIgnore.Add(EnemyType.V2);
            }
        }
    }

    class V2RocketLauncher : MonoBehaviour
    {
        public Transform shootPoint;
        public Collider v2collider;

        void PrepareFire()
        {
            Instantiate<GameObject>(Plugin.v2flashUnparryable, this.shootPoint.position, this.shootPoint.rotation).transform.localScale *= 4f;
        }

        void SetRocketRotation(Transform rocket)
        {
            Rigidbody rb = rocket.GetComponent<Rigidbody>();
            Grenade grn = rocket.GetComponent<Grenade>();
            float magnitude = grn.rocketSpeed;

            //float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position, __0.transform.position);
            float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetTarget().position, rocket.transform.position);
            Vector3 predictedPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(1.0f);

            float velocity = Mathf.Clamp(distance, Mathf.Max(magnitude - 5.0f, 0), magnitude + 5);

            rocket.transform.LookAt(predictedPosition);
            rocket.GetComponent<Grenade>().rocketSpeed = velocity;
            rb.maxAngularVelocity = velocity;
            rb.velocity = Vector3.zero;
            rb.AddRelativeForce(Vector3.forward * magnitude * rb.mass, ForceMode.VelocityChange);
            // rb.velocity = rocket.transform.forward * velocity;
        }

        void Fire()
        {
            GameObject rocket = Instantiate<GameObject>(Plugin.rocket, shootPoint.transform.position, shootPoint.transform.rotation);
            rocket.transform.position = new Vector3(rocket.transform.position.x, v2collider.bounds.center.y, rocket.transform.position.z);
            rocket.transform.LookAt(PlayerTracker.Instance.GetTarget());
            rocket.transform.position += rocket.transform.forward * 2f;
            SetRocketRotation(rocket.transform);
            Grenade component = rocket.GetComponent<Grenade>();
            if (component)
            {
                component.harmlessExplosion = component.explosion;
                component.enemy = true;
                component.CanCollideWithPlayer(true);
            }

            //Physics.IgnoreCollision(rocket.GetComponent<Collider>(), v2collider);
        }

        void PrepareAltFire()
        {

        }

        void AltFire()
        {

        }
    }

    class V2MaliciousCannon : MonoBehaviour
    {
        //readonly static FieldInfo maliciousIgnorePlayer = typeof(RevolverBeam).GetField("maliciousIgnorePlayer", BindingFlags.NonPublic | BindingFlags.Instance);

        Transform shootPoint;
        public Transform v2trans;
        public float cooldown = 0f;

        static readonly string debugTag = "[V2][MalCannonShoot]";

        void Awake()
        {
            shootPoint = UnityUtils.GetChildByNameRecursively(transform, "Shootpoint");
        }

        void PrepareFire()
        {
            Instantiate<GameObject>(Plugin.v2flashUnparryable, this.shootPoint.position, this.shootPoint.rotation).transform.localScale *= 4f;
        }

        void Fire()
        {
            cooldown = 15f;

            Grenade target = V2Utils.GetClosestGrenade();
            Vector3 targetPosition = Vector3.zero;

            if (target != null)
            {
                Debug.Log($"{debugTag} Targeted grenade");
                targetPosition = target.transform.position;
            }
            else
            {
                Transform playerTarget = PlayerTracker.Instance.GetTarget();
                if (Physics.Raycast(new Ray(playerTarget.position, Vector3.down), out RaycastHit hit, 100f, new LayerMask() { value = (1 << 8 | 1 << 24) }, QueryTriggerInteraction.Ignore))
                {
                    Debug.Log($"{debugTag} Targeted ground below player");
                    targetPosition = hit.point;
                }
                else
                {
                    Debug.Log($"{debugTag} Targeted player with random spread");
                    targetPosition = playerTarget.transform.position + UnityEngine.Random.onUnitSphere * 2f;
                }
            }

            GameObject beam = Instantiate(Plugin.maliciousCannonBeam, v2trans.position, Quaternion.identity);
            beam.transform.position = new Vector3(beam.transform.position.x, v2trans.GetComponent<Collider>().bounds.center.y, beam.transform.position.z);
            beam.transform.LookAt(targetPosition);
            beam.transform.position += beam.transform.forward * 2f;
            if(beam.TryGetComponent<RevolverBeam>(out RevolverBeam comp))
            {
                comp.alternateStartPoint = shootPoint.transform.position;
                comp.ignoreEnemyType = EnemyType.V2;
                //comp.beamType = BeamType.Enemy;
                //maliciousIgnorePlayer.SetValue(comp, false);
            }
        }

        void PrepareAltFire()
        {

        }

        void AltFire()
        {

        }
    }

    static class V2Utils
    {
        public static Grenade GetClosestGrenade()
        {
            return GrenadeList.Instance.grenadeList.OrderBy((Grenade g) => Vector3.Distance(g.transform.position, PlayerTracker.Instance.GetTarget().position)).FirstOrDefault();
        }

        /*public static Cannonball GetClosestCannonball()
        {
            return Cannon
            return null;
        }*/
    }

    [HarmonyPatch(typeof(V2), "Update")]
    class V2Update
    {
        static MethodInfo ShootWeapon = typeof(V2).GetMethod("ShootWeapon", BindingFlags.Instance | BindingFlags.NonPublic);
        public static Transform targetGrenade;

        static bool Prefix(V2 __instance, ref int ___currentWeapon, ref Transform ___overrideTarget, ref Rigidbody ___overrideTargetRb, ref float ___shootCooldown,
            ref bool ___aboutToShoot, ref EnemyIdentifier ___eid)
        {
            if (__instance.secondEncounter)
                return true;

            V2FirstFlag flag = __instance.GetComponent<V2FirstFlag>();
            if (flag == null)
                return true;

            float distanceToPlayer = Vector3.Distance(__instance.transform.position, PlayerTracker.Instance.GetTarget().transform.position);
            if(distanceToPlayer < 4f && flag.punchCooldown == 0)
            {
                Debug.Log("V2: Trying to punch");
                flag.punchCooldown = 3f;
                NewMovement.Instance.GetHurt(flag.punchDamage, true, 1, false, false);
                flag.Invoke("PunchShockwave", 1f);
            }

            //Quaternion playerDirection = Quaternion.FromToRotation(Vector3.up, PlayerTracker.Instance.GetTarget().transform.position - flag.v2collider.bounds.center);
            //if (Physics.BoxCast(flag.v2collider.bounds.center, new Vector3(2.5f, 2f, 2.5f), playerDirection., out RaycastHit hit, Quaternion.identity, 1f, 14, QueryTriggerInteraction.Collide))
            if (flag.punchCooldown == 0)
                foreach (Collider col in Physics.OverlapSphere(flag.v2collider.bounds.center, 10f, 1 << 14, QueryTriggerInteraction.Collide))
                {
                    Debug.Log($"Projectile name: {col.gameObject.name}");
                    Projectile proj = col.gameObject.GetComponent<Projectile>();
                    if (proj == null)
                        continue;

                    Debug.Log("Detected projectile");

                    if (proj.playerBullet)
                    {
                        Debug.Log("V2: Trying to deflect projectiles");
                        flag.PunchShockwave();
                        flag.punchCooldown = 3f;
                        break;
                    }
                }

            if (flag.maliciousCannon.cooldown > 0)
                flag.maliciousCannon.cooldown = Mathf.MoveTowards(flag.maliciousCannon.cooldown, 0, Time.deltaTime);

            Grenade target = V2Utils.GetClosestGrenade();
            if (target != null && Vector3.Distance(target.transform.position, PlayerTracker.Instance.GetTarget().transform.position) < 20f
                && Vector3.Distance(target.transform.position, __instance.transform.position) > 10f && ___shootCooldown <= 0.9f && !___aboutToShoot && flag.maliciousCannon.cooldown == 0f)
            {
                V2SwitchWeapon.SwitchWeapon.Invoke(__instance, new object[1] {3});

                targetGrenade = target.transform;
                ___shootCooldown = 1f;
                ___aboutToShoot = true;
                __instance.CancelInvoke("ShootWeapon");
                __instance.CancelInvoke("AltShootWeapon");
                __instance.Invoke("ShootWeapon", 0.2f / ___eid.totalSpeedModifier);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(V2), "AltShootWeapon")]
    class V2AltShootWeapon
    {
        static void Postfix(V2 __instance, ref int ___currentWeapon)
        {
            if (__instance.secondEncounter)
                return;

            // Threw a core
            if(___currentWeapon == 1)
            {
                V2SwitchWeapon.SwitchWeapon.Invoke(__instance, new object[1] {3});
                //Grenade target = GrenadeList.Instance.grenadeList.OrderBy((Grenade g) => Vector3.Distance(g.transform.position, __instance.transform.position)).First();
            }
        }
    }

    [HarmonyPatch(typeof(V2), "ShootWeapon")]
    class V2ShootWeapon
    {
        static bool Prefix(V2 __instance, ref int ___currentWeapon)
        {
            if (__instance.secondEncounter)
                return true;

            // PISTOL
            if(___currentWeapon == 0)
            {
                Grenade closestGrenade = V2Utils.GetClosestGrenade();
                if(closestGrenade != null && Vector3.Distance(closestGrenade.transform.position, PlayerTracker.Instance.GetTarget().position) <= 20f)
                {
                    Debug.Log("Attempting to shoot the grenade");
                    GameObject revolverBeam = GameObject.Instantiate(Plugin.revolverBeam, __instance.transform.position + __instance.transform.forward, Quaternion.identity);
                    revolverBeam.transform.LookAt(closestGrenade.transform.position);
                    if(revolverBeam.TryGetComponent<RevolverBeam>(out RevolverBeam comp))
                    {
                        comp.beamType = BeamType.Enemy;
                    }

                    return false;
                }
            }

            return true;
        }

        static void Postfix(V2 __instance, ref int ___currentWeapon)
        {
            if(___currentWeapon == 3)
            {
                V2SwitchWeapon.SwitchWeapon.Invoke(__instance, new object[] { 0 });
            }
        }
    }

    [HarmonyPatch(typeof(V2), "SwitchWeapon")]
    class V2SwitchWeapon
    {
        public static MethodInfo SwitchWeapon = typeof(V2).GetMethod("SwitchWeapon", BindingFlags.Instance | BindingFlags.NonPublic);
        static bool rocketLauncher = true;

        static bool Prefix(V2 __instance, ref int __0)
        {
            if (__instance.secondEncounter)
                return true;

            if (__0 != 1)
                return true;

            if (rocketLauncher)
                __0 = 2;
            rocketLauncher = !rocketLauncher;

            return true;
        }
    }

    [HarmonyPatch(typeof(V2), "Start")]
    class V2Start
    {
        static void RemoveAlwaysOnTop(Transform t)
        {
            foreach (Transform child in UnityUtils.GetComponentsInChildrenRecursively<Transform>(t))
            {
                child.gameObject.layer = Physics.IgnoreRaycastLayer;
            }
            t.gameObject.layer = Physics.IgnoreRaycastLayer;
        }

        static void Postfix(V2 __instance)
        {
            if (__instance.secondEncounter)
                return;

            V2FirstFlag flag = __instance.gameObject.AddComponent<V2FirstFlag>();
            flag.v2collider = __instance.GetComponent<Collider>();

            GameObject player = SceneManager.GetActiveScene().GetRootGameObjects().Where(obj => obj.name == "Player").FirstOrDefault();
            if (player == null)
                return;

            Transform v2WeaponTrans = __instance.weapons[0].transform.parent;
            GameObject rocketLauncher = player.transform.Find("Main Camera/Guns/Rocket Launcher Cannonball(Clone)").gameObject;
            GameObject maliciousCannon = player.transform.Find("Main Camera/Guns/RailcannonMalicious(Clone)").gameObject;

            GameObject v2rocketLauncher = GameObject.Instantiate(rocketLauncher, v2WeaponTrans);
            v2rocketLauncher.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            v2rocketLauncher.transform.localPosition = new Vector3(0.1f, - 0.2f, - 0.1f);
            v2rocketLauncher.transform.localRotation = Quaternion.Euler(new Vector3(10.2682f, 12.6638f, 198.834f));
            v2rocketLauncher.transform.GetChild(0).localPosition = Vector3.zero;
            v2rocketLauncher.transform.GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);
            GameObject.DestroyImmediate(v2rocketLauncher.GetComponent<RocketLauncher>());
            GameObject.DestroyImmediate(v2rocketLauncher.GetComponent<WeaponIcon>());
            GameObject.DestroyImmediate(v2rocketLauncher.GetComponent<WeaponIdentifier>());
            GameObject.DestroyImmediate(v2rocketLauncher.GetComponent<WeaponPos>());
            GameObject.DestroyImmediate(v2rocketLauncher.GetComponent<Animator>());
            V2RocketLauncher rocketComp = v2rocketLauncher.transform.GetChild(0).gameObject.AddComponent<V2RocketLauncher>();
            rocketComp.v2collider = __instance.GetComponent<Collider>();
            rocketComp.shootPoint = __instance.transform;
            RemoveAlwaysOnTop(v2rocketLauncher.transform);
            flag.rocketLauncher = rocketComp;

            GameObject v2maliciousCannon = GameObject.Instantiate(maliciousCannon, v2WeaponTrans);
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<Railcannon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIcon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIdentifier>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIcon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponPos>());
            foreach(RailCannonPip pip in UnityUtils.GetComponentsInChildrenRecursively<RailCannonPip>(v2maliciousCannon.transform))
                GameObject.DestroyImmediate(pip);
            //GameObject.Destroy(v2maliciousCannon.GetComponent<Animator>());
            v2maliciousCannon.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            v2maliciousCannon.transform.localRotation = Quaternion.Euler(270, 90, 0);
            v2maliciousCannon.transform.localPosition = Vector3.zero;
            v2maliciousCannon.transform.GetChild(0).localPosition = Vector3.zero;
            V2MaliciousCannon cannonComp = v2maliciousCannon.transform.GetChild(0).gameObject.AddComponent<V2MaliciousCannon>();
            cannonComp.v2trans = __instance.transform;
            RemoveAlwaysOnTop(v2maliciousCannon.transform);
            flag.maliciousCannon = cannonComp;

            __instance.weapons = new GameObject[] { __instance.weapons[0], __instance.weapons[1], v2rocketLauncher, v2maliciousCannon };
        }
    }
}
