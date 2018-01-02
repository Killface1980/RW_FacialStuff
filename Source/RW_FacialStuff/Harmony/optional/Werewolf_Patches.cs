using Werewolf;

namespace FacialStuff.Harmony.Optional
{
    public static class Werewolf_Patches
    {
        public static void TransformBack_Postfix(CompWerewolf __instance)
        {
            if (!__instance.Pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            compFace.Deactivated = false;
            __instance.Pawn.Drawer.renderer.graphics.nakedGraphic = null;
        }

        public static void TransformInto_Prefix(CompWerewolf __instance)
        {
            if (!__instance.Pawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            compFace.Deactivated = true;
        }
    }
}