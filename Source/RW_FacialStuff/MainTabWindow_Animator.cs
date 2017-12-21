using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacialStuff
{
    using System.IO;

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


            Dictionary<float, PawnKeyframe> frames = new Dictionary<float, PawnKeyframe>();

            foreach (PawnKeyframe keyframe in this.compFace.walkCycle.animation)
            {
                frames.Add(keyframe.keyFrameAt, keyframe);
            }

            var width = inRect.width / 2 / frames.Count;
            width -= 12f;
            Rect pawnRect = AddPortraitWidget(0f, 0f);

            // Menu next to pawn
            {
                Listing_Standard listing_Standard = new Listing_Standard { ColumnWidth = (inRect.width - pawnRect.width - 34f) / 3f };

                var newRect = inRect;
                newRect.xMin = pawnRect.xMax + 12f;
                newRect.height = pawnRect.height;

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
                    this.compFace.AnimationPercent = Mathf.Ceil(listing_Standard.Slider(this.compFace.AnimationPercent, 0f, 1f) * 8) / 8;

                }

                listing_Standard.CheckboxLabeled("Loop", ref loop);

                if (listing_Standard.ButtonText(this.pawn.LabelCap))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (Pawn current in from bsm in this.pawn.Map.FreeColonistsForStoryteller
                                                     orderby bsm.LabelCap
                                                     select bsm)
                    {
                        Pawn smLocal = current;
                        list.Add(new FloatMenuOption(smLocal.LabelCap, delegate
                            {
                                this.compFace.AnimatorOpen = false;

                                this.pawn = smLocal;
                                smLocal.GetCompFace(out this.compFace);

                            }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }

                    Find.WindowStack.Add(new FloatMenu(list));
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
                    List<FloatMenuOption> list =
                        new List<FloatMenuOption>
                            {
                                new FloatMenuOption(
                                    Rot4.East.ToStringHuman(),
                                    delegate { this.compFace.rotation = Rot4.East; }),
                                new FloatMenuOption(
                                    Rot4.North.ToStringHuman(),
                                    delegate { this.compFace.rotation = Rot4.North; }),
                                new FloatMenuOption(
                                    Rot4.South.ToStringHuman(),
                                    delegate { this.compFace.rotation = Rot4.South; }),
                                new FloatMenuOption(
                                    Rot4.West.ToStringHuman(),
                                    delegate { this.compFace.rotation = Rot4.West; })
                            };


                    Find.WindowStack.Add(new FloatMenu(list));
                }




                // foreach (PawnKeyframe keyframe in frames.Values)
                // {
                //     listing_Standard.Label(keyframe.keyFrameAt.ToString());
                //     listing_Standard.Label(keyframe.FootPositionX.ToString());
                //     listing_Standard.Label(keyframe.FootPositionY.ToString());
                //     listing_Standard.Gap();
                // }
                listing_Standard.Gap();
                if (listing_Standard.ButtonText("Export"))
                {
                    string configFolder = Path.GetDirectoryName(GenFilePaths.ModsConfigFilePath);
                    DirectXmlSaver.SaveDataObject(
                        this.compFace.walkCycle,
                        configFolder + "/Exported_" + this.compFace.walkCycle.defName + ".xml");
                }

                listing_Standard.End();

            }


            var buttonRect = new Rect(0f, pawnRect.yMax + 12f, width, 32f);
            foreach (float frame in frames.Keys)
            {
                if (Widgets.ButtonText(buttonRect, frame.ToString()))
                {
                    this.compFace.AnimationPercent = frame;
                }
                buttonRect.x += buttonRect.width + 12f;
            }
            Rect controller = inRect.BottomHalf();
            GUI.BeginGroup(controller);

            if (frames.TryGetValue(this.compFace.AnimationPercent, out PawnKeyframe thisFrame))
            {
                var editorRect = new Rect(0f, 0f, (inRect.width - 34f) / 2, 48f);

                this.SetPosition(
                    ref thisFrame.FootPositionX,
                    editorRect,
                    this.compFace.walkCycle.FootPositionX,
                    "FootPosX");

                editorRect.y += editorRect.height;

                this.SetPosition(
                    ref thisFrame.FootPositionY,
                     editorRect,
                    this.compFace.walkCycle.FootPositionY,
                    "FootPosY");

                editorRect.y += editorRect.height;

                this.SetAngle(
                    ref thisFrame.FootAngle,
                    editorRect,
                    this.compFace.walkCycle.FootAngle,
                    "FootAngle");

                editorRect.x = editorRect.width + 17f;
                editorRect.y = 0f;

                this.SetAngle(
                    ref this.compFace.walkCycle.shoulderAngle,
                    editorRect,
                    "ShoulderAngle");

                editorRect.y += editorRect.height;

                this.SetAngle(
                    ref thisFrame.HandsSwingAngle,
                    editorRect,
                    this.compFace.walkCycle.HandsSwingAngle,
                    "HandSwing");

                editorRect.y += editorRect.height;

                this.SetAngle(
                    ref thisFrame.BodyAngle,
                    editorRect,
                    this.compFace.walkCycle.BodyAngle,
                    "BodyAngle");

                editorRect.y += editorRect.height;

                this.SetAngle(
                    ref thisFrame.BodyAngleVertical,
                    editorRect,
                    this.compFace.walkCycle.BodyAngleVertical,
                    "BodyAngleVertical");

                editorRect.y += editorRect.height;

                this.SetPosition(
                    ref thisFrame.BodyOffsetVertical,
                     editorRect,
                    this.compFace.walkCycle.BodyOffsetVertical,
                    "BodyOffsetVertical");

                editorRect.y += editorRect.height;

                this.SetPosition(
                    ref thisFrame.HandsSwingPosVertical,
                     editorRect,
                    this.compFace.walkCycle.HandsSwingPosVertical,
                    "HandsSwingPosVertical");

                // Cloase the cycle
                if (thisFrame.keyFrameAt == 0f)
                {
                    frames.LastOrDefault().Value.BodyAngle = thisFrame.BodyAngle;
                    frames.LastOrDefault().Value.BodyAngleVertical = thisFrame.BodyAngleVertical;
                    frames.LastOrDefault().Value.BodyOffsetVertical = thisFrame.BodyOffsetVertical;
                    frames.LastOrDefault().Value.FootAngle = thisFrame.FootAngle;
                    frames.LastOrDefault().Value.FootPositionX = thisFrame.FootPositionX;
                    frames.LastOrDefault().Value.FootPositionY = thisFrame.FootPositionY;
                    frames.LastOrDefault().Value.HandsSwingAngle = thisFrame.HandsSwingAngle;
                    frames.LastOrDefault().Value.HandsSwingPosVertical = thisFrame.HandsSwingPosVertical;
                }
                else if (thisFrame.keyFrameAt == 1f)
                {
                    frames.FirstOrDefault().Value.BodyAngle = thisFrame.BodyAngle;
                    frames.FirstOrDefault().Value.BodyAngleVertical = thisFrame.BodyAngleVertical;
                    frames.FirstOrDefault().Value.BodyOffsetVertical = thisFrame.BodyOffsetVertical;
                    frames.FirstOrDefault().Value.FootAngle = thisFrame.FootAngle;
                    frames.FirstOrDefault().Value.FootPositionX = thisFrame.FootPositionX;
                    frames.FirstOrDefault().Value.FootPositionY = thisFrame.FootPositionY;
                    frames.FirstOrDefault().Value.HandsSwingAngle = thisFrame.HandsSwingAngle;
                    frames.FirstOrDefault().Value.HandsSwingPosVertical = thisFrame.HandsSwingPosVertical;
                }
            }


            GUI.EndGroup();


            if (GUI.changed)
            {
                GameComponent_FacialStuff.BuildWalkCycles();
            }

            //  HarmonyPatch_PawnRenderer.Prefix(this.pawn.Drawer.renderer, Vector3.zero, Rot4.East.AsQuat, true, Rot4.East, Rot4.East, RotDrawMode.Fresh, false, false);
            base.DoWindowContents(inRect);
        }

        private void SetAngle(ref float? angle, Rect editorRect, SimpleCurve thisFrame, string label)
        {
            Rect labelRect = new Rect(editorRect.x, editorRect.y, this.widthLabel, this.defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax, editorRect.y, this.widthButton, this.defaultHeight);

            Widgets.Label(labelRect, label + " " + angle);

            if (angle.HasValue)
            {
                angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle.Value, -180f, 180f));
                if (Widgets.ButtonText(buttonRect, "Remove key"))
                {
                    angle = null;
                }
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.compFace.AnimationPercent), -180f, 180f);
                if (Widgets.ButtonText(buttonRect, "Add key"))
                {
                    angle = thisFrame.Evaluate(this.compFace.AnimationPercent);
                }
            }
        }

        private void SetAngle(ref float angle, Rect editorRect, string label)
        {
            Rect labelRect = new Rect(editorRect.x, editorRect.y, this.widthLabel, this.defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax, editorRect.y, this.widthButton, this.defaultHeight);

            Widgets.Label(labelRect, label + " " + angle);
            angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle, -180f, 180f));
        }

        private void SetPosition(ref float? position, Rect editorRect, SimpleCurve thisFrame, string label)
        {
            Rect labelRect = new Rect(editorRect.x, editorRect.y, this.widthLabel, this.defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax, editorRect.y, this.widthButton, this.defaultHeight);

            Widgets.Label(labelRect, label + " " + position);
            if (position.HasValue)
            {
                position = Mathf.Ceil(Widgets.HorizontalSlider(sliderRect, position.Value, -0.4f, 0.4f) * 40) / 40;
                if (Widgets.ButtonText(buttonRect, "Remove key"))
                {
                    position = null;
                }
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.compFace.AnimationPercent), -0.4f, 0.4f);

                if (Widgets.ButtonText(buttonRect, "Add key"))
                {
                    position = thisFrame.Evaluate(this.compFace.AnimationPercent);
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

        private float width = 200f;

        private float widthButton = 120f;

        private float widthLabel = 150f;

        private float defaultHeight = 32f;

        public Rect AddPortraitWidget(float left, float top)
        {
            // Portrait
            Rect rect = new Rect(left, top, portraitSize.x, portraitSize.y);
            GUI.DrawTexture(rect, FaceTextures.backgroundAnimTex);

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
