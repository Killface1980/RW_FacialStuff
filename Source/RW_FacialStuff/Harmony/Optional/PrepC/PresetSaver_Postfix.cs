// ReSharper disable All

using HarmonyLib;

namespace FacialStuff.Harmony.Optional.PrepC
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using EdB.PrepareCarefully;


    public static class PresetSaver_Postfix
    {
        public static void AddFaceToDictionary(CustomPawn pawn)
        {
            if (SaveRecordPawnV4_Postfix.SavedPawns.ContainsKey(pawn.Id))
            {
                SaveRecordPawnV4_Postfix.SavedPawns.Remove(pawn.Id);
            }

            SaveRecordPawnV4_Postfix.SavedPawns.Add(pawn.Id, pawn);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SavePawnRef(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {

            CodeInstruction last = null;
            foreach (CodeInstruction itr in instrs)
            {
                if (last != null && itr.opcode == OpCodes.Newobj && itr.operand
                    == AccessTools.Constructor(typeof(SaveRecordPawnV4), new[] { typeof(CustomPawn) }))
                {
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.Method(
                            typeof(PresetSaver_Postfix),
                            nameof(AddFaceToDictionary),
                            new[] { typeof(CustomPawn) }));
                    if (last.opcode == OpCodes.Call)
                        yield return new CodeInstruction(OpCodes.Ldloca_S, 2);
                    yield return last;
                }

                yield return itr;
                last = itr;
            }
        }
    }
}