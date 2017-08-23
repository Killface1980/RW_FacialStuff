namespace FacialStuff.Harmony.optional.PrepC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using global::Harmony;

    public static class ColonistSaverPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SavePawnRef(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            CodeInstruction last = null;
            foreach (CodeInstruction itr in instrs)
            {
                if (last != null && itr.opcode == OpCodes.Newobj && itr.operand == AccessTools.Constructor(typeof(EdB.PrepareCarefully.SaveRecordPawnV3), new[] { typeof(EdB.PrepareCarefully.CustomPawn) }))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PresetSaverPatch), nameof(PresetSaverPatch.AddFaceToDictionary), new[] { typeof(EdB.PrepareCarefully.CustomPawn) }));
                    yield return last;
                }

                yield return itr;
                last = itr;
            }
        }
    }
}
