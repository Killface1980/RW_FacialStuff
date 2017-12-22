using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
    using System.IO;

    using FacialStuff.Defs;
    using FacialStuff.Graphics;

    using RimWorld;

    using UnityEngine;

    using Verse;

    public class MainTabWindow_Animator : MainTabWindow
    {
        public Pawn pawn;

        public bool loop;

        int currentFrame = 0;

        public int frameLabel = 1;

        public override void DoWindowContents(Rect inRect)
        {
            this.FindRandomPawn();

            PortraitsCache.SetDirty(this.pawn);

            List<PawnKeyframe> frames = this.compFace.walkCycle.animation;

            int count = frames.Count;

            float width = inRect.width / 2 / count;
            width -= this.spacing;
            Rect pawnRect = this.AddPortraitWidget(0f, 0f);
            {
                // Menu next to pawn
                Listing_Standard listing_Standard = new Listing_Standard { ColumnWidth = (inRect.width - pawnRect.width - 34f) / 3f };

                Rect newRect = inRect;
                newRect.xMin = pawnRect.xMax + this.spacing;
                newRect.height = pawnRect.height;

                listing_Standard.Begin(newRect);

                listing_Standard.Label(this.pawn.LabelCap);




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

            var rotator = new Rect(0f, pawnRect.yMax + this.spacing * 2, inRect.width / 2 - this.spacing, 32f);

            GUI.BeginGroup(rotator);

            Rect butt = new Rect(0f, 0f, 54f, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.compFace.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.compFace.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.compFace.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.compFace.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Loop", ref this.loop);


            GUI.EndGroup();



            float space = this.spacing * (count - 1);
            float widthButton = ((inRect.width / 2) - this.spacing - space) / count;

            Rect buttonRect = new Rect(0f, rotator.yMax + this.spacing, widthButton, 32f);

            float animationSlider = this.compFace.AnimationPercent;

            // Keyframe buttons
            foreach (PawnKeyframe keyframe in frames)
            {
                int keyIndex = keyframe.keyIndex;
                if (Widgets.ButtonText(buttonRect, (keyIndex + 1).ToString()))
                {
                    this.currentFrame = keyIndex;
                    this.compFace.AnimationPercent = (float)keyIndex / ((float)count - 1);
                }

                buttonRect.x += buttonRect.width + this.spacing;
            }

            // Rotation


            this.frameLabel = this.currentFrame + 1;
            Rect timeline = new Rect(0f, buttonRect.yMax + this.spacing, inRect.width / 2 - this.spacing, 32f);
            float steps = (float)1 / (count - 1);
            string label = "Current frame: " + this.frameLabel;
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
                animationSlider = Widgets.HorizontalSlider(
                    timeline,
                    this.compFace.AnimationPercent,
                    0f,
                    1f,
                    false,
                    label);

            }

            Rect controller = inRect.BottomHalf();
            controller.yMin = timeline.yMax + this.spacing;

            Rect leftController = controller.LeftHalf();
            Rect rightController = controller.RightHalf();
            leftController.xMax -= this.spacing;

            rightController.xMin += this.spacing;

            GUI.BeginGroup(leftController);

            PawnKeyframe thisFrame = frames[this.currentFrame];
            {
                Rect editorRect = new Rect(0f, 0f, leftController.width, 48f);

                // Dictionary<int, float> keysFloats = new Dictionary<int, float>();

                // // Get the next keyframe
                // for (int i = 0; i < frames.Count; i++)
                // {
                //     float? footPositionX = frames[i].FootPositionX;
                //     if (!footPositionX.HasValue)
                //     {
                //         continue;
                //     }
                //     keysFloats.Add(frames[i].keyIndex, footPositionX.Value);
                // }

                if (this.compFace.rotation.IsHorizontal)
                {
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
                }
                else
                {

                    this.SetPosition(
                        ref thisFrame.FootPositionVerticalY,
                        editorRect,
                        this.compFace.walkCycle.FootPositionVerticalY,
                        "FootPosVerticalY");

                    editorRect.y += editorRect.height;
                }

                GUI.EndGroup();

                GUI.BeginGroup(rightController);

                editorRect.x = 0f;
                editorRect.y = 0f;

                if (this.compFace.rotation.IsHorizontal)
                {
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
                }
                else
                {

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
            }




            if (GUI.changed)
            {
                // Close the cycle
                if (!this.loop && animationSlider != this.compFace.AnimationPercent)
                {
                    this.compFace.AnimationPercent = animationSlider;
                    this.currentFrame = (int)(this.compFace.AnimationPercent * (count - 1));
                    // Log.Message("current frame: " + this.currentFrame);
                }

                PawnKeyframe firstFrame = frames.FirstOrDefault();
                PawnKeyframe lastFrame = frames.LastOrDefault();

                if (this.currentFrame == firstFrame.keyIndex)
                {
                    lastFrame.BodyAngle = firstFrame.BodyAngle;
                    lastFrame.BodyAngleVertical = firstFrame.BodyAngleVertical;
                    lastFrame.BodyOffsetVertical = firstFrame.BodyOffsetVertical;
                    lastFrame.FootAngle = firstFrame.FootAngle;
                    lastFrame.FootPositionX = firstFrame.FootPositionX;
                    lastFrame.FootPositionY = firstFrame.FootPositionY;
                    lastFrame.FootPositionVerticalY = firstFrame.FootPositionVerticalY;
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
                    firstFrame.FootPositionVerticalY = lastFrame.FootPositionVerticalY;
                    firstFrame.HandsSwingAngle = lastFrame.HandsSwingAngle;
                    firstFrame.HandsSwingPosVertical = lastFrame.HandsSwingPosVertical;
                }


                if (!this.loop)
                {
                    GameComponent_FacialStuff.BuildWalkCycles();
                }
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
                if (Widgets.ButtonText(buttonRect, "Remove key at" + this.frameLabel))
                {
                    angle = null;
                }
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.compFace.AnimationPercent), -180f, 180f, false, label + " " + angle, "-180", "180");
                if (Widgets.ButtonText(buttonRect, "Add key at " + this.frameLabel))
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

        private void SetPosition(ref float? position, Rect editorRect, SimpleCurve thisFrame, string label, Dictionary<int, float> keysFloats = null)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax + this.spacing, editorRect.y, this.widthButton, this.defaultHeight);

            float leftValue = -0.4f;
            float rightValue = 0.4f;

            if (!this.loop && position.HasValue)
            {
                position = Widgets.HorizontalSlider(sliderRect, position.Value, leftValue, rightValue, false, label + " " + position, "-0.4", "0.4", 0.025f);
                if (Widgets.ButtonText(buttonRect, "Remove key at " + this.frameLabel))
                {
                    position = null;
                }
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.compFace.AnimationPercent), leftValue, rightValue, false, label + " " + position, "-0.4", "0.4");

                if (Widgets.ButtonText(buttonRect, "Add key at " + this.frameLabel))
                {
                    position = thisFrame.Evaluate(this.compFace.AnimationPercent);
                }
            }
            // GUI.color = Color.red;
            // if (keysFloats != null)
            // {
            //     foreach (var value in keysFloats)
            //     {
            //
            //         var key = value.Key;
            //         var val = value.Value;
            //         float pos = Mathf.Lerp(leftValue, rightValue, val);
            //
            //         Widgets.DrawLineVertical(sliderRect.center.x + sliderRect.width * pos, sliderRect.y + 10f, 16f);
            //     }
            // }
            // GUI.color = Color.white;
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

