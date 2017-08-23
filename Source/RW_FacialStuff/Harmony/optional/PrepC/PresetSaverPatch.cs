// ReSharper disable All
namespace FacialStuff.Harmony.optional.PrepC
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using EdB.PrepareCarefully;

    using global::Harmony;

    public static class PresetSaverPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SavePawnRef(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            CodeInstruction last = null;
            foreach (CodeInstruction itr in instrs)
            {
                if (last != null && itr.opcode == OpCodes.Newobj && itr.operand == AccessTools.Constructor(
                        typeof(SaveRecordPawnV3),
                        new[] { typeof(CustomPawn) }))
                {
                    yield return new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.Method(
                            typeof(PresetSaverPatch),
                           nameof(AddFaceToDictionary),
                            new[] { typeof(CustomPawn) }));
                    yield return last;
                }

                yield return itr;
                last = itr;
            }
        }

        public static void AddFaceToDictionary(CustomPawn pawn)
        {
            if (SaveRecordPawnV3Patch.SavedPawns.ContainsKey(pawn.Id))
            {
                SaveRecordPawnV3Patch.SavedPawns.Remove(pawn.Id);
            }

            SaveRecordPawnV3Patch.SavedPawns.Add(pawn.Id, pawn);
        }
    }
}
