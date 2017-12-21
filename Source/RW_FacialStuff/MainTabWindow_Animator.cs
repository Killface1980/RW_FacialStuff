using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using FacialStuff.Defs;
    using FacialStuff.Graphics;
    using FacialStuff.Harmony;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class MainTabWindow_Animator : MainTabWindow
    {
        public Pawn pawn;

        public bool loop;

        public override void DoWindowContents(Rect inRect)
        {
            FindRandomPawn();

            PortraitsCache.SetDirty(pawn);

            Rect pawnRect = AddPortraitWidget(0f, 30f);

            Listing_Standard listing_Standard = new Listing_Standard { ColumnWidth = (inRect.width - 34f) / 3f };


            var newRect = inRect;
            newRect.yMin = pawnRect.yMax + 12f;

            listing_Standard.Begin(newRect);

            listing_Standard.Label(this.pawn.LabelCap);

            if (this.loop)
            {

                this.compFace.AnimationPercent += 0.01f;
                if (this.compFace.AnimationPercent > 1f)
                {
                    this.compFace.AnimationPercent = 0f;
                }
                listing_Standard.Slider(this.compFace.AnimationPercent, 0f, 1f);
            }
            else
            {

                this.compFace.AnimationPercent = listing_Standard.Slider(this.compFace.AnimationPercent, 0f, 1f);
            }

            if (listing_Standard.ButtonText(this.compFace.walkCycle.LabelCap))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (WalkCycleDef current in from bsm in DefDatabase<WalkCycleDef>.AllDefs
                                                 orderby bsm.LabelCap
                                                 select bsm)
                {
                    WalkCycleDef smLocal = current;
                    list.Add(new FloatMenuOption(smLocal.LabelCap, delegate
                        {
                            this.compFace.walkCycle = smLocal;
                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (listing_Standard.ButtonText(this.compFace.rotation.ToStringHuman()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                list.Add(new FloatMenuOption(Rot4.East.ToStringHuman(), delegate
                    {
                        this.compFace.rotation = Rot4.East;
                    }));
                list.Add(new FloatMenuOption(Rot4.North.ToStringHuman(), delegate
                    {
                        this.compFace.rotation = Rot4.North;
                    }));
                list.Add(new FloatMenuOption(Rot4.South.ToStringHuman(), delegate
                    {
                        this.compFace.rotation = Rot4.South;
                    }));
                list.Add(new FloatMenuOption(Rot4.West.ToStringHuman(), delegate
                    {
                        this.compFace.rotation = Rot4.West;
                    }));

                Find.WindowStack.Add(new FloatMenu(list));
            }

            listing_Standard.CheckboxLabeled("Loop", ref loop);
            var frames = new List<float>();
            listing_Standard.End();

            var listing2 = new Listing_Standard();

            var rect2 = new Rect(0, listing_Standard.CurHeight + newRect.y, inRect.width, inRect.height);

            listing2.ColumnWidth = (inRect.width - 34f) / frames.Count;
            listing2.Begin(rect2);
            foreach (PawnKeyframe keyframe in this.compFace.walkCycle.animation)
            {
                if (listing2.ButtonText(keyframe.keyFrameAt.ToString()))
                {
                    this.compFace.AnimationPercent = keyframe.keyFrameAt;
                }
                frames.Add(keyframe.keyFrameAt);
            }
            listing2.End();

            if (frames.Contains(this.compFace.AnimationPercent))
            {

            }

            //  HarmonyPatch_PawnRenderer.Prefix(this.pawn.Drawer.renderer, Vector3.zero, Rot4.East.AsQuat, true, Rot4.East, Rot4.East, RotDrawMode.Fresh, false, false);
            base.DoWindowContents(inRect);
        }

        public override void PostClose()
        {
            this.compFace.AnimatorOpen = false;
            base.PostClose();
        }

        private void FindRandomPawn()
        {
            if (this.pawn == null)
            {
                this.pawn = Find.AnyPlayerHomeMap.FreeColonistsForStoryteller.FirstOrDefault();
            }
            this.pawn.GetCompFace(out this.compFace);
            compFace.AnimatorOpen = true;


        }

        private static Vector2 portraitSize = new Vector2(240, 240f);

        private CompFace compFace;

        public Rect AddPortraitWidget(float left, float top)
        {
            // Portrait
            Rect rect = new Rect(left, top, portraitSize.x, portraitSize.y);
            GUI.DrawTexture(rect, FaceTextures.backgroundTex);

            // Draw the pawn's portrait
            GUI.BeginGroup(rect);
            Vector2 size = new Vector2(rect.height / 1.4f, rect.height); // 128x180
            Rect position = new Rect(
                rect.width * 0.5f - size.x * 0.5f,
                rect.height * 0.5f - size.y * 0.5f - 10f,
                size.x,
                size.y);
            GUI.DrawTexture(position, PortraitsCache.Get(this.pawn, size, new Vector3(0f, 0f, 0.1f), 1.25f));

            // GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, default(Vector3)));
            GUI.EndGroup();

            GUI.color = Color.white;
            Widgets.DrawBox(rect, 1);

            return rect;
        }


    }
}
