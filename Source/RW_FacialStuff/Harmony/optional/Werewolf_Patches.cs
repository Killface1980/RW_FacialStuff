namespace FacialStuff.Harmony.optional
{
    using FacialStuff.newStuff;

    using Verse;

    using Werewolf;

    public static class Werewolf_Patches
    {
        public static void TransformBack_Postfix(CompWerewolf __instance)
        {
            if (!__instance.Pawn.GetCompFace(out CompFace face))
            {
                return;
            }

            face.DontRender = false;
            __instance.Pawn.Drawer.renderer.graphics.nakedGraphic = null;
        }

        public static void TransformInto_Prefix(CompWerewolf __instance)
        {
            if (__instance.Pawn.GetCompFace(out CompFace face))
            {
                face.DontRender = true;
            }
        }
    }
}