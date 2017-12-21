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

            Rect pawnRect = AddPortraitWidget(0f, 0f);

            var frames = new Dictionary<float, PawnKeyframe>();

            foreach (PawnKeyframe keyframe in this.compFace.walkCycle.animation)
            {
                frames.Add(keyframe.keyFrameAt, keyframe);
            }
            var width = inRect.width / 2 / frames.Count;
            width -= 12f;
            var buttonRect = new Rect(0f, pawnRect.yMax + 12f, width, 32f);
            foreach (float frame in frames.Keys)
            {
                if (Widgets.ButtonText(buttonRect, frame.ToString()))
                {
                    this.compFace.AnimationPercent = frame;
                }
                buttonRect.x += buttonRect.width + 12f;
            }

            Listing_Standard listing_Standard = new Listing_Standard { ColumnWidth = (inRect.width - 34f) / 3f };

            var newRect = inRect;
            newRect.yMin = buttonRect.yMax + 12f;

            listing_Standard.Begin(newRect);

            listing_Standard.Label(this.pawn.LabelCap);

            if (this.loop)
            {

                this.compFace.AnimationPercent += 0.01f;
                if (this.compFace.AnimationPercent > 1f)
                {
                    this.compFace.AnimationPercent = 0f;
                }
                // listing_Standard.Slider(this.compFace.AnimationPercent, 0f, 1f);
            }
            else
            {

            }
            this.compFace.AnimationPercent = Mathf.Ceil(listing_Standard.Slider(this.compFace.AnimationPercent, 0f, 1f) * 8) / 8;

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

            if (frames.TryGetValue(this.compFace.AnimationPercent, out PawnKeyframe thisFrame))
            {

                this.SetAngle(ref thisFrame.HandsSwingAngle, listing_Standard, this.compFace.walkCycle.HandsSwingAngle, "HandSwing");

                this.SetAngle(ref thisFrame.FootAngle, listing_Standard, this.compFace.walkCycle.FootAngle, "FootAngle");

                if (thisFrame.FootPositionX.HasValue)
                {
                    listing_Standard.Label("FootPosX " + thisFrame.FootPositionX);
                    thisFrame.FootPositionX = Mathf.Ceil(
                                                  listing_Standard.Slider(thisFrame.FootPositionX.Value, -0.5f, 0.5f)
                                                  * 40) / 40;
                }
                if (thisFrame.FootPositionY.HasValue)
                {
                    listing_Standard.Label("FootPosY " + thisFrame.FootPositionY);
                    thisFrame.FootPositionY = Mathf.Ceil(
                                                  listing_Standard.Slider(thisFrame.FootPositionY.Value, -0.5f, 0.5f)
                                                  * 40) / 40;
                }
            }

            if (GUI.changed)
            {
                GameComponent_FacialStuff.BuildWalkCycles();
            }

            // foreach (PawnKeyframe keyframe in frames.Values)
            // {
            //     listing_Standard.Label(keyframe.keyFrameAt.ToString());
            //     listing_Standard.Label(keyframe.FootPositionX.ToString());
            //     listing_Standard.Label(keyframe.FootPositionY.ToString());
            //     listing_Standard.Gap();
            // }

            listing_Standard.End();

            //  HarmonyPatch_PawnRenderer.Prefix(this.pawn.Drawer.renderer, Vector3.zero, Rot4.East.AsQuat, true, Rot4.East, Rot4.East, RotDrawMode.Fresh, false, false);
            base.DoWindowContents(inRect);
        }

        private void SetAngle(ref float? angle, Listing_Standard listing_Standard, SimpleCurve thisFrame, string label)
        {
            if (angle.HasValue)
            {
                listing_Standard.Label(label + " " + angle);
                angle = Mathf.FloorToInt(listing_Standard.Slider(angle.Value, -180f, 180f));
            }
            else
            {
                if (listing_Standard.ButtonText("Add " + label))
                {
                    angle =
                        thisFrame.Evaluate(this.compFace.AnimationPercent);
                }
            }
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
