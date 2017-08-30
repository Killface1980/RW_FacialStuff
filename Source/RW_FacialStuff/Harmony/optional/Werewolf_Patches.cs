namespace FacialStuff.Harmony.optional
{
    using Verse;

    using Werewolf;

    public class Werewolf_Patches
    {
        public static void TransformInto_Prefix(CompWerewolf __instance)
        {
            CompFace face = __instance.Pawn.TryGetComp<CompFace>();
            if (face != null)
            {
                face.Dontrender = true;
            }
        }

        public static void TransformBack_Postfix(CompWerewolf __instance)
        {
            CompFace face = __instance.Pawn.TryGetComp<CompFace>();
            if (face == null)
            {
                return;
            }
            face.Dontrender = false;
            __instance.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }
    }
}