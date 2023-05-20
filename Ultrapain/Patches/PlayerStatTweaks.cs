using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    /*
    u = initial, f = final, d = delta, s = speed multiplier

    u = 40f * Time.deltaTime
    f = 40f * S * Time.deltaTime
    d = 40f * Time.deltaTime * (S - 1)
    revCharge += 40f * Time.deltaTime * (S - 1f) * (alt ? 0.5f : 1f)
     */

    class Revolver_Update
    {
        static bool Prefix(Revolver __instance)
        {
            if(__instance.gunVariation == 0 && __instance.pierceCharge < 100f)
            {
                __instance.pierceCharge = Mathf.Min(100f, __instance.pierceCharge + 40f * Time.deltaTime * (ConfigManager.chargedRevRegSpeedMulti.value - 1f) * (__instance.altVersion ? 0.5f : 1f));
            }

            return true;
        }
    }

    class NailGun_Update
    {
        static bool Prefix(Nailgun __instance, ref float ___heatSinks)
        {
            if(__instance.variation == 0)
            {
                float maxSinks = (__instance.altVersion ? 1f : 2f);
                float multi = (__instance.altVersion ? ConfigManager.sawHeatsinkRegSpeedMulti.value : ConfigManager.nailgunHeatsinkRegSpeedMulti.value);
                float rate = 0.125f;

                if (___heatSinks < maxSinks && multi != 1)
                    ___heatSinks = Mathf.Min(maxSinks, ___heatSinks + Time.deltaTime * rate * (multi - 1f));
            }

            return true;
        }
    }

    class NewMovement_Update
    {
        static bool Prefix(NewMovement __instance, int ___difficulty)
        {
            if (__instance.boostCharge < 300f && !__instance.sliding && !__instance.slowMode)
            {
                float multi = 1f;
                if (___difficulty == 1)
                    multi = 1.5f;
                else if (___difficulty == 0f)
                    multi = 2f;

                __instance.boostCharge = Mathf.Min(300f, __instance.boostCharge + Time.deltaTime * 70f * multi * (ConfigManager.staminaRegSpeedMulti.value - 1f));
            }

            return true;
        }
    }

    class WeaponCharges_Charge
    {
        static bool Prefix(WeaponCharges __instance, float __0)
        {
            if (__instance.rev1charge < 400f)
                __instance.rev1charge = Mathf.Min(400f, __instance.rev1charge + 25f * __0 * (ConfigManager.coinRegSpeedMulti.value - 1f));
            if (__instance.rev2charge < 300f)
                __instance.rev2charge = Mathf.Min(300f, __instance.rev2charge + (__instance.rev2alt ? 35f : 15f) * __0 * (ConfigManager.sharpshooterRegSpeedMulti.value - 1f));

            if(!__instance.naiAmmoDontCharge)
            {
                if (__instance.naiAmmo < 100f)
                    __instance.naiAmmo = Mathf.Min(100f, __instance.naiAmmo + __0 * 3.5f * (ConfigManager.nailgunAmmoRegSpeedMulti.value - 1f)); ;
                if (__instance.naiSaws < 10f)
                    __instance.naiSaws = Mathf.Min(10f, __instance.naiSaws + __0 * 0.5f * (ConfigManager.sawAmmoRegSpeedMulti.value - 1f));
            }

            if (__instance.raicharge < 5f)
                __instance.raicharge = Mathf.Min(5f, __instance.raicharge + __0 * 0.25f * (ConfigManager.railcannonRegSpeedMulti.value - 1f));
            if (!__instance.rocketFrozen && __instance.rocketFreezeTime < 5f)
                __instance.rocketFreezeTime = Mathf.Min(5f, __instance.rocketFreezeTime + __0 * 0.5f * (ConfigManager.rocketFreezeRegSpeedMulti.value - 1f));
            if (__instance.rocketCannonballCharge < 1f)
                __instance.rocketCannonballCharge = Mathf.Min(1f, __instance.rocketCannonballCharge + __0 * 0.125f * (ConfigManager.rocketCannonballRegSpeedMulti.value - 1f));

            return true;
        }
    }
}
