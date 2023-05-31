using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Ultrapain
{
    public static class ILUtils
    {
        public static OpCode LoadLocalOpcode(int localIndex)
        {
            if (localIndex == 0)
                return OpCodes.Ldloc_0;
            if (localIndex == 1)
                return OpCodes.Ldloc_1;
            if (localIndex == 2)
                return OpCodes.Ldloc_2;
            if (localIndex == 3)
                return OpCodes.Ldloc_3;
            if (localIndex <= byte.MaxValue)
                return OpCodes.Ldloc_S;
            return OpCodes.Ldloc;
        }

        public static CodeInstruction LoadLocalInstruction(int localIndex)
        {
            if (localIndex == 0)
                return new CodeInstruction(OpCodes.Ldloc_0);
            if (localIndex == 1)
                return new CodeInstruction(OpCodes.Ldloc_1);
            if (localIndex == 2)
                return new CodeInstruction(OpCodes.Ldloc_2);
            if (localIndex == 3)
                return new CodeInstruction(OpCodes.Ldloc_3);
            if (localIndex <= byte.MaxValue)
                return new CodeInstruction(OpCodes.Ldloc_S, (byte) localIndex);
            return new CodeInstruction(OpCodes.Ldloc, localIndex);
        }

        public static CodeInstruction LoadLocalInstruction(object localIndex)
        {
            if (localIndex is int intIndex)
                return LoadLocalInstruction(intIndex);

            // Wish I could access LocalBuilder, this is just a logic bomb
            // I hope they don't use more than 255 local variables
            return new CodeInstruction(OpCodes.Ldloc_S, localIndex);
        }

        public static OpCode StoreLocalOpcode(int localIndex)
        {
            if (localIndex == 0)
                return OpCodes.Stloc_0;
            if (localIndex == 1)
                return OpCodes.Stloc_1;
            if (localIndex == 2)
                return OpCodes.Stloc_2;
            if (localIndex == 3)
                return OpCodes.Stloc_3;
            if (localIndex <= byte.MaxValue)
                return OpCodes.Stloc_S;
            return OpCodes.Stloc;
        }

        public static CodeInstruction StoreLocalInstruction(int localIndex)
        {
            return new CodeInstruction(StoreLocalOpcode(localIndex));
        }

        public static CodeInstruction StoreLocalInstruction(object localIndex)
        {
            if (localIndex is int integerIndex)
                return StoreLocalInstruction(integerIndex);

            // Another logic bomb
            return new CodeInstruction(OpCodes.Stloc_S, localIndex);
        }

        public static object GetLocalIndex(CodeInstruction inst)
        {
            if (inst.opcode == OpCodes.Ldloc_0 || inst.opcode == OpCodes.Stloc_0)
                return 0;
            if (inst.opcode == OpCodes.Ldloc_1 || inst.opcode == OpCodes.Stloc_1)
                return 1;
            if (inst.opcode == OpCodes.Ldloc_2 || inst.opcode == OpCodes.Stloc_2)
                return 2;
            if (inst.opcode == OpCodes.Ldloc_3 || inst.opcode == OpCodes.Stloc_3)
                return 3;

            // Return the local builder object (which is not defined in mscorlib for some reason, so cannot access members)
            if (inst.opcode == OpCodes.Ldloc_S || inst.opcode == OpCodes.Stloc_S || inst.opcode == OpCodes.Ldloc || inst.opcode == OpCodes.Stloc)
                return inst.operand;

            return null;
        }

        public static bool IsCodeSequence(List<CodeInstruction> code, int index, List<CodeInstruction> seq)
        {
            if (index + seq.Count > code.Count)
                return false;

            for (int i = 0; i < seq.Count; i++)
            {
                if (seq[i].opcode != code[i + index].opcode)
                    return false;
                else if (code[i + index].operand != null && !code[i + index].OperandIs(seq[i].operand))
                    return false;
            }

            return true;
        }
    }
}
