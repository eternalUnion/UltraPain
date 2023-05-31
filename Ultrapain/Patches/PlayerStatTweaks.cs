using HarmonyLib;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

    public class Revolver_Shoot
    {
        public static void RevolverBeamEdit(RevolverBeam beam)
        {
            beam.damage -= beam.strongAlt ? 1.25f : 1f;
            beam.damage += beam.strongAlt ? ConfigManager.revolverAltDamage.value : ConfigManager.revolverDamage.value;
        }

        public static void RevolverBeamSuperEdit(RevolverBeam beam)
        {
            if (beam.gunVariation == 0)
            {
                beam.damage -= beam.strongAlt ? 1.25f : 1f;
                beam.damage += beam.strongAlt ? ConfigManager.chargedAltRevDamage.value : ConfigManager.chargedRevDamage.value;
                beam.hitAmount = beam.strongAlt ? ConfigManager.chargedAltRevTotalHits.value : ConfigManager.chargedRevTotalHits.value;
                beam.maxHitsPerTarget = beam.strongAlt ? ConfigManager.chargedAltRevMaxHitsPerTarget.value : ConfigManager.chargedRevMaxHitsPerTarget.value;
            }
            else if (beam.gunVariation == 2)
            {
                beam.damage -= beam.strongAlt ? 1.25f : 1f;
                beam.damage += beam.strongAlt ? ConfigManager.sharpshooterAltDamage.value : ConfigManager.sharpshooterDamage.value;
                beam.maxHitsPerTarget = beam.strongAlt ? ConfigManager.sharpshooterAltMaxHitsPerTarget.value : ConfigManager.sharpshooterMaxHitsPerTarget.value;
            }
        }

        static FieldInfo f_RevolverBeam_gunVariation = typeof(RevolverBeam).GetField("gunVariation", UnityUtils.instanceFlag);
        static MethodInfo m_Revolver_Shoot_RevolverBeamEdit = typeof(Revolver_Shoot).GetMethod("RevolverBeamEdit", UnityUtils.staticFlag);
        static MethodInfo m_Revolver_Shoot_RevolverBeamSuperEdit = typeof(Revolver_Shoot).GetMethod("RevolverBeamSuperEdit", UnityUtils.staticFlag);
        static MethodInfo m_GameObject_GetComponent_RevolverBeam = typeof(GameObject).GetMethod("GetComponent", new Type[0], new ParameterModifier[0]).MakeGenericMethod(new Type[1] { typeof(RevolverBeam) });

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            object normalBeamLocalIndex = null;
            object superBeamLocalIndex = null;

            // Get local indexes of components for RevolverBeam references
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt && code[i].OperandIs(m_GameObject_GetComponent_RevolverBeam))
                {
                    object localIndex = ILUtils.GetLocalIndex(code[i + 1]);
                    if (localIndex == null)
                        continue;

                    if (normalBeamLocalIndex == null)
                    {
                        normalBeamLocalIndex = localIndex;
                    }
                    else
                    {
                        superBeamLocalIndex = localIndex;
                        break;
                    }
                }
            }

            Debug.Log($"Normal beam index: {normalBeamLocalIndex}");
            Debug.Log($"Super beam index: {superBeamLocalIndex}");

            // Modify normal beam
            for (int i = 3; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Stfld && code[i].OperandIs(f_RevolverBeam_gunVariation))
                {
                    object localIndex = ILUtils.GetLocalIndex(code[i - 3]);
                    if (localIndex == null)
                        continue;

                    if (localIndex.Equals(normalBeamLocalIndex))
                    {
                        Debug.Log($"Patching normal beam");

                        i += 1;
                        code.Insert(i, ILUtils.LoadLocalInstruction(localIndex));
                        i += 1;
                        code.Insert(i, new CodeInstruction(OpCodes.Call, m_Revolver_Shoot_RevolverBeamEdit));
                        break;
                    }
                }
            }

            // Modify super beam
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Stfld && code[i].OperandIs(f_RevolverBeam_gunVariation))
                {
                    object localIndex = ILUtils.GetLocalIndex(code[i - 3]);
                    if (localIndex == null)
                        continue;

                    if (localIndex.Equals(superBeamLocalIndex))
                    {
                        Debug.Log($"Patching super beam");

                        i += 1;
                        code.Insert(i, ILUtils.LoadLocalInstruction(localIndex));
                        i += 1;
                        code.Insert(i, new CodeInstruction(OpCodes.Call, m_Revolver_Shoot_RevolverBeamSuperEdit));
                        break;
                    }
                }
            }

            return code.AsEnumerable();
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

    class NewMovement_GetHurt
    {
        static bool Prefix(NewMovement __instance, out float __state)
        {
            __state = __instance.antiHp;
            return true;
        }

        static void Postfix(NewMovement __instance, float __state)
        {
            float deltaAnti = __instance.antiHp - __state;
            if (deltaAnti <= 0)
                return;

            deltaAnti *= ConfigManager.hardDamagePercent.normalizedValue;
            __instance.antiHp = __state + deltaAnti;
        }

        static FieldInfo hpField = typeof(NewMovement).GetField("hp");

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && (FieldInfo)code[i].operand == hpField)
                {
                    i += 1;
                    if (code[i].opcode == OpCodes.Ldc_I4_S)
                    {
                        code[i] = new CodeInstruction(OpCodes.Ldc_I4, (Int32)ConfigManager.maxPlayerHp.value);
                    }
                }
                else if (code[i].opcode == OpCodes.Ldc_R4 && (Single)code[i].operand == (Single)99f)
                {
                    code[i] = new CodeInstruction(OpCodes.Ldc_R4, (Single)(ConfigManager.maxPlayerHp.value - 1));
                }
            }

            return code.AsEnumerable();
        }
    }

    class HookArm_FixedUpdate
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && (Single)code[i].operand == 66f)
                {
                    code[i] = new CodeInstruction(OpCodes.Ldc_R4, (Single)(66f * (ConfigManager.maxPlayerHp.value / 100f) * ConfigManager.whiplashHardDamageSpeed.value));
                }
                else if (code[i].opcode == OpCodes.Ldc_R4 && (Single)code[i].operand == 50f)
                {
                    code[i] = new CodeInstruction(OpCodes.Ldc_R4, (Single)(ConfigManager.whiplashHardDamageCap.value));
                }
            }

            return code.AsEnumerable();
        }
    }

    class NewMovement_ForceAntiHP
    {
        static FieldInfo hpField = typeof(NewMovement).GetField("hp");

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && (FieldInfo)code[i].operand == hpField)
                {
                    i += 1;
                    if (i < code.Count && code[i].opcode == OpCodes.Ldc_I4_S && (SByte)code[i].operand == (SByte)100)
                    {
                        code[i] = new CodeInstruction(OpCodes.Ldc_I4, (Int32)ConfigManager.maxPlayerHp.value);
                    }
                }
                else if (code[i].opcode == OpCodes.Ldarg_1)
                {
                    i += 2;
                    if (i < code.Count && code[i].opcode == OpCodes.Ldc_R4 && (Single)code[i].operand == 99f)
                    {
                        code[i] = new CodeInstruction(OpCodes.Ldc_R4, (Single)(ConfigManager.maxPlayerHp.value - 1));
                    }
                }
                else if (code[i].opcode == OpCodes.Ldc_R4 && (Single)code[i].operand == 100f)
                {
                    code[i] = new CodeInstruction(OpCodes.Ldc_R4, (Single)ConfigManager.maxPlayerHp.value);
                }
                else if (code[i].opcode == OpCodes.Ldc_R4 && (Single)code[i].operand == 50f)
                {
                    code[i] = new CodeInstruction(OpCodes.Ldc_R4, (Single)ConfigManager.maxPlayerHp.value / 2);
                }
                else if (code[i].opcode == OpCodes.Ldc_I4_S && (SByte)code[i].operand == (SByte)100)
                {
                    code[i] = new CodeInstruction(OpCodes.Ldc_I4, (Int32)ConfigManager.maxPlayerHp.value);
                }
            }

            return code.AsEnumerable();
        }
    }

    class NewMovement_GetHealth
    {
        static bool Prefix(NewMovement __instance, int __0, bool __1, ref AudioSource ___greenHpAud, Canvas ___fullHud)
        {
            if (__instance.dead || __instance.exploded)
                return false;

            int maxHp = Mathf.RoundToInt(ConfigManager.maxPlayerHp.value - __instance.antiHp);
            int maxDelta = maxHp - __instance.hp;
            if (maxDelta <= 0)
                return true;

            if (!__1 && __0 > 5 && MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled", false))
            {
                GameObject.Instantiate<GameObject>(__instance.scrnBlood, ___fullHud.transform);
            }

            __instance.hp = Mathf.Min(maxHp, __instance.hp + __0);
            __instance.hpFlash.Flash(1f);

            if (!__1 && __0 > 5)
            {
                if (___greenHpAud == null)
                {
                    ___greenHpAud = __instance.hpFlash.GetComponent<AudioSource>();
                }
                ___greenHpAud.Play();
            }

            return false;
        }
    }

    class NewMovement_SuperCharge
    {
        static bool Prefix(NewMovement __instance)
        {
            __instance.hp = Mathf.Max(ConfigManager.maxPlayerHp.value, ConfigManager.playerHpSupercharge.value);
            return false;
        }
    }

    class NewMovement_Respawn
    {
        static void Postfix(NewMovement __instance)
        {
            __instance.hp = ConfigManager.maxPlayerHp.value;
        }
    }

    class NewMovement_Start
    {
        static void Postfix(NewMovement __instance)
        {
            __instance.hp = ConfigManager.maxPlayerHp.value;
        }
    }

    class HealthBarTracker : MonoBehaviour
    {
        public static List<HealthBarTracker> instances = new List<HealthBarTracker>();
        private HealthBar hb;

        private void Awake()
        {
            if (hb == null)
                hb = GetComponent<HealthBar>();

            instances.Add(this);

            for (int i = instances.Count - 1; i >= 0; i--)
            {
                if (instances[i] == null)
                    instances.RemoveAt(i);
            }
        }

        private void OnDestroy()
        {
            if (instances.Contains(this))
                instances.Remove(this);
        }

        public void SetSliderRange()
        {
            if (hb == null)
                hb = GetComponent<HealthBar>();

            hb.hpSliders[0].maxValue = hb.afterImageSliders[0].maxValue = ConfigManager.maxPlayerHp.value;
            hb.hpSliders[1].minValue = hb.afterImageSliders[1].minValue = ConfigManager.maxPlayerHp.value;
            hb.hpSliders[1].maxValue = hb.afterImageSliders[1].maxValue = Mathf.Max(ConfigManager.maxPlayerHp.value, ConfigManager.playerHpSupercharge.value);
            hb.antiHpSlider.maxValue = ConfigManager.maxPlayerHp.value;
        }
    }

    class HealthBar_Start
    {
        static void Postfix(HealthBar __instance)
        {
            __instance.gameObject.AddComponent<HealthBarTracker>().SetSliderRange();
        }
    }
}
