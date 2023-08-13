using HarmonyLib;
using System.Security.Cryptography;
using UnityEngine;

namespace Ultrapain.Patches
{
    class SwordsMachineFlag : MonoBehaviour
    {
        public SwordsMachine sm;
        public Animator anim;
        public EnemyIdentifier eid;
        public bool speedingUp = false;

        private void ResetAnimSpeed()
        {
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Knockdown"))
            {
                Invoke("ResetAnimSpeed", 0.01f);
                return;
            }

            Debug.Log("Resetting speed");
            speedingUp = false;
            sm.SendMessage("SetSpeed");
        }

        private void Awake()
        {
            anim = GetComponent<Animator>();
            eid = GetComponent<EnemyIdentifier>();
        }

        public float speed = 1f;
        private void Update()
        {
            if (speedingUp)
            {
                if (anim == null)
                {
                    anim = sm.GetComponent<Animator>();
                    if (anim == null)
                    {
                        Destroy(this);
                        return;
                    }
                }
                anim.speed = speed;
            }
        }
    }

    class SwordsMachine_Start
    {
        static void Postfix(SwordsMachine __instance)
        {
            SwordsMachineFlag flag = __instance.gameObject.AddComponent<SwordsMachineFlag>();
            flag.sm = __instance;
        }
    }

    class SwordsMachine_Knockdown_Patch
    {
        static bool Prefix(SwordsMachine __instance, bool __0)
        {
            __instance.Enrage();
            if (!__0)
                __instance.SwordCatch();

            return false;
        }
    }

    class SwordsMachine_Down_Patch
    {
        static bool Prefix(SwordsMachine __instance)
        {
            if (ConfigManager.swordsMachineSecondPhaseMode.value == ConfigManager.SwordsMachineSecondPhase.Skip && __instance.secondPhasePosTarget == null)
                return false;
            return true;
        }

        static void Postfix(SwordsMachine __instance, Animator ___anim, EnemyIdentifier ___eid)
        {
            if (ConfigManager.swordsMachineSecondPhaseMode.value != ConfigManager.SwordsMachineSecondPhase.SpeedUp || __instance.secondPhasePosTarget != null)
                return;

            SwordsMachineFlag flag = __instance.GetComponent<SwordsMachineFlag>();
            if (flag == null)
            {
                flag = __instance.gameObject.AddComponent<SwordsMachineFlag>();
                flag.sm = __instance;
            }
            flag.speedingUp = true;
            flag.speed = (1f * ___eid.totalSpeedModifier) * ConfigManager.swordsMachineSecondPhaseSpeed.value;
            ___anim.speed = flag.speed;

            AnimatorClipInfo clipInfo = ___anim.GetCurrentAnimatorClipInfo(0)[0];
            flag.Invoke("ResetAnimSpeed", clipInfo.clip.length / flag.speed);
        }
    }

    class SwordsMachine_EndFirstPhase_Patch
    {
        static bool Prefix(SwordsMachine __instance)
        {
            if (ConfigManager.swordsMachineSecondPhaseMode.value == ConfigManager.SwordsMachineSecondPhase.Skip && __instance.secondPhasePosTarget == null)
                return false;
            return true;
        }

        static void Postfix(SwordsMachine __instance, Animator ___anim, EnemyIdentifier ___eid)
        {
            if (ConfigManager.swordsMachineSecondPhaseMode.value != ConfigManager.SwordsMachineSecondPhase.SpeedUp || __instance.secondPhasePosTarget != null)
                return;

            SwordsMachineFlag flag = __instance.GetComponent<SwordsMachineFlag>();
            if (flag == null)
            {
                flag = __instance.gameObject.AddComponent<SwordsMachineFlag>();
                flag.sm = __instance;
            }
            flag.speedingUp = true;
            flag.speed = (1f * ___eid.totalSpeedModifier) * ConfigManager.swordsMachineSecondPhaseSpeed.value;
            ___anim.speed = flag.speed;

            AnimatorClipInfo clipInfo = ___anim.GetCurrentAnimatorClipInfo(0)[0];
            flag.Invoke("ResetAnimSpeed", clipInfo.clip.length / flag.speed);
        }
    }

