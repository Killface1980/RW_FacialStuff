namespace FacialStuff.Harmony.optional
{
    using JetBrains.Annotations;

    using Verse;

    using Werewolf;

    public class Werewolf_Patches
    {
        public static void TransformInto_Prefix([NotNull] CompWerewolf __instance)
        {
            var face = __instance.Pawn.TryGetComp<CompFace>();
            if (face != null)
            {
                face.Dontrender = true;
            }
        }

        public static void TransformBack_Postfix([NotNull] CompWerewolf __instance)
        {
            var face = __instance.Pawn.TryGetComp<CompFace>();
            if (face != null)
            {
                face.Dontrender = false;
                __instance.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }

    }
}