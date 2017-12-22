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

        int currentFrame = 0;

        public override void DoWindowContents(Rect inRect)
        {
            this.FindRandomPawn();

            PortraitsCache.SetDirty(this.pawn);

            List<PawnKeyframe> frames = this.compFace.walkCycle.animation;

            var count = frames.Count;

            var width = inRect.width / 2 / count;
            width -= this.spacing;
            Rect pawnRect = this.AddPortraitWidget(0f, 0f);
            {
                // Menu next to pawn
                Listing_Standard listing_Standard = new Listing_Standard { ColumnWidth = (inRect.width - pawnRect.width - 34f) / 3f };

                var newRect = inRect;
                newRect.xMin = pawnRect.xMax + this.spacing;
                newRect.height = pawnRect.height;

                listing_Standard.Begin(newRect);

                listing_Standard.Label(this.pawn.LabelCap);



                listing_Standard.CheckboxLabeled("Loop", ref this.loop);

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
                // listing_Standard.Label(keyframe.keyIndex.ToString());
                // listing_Standard.Label(keyframe.FootPositionX.ToString());
                // listing_Standard.Label(keyframe.FootPositionY.ToString());
                // listing_Standard.Gap();
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

            var space = this.spacing * (count - 1);
            var widthButton = ((inRect.width / 2) - space) / count;

            Rect buttonRect = new Rect(0f, pawnRect.yMax + this.spacing, widthButton, 32f);

            foreach (PawnKeyframe keyframe in frames)
            {
                int keyIndex = keyframe.keyIndex;
                if (Widgets.ButtonText(buttonRect, keyIndex.ToString()))
                {
                    this.currentFrame = keyIndex;
                    this.compFace.AnimationPercent = (float)keyIndex / (count - 1);
                }

                buttonRect.x += buttonRect.width + this.spacing;
            }

            Rect timeline = new Rect(0f, buttonRect.yMax, inRect.width / 2, 32f);
            float steps = (float)1 / (count - 1);
            string label = "Current frame: " + this.currentFrame;
            if (this.loop)
            {
                this.compFace.AnimationPercent += 0.01f;
                if (this.compFace.AnimationPercent > 1f)
                {
                    this.compFace.AnimationPercent = 0f;
                }

                Widgets.HorizontalSlider(timeline, this.compFace.AnimationPercent, 0f, 1f, false, label);
            }
            else
            {
                this.compFace.AnimationPercent = Widgets.HorizontalSlider(timeline, this.compFace.AnimationPercent, 0f, 1f, false, label);

            }

            Rect controller = inRect.BottomHalf();
            GUI.BeginGroup(controller);

            PawnKeyframe thisFrame = frames[this.currentFrame];
            {
                var editorRect = new Rect(0f, 0f, (inRect.width - 34f) / 2, 48f);

                // Get the next keyframe
                // for (int i = this.currentFrame; i < frames.Count; i++)
                // {
                //     float? nextKey = frames[i].FootPositionX;
                //     if (!nextKey.HasValue)
                //     {
                //         continue;
                //     }
                //
                //     float percent = i / count;
                //     GUI.color = Color.red;
                //     Widgets.HorizontalSlider(editorRect, this.compFace.walkCycle.FootPositionX.Evaluate(percent), -0.4f, 0.4f);
                //     GUI.color = Color.white;
                //
                // }

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

                this.SetAngleShoulder(
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

            }


            GUI.EndGroup();


            if (GUI.changed)
            {
                // Close the cycle
                this.currentFrame = (int)(this.compFace.AnimationPercent * (count - 1) + 1);

                PawnKeyframe firstFrame = frames[0];
                PawnKeyframe lastFrame = frames[count - 1];
                if (this.currentFrame == firstFrame.keyIndex)
                {
                    lastFrame.BodyAngle = firstFrame.BodyAngle;
                    lastFrame.BodyAngleVertical = firstFrame.BodyAngleVertical;
                    lastFrame.BodyOffsetVertical = firstFrame.BodyOffsetVertical;
                    lastFrame.FootAngle = firstFrame.FootAngle;
                    lastFrame.FootPositionX = firstFrame.FootPositionX;
                    lastFrame.FootPositionY = firstFrame.FootPositionY;
                    lastFrame.HandsSwingAngle = firstFrame.HandsSwingAngle;
                    lastFrame.HandsSwingPosVertical = firstFrame.HandsSwingPosVertical;
                }
                else if (this.currentFrame == lastFrame.keyIndex)
                {
                    firstFrame.BodyAngle = lastFrame.BodyAngle;
                    firstFrame.BodyAngleVertical = lastFrame.BodyAngleVertical;
                    firstFrame.BodyOffsetVertical = lastFrame.BodyOffsetVertical;
                    firstFrame.FootAngle = lastFrame.FootAngle;
                    firstFrame.FootPositionX = lastFrame.FootPositionX;
                    firstFrame.FootPositionY = lastFrame.FootPositionY;
                    firstFrame.HandsSwingAngle = lastFrame.HandsSwingAngle;
                    firstFrame.HandsSwingPosVertical = lastFrame.HandsSwingPosVertical;
                }


                GameComponent_FacialStuff.BuildWalkCycles();
            }

            // HarmonyPatch_PawnRenderer.Prefix(this.pawn.Drawer.renderer, Vector3.zero, Rot4.East.AsQuat, true, Rot4.East, Rot4.East, RotDrawMode.Fresh, false, false);
            base.DoWindowContents(inRect);
        }

        private void SetAngle(ref float? angle, Rect editorRect, SimpleCurve thisFrame, string label)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax + this.spacing, editorRect.y, this.widthButton, this.defaultHeight);

            if (!loop && angle.HasValue)
            {
                angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle.Value, -180f, 180f, false, label + " " + angle, "-180", "180"));
                if (Widgets.ButtonText(buttonRect, "Remove key"))
                {
                    angle = null;
                }
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.compFace.AnimationPercent), -180f, 180f, false, label + " " + angle, "-180", "180");
                if (Widgets.ButtonText(buttonRect, "Add key"))
                {
                    angle = thisFrame.Evaluate(this.compFace.AnimationPercent);
                }
            }
        }

        private void SetAngleShoulder(ref float angle, Rect editorRect, string label)
        {
            Rect labelRect = new Rect(editorRect.x, editorRect.y, this.widthLabel, this.defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax, editorRect.y, this.widthButton, this.defaultHeight);

            Widgets.Label(labelRect, label + " " + angle);
            angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle, -180f, 180f));
        }

        private void SetPosition(ref float? position, Rect editorRect, SimpleCurve thisFrame, string label)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(this.width + this.spacing, editorRect.y, this.widthButton, this.defaultHeight);

            if (!this.loop && position.HasValue)
            {
                position = Widgets.HorizontalSlider(sliderRect, position.Value, -0.4f, 0.4f, false, label + " " + position, "-0.4", "0.4", 0.025f);
                if (Widgets.ButtonText(buttonRect, "Remove key"))
                {
                    position = null;
                }
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.compFace.AnimationPercent), -0.4f, 0.4f, false, label + " " + position, "-0.4", "0.4");

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
            this.compFace.AnimatorOpen = true;


        }

        private static Vector2 portraitSize = new Vector2(240, 240f);

        private CompFace compFace;

        private float width = 350f;

        private float widthButton = 120f;

        private float widthLabel = 150f;

        private float defaultHeight = 32f;

        private float spacing = 12f;

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
