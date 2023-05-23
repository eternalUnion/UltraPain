using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class DeathZone_Start
    {
        internal static GameObject _pitDeahtObj;
        internal static GameObject pitDeahtObj
        {
            get
            {
                if (!ConfigManager.levelDeathPitAud.value)
                    return null;

                if(_pitDeahtObj == null)
                {
                    _pitDeahtObj = new GameObject();
                    _pitDeahtObj.transform.position = new Vector3(1000000, 1000000, 1000000);

                    AudioSource src = _pitDeahtObj.AddComponent<AudioSource>();
                    src.playOnAwake = true;
                    src.clip = Plugin.deathPitFallAud;

                    RemoveOnTime remover = _pitDeahtObj.AddComponent<RemoveOnTime>();
                    remover.enabled = false;
                    remover.time = src.clip.length + 1;

                    CommonActivator activator = _pitDeahtObj.AddComponent<CommonActivator>();
                    activator.comps.Add(remover);
                }

                return _pitDeahtObj;
            }
        }

        static void Postfix(DeathZone __instance, bool ___playerAffected)
        {
            if (___playerAffected)
            {
                __instance.notInstakill = false;
                if (Plugin.deathPitFallAud != null)
                {
                    __instance.sawSound = pitDeahtObj;
                }
            }
        }
    }

    class DeathZone_GotHit
    {
        static bool Prefix(DeathZone __instance, bool ___playerAffected)
        {
            if (NewMovement.Instance.hp <= 0)
            {
                if (__instance.sawSound == DeathZone_Start.pitDeahtObj)
                    __instance.sawSound = null;
            }
            else
            {
                if (___playerAffected && !__instance.notInstakill && __instance.sawSound == null)
                    __instance.sawSound = DeathZone_Start.pitDeahtObj;
            }

            return true;
        }
    }
}
