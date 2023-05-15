using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ultrapain.Patches
{
    public class V2SecondFlag : MonoBehaviour
    {
        public V2RocketLauncher rocketLauncher;
        public V2MaliciousCannon maliciousCannon;
        public Collider v2collider;

        public Transform targetGrenade;
    }

    public class V2RocketLauncher : MonoBehaviour
    {
        public Transform shootPoint;
        public Collider v2collider;
        AudioSource aud;

        float altFireCharge = 0f;
        bool altFireCharging = false;

        void Awake()
        {
            aud = GetComponent<AudioSource>();
            if (aud == null)
                aud = gameObject.AddComponent<AudioSource>();

            aud.playOnAwake = false;
            aud.clip = Plugin.cannonBallChargeAudio;
        }

        void Update()
        {
            if (altFireCharging)
            {
                if (!aud.isPlaying)
                {
                    aud.pitch = Mathf.Min(1f, altFireCharge) + 0.5f;
                    aud.Play();
                }

                altFireCharge += Time.deltaTime;
            }
        }

        void OnDisable()
        {
            altFireCharging = false;
        }

        void PrepareFire()
        {
            Instantiate<GameObject>(Plugin.v2flashUnparryable, this.shootPoint.position, this.shootPoint.rotation).transform.localScale *= 4f;
        }

        void SetRocketRotation(Transform rocket)
        {
            // OLD PREDICTION
            /*Rigidbody rb = rocket.GetComponent<Rigidbody>();
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
            */

            // NEW PREDICTION
            Vector3 playerPos = Tools.PredictPlayerPosition(0.5f);
            rocket.LookAt(playerPos);
            Rigidbody rb = rocket.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.AddForce(rocket.transform.forward * 10000f);
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
            altFireCharging = true;
        }

        void AltFire()
        {
            altFireCharging = false;
            altFireCharge = 0;
            GameObject cannonBall = Instantiate(Plugin.cannonBall, shootPoint.transform.position, shootPoint.transform.rotation);
            cannonBall.transform.position = new Vector3(cannonBall.transform.position.x, v2collider.bounds.center.y, cannonBall.transform.position.z);
            cannonBall.transform.LookAt(PlayerTracker.Instance.GetTarget());
            cannonBall.transform.position += cannonBall.transform.forward * 2f;

            if(cannonBall.TryGetComponent<Cannonball>(out Cannonball comp))
            {
                comp.sourceWeapon = this.gameObject;
            }

            if(cannonBall.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.velocity = rb.transform.forward * 150f;
            }
        }

        static MethodInfo bounce = typeof(Cannonball).GetMethod("Bounce", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        public static bool CannonBallTriggerPrefix(Cannonball __instance, Collider __0)
        {
            if(__instance.sourceWeapon != null && __instance.sourceWeapon.GetComponent<V2RocketLauncher>() != null)
            {
                if (__0.gameObject.tag == "Player")
                {
                    if (!__instance.hasBounced)
                    {
                        bounce.Invoke(__instance, new object[0]);
                        NewMovement.Instance.GetHurt((int)__instance.damage, true, 1, false, false);
                        return false;
                    }
                }
                else
                {
                    EnemyIdentifierIdentifier eii = __0.gameObject.GetComponent<EnemyIdentifierIdentifier>();
                    if (!__instance.launched && eii != null && (eii.eid.enemyType == EnemyType.V2 || eii.eid.enemyType == EnemyType.V2Second))
                        return false;
                }

                return true;
            }

            return true;
        }
    }

    public class V2MaliciousCannon : MonoBehaviour
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
            cooldown = ConfigManager.v2SecondMalCannonSnipeCooldown.value;

            Transform target = V2Utils.GetClosestGrenade();
            Vector3 targetPosition = Vector3.zero;

            if (target != null)
            {
                Debug.Log($"{debugTag} Targeted grenade");
                targetPosition = target.position;
            }
            else
            {
                Transform playerTarget = PlayerTracker.Instance.GetTarget();
                /*if (Physics.Raycast(new Ray(playerTarget.position, Vector3.down), out RaycastHit hit, 100f, new LayerMask() { value = (1 << 8 | 1 << 24) }, QueryTriggerInteraction.Ignore))
                {
                    Debug.Log($"{debugTag} Targeted ground below player");
                    targetPosition = hit.point;
                }
                else
                {*/
                    Debug.Log($"{debugTag} Targeted player with random spread");
                    targetPosition = playerTarget.transform.position + UnityEngine.Random.onUnitSphere * 2f;
                //}
            }

            GameObject beam = Instantiate(Plugin.maliciousCannonBeam, v2trans.position, Quaternion.identity);
            beam.transform.position = new Vector3(beam.transform.position.x, v2trans.GetComponent<Collider>().bounds.center.y, beam.transform.position.z);
            beam.transform.LookAt(targetPosition);
            beam.transform.position += beam.transform.forward * 2f;
            if (beam.TryGetComponent<RevolverBeam>(out RevolverBeam comp))
            {
                comp.alternateStartPoint = shootPoint.transform.position;
                comp.ignoreEnemyType = EnemyType.V2Second;
                comp.sourceWeapon = gameObject;
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


    class V2SecondUpdate
    {
        static bool Prefix(V2 __instance, ref int ___currentWeapon, ref Transform ___overrideTarget, ref Rigidbody ___overrideTargetRb, ref float ___shootCooldown,
            ref bool ___aboutToShoot, ref EnemyIdentifier ___eid, bool ___escaping)
        {
            if (!__instance.secondEncounter)
                return true;

            if (!__instance.active || ___escaping || BlindEnemies.Blind)
                return true;

            V2SecondFlag flag = __instance.GetComponent<V2SecondFlag>();
            if (flag == null)
                return true;

            if (flag.maliciousCannon.cooldown > 0)
                flag.maliciousCannon.cooldown = Mathf.MoveTowards(flag.maliciousCannon.cooldown, 0, Time.deltaTime);

            if (flag.targetGrenade == null)
            {
                Transform target = V2Utils.GetClosestGrenade();

                //if (ConfigManager.v2SecondMalCannonSnipeToggle.value && target != null
                //    && ___shootCooldown <= 0.9f && !___aboutToShoot && flag.maliciousCannon.cooldown == 0f)
                if(target != null)
                {
                    float distanceToPlayer = Vector3.Distance(target.position, PlayerTracker.Instance.GetTarget().transform.position);
                    float distanceToV2 = Vector3.Distance(target.position, flag.v2collider.bounds.center);
                    if (ConfigManager.v2SecondMalCannonSnipeToggle.value && flag.maliciousCannon.cooldown == 0 && distanceToPlayer <= ConfigManager.v2SecondMalCannonSnipeMaxDistanceToPlayer.value && distanceToV2 >= ConfigManager.v2SecondMalCannonSnipeMinDistanceToV2.value)
                    {
                        flag.targetGrenade = target;

                        ___shootCooldown = 1f;
                        ___aboutToShoot = true;
                        __instance.weapons[___currentWeapon].transform.GetChild(0).SendMessage("CancelAltCharge", SendMessageOptions.DontRequireReceiver);
                        __instance.CancelInvoke("ShootWeapon");
                        __instance.CancelInvoke("AltShootWeapon");
                        __instance.Invoke("ShootWeapon", ConfigManager.v2SecondMalCannonSnipeReactTime.value / ___eid.totalSpeedModifier);
                        
                        V2SecondSwitchWeapon.SwitchWeapon.Invoke(__instance, new object[1] { 4 });
                    }
                    else if(ConfigManager.v2SecondCoreSnipeToggle.value && distanceToPlayer <= ConfigManager.v2SecondCoreSnipeMaxDistanceToPlayer.value && distanceToV2 >= ConfigManager.v2SecondCoreSnipeMinDistanceToV2.value)
                    {
                        flag.targetGrenade = target;

                        __instance.weapons[___currentWeapon].transform.GetChild(0).SendMessage("CancelAltCharge", SendMessageOptions.DontRequireReceiver);
                        __instance.CancelInvoke("ShootWeapon");
                        __instance.CancelInvoke("AltShootWeapon");
                        __instance.Invoke("ShootWeapon", ConfigManager.v2SecondCoreSnipeReactionTime.value / ___eid.totalSpeedModifier);
                        ___shootCooldown = 1f;
                        ___aboutToShoot = true;

                        V2SecondSwitchWeapon.SwitchWeapon.Invoke(__instance, new object[1] { 0 });
                        Debug.Log("Preparing to fire for grenade");
                    }
                }
            }

            return true;
        }
    }

    class V2SecondShootWeapon
    {
        static bool Prefix(V2 __instance, ref int ___currentWeapon)
        {
            if (!__instance.secondEncounter)
                return true;

            V2SecondFlag flag = __instance.GetComponent<V2SecondFlag>();
            if (flag == null)
                return true;

            if (___currentWeapon == 0)
            {
                //Transform closestGrenade = V2Utils.GetClosestGrenade();
                Transform closestGrenade = flag.targetGrenade;
                if (closestGrenade != null && ConfigManager.v2SecondCoreSnipeToggle.value)
                {
                    float distanceToPlayer = Vector3.Distance(closestGrenade.position, PlayerTracker.Instance.GetTarget().position);
                    float distanceToV2 = Vector3.Distance(closestGrenade.position, flag.v2collider.bounds.center);
                    if (distanceToPlayer <= ConfigManager.v2SecondCoreSnipeMaxDistanceToPlayer.value && distanceToV2 >= ConfigManager.v2SecondCoreSnipeMinDistanceToV2.value)
                    {
                        Debug.Log("Attempting to shoot the grenade");
                        GameObject revolverBeam = GameObject.Instantiate(Plugin.revolverBeam, __instance.transform.position + __instance.transform.forward, Quaternion.identity);
                        revolverBeam.transform.LookAt(closestGrenade.position);
                        if (revolverBeam.TryGetComponent<RevolverBeam>(out RevolverBeam comp))
                        {
                            comp.beamType = BeamType.Enemy;
                            comp.sourceWeapon = __instance.weapons[0];
                        }

                        __instance.ForceDodge(V2Utils.GetDirectionAwayFromTarget(flag.v2collider.bounds.center, closestGrenade.transform.position));
                        return false;
                    }
                }
            }
            else if(___currentWeapon == 4)
            {
                __instance.ForceDodge(V2Utils.GetDirectionAwayFromTarget(flag.v2collider.bounds.center, PlayerTracker.Instance.GetTarget().position));
            }

            return true;
        }

        static void Postfix(V2 __instance, ref int ___currentWeapon)
        {
            if (!__instance.secondEncounter)
                return;

            if (___currentWeapon == 4)
            {
                V2SecondSwitchWeapon.SwitchWeapon.Invoke(__instance, new object[] { 0 });
            }
        }
    }

    class V2SecondSwitchWeapon
    {
        public static MethodInfo SwitchWeapon = typeof(V2).GetMethod("SwitchWeapon", BindingFlags.Instance | BindingFlags.NonPublic);

        static bool Prefix(V2 __instance, ref int __0)
        {
            if (!__instance.secondEncounter || !ConfigManager.v2SecondRocketLauncherToggle.value)
                return true;

            if (__0 != 1 && __0 != 2)
                return true;

            int[] weapons = new int[] { 1, 2, 3 };
            int weapon = weapons[UnityEngine.Random.RandomRangeInt(0, weapons.Length)];
            __0 = weapon;

            return true;
        }
    }

    class V2SecondFastCoin
    {
        static MethodInfo switchWeapon = typeof(V2).GetMethod("SwitchWeapon", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        static bool Prefix(V2 __instance, ref int ___coinsToThrow, ref bool ___aboutToShoot, ref Transform ___overrideTarget, ref Rigidbody ___overrideTargetRb,
            ref Transform ___target, Animator ___anim, ref bool ___shootingForCoin, ref int ___currentWeapon, ref float ___shootCooldown, ref bool ___aiming)
        {
            if (___coinsToThrow == 0)
            {
                return false;
            }
            GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.coin, __instance.transform.position, __instance.transform.rotation);
            Rigidbody rigidbody;
            if (gameObject.TryGetComponent<Rigidbody>(out rigidbody))
            {
                rigidbody.AddForce((___target.transform.position - ___anim.transform.position).normalized * 20f + Vector3.up * 30f, ForceMode.VelocityChange);
            }
            Coin coin;
            if (gameObject.TryGetComponent<Coin>(out coin))
            {
                GameObject gameObject2 = GameObject.Instantiate<GameObject>(coin.flash, coin.transform.position, MonoSingleton<CameraController>.Instance.transform.rotation);
                gameObject2.transform.localScale *= 2f;
                gameObject2.transform.SetParent(gameObject.transform, true);
            }
            ___coinsToThrow--;

            ___aboutToShoot = true;
            ___shootingForCoin = true;
            switchWeapon.Invoke(__instance, new object[1] { 0 });
            __instance.CancelInvoke("ShootWeapon");
            __instance.Invoke("ShootWeapon", ConfigManager.v2SecondFastCoinShootDelay.value);

            ___overrideTarget = coin.transform;
            ___overrideTargetRb = coin.GetComponent<Rigidbody>();
            __instance.CancelInvoke("AltShootWeapon");
            __instance.weapons[___currentWeapon].transform.GetChild(0).SendMessage("CancelAltCharge", SendMessageOptions.DontRequireReceiver);
            ___shootCooldown = 1f;

            __instance.CancelInvoke("ThrowCoins");
            __instance.Invoke("ThrowCoins", ConfigManager.v2SecondFastCoinThrowDelay.value);

            return false;
        }
    }

    class V2SecondEnrage
    {
        static void Postfix(BossHealthBar __instance, ref EnemyIdentifier ___eid, ref int ___currentHpSlider)
        {
            V2 v2 = __instance.GetComponent<V2>();
            if (v2 != null && v2.secondEncounter && ___currentHpSlider == 1)
                v2.Invoke("Enrage", 0.01f);
        }
    }

    class V2SecondStart
    {
        static void RemoveAlwaysOnTop(Transform t)
        {
            foreach (Transform child in UnityUtils.GetComponentsInChildrenRecursively<Transform>(t))
            {
                child.gameObject.layer = Physics.IgnoreRaycastLayer;
            }
            t.gameObject.layer = Physics.IgnoreRaycastLayer;
        }

        static FieldInfo machineV2 = typeof(Machine).GetField("v2", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        static void Postfix(V2 __instance, EnemyIdentifier ___eid)
        {
            if (!__instance.secondEncounter)
                return;

            V2SecondFlag flag = __instance.gameObject.AddComponent<V2SecondFlag>();
            flag.v2collider = __instance.GetComponent<Collider>();

            /*___eid.enemyType = EnemyType.V2Second;
            ___eid.UpdateBuffs();
            machineV2.SetValue(__instance.GetComponent<Machine>(), __instance);*/

            GameObject player = SceneManager.GetActiveScene().GetRootGameObjects().Where(obj => obj.name == "Player").FirstOrDefault();
            if (player == null)
                return;

            Transform v2WeaponTrans = __instance.weapons[0].transform.parent;

            GameObject v2rocketLauncher = GameObject.Instantiate(Plugin.rocketLauncherAlt, v2WeaponTrans);
            v2rocketLauncher.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            v2rocketLauncher.transform.localPosition = new Vector3(0.1f, -0.2f, -0.1f);
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

            GameObject v2maliciousCannon = GameObject.Instantiate(Plugin.maliciousRailcannon, v2WeaponTrans);
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<Railcannon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIcon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIdentifier>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponIcon>());
            GameObject.DestroyImmediate(v2maliciousCannon.GetComponent<WeaponPos>());
            foreach (RailCannonPip pip in UnityUtils.GetComponentsInChildrenRecursively<RailCannonPip>(v2maliciousCannon.transform))
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

            EnemyRevolver rev = UnityUtils.GetComponentInChildrenRecursively<EnemyRevolver>(__instance.weapons[0].transform);
            V2CommonRevolverComp revComp;
            if (ConfigManager.v2SecondSharpshooterToggle.value)
            {
                revComp = rev.gameObject.AddComponent<V2CommonRevolverComp>();
                revComp.secondPhase = __instance.secondEncounter;
            }

            __instance.weapons = new GameObject[] { __instance.weapons[0], __instance.weapons[1], __instance.weapons[2], v2rocketLauncher, v2maliciousCannon };
        }
    }
}
