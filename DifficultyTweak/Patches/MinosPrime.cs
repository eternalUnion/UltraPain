using HarmonyLib;
using Sandbox;
using UnityEngine;
using UnityEngine.AI;

namespace DifficultyTweak.Patches
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

            while(currentPosition != targetPosition)
            {
                GameObject gameObject = GameObject.Instantiate(decoy, currentPosition, instance.transform.rotation);
                gameObject.SetActive(true);
                Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
                AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
                componentInChildren.Play(currentAnimatorStateInfo.shortNameHash, 0, currentAnimatorStateInfo.normalizedTime);
                componentInChildren.speed = 0f;

                currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, deltaDistance);
            }
        }

        static void Postfix(MinosPrime __instance, Animator ___anim)
        {
            string stateName = ___anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (stateName == "Combo")
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
}