    /*class SwordsMachine_SetSpeed_Patch
    {
        static bool Prefix(SwordsMachine __instance, ref Animator ___anim)
        {
            if (___anim == null)
                ___anim = __instance.GetComponent<Animator>();

            SwordsMachineFlag flag = __instance.GetComponent<SwordsMachineFlag>();
            if (flag == null || !flag.speedingUp)
                return true;

            return false;
        }
    }*/

    /*[HarmonyPatch(typeof(SwordsMachine))]
    [HarmonyPatch("Down")]
    class SwordsMachine_Down_Patch
    {
        static void Postfix(SwordsMachine __instance, ref Animator ___anim, ref Machine ___mach)
        {
            ___anim.Play("Knockdown", 0, Plugin.SwordsMachineKnockdownTimeNormalized);

            __instance.CancelInvoke("CheckLoop");
            ___mach.health = ___mach.symbiote.health;
            __instance.downed = false;
        }
    }

    [HarmonyPatch(typeof(SwordsMachine))]
    [HarmonyPatch("CheckLoop")]
    class SwordsMachine_CheckLoop_Patch
    {
        static bool Prefix(SwordsMachine __instance)
        {
            return false;
        }
    }*/

    /*[HarmonyPatch(typeof(SwordsMachine))]
    [HarmonyPatch("ShootGun")]
    class SwordsMachine_ShootGun_Patch
    {
        static bool Prefix(SwordsMachine __instance)
        {
            if(UnityEngine.Random.RandomRangeInt(0, 2) == 1)
            {
                GameObject grn = GameObject.Instantiate(Plugin.shotgunGrenade.gameObject, __instance.transform.position, __instance.transform.rotation);
                grn.transform.position += grn.transform.forward * 0.5f + grn.transform.up * 0.5f;

                Grenade grnComp = grn.GetComponent<Grenade>();
                grnComp.enemy = true;
                grnComp.CanCollideWithPlayer(true);

                Vector3 playerPosition = MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position;
                float distanceFromPlayer = Vector3.Distance(playerPosition, grn.transform.position);
                Vector3 predictedPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(distanceFromPlayer / 40);

                grn.transform.LookAt(predictedPosition);
                grn.GetComponent<Rigidbody>().maxAngularVelocity = 40;
                grn.GetComponent<Rigidbody>().velocity = grn.transform.forward * 40;

                return false;
            }

            return true;
        }
    }*/

    class ThrownSword_Start_Patch
    {
        static void Postfix(ThrownSword __instance)
        {
            __instance.gameObject.AddComponent<ThrownSwordCollisionDetector>();
        }
    }

    class ThrownSword_OnTriggerEnter_Patch
    {
        static void Postfix(ThrownSword __instance, Collider __0)
        {
            if (__0.gameObject.tag == "Player")
            {
                GameObject explosionObj = GameObject.Instantiate(Plugin.shotgunGrenade.obj.gameObject.GetComponent<Grenade>().explosion, __0.gameObject.transform.position, __0.gameObject.transform.rotation);
                foreach (Explosion explosion in explosionObj.GetComponentsInChildren<Explosion>())
                {
                    explosion.enemy = true;
                }
            }
        }
    }

    class ThrownSwordCollisionDetector : MonoBehaviour
    {
        public bool exploded = false;

        public void OnCollisionEnter(Collision other)
        {
            if (exploded)
                return;

            if (other.gameObject.layer != 24)
            {
                Debug.Log($"Hit layer {other.gameObject.layer}");
                return;
            }

            exploded = true;

            GameObject explosionObj = GameObject.Instantiate(Plugin.shotgunGrenade.obj.gameObject.GetComponent<Grenade>().explosion, transform.position, transform.rotation);
            foreach (Explosion explosion in explosionObj.GetComponentsInChildren<Explosion>())
            {
                explosion.enemy = true;
                explosion.damage = ConfigManager.swordsMachineExplosiveSwordDamage.value;
                explosion.maxSize *= ConfigManager.swordsMachineExplosiveSwordSize.value;
                explosion.speed *= ConfigManager.swordsMachineExplosiveSwordSize.value;
            }

            gameObject.GetComponent<ThrownSword>().Invoke("Return", 0.1f);
        }
    }
}
