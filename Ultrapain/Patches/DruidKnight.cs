using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace Ultrapain.Patches
{
    class DruidKnight_FullBurst
    {
        public static AudioMixer mixer;
        public static float offset = 0.205f;

        class StateInfo
        {
            public GameObject oldProj;
            public GameObject tempProj;
        }

        static bool Prefix(Mandalore __instance, out StateInfo __state)
        {
            __state = new StateInfo() { oldProj = __instance.fullAutoProjectile };

            GameObject obj = new GameObject();
            obj.transform.position = __instance.transform.position;
            AudioSource aud = obj.AddComponent<AudioSource>();
            aud.playOnAwake = false;
            aud.clip = Plugin.druidKnightFullAutoAud;
            aud.time = offset;
            aud.Play();

            GameObject proj = GameObject.Instantiate(__instance.fullAutoProjectile, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
            proj.GetComponent<AudioSource>().enabled = false;
            __state.tempProj = __instance.fullAutoProjectile = proj;

            return true;
        }

        static void Postfix(Mandalore __instance, StateInfo __state)
        {
            __instance.fullAutoProjectile = __state.oldProj;
            if (__state.tempProj != null)
                GameObject.Destroy(__state.tempProj);
        }
    }

    class DruidKnight_FullerBurst
    {
        public static float offset = 0.5f;

        static bool Prefix(Mandalore __instance, int ___shotsLeft)
        {
            if (___shotsLeft != 40)
                return true;

            GameObject obj = new GameObject();
            obj.transform.position = __instance.transform.position;
            AudioSource aud = obj.AddComponent<AudioSource>();
            aud.playOnAwake = false;
            aud.clip = Plugin.druidKnightFullerAutoAud;
            aud.time = offset;
            aud.Play();
            return true;
        }
    }

    class Drone_Explode
    {
        static bool Prefix(bool ___exploded, out bool __state)
        {
            __state = ___exploded;
            return true;
        }

        public static float offset = 0.2f;
        static void Postfix(Drone __instance, bool ___exploded, bool __state)
        {
            if (__state)
                return;

            if (!___exploded || __instance.gameObject.GetComponent<Mandalore>() == null)
                return;

            GameObject obj = new GameObject();
            obj.transform.position = __instance.transform.position;
            AudioSource aud = obj.AddComponent<AudioSource>();
            aud.playOnAwake = false;
            aud.clip = Plugin.druidKnightDeathAud;
            aud.time = offset;
            aud.Play();
        }
    }
}
