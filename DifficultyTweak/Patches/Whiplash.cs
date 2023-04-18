using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(HookArm), "FixedUpdate")]
    class HookArm_FixedUpdate_Patch
    {
        static bool Prefix(HookArm __instance, ref Grenade ___caughtGrenade, ref Vector3 ___caughtPoint, ref Vector3 ___hookPoint, ref float ___cooldown, ref List<Rigidbody> ___caughtObjects)
        {
            if (___caughtGrenade != null && ___caughtGrenade.rocket && !___caughtGrenade.playerRiding && MonoSingleton<WeaponCharges>.Instance.rocketFrozen)
            {
                if (__instance.state == HookState.Throwing)
                {
                    if (!MonoSingleton<InputManager>.Instance.InputSource.Hook.IsPressed && (___cooldown <= 0.1f || ___caughtObjects.Count > 0))
                    {
                        __instance.StopThrow(0f, false);
                    }

                    return false;
                }
                else if (__instance.state == HookState.Ready)
                {
                    if (MonoSingleton<NewMovement>.Instance.boost || MonoSingleton<NewMovement>.Instance.sliding)
                        return true;

                    ___hookPoint = ___caughtGrenade.transform.position + ___caughtPoint; //__instance.caughtTransform.position + __instance.caughtPoint;
                    __instance.beingPulled = true;
                    MonoSingleton<NewMovement>.Instance.rb.velocity = (/*___hookPoint*/___caughtGrenade.transform.position - MonoSingleton<NewMovement>.Instance.transform.position).normalized * 60f;
                    if (MonoSingleton<NewMovement>.Instance.gc.onGround)
                        MonoSingleton<NewMovement>.Instance.rb.MovePosition(MonoSingleton<NewMovement>.Instance.transform.position + Vector3.up);
                    return false;
                }
            }

            return true;
        }
    }
}
