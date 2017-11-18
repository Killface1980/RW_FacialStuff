// ReSharper disable All

namespace FacialStuff.Harmony.Optional.PrepC
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using EdB.PrepareCarefully;

    using global::Harmony;

    public static class PresetSaver_Postfix
    {
        public static void AddFaceToDictionary(CustomPawn pawn)
        {
            if (SaveRecordPawnV3_Postfix.SavedPawns.ContainsKey(pawn.Id))
            {
                SaveRecordPawnV3_Postfix.SavedPawns.Remove(pawn.Id);
            }

            SaveRecordPawnV3_Postfix.SavedPawns.Add(pawn.Id, pawn);
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SavePawnRef(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            CodeInstruction last = null;
            foreach (CodeInstruction itr in instrs)
            {
                if (last != null && itr.opcode == OpCodes.Newobj && itr.operand
                    == AccessTools.Constructor(typeof(SaveRecordPawnV3), new[] { typeof(CustomPawn) }))
                {
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.Method(
                            typeof(PresetSaver_Postfix),
                            nameof(AddFaceToDictionary),
                            new[] { typeof(CustomPawn) }));
                    yield return last;
                }

                yield return itr;
                last = itr;
            }
        }
    }
}