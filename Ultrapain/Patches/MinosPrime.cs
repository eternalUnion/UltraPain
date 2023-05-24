using HarmonyLib;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Ultrapain.Patches
{
    class MinosPrimeCharge
    {
        static GameObject decoy;

        public static void CreateDecoy()
        {
            if (decoy != null || Plugin.minosPrime == null)
                return;

            decoy = GameObject.Instantiate(Plugin.minosPrime, Vector3.zero, Quaternion.identity);
            decoy.SetActive(false);

            GameObject.Destroy(decoy.GetComponent<MinosPrime>());
            GameObject.Destroy(decoy.GetComponent<Machine>());
            GameObject.Destroy(decoy.GetComponent<BossHealthBar>());
            GameObject.Destroy(decoy.GetComponent<EventOnDestroy>());
            GameObject.Destroy(decoy.GetComponent<BossIdentifier>());
            GameObject.Destroy(decoy.GetComponent<EnemyIdentifier>());
            GameObject.Destroy(decoy.GetComponent<BasicEnemyDataRelay>());
            GameObject.Destroy(decoy.GetComponent<Rigidbody>());
            GameObject.Destroy(decoy.GetComponent<CapsuleCollider>());
            GameObject.Destroy(decoy.GetComponent<AudioSource>());
            GameObject.Destroy(decoy.GetComponent<NavMeshAgent>());
            SandboxEnemy sbe = decoy.GetComponent<SandboxEnemy>();
            if (sbe != null)
                GameObject.Destroy(sbe);
            MindflayerDecoy comp = decoy.AddComponent<MindflayerDecoy>();
            comp.fadeSpeed = 2.5f;
            //decoy.GetComponent<Animator>().StopPlayback();
            //decoy.GetComponent<Animator>().Update(100f);

            GameObject.Destroy(decoy.transform.Find("SwingCheck").gameObject);
            GameObject.Destroy(decoy.transform.Find("Capsule").gameObject);
            GameObject.Destroy(decoy.transform.Find("Point Light").gameObject);
            foreach (EnemyIdentifierIdentifier eii in UnityUtils.GetComponentsInChildrenRecursively<EnemyIdentifierIdentifier>(decoy.transform))
                GameObject.Destroy(eii);
        }

        static void DrawTrail(MinosPrime instance, Animator anim, Vector3 startPosition, Vector3 targetPosition)
        {
            if(decoy == null)
            {
                CreateDecoy();
                return;
            }
            targetPosition = Vector3.MoveTowards(targetPosition, startPosition, 5f);

            Vector3 currentPosition = startPosition;
            float distance = Vector3.Distance(startPosition, targetPosition);
            if (distance < 5)
                return;

            float deltaDistance = 5f;
            if (distance / deltaDistance > 15)
                deltaDistance = distance / 15;

            float fadeSpeed = 1f / ConfigManager.minosPrimeTeleportTrailDuration.value;
            while (currentPosition != targetPosition)
            {
                GameObject gameObject = GameObject.Instantiate(decoy, currentPosition, instance.transform.rotation);
                gameObject.SetActive(true);
                Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
                AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
                componentInChildren.Play(currentAnimatorStateInfo.shortNameHash, 0, currentAnimatorStateInfo.normalizedTime);
                componentInChildren.speed = 0f;
                gameObject.GetComponent<MindflayerDecoy>().fadeSpeed = fadeSpeed;

                currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, deltaDistance);
            }
        }

        static void Postfix(MinosPrime __instance, Animator ___anim)
        {
            string stateName = ___anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            MinosPrimeFlag flag = __instance.GetComponent<MinosPrimeFlag>();
            if (stateName == "Combo" || (flag != null && flag.throwingProjectile))
                return;

            Transform player = MonoSingleton<PlayerTracker>.Instance.GetPlayer();

            float min = ConfigManager.minosPrimeRandomTeleportMinDistance.value;
            float max = ConfigManager.minosPrimeRandomTeleportMaxDistance.value;

            Vector3 unitSphere = UnityEngine.Random.onUnitSphere;
            unitSphere.y = Mathf.Abs(unitSphere.y);
            float distance = UnityEngine.Random.Range(min, max);

            Ray ray = new Ray(player.position, unitSphere);

            LayerMask mask = new LayerMask();
            mask.value |= 256 | 16777216;
            if (Physics.Raycast(ray, out RaycastHit hit, max, mask, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance < min)
                    return;
                Vector3 point = ray.GetPoint(hit.distance - 5);
                __instance.Teleport(point, __instance.transform.position);
            }
            else
            {
                Vector3 point = ray.GetPoint(distance);
                __instance.Teleport(point, __instance.transform.position);
            }
        }

        static void TeleportPostfix(MinosPrime __instance, Animator ___anim, Vector3 __0, Vector3 __1)
        {
            DrawTrail(__instance, ___anim, __1, __0);
        }
    }

    class MinosPrimeFlag : MonoBehaviour
    {
        void Start()
        {

        }

        public void ComboExplosion()
        {
            GameObject explosion = Instantiate(Plugin.lightningStrikeExplosive, transform.position, Quaternion.identity);
            foreach(Explosion e in explosion.GetComponentsInChildren<Explosion>())
            {
                e.toIgnore.Add(EnemyType.MinosPrime);
                e.maxSize *= ConfigManager.minosPrimeComboExplosionSize.value;
                e.speed *= ConfigManager.minosPrimeComboExplosionSize.value;
                e.damage = (int)(e.damage * ConfigManager.minosPrimeComboExplosionDamage.value);
            }
        }

        public void BigExplosion()
        {
            GameObject explosion = Instantiate(Plugin.lightningStrikeExplosive, transform.position, Quaternion.identity);
            foreach (Explosion e in explosion.GetComponentsInChildren<Explosion>())
            {
                e.toIgnore.Add(EnemyType.MinosPrime);
                e.maxSize *= ConfigManager.minosPrimeExplosionSize.value;
                e.speed *= ConfigManager.minosPrimeExplosionSize.value;
                e.damage = (int)(e.damage * ConfigManager.minosPrimeExplosionDamage.value);
            }
        }

        public bool throwingProjectile = false;
        public string plannedAttack = "";

        public bool explosionAttack = false;
    }

    class MinosPrime_Start
    {
        static void Postfix(MinosPrime __instance, Animator ___anim, ref bool ___enraged)
        {
            if (ConfigManager.minosPrimeEarlyPhaseToggle.value)
                ___enraged = true;
            __instance.gameObject.AddComponent<MinosPrimeFlag>();

            if (ConfigManager.minosPrimeComboExplosionToggle.value)
            {
                AnimationClip boxing = ___anim.runtimeAnimatorController.animationClips.Where(item => item.name == "Boxing").First();
                List<UnityEngine.AnimationEvent> boxingEvents = boxing.events.ToList();
                boxingEvents.Insert(15, new UnityEngine.AnimationEvent() { time = 2.4f, functionName = "ComboExplosion", messageOptions = SendMessageOptions.RequireReceiver });
                boxing.events = boxingEvents.ToArray();
            }
        }
    }

    class MinosPrime_StopAction
    {
        static void Postfix(MinosPrime __instance, EnemyIdentifier ___eid)
        {
            MinosPrimeFlag flag = __instance.GetComponent<MinosPrimeFlag>();
            if (flag == null)
                return;

            if (flag.plannedAttack != "")
            {
                __instance.SendMessage(flag.plannedAttack);
                flag.plannedAttack = "";
            }
        }
    }

    // aka JUDGEMENT
    class MinosPrime_Dropkick
    {
        static bool Prefix(MinosPrime __instance, EnemyIdentifier ___eid, ref bool ___inAction, Animator ___anim)
        {
            MinosPrimeFlag flag = __instance.GetComponent<MinosPrimeFlag>();
            if (flag == null)
                return true;

            if (!flag.throwingProjectile)
            {
                if (ConfigManager.minosPrimeExplosionToggle.value
                    && UnityEngine.Random.Range(0, 99.9f) < ConfigManager.minosPrimeExplosionChance.value)
                {
                    __instance.TeleportAnywhere();
                    ___inAction = true;
                    flag.explosionAttack = true;
                    ___anim.speed = ___eid.totalSpeedModifier * ConfigManager.minosPrimeExplosionWindupSpeed.value;
                    ___anim.Play("Outro", 0, 0.5f);
                    __instance.PlayVoice(new AudioClip[] { __instance.phaseChangeVoice });

                    return false;
                }

                if (ConfigManager.minosPrimeComboToggle.value)
                {
                    flag.throwingProjectile = true;
                    flag.plannedAttack = "Dropkick";
                    __instance.SendMessage("ProjectilePunch");
                }

                return false;
            }
            else
            {
                if (ConfigManager.minosPrimeComboToggle.value)
                {
                    flag.plannedAttack = "ProjectilePunch";
                    flag.throwingProjectile = false;
                }
            }

            return true;
        }
    }

    // aka PREPARE THYSELF
    class MinosPrime_Combo
    {
        static float timing = 3f;

        static void Postfix(MinosPrime __instance, EnemyIdentifier ___eid)
        {
            if (!ConfigManager.minosPrimeComboToggle.value)
                return;

            MinosPrimeFlag flag = __instance.GetComponent<MinosPrimeFlag>();
            if (flag == null)
                return;

            flag.plannedAttack = "Uppercut";
        }
    }

    class MinosPrime_Ascend
    {
        static bool Prefix(MinosPrime __instance, EnemyIdentifier ___eid, Animator ___anim, ref bool ___vibrating)
        {
            if (___eid.health <= 0)
                return true;

            MinosPrimeFlag flag = __instance.GetComponent<MinosPrimeFlag>();
            if (flag == null)
                return true;

            if (!flag.explosionAttack)
                return true;

            ___anim.speed = ___eid.totalSpeedModifier;
            ___vibrating = false;
            flag.explosionAttack = false;
            flag.BigExplosion();
            __instance.Invoke("Uppercut", 0.5f);
            return false;
        }
    }
}
