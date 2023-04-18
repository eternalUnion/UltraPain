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

    }

    class V2RocketLauncher : MonoBehaviour
    {
        Transform shootPoint;
        public Collider v2collider;

        void Awake()
        {
            shootPoint = UnityUtils.GetChildByNameRecursively(transform, "Shootpoint");
        }

        void PrepareFire()
        {
            Instantiate<GameObject>(Plugin.v2flashUnparryable, this.shootPoint.position, this.shootPoint.rotation).transform.localScale *= 4f;
        }

        void SetRocketRotation(Transform rocket)
        {
            /*Transform target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            Vector3 projectedPos = target.position;
            if (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude == 0f)
            {
                rocket.transform.LookAt(new Vector3(target.position.x, base.transform.position.y, target.position.z));
                return;
            }
            RaycastHit raycastHit;
            if (Physics.Raycast(target.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(), out raycastHit, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.35f, 4096, QueryTriggerInteraction.Collide))
            {
                projectedPos = target.position;
            }
            else if (Physics.Raycast(target.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(), out raycastHit, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.35f, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide))
            {
                projectedPos = raycastHit.point;
            }
            else
            {
                projectedPos = target.position + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity() * 0.35f;
                projectedPos = new Vector3(projectedPos.x, target.transform.position.y + (target.transform.position.y - projectedPos.y) * 0.5f, projectedPos.z);
            }
            rocket.transform.LookAt(projectedPos);*/

            Rigidbody rb = rocket.GetComponent<Rigidbody>();
            Grenade grn = rocket.GetComponent<Grenade>();
            float magnitude = grn.rocketSpeed;

            //float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position, __0.transform.position);
            float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position, rocket.transform.position);
            Vector3 predictedPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(1.0f);

            float velocity = Mathf.Clamp(distance, Mathf.Max(magnitude - 5.0f, 0), magnitude + 5);

            rocket.transform.LookAt(predictedPosition);
            rocket.GetComponent<Grenade>().rocketSpeed = velocity;
            rb.maxAngularVelocity = velocity;
            rb.velocity = rocket.transform.forward * velocity;
        }

        void Fire()
        {
            GameObject rocket = Instantiate<GameObject>(Plugin.rocket, shootPoint.transform.position + shootPoint.transform.forward * 2, shootPoint.transform.rotation);
            SetRocketRotation(rocket.transform);
            Grenade component = rocket.GetComponent<Grenade>();
            if (component)
            {
                component.harmlessExplosion = component.explosion;
                component.enemy = true;
                component.CanCollideWithPlayer(true);
            }

            Physics.IgnoreCollision(rocket.GetComponent<Collider>(), v2collider);
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
        Transform shootPoint;
        public Transform v2trans;

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
            Grenade target = GrenadeList.Instance.grenadeList.OrderBy((Grenade g) => Vector3.Distance(g.transform.position, transform.position)).FirstOrDefault();
            Transform targetTransform = null;
            if (target != null && Vector3.Distance(target.transform.position, PlayerTracker.Instance.GetTarget().transform.position) < 20)
            {
                Debug.Log("Targeted grenade");
                targetTransform = target.transform;
            }
            else
            {
                Debug.Log("Targeted player");
                targetTransform = PlayerTracker.Instance.GetTarget();
            }

            GameObject beam = Instantiate(Plugin.maliciousCannonBeam, v2trans.position + v2trans.forward, Quaternion.identity);
            beam.transform.LookAt(targetTransform);
            if(beam.TryGetComponent<RevolverBeam>(out RevolverBeam comp))
            {
                comp.alternateStartPoint = shootPoint.transform.position;
                comp.ignoreEnemyType = EnemyType.V2;
            }
        }

        void PrepareAltFire()
        {

        }

        void AltFire()
        {

        }
    }

    [HarmonyPatch(typeof(V2), "Update")]
    class V2Update
    {
        static MethodInfo ShootWeapon = typeof(V2).GetMethod("ShootWeapon", BindingFlags.Instance | BindingFlags.NonPublic);

        static bool Prefix(V2 __instance, ref int ___currentWeapon, ref Transform ___overrideTarget, ref Rigidbody ___overrideTargetRb, ref float ___shootCooldown,
            ref bool ___aboutToShoot, ref EnemyIdentifier ___eid)
        {
            if (__instance.secondEncounter)
                return true;

            Grenade target = GrenadeList.Instance.grenadeList.OrderBy((Grenade g) => Vector3.Distance(g.transform.position, __instance.transform.position)).FirstOrDefault();
            if (target != null && Vector3.Distance(target.transform.position, PlayerTracker.Instance.GetTarget().transform.position) < 10 && ___shootCooldown <= 0.9f && !___aboutToShoot)
            {
                V2SwitchWeapon.SwitchWeapon.Invoke(__instance, new object[1] {3});

                ___overrideTarget = target.transform;
                ___overrideTargetRb = target.GetComponent<Rigidbody>();
                ___shootCooldown = 1f;
                ___aboutToShoot = true;
                __instance.CancelInvoke("ShootWeapon");
                __instance.CancelInvoke("AltShootWeapon");
                __instance.Invoke("ShootWeapon", 0.2f / ___eid.totalSpeedModifier);
                //ShootWeapon.Invoke(__instance, new object[0]);

                /*GameObject gameObject = GameObject.Instantiate<GameObject>(Plugin.revolverBullet, __instance.transform.position + __instance.transform.forward, Quaternion.identity);
                gameObject.transform.LookAt(target.transform);
                Projectile component = gameObject.GetComponent<Projectile>();
                if (component)
                {
                    component.safeEnemyType = EnemyType.V2;
                    component.speed *= 20;
                    component.damage *= 1;
                }

                ___shootCooldown = 1f;*/
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(V2), "AltShootWeapon")]
    class V2AltShootWeapon
    {
        public static Grenade targetGrenade;

        static void Postfix(V2 __instance, ref int ___currentWeapon)
        {
            if (__instance.secondEncounter)
                return;

            // Threw a core
            if(___currentWeapon == 1)
            {
                V2SwitchWeapon.SwitchWeapon.Invoke(__instance, new object[1] {0});
                //Grenade target = GrenadeList.Instance.grenadeList.OrderBy((Grenade g) => Vector3.Distance(g.transform.position, __instance.transform.position)).First();
            }
        }
    }

    [HarmonyPatch(typeof(V2), "ShootWeapon")]
    class V2ShootWeapon
    {
        static bool Prefix(V2 __instance)
        {
            if (__instance.secondEncounter)
                return true;

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

            __instance.gameObject.AddComponent<V2FirstFlag>();

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
            RemoveAlwaysOnTop(v2rocketLauncher.transform);

            GameObject v2maliciousCannon = GameObject.Instantiate(maliciousCannon, v2WeaponTrans);
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<Railcannon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIcon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIdentifier>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIcon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponPos>());
            foreach(RailCannonPip pip in UnityUtils.GetComponentsInChildrenRecursively<RailCannonPip>(v2maliciousCannon.transform))
                GameObject.DestroyImmediate(pip);
            //GameObject.Destroy(v2maliciousCannon.GetComponent<Animator>());
            v2maliciousCannon.transform.localScale = new Vector3(1f, 1.125f, 1.125f);
            v2maliciousCannon.transform.localPosition = Vector3.zero;
            v2maliciousCannon.transform.GetChild(0).localPosition = Vector3.zero;
            V2MaliciousCannon cannonComp = v2maliciousCannon.transform.GetChild(0).gameObject.AddComponent<V2MaliciousCannon>();
            cannonComp.v2trans = __instance.transform;
            RemoveAlwaysOnTop(v2maliciousCannon.transform);

            __instance.weapons = new GameObject[] { __instance.weapons[0], __instance.weapons[1], v2rocketLauncher, v2maliciousCannon };
        }
    }
}
