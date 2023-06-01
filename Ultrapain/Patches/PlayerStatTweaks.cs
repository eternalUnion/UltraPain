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

    public class Shotgun_Shoot
    {
        public static void ModifyShotgunPellet(Projectile proj, Shotgun shotgun, int primaryCharge)
        {
            if (shotgun.variation == 0)
            {
                proj.damage = ConfigManager.shotgunBlueDamagePerPellet.value;
            }
            else
            {
                if (primaryCharge == 0)
                    proj.damage = ConfigManager.shotgunGreenPump1Damage.value;
                else if (primaryCharge == 1)
                    proj.damage = ConfigManager.shotgunGreenPump2Damage.value;
                else if (primaryCharge == 2)
                    proj.damage = ConfigManager.shotgunGreenPump3Damage.value;
            }
        }

        public static void ModifyPumpExplosion(Explosion exp)
        {
            exp.damage = ConfigManager.shotgunGreenExplosionDamage.value;
            exp.playerDamageOverride = ConfigManager.shotgunGreenExplosionPlayerDamage.value;
            float sizeMulti = ConfigManager.shotgunGreenExplosionSize.value / 9f;
            exp.maxSize *= sizeMulti;
            exp.speed *= sizeMulti;
            exp.speed *= ConfigManager.shotgunGreenExplosionSpeed.value;
        }

        static MethodInfo m_GameObject_GetComponent_Projectile = typeof(GameObject).GetMethod("GetComponent", new Type[0], new ParameterModifier[0]).MakeGenericMethod(new Type[1] { typeof(Projectile) });
        static MethodInfo m_GameObject_GetComponentsInChildren_Explosion = typeof(GameObject).GetMethod("GetComponentsInChildren", new Type[0], new ParameterModifier[0]).MakeGenericMethod(new Type[1] { typeof(Explosion) });
        static MethodInfo m_Shotgun_Shoot_ModifyShotgunPellet = typeof(Shotgun_Shoot).GetMethod("ModifyShotgunPellet", UnityUtils.staticFlag);
        static MethodInfo m_Shotgun_Shoot_ModifyPumpExplosion = typeof(Shotgun_Shoot).GetMethod("ModifyPumpExplosion", UnityUtils.staticFlag);
        static FieldInfo f_Shotgun_primaryCharge = typeof(Shotgun).GetField("primaryCharge", UnityUtils.instanceFlag);
        static FieldInfo f_Explosion_damage = typeof(Explosion).GetField("damage", UnityUtils.instanceFlag);
        
        static bool Prefix(Shotgun __instance, int ___primaryCharge)
        {
            if (__instance.variation == 0)
            {
                __instance.spread = ConfigManager.shotgunBlueSpreadAngle.value;
            }
            else
            {
                if (___primaryCharge == 0)
                    __instance.spread = ConfigManager.shotgunGreenPump1Spread.value * 1.5f;
                else if (___primaryCharge == 1)
                    __instance.spread = ConfigManager.shotgunGreenPump2Spread.value;
                else if (___primaryCharge == 2)
                    __instance.spread = ConfigManager.shotgunGreenPump3Spread.value / 2f;
            }

            return true;
        }

        static void Postfix(Shotgun __instance)
        {
            __instance.spread = 10f;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            CodeInstruction pelletStoreInst = new CodeInstruction(OpCodes.Stloc_0);
            int pelletCodeIndex = 0;

            // Find pellet local variable index
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_I4_S && code[i].OperandIs(12))
                {
                    if (ConfigManager.shotgunBluePelletCount.value > sbyte.MaxValue)
                        code[i].opcode = OpCodes.Ldc_I4;
                    code[i].operand = ConfigManager.shotgunBluePelletCount.value;
                    i += 1;
                    pelletCodeIndex = i;
                    pelletStoreInst = code[i];
                    break;
                }
            }

            // Debug.Log($"Pellet store instruction: {ILUtils.TurnInstToString(pelletStoreInst)}");

            // Modify pellet counts
            for (int i = pelletCodeIndex + 1; i < code.Count; i++)
            {
                if (code[i].opcode == pelletStoreInst.opcode
                    && (pelletStoreInst.operand == null ? true : pelletStoreInst.operand.Equals(code[i].operand))
                    && ILUtils.IsConstI4LoadWithOperand(code[i - 1].opcode))
                {
                    int constIndex = i - 1;
                    int pelletCount = ILUtils.GetI4LoadOperand(code[constIndex]);

                    if (pelletCount == 10)
                        pelletCount = ConfigManager.shotgunGreenPump1Count.value;
                    else if (pelletCount == 16)
                        pelletCount = ConfigManager.shotgunGreenPump2Count.value;
                    else if (pelletCount == 24)
                        pelletCount = ConfigManager.shotgunGreenPump3Count.value;

                    if (ILUtils.TryEfficientLoadI4(pelletCount, out OpCode efficientOpcode))
                    {
                        code[constIndex].operand = null;
                        code[constIndex].opcode = efficientOpcode;
                    }
                    else
                    {
                        if (pelletCount > sbyte.MaxValue)
                            code[constIndex].opcode = OpCodes.Ldc_I4;
                        else
                            code[constIndex].opcode = OpCodes.Ldc_I4_S;
                        code[constIndex].operand = pelletCount;
                    }
                }
            }

            // Modify projectile damage
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt && code[i].OperandIs(m_GameObject_GetComponent_Projectile))
                {
                    i += 1;
                    // Duplicate component (arg 0)
                    code.Insert(i, new CodeInstruction(OpCodes.Dup));
                    i += 1;
                    // Add instance to stack (arg 1)
                    code.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 1;
                    // Load instance then get primary field (arg 2)
                    code.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 1;
                    code.Insert(i, new CodeInstruction(OpCodes.Ldfld, f_Shotgun_primaryCharge));
                    i += 1;
                    // Call the static method
                    code.Insert(i, new CodeInstruction(OpCodes.Call, m_Shotgun_Shoot_ModifyShotgunPellet));

                    break;
                }
            }

            // Modify pump explosion
            int pumpExplosionIndex = 0;
            while (code[pumpExplosionIndex].opcode != OpCodes.Callvirt && !code[pumpExplosionIndex].OperandIs(m_GameObject_GetComponentsInChildren_Explosion))
                pumpExplosionIndex += 1;

            for (int i = pumpExplosionIndex; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Stfld)
                {
                    if (code[i].OperandIs(f_Explosion_damage))
                    {
                        // Duplicate before damage assignment
                        code.Insert(i - 1, new CodeInstruction(OpCodes.Dup));
                        i += 2;

                        // Argument 0 already loaded, call the method
                        code.Insert(i, new CodeInstruction(OpCodes.Call, m_Shotgun_Shoot_ModifyPumpExplosion));

                        // Stack is now clear

                        break;
                    }
                }
            }

            return code.AsEnumerable();
        }
    }

    // Core eject
    class Shotgun_ShootSinks
    {
        public static void ModifyCoreEject(GameObject core)
        {
            GrenadeExplosionOverride ovr = core.AddComponent<GrenadeExplosionOverride>();

            ovr.normalMod = true;
            ovr.normalDamage = (float)ConfigManager.shotgunCoreExplosionDamage.value / 35f;
            ovr.normalSize = (float)ConfigManager.shotgunCoreExplosionSize.value / 6f * ConfigManager.shotgunCoreExplosionSpeed.value;
            ovr.normalPlayerDamageOverride = ConfigManager.shotgunCoreExplosionPlayerDamage.value;

            ovr.superMod = true;
            ovr.superDamage = (float)ConfigManager.shotgunCoreExplosionDamage.value / 35f;
            ovr.superSize = (float)ConfigManager.shotgunCoreExplosionSize.value / 6f * ConfigManager.shotgunCoreExplosionSpeed.value;
            ovr.superPlayerDamageOverride = ConfigManager.shotgunCoreExplosionPlayerDamage.value;
        }

        static FieldInfo f_Grenade_sourceWeapon = typeof(Grenade).GetField("sourceWeapon", UnityUtils.instanceFlag);
        static MethodInfo m_Shotgun_ShootSinks_ModifyCoreEject = typeof(Shotgun_ShootSinks).GetMethod("ModifyCoreEject", UnityUtils.staticFlag);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Stfld && code[i].OperandIs(f_Grenade_sourceWeapon))
                {
                    i += 1;

                    // Add arg 0
                    code.Insert(i, new CodeInstruction(OpCodes.Dup));
                    i += 1;
                    // Call mod method
                    code.Insert(i, new CodeInstruction(OpCodes.Call, m_Shotgun_ShootSinks_ModifyCoreEject));

                    break;
                }
            }

            return code.AsEnumerable();
        }
    }

    class Nailgun_Shoot
    {
        static FieldInfo f_Nailgun_heatSinks = typeof(Nailgun).GetField("heatSinks", UnityUtils.instanceFlag);
        static FieldInfo f_Nailgun_heatUp = typeof(Nailgun).GetField("heatUp", UnityUtils.instanceFlag);
        
        public static void ModifyNail(Nailgun inst, GameObject nail)
        {
            Nail comp = nail.GetComponent<Nail>();

            if (inst.altVersion)
            {
                // Blue saw launcher
                if (inst.variation == 1)
                {
                    comp.damage = ConfigManager.sawBlueDamage.value;
                    comp.hitAmount = ConfigManager.sawBlueHitAmount.value;
                }
                // Green saw launcher
                else
                {
                    comp.damage = ConfigManager.sawGreenDamage.value;
                    float maxHit = ConfigManager.sawGreenHitAmount.value;
                    float heatSinks = (float)f_Nailgun_heatSinks.GetValue(inst);
                    float heatUp = (float)f_Nailgun_heatUp.GetValue(inst);

                    if (heatSinks >= 1)
                        comp.hitAmount = Mathf.Lerp(maxHit, Mathf.Max(1f, maxHit), (maxHit - 2f) * heatUp);
                    else
                        comp.hitAmount = 1f;
                }
            }
            else
            {
                // Blue nailgun
                if (inst.variation == 1)
                {
                    comp.damage = ConfigManager.nailgunBlueDamage.value;
                }
                else
                {
                    if (comp.heated)
                        comp.damage = ConfigManager.nailgunGreenBurningDamage.value;
                    else
                        comp.damage = ConfigManager.nailgunGreenDamage.value;
                }
            }
        }

        static FieldInfo f_Nailgun_nail = typeof(Nailgun).GetField("nail", UnityUtils.instanceFlag);
        static MethodInfo m_Nailgun_Shoot_ModifyNail = typeof(Nailgun_Shoot).GetMethod("ModifyNail", UnityUtils.staticFlag);
        static MethodInfo m_Transform_set_forward = typeof(Transform).GetProperty("forward", UnityUtils.instanceFlag).GetSetMethod();

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            CodeInstruction localObjectStoreInst = null;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].OperandIs(f_Nailgun_nail))
                {
                    for (; i < code.Count; i++)
                        if (ILUtils.IsStoreLocalOpcode(code[i].opcode))
                            break;

                    localObjectStoreInst = code[i];
                }
            }

            Debug.Log($"Nail local reference: {ILUtils.TurnInstToString(localObjectStoreInst)}");

            int insertIndex = 0;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt && code[i].OperandIs(m_Transform_set_forward))
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            // Push instance reference
            code.Insert(insertIndex, new CodeInstruction(OpCodes.Ldarg_0));
            insertIndex += 1;
            // Push local nail object
            code.Insert(insertIndex, new CodeInstruction(ILUtils.GetLoadLocalFromStoreLocal(localObjectStoreInst.opcode), localObjectStoreInst.operand));
            insertIndex += 1;
            // Call the method
            code.Insert(insertIndex, new CodeInstruction(OpCodes.Call, m_Nailgun_Shoot_ModifyNail));

            return code.AsEnumerable();
        }
    }

    class Nailgun_SuperSaw
    {
        public static void ModifySupersaw(GameObject supersaw)
        {
            Nail saw = supersaw.GetComponent<Nail>();

            saw.damage = ConfigManager.sawGreenBurningDamage.value;
            saw.hitAmount = ConfigManager.sawGreenBurningHitAmount.value;
        }

        static FieldInfo f_Nailgun_heatedNail = typeof(Nailgun).GetField("heatedNail", UnityUtils.instanceFlag);
        static MethodInfo m_Nailgun_SuperSaw_ModifySupersaw = typeof(Nailgun_SuperSaw).GetMethod("ModifySupersaw", UnityUtils.staticFlag);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            CodeInstruction localObjectStoreInst = null;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].OperandIs(f_Nailgun_heatedNail))
                {
                    for (; i < code.Count; i++)
                        if (ILUtils.IsStoreLocalOpcode(code[i].opcode))
                            break;

                    localObjectStoreInst = code[i];
                }
            }

            Debug.Log($"Supersaw local reference: {ILUtils.TurnInstToString(localObjectStoreInst)}");

            int insertIndex = code.Count - 1;

            // Push local nail object
            code.Insert(insertIndex, new CodeInstruction(ILUtils.GetLoadLocalFromStoreLocal(localObjectStoreInst.opcode), localObjectStoreInst.operand));
            insertIndex += 1;
            // Call the method
            code.Insert(insertIndex, new CodeInstruction(OpCodes.Call, m_Nailgun_SuperSaw_ModifySupersaw));

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

    class NewMovement_DeltaHpComp : MonoBehaviour
    {
        public static NewMovement_DeltaHpComp instance;
        private NewMovement player;
        private AudioSource hurtAud;

        private bool levelMap = false;

        private void Awake()
        {
            instance = this;
            player = NewMovement.Instance;
            hurtAud = player.hurtScreen.GetComponent<AudioSource>();
            levelMap = SceneHelper.CurrentLevelNumber > 0;

            UpdateEnabled();
        }

        public void UpdateEnabled()
        {
            if (!ConfigManager.playerHpDeltaToggle.value)
                enabled = false;

            if (SceneHelper.CurrentScene == "uk_construct")
                enabled = ConfigManager.playerHpDeltaSandbox.value;
            else if (SceneHelper.CurrentScene == "Endless")
                enabled = ConfigManager.playerHpDeltaCybergrind.value;
            else
            {
                enabled = SceneHelper.CurrentLevelNumber > 0;
            }
        }

        public void ResetCooldown()
        {
            deltaCooldown = ConfigManager.playerHpDeltaDelay.value;
        }

        public float deltaCooldown = ConfigManager.playerHpDeltaDelay.value;
        public void Update()
        {
            if (player.dead || !ConfigManager.playerHpDeltaToggle.value || !StatsManager.Instance.timer)
            {
                ResetCooldown();
                return;
            }

            if (levelMap)
            {
                // Calm
                if (MusicManager.Instance.requestedThemes == 0)
                {
                    if (!ConfigManager.playerHpDeltaCalm.value)
                    {
                        ResetCooldown();
                        return;
                    }
                }
                // Combat
                else
                {
                    if (!ConfigManager.playerHpDeltaCombat.value)
                    {
                        ResetCooldown();
                        return;
                    }

                }
            }

            deltaCooldown = Mathf.MoveTowards(deltaCooldown, 0f, Time.deltaTime);
            if (deltaCooldown == 0f)
            {
                ResetCooldown();
                int deltaHp = ConfigManager.playerHpDeltaAmount.value;
                int limit = ConfigManager.playerHpDeltaLimit.value;

                if (deltaHp == 0)
                    return;

                if (deltaHp > 0)
                {
                    if (player.hp > limit)
                        return;

                    player.GetHealth(deltaHp, true);
                }
                else
                {
                    if (player.hp < limit)
                        return;

                    if (player.hp - deltaHp <= 0)
                        player.GetHurt(-deltaHp, false, 0, false, false);
                    else
                    {
                        player.hp += deltaHp;
                        if (ConfigManager.playerHpDeltaHurtAudio.value)
                        {
                            hurtAud.pitch = UnityEngine.Random.Range(0.8f, 1f);
                            hurtAud.PlayOneShot(hurtAud.clip);
                        }
                    }
                }
            }
        }
    }

    class NewMovement_Start
    {
        static void Postfix(NewMovement __instance)
        {
            __instance.gameObject.AddComponent<NewMovement_DeltaHpComp>();
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
