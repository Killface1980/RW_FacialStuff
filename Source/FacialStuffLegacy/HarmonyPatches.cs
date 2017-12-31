namespace FacialStuff.Harmony
{
    using System.Linq;

    using FacialStuff.FaceStyling_Bench;

    using global::Harmony;

    using Verse;

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            // this just kills all old FS benches. Editor window now handled via ThingComp.
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.facialstuff.legacy.mod");

            foreach (ThingDef bench in DefDatabase<ThingDef>.AllDefsListForReading.Where(
                x => x.thingClass == typeof(FaceStyler) || x.thingClass == typeof(FaceStyling.FaceStyler)))
            {
                bench.thingClass = typeof(Building);
            }
        }
    }
}