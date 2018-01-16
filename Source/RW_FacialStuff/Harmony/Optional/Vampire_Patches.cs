using Vampire;

namespace FacialStuff.Harmony.Optional
{
    public static class Vampire_Patches
    {
        public static void Transformed_Postfix(CompVampire __instance)
        {
            if (__instance.IsVampire)
            {
                if (!__instance.Pawn.GetCompFace(out CompFace compFace))
                {
                    return;
                }

                if (__instance.Transformed || __instance.Bloodline?.headGraphicsPath != string.Empty)
                {
                    compFace.Deactivated = true;
                    return;
                }

                compFace.Deactivated = false;
            }
        }
    }
}