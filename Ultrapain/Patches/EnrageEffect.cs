using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class EnrageEffect_Start
    {
        static void Postfix(EnrageEffect __instance)
        {
            AudioSource enrageAud = __instance.gameObject.GetComponents<AudioSource>().Where(src => src.loop).First();
            if (enrageAud.isPlaying)
                enrageAud.Stop();
            enrageAud.clip = Plugin.enrageAudioCustom;
            enrageAud.Play();
        }
    }
}
