namespace FacialStuff.Harmony.Optional
{
    using Verse;

    using Werewolf;

    public static class Werewolf_Patches
    {
        public static void TransformBack_Postfix(CompWerewolf __instance)
        {
            if (!__instance.Pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            compFace.DontRender = false;
            __instance.Pawn.Drawer.renderer.graphics.nakedGraphic = null;
        }

        public static void TransformInto_Prefix(CompWerewolf __instance)
        {
            if (__instance.Pawn.GetCompFace(out CompFace compFace))
            {
                compFace.DontRender = true;
            }
        }
    }
}