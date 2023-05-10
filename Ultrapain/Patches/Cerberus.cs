using UnityEngine;

namespace Ultrapain.Patches
{
    class CerberusFlag : MonoBehaviour
    {
        public int extraDashesRemaining = ConfigManager.cerberusTotalDashCount.value - 1;
        public Transform head;
        public float lastParryTime;
        private EnemyIdentifier eid;

        private void Awake()
        {
            eid = GetComponent<EnemyIdentifier>();
            head = transform.Find("Armature/Control/Waist/Chest/Chest_001/Head");
            if (head == null)
                head = UnityUtils.GetChildByTagRecursively(transform, "Head");
        }

        public void MakeParryable()
        {
            lastParryTime = Time.time;
            GameObject flash = GameObject.Instantiate(Plugin.parryableFlash, head.transform.position, head.transform.rotation, head);
            flash.transform.LookAt(CameraController.Instance.transform);
            flash.transform.position += flash.transform.forward;
            flash.transform.Rotate(Vector3.up, 90, Space.Self);
        }
    }

    class StatueBoss_StopTracking_Patch
    {
        static void Postfix(StatueBoss __instance, Animator ___anim)
        {
            CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
            if (flag == null)
                return;

            if (___anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Tackle")
                return;

            flag.MakeParryable();
        }
    }

    class StatueBoss_Stomp_Patch
    {
        static void Postfix(StatueBoss __instance)
        {
            CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
            if (flag == null)
                return;

            flag.MakeParryable();
        }
    }

    class Statue_GetHurt_Patch
    {
        static bool Prefix(Statue __instance, EnemyIdentifier ___eid)
        {
            CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
            if (flag == null)
                return true;

            if (___eid.hitter != "punch" && ___eid.hitter != "shotgunzone")
                return true;

            float deltaTime = Time.time - flag.lastParryTime;
            if (deltaTime > 0.5f / ___eid.totalSpeedModifier)
                return true;

            flag.lastParryTime = 0;
            ___eid.health -= 5;
            MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, ___eid);
            return true;
        }
    }

    class StatueBoss_StopDash_Patch
    {
        public static void Postfix(StatueBoss __instance, ref int ___tackleChance)
        {
            CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
            if (flag == null)
                return;

            if (flag.extraDashesRemaining > 0)
            {
                flag.extraDashesRemaining -= 1;
                __instance.SendMessage("Tackle");
                ___tackleChance -= 20;
            }
            else
                flag.extraDashesRemaining = ConfigManager.cerberusTotalDashCount.value - 1;
        }
    }

    class StatueBoss_Start_Patch
    {
        static void Postfix(StatueBoss __instance)
        {
            __instance.gameObject.AddComponent<CerberusFlag>();
        }
    }
}
