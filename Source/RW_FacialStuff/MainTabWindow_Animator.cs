using System.Collections.Generic;
using System.Linq;

namespace FacialStuff
{
    using System;
    using System.IO;

    using FacialStuff.Components;
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

        public static bool develop = false;
        public static bool Colored;


        public override void DoWindowContents(Rect inRect)
        {
            this.FindRandomPawn();

            PortraitsCache.SetDirty(this.pawn);

            List<PawnKeyframe> frames = EditorWalkcycle.animation;

            int count = frames.Count;

            float width = inRect.width / 2 / count;

            Rot4 rotation = this.CompAnim.rotation;

            Rect pawnRect = this.AddPortraitWidget(0f, 0f);
            {
                var settingsRect = new Rect(
                    pawnRect.xMax + this.spacing,
                    pawnRect.y,
                    inRect.width - pawnRect.width - this.spacing,
                    pawnRect.height);

                // Menu next to pawn
                Rect newRect = settingsRect.LeftHalf();
                this.DoSettingsMenu(newRect);

                Rect bodyEditor = settingsRect.RightHalf();
                bodyEditor.xMin += this.spacing;

                this.DrawBodySettingsEditor(bodyEditor, rotation);
            }

            this.frameLabel = this.currentFrame + 1;
            int lastIndex = count - 1;
            float space = this.spacing * lastIndex;
            float widthButton = ((inRect.width / 2) - this.spacing - space) / count;
            float animationSlider = this.CompAnim.AnimationPercent;

            Rect rotator = new Rect(0f, pawnRect.yMax + this.spacing * 2, inRect.width / 2 - this.spacing, 32f);
            Rect timeline = new Rect(0f, rotator.yMax + this.spacing, inRect.width / 2 - this.spacing, 32f);

            Rect buttonRect = new Rect(0f, timeline.yMax + this.spacing, widthButton, 32f);
            Rect controller = inRect.BottomHalf();
            controller.yMin = buttonRect.yMax + this.spacing;

            this.DrawRotator(rotator);
            this.DrawTimelineSlider(count, timeline, ref animationSlider);
            this.DrawTimelineButtons(frames, buttonRect, count, ref animationSlider);

            this.DrawKeyframeEditor(controller, frames, rotation);

            if (GUI.changed)
            {
                // Close the cycle
                if (!this.loop && animationSlider != this.CompAnim.AnimationPercent)
                {
                    this.CompAnim.AnimationPercent = animationSlider;
                    this.currentFrame = (int)(this.CompAnim.AnimationPercent * lastIndex);
                    // Log.Message("current frame: " + this.currentFrame);
                }

                PawnKeyframe firstOrDefault = frames.FirstOrDefault();
                PawnKeyframe lastOrDefault = frames.LastOrDefault();
                PawnKeyframe thisFrame = frames[this.currentFrame];

                if (firstOrDefault != null && lastOrDefault != null)
                {
                    if (this.currentFrame == 0)
                    {
                        SynchronizeFrames(thisFrame, frames[lastIndex]);
                    }
                    if (this.currentFrame == lastIndex)
                    {
                        SynchronizeFrames(thisFrame, frames[0]);
                    }
                }

                if (!this.loop)
                {
                    GameComponent_FacialStuff.BuildWalkCycles();
                }
            }

            // HarmonyPatch_PawnRenderer.Prefix(this.pawn.Drawer.renderer, Vector3.zero, Rot4.East.AsQuat, true, Rot4.East, Rot4.East, RotDrawMode.Fresh, false, false);
            base.DoWindowContents(inRect);
        }

        private static void SynchronizeFrames(PawnKeyframe sourceFrame, PawnKeyframe targetFrame)
        {
            targetFrame.BodyAngle = sourceFrame.BodyAngle;
            targetFrame.BodyAngleVertical = sourceFrame.BodyAngleVertical;
            targetFrame.BodyOffsetVerticalZ = sourceFrame.BodyOffsetVerticalZ;
            targetFrame.BodyOffsetZ = sourceFrame.BodyOffsetZ;
            targetFrame.FootAngle = sourceFrame.FootAngle;
            targetFrame.FootPositionVerticalZ = sourceFrame.FootPositionVerticalZ;
            targetFrame.FootPositionX = sourceFrame.FootPositionX;
            targetFrame.FootPositionZ = sourceFrame.FootPositionZ;
            targetFrame.FrontPawAngle = sourceFrame.FrontPawAngle;
            targetFrame.FrontPawPositionVerticalZ = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.FrontPawPositionX = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.FrontPawPositionZ = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.HandsSwingAngle = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.HandsSwingPosVertical = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.HipOffsetHorizontalX = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.ShoulderOffsetHorizontalX = sourceFrame.FrontPawPositionVerticalZ;
        }

        private void DrawBodySettingsEditor(Rect bodyEditor, Rot4 rotation)
        {
            GUI.BeginGroup(bodyEditor);

            Rect sliderRect = new Rect(0, 0, this.width, bodyEditor.height / 8);

            this.bodyAnimDef = this.CompAnim.bodySizeDefinition;

            // this.DrawBodyStats("legLength", ref bodyAnimDef.legLength, ref sliderRect);
            // this.DrawBodyStats("hipOffsetVerticalFromCenter",
            //     ref bodyAnimDef.hipOffsetVerticalFromCenter, ref sliderRect);

            Vector3 shoulderOffset = this.bodyAnimDef.shoulderOffsets[rotation.AsInt];

            if (shoulderOffset.y == 0f)
            {
                if (rotation == Rot4.West)
                {
                    shoulderOffset.y = -0.025f;
                }
                else
                {
                    shoulderOffset.y = 0.025f;
                }
            }

            bool front = shoulderOffset.y > 0;

            if (rotation == Rot4.West)
            {
                front = shoulderOffset.y < 0;
            }

            this.DrawBodyStats("shoulderOffsetX", ref shoulderOffset.x, ref sliderRect);
            this.DrawBodyStats("shoulderOffsetZ", ref shoulderOffset.z, ref sliderRect);
            this.DrawBodyStats("shoulderFront", ref front, ref sliderRect);

            Vector3 hipOffset = this.bodyAnimDef.hipOffsets[rotation.AsInt];
            if (hipOffset.y == 0f)
            {
                if (rotation == Rot4.West)
                {
                    hipOffset.y = -0.025f;
                }
                else
                {
                    hipOffset.y = 0.025f;
                }
            }
            bool hipFront = hipOffset.y > 0;
            if (rotation == Rot4.West)
            {
                hipFront = hipOffset.y < 0;
            }
            this.DrawBodyStats("hipOffsetX", ref hipOffset.x, ref sliderRect);
            this.DrawBodyStats("hipOffsetZ", ref hipOffset.z, ref sliderRect);
            this.DrawBodyStats("hipFront", ref hipFront, ref sliderRect);

            if (GUI.changed)
            {
                this.SetNewVector(rotation, shoulderOffset, this.bodyAnimDef.shoulderOffsets, front);
                this.SetNewVector(rotation, hipOffset, this.bodyAnimDef.hipOffsets, hipFront);
            }

            this.DrawBodyStats("armLength", ref this.bodyAnimDef.armLength, ref sliderRect);
            this.DrawBodyStats("extraLegLength", ref this.bodyAnimDef.extraLegLength, ref sliderRect);

            GUI.EndGroup();
        }

        private void SetNewVector(Rot4 rotation, Vector3 newOffset, List<Vector3> offset, bool front)
        {
            newOffset.y = (front ? 1 : -1) * 0.025f;
            if (rotation == Rot4.West)
            {
                newOffset.y *= -1;
            }

            // Set new offset
            offset[rotation.AsInt] = newOffset;

            Vector3 opposite = newOffset;

            var oppY = offset[rotation.Opposite.AsInt].y;
            var oppZ = offset[rotation.Opposite.AsInt].z;

            // Opposite side
            opposite.x *= -1;
            if (!rotation.IsHorizontal)
            {
                // Keep the north south stats
                opposite.y = oppY;
                opposite.z = oppZ;
            }
            else
            {
                opposite.y *= -1;

            }

            offset[rotation.Opposite.AsInt] = opposite;
        }

        private void DrawKeyframeEditor(Rect controller, List<PawnKeyframe> frames, Rot4 rotation)
        {
            Rect leftController = controller.LeftHalf();
            Rect rightController = controller.RightHalf();
            leftController.xMax -= this.spacing;

            rightController.xMin += this.spacing;

            PawnKeyframe thisFrame = frames[this.currentFrame];
            {
                GUI.BeginGroup(leftController);
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

                if (rotation.IsHorizontal)
                {
                    this.SetPosition(ref thisFrame.FootPositionX, ref editorRect, EditorWalkcycle.FootPositionX, "FootPosX");

                    this.SetPosition(ref thisFrame.FootPositionZ, ref editorRect, EditorWalkcycle.FootPositionZ, "FootPosY");

                    this.SetAngle(ref thisFrame.FootAngle, ref editorRect, EditorWalkcycle.FootAngle, "FootAngle");

                    this.SetPosition(
                        ref thisFrame.HipOffsetHorizontalX,
                        ref editorRect,
                        EditorWalkcycle.HipOffsetHorizontalX,
                        "HipOffsetHorizontalX");

                    // Quadruped
                }
                else
                {
                    this.SetPosition(
                        ref thisFrame.FootPositionVerticalZ,
                        ref editorRect,
                        EditorWalkcycle.FootPositionVerticalZ,
                        "FootPosVerticalY");
                }

                GUI.EndGroup();

                GUI.BeginGroup(rightController);

                editorRect.x = 0f;
                editorRect.y = 0f;

                if (rotation.IsHorizontal)
                {
                    if (this.CompAnim.Props.bipedWithHands)
                    {
                        this.SetAngle(ref thisFrame.HandsSwingAngle, ref editorRect, EditorWalkcycle.HandsSwingAngle, "HandSwing");
                    }

                    if (this.CompAnim.Props.quadruped)
                    {
                        this.SetPosition(
                            ref thisFrame.FrontPawPositionX,
                            ref editorRect,
                            EditorWalkcycle.FrontPawPositionX,
                            "FrontPawPosX");

                        this.SetPosition(
                            ref thisFrame.FrontPawPositionZ,
                            ref editorRect,
                            EditorWalkcycle.FrontPawPositionZ,
                            "FrontPawPosY");


                        this.SetAngle(ref thisFrame.FrontPawAngle, ref editorRect, EditorWalkcycle.FrontPawAngle, "FrontPawAngle");

                    }
                    this.SetPosition(
                        ref thisFrame.ShoulderOffsetHorizontalX,
                        ref editorRect,
                        EditorWalkcycle.ShoulderOffsetHorizontalX,
                        "ShoulderOffsetHorizontalX");


                    this.SetPosition(ref thisFrame.BodyOffsetZ, ref editorRect, EditorWalkcycle.BodyOffsetZ, "BodyOffsetZ");

                    this.SetAngle(ref thisFrame.BodyAngle, ref editorRect, EditorWalkcycle.BodyAngle, "BodyAngle");
                }
                else
                {
                    if (this.CompAnim.Props.bipedWithHands)
                    {
                        this.SetAngleShoulder(ref EditorWalkcycle.shoulderAngle, ref editorRect, "ShoulderAngle");

                        this.SetPosition(
                            ref thisFrame.HandsSwingPosVertical,
                            ref editorRect,
                            EditorWalkcycle.HandsSwingPosVertical,
                            "HandsSwingPosVertical");
                    }
                    if (this.CompAnim.Props.quadruped)
                    {
                        this.SetPosition(
                            ref thisFrame.FrontPawPositionVerticalZ,
                            ref editorRect,
                            EditorWalkcycle.FrontPawPositionVerticalZ,
                            "FrontPawPosVerticalY");
                    }
                    this.SetPosition(
                        ref thisFrame.BodyOffsetVerticalZ,
                        ref editorRect,
                        EditorWalkcycle.BodyOffsetVerticalZ,
                        "BodyOffsetVerticalZ");


                    this.SetAngle(
                        ref thisFrame.BodyAngleVertical,
                        ref editorRect,
                        EditorWalkcycle.BodyAngleVertical,
                        "BodyAngleVertical");
                }

                GUI.EndGroup();
            }
        }

        private void DoSettingsMenu(Rect newRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();

            newRect.width -= this.spacing;

            listing_Standard.Begin(newRect);

            listing_Standard.Label(this.pawn.LabelCap);
            this.zoom = listing_Standard.Slider(this.zoom, 0.5f, 1.5f);

            listing_Standard.CheckboxLabeled("Develop", ref develop);
            listing_Standard.CheckboxLabeled("Colored", ref Colored);

            if (listing_Standard.ButtonText(this.pawn.LabelCap))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn current in from bsm in this.pawn.Map.mapPawns.AllPawnsSpawned
                                         where bsm.GetComp<CompBodyAnimator>() != null
                                         orderby bsm.LabelCap
                                         select bsm)
                {
                    Pawn smLocal = current;
                    list.Add(
                        new FloatMenuOption(
                            smLocal.LabelCap,
                            delegate
                                {
                                    this.CompAnim.AnimatorOpen = false;

                                    this.pawn = smLocal;
                                    smLocal.GetCompAnim(out this.compAnim);
                                    //        editorWalkcycle = this.CompAnim.Props.defaultCycleWalk;
                                    this.CompAnim.AnimatorOpen = true;
                                }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (listing_Standard.ButtonText(EditorWalkcycle.LabelCap))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (WalkCycleDef current in from bsm in DefDatabase<WalkCycleDef>.AllDefs orderby bsm.LabelCap select bsm)
                {
                    WalkCycleDef smLocal = current;
                    list.Add(new FloatMenuOption(smLocal.LabelCap, delegate { editorWalkcycle = smLocal; }));
                }
                // list.Add(new FloatMenuOption("Add new ...",
                //     delegate
                //         {
                //             var newCycle = new WalkCycleDef();
                //             GameComponent_FacialStuff.BuildWalkCycles(newCycle);
                //             this.compAnim.walkCycle = newCycle;
                //         }));
                Find.WindowStack.Add(new FloatMenu(list));
            }

            listing_Standard.Gap();
            if (listing_Standard.ButtonText("Export BodyDef"))
            {
                string configFolder = DefPath;

                // BodyAnimDef animDef = this.bodyAnimDef;
                ExportAnimDefs.Defs animDef = new ExportAnimDefs.Defs(this.bodyAnimDef); ;

                DirectXmlSaver.SaveDataObject(
                    animDef,
                    configFolder + "/BodyAnimDefs/" + this.bodyAnimDef.defName + ".xml");
            }
            if (listing_Standard.ButtonText("Export WalkCycle"))
            {
                string configFolder = DefPath;
                ExportCycleDefs.Defs cycle = new ExportCycleDefs.Defs(EditorWalkcycle);
                DirectXmlSaver.SaveDataObject(cycle,
                    configFolder + "/WalkCycleDefs/" + EditorWalkcycle.defName + ".xml");
            }

            listing_Standard.End();
        }
        private static string defPath;

        public static string DefPath
        {
            get
            {
                if (!defPath.NullOrEmpty())
                {
                    return defPath;
                }

                ModMetaData mod = ModLister.AllInstalledMods.FirstOrDefault(
                    x => { return x?.Name != null && x.Active && x.Name.StartsWith("Facial Stuff"); });
                if (mod != null)
                {
                    defPath = mod.RootDir + "/Defs";
                }

                return defPath;
            }
        }

        private void DrawBodyStats(string label, ref float value, ref Rect sliderRect)
        {
            float leftValue = -0.8f;
            float rightValue = 0.8f;

            value = Widgets.HorizontalSlider(
                sliderRect,
                value,
                leftValue,
                rightValue,
                false,
                label + ": " + value,
                leftValue.ToString(),
                rightValue.ToString(),
                0.025f);

            sliderRect.y += sliderRect.height;

        }

        private void DrawBodyStats(string label, ref bool flipped, ref Rect sliderRect)
        {

            Widgets.CheckboxLabeled(sliderRect, label, ref flipped);

            sliderRect.y += sliderRect.height;

        }

        private void DrawTimelineButtons(List<PawnKeyframe> frames, Rect buttonRect, int count, ref float animationSlider)
        {
            foreach (PawnKeyframe keyframe in frames)
            {
                int keyIndex = keyframe.keyIndex;
                if (Widgets.ButtonText(buttonRect, (keyIndex + 1).ToString()))
                {
                    animationSlider = (float)keyIndex / ((float)count - 1);
                }

                buttonRect.x += buttonRect.width + this.spacing;
            }
        }

        private void DrawTimelineSlider(int count, Rect timeline, ref float animationSlider)
        {
            float steps = (float)1 / (count - 1);
            string label = "Current frame: " + this.frameLabel;
            if (this.loop)
            {
                this.CompAnim.AnimationPercent += 0.01f;
                if (this.CompAnim.AnimationPercent > 1f)
                {
                    this.CompAnim.AnimationPercent = 0f;
                }

                Widgets.HorizontalSlider(timeline, this.CompAnim.AnimationPercent, 0f, 1f, false, label);
            }
            else
            {
                animationSlider = Widgets.HorizontalSlider(timeline, this.CompAnim.AnimationPercent, 0f, 1f, false, label);
            }
        }

        private void DrawRotator(Rect rotator)
        {
            GUI.BeginGroup(rotator);

            Rect butt = new Rect(0f, 0f, 54f, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.CompAnim.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.CompAnim.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.CompAnim.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                this.CompAnim.rotation = rotation;
            }
            butt.x += butt.width + this.spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Loop", ref this.loop);

            GUI.EndGroup();
        }

        private void SetAngle(ref float? angle, ref Rect editorRect, SimpleCurve thisFrame, string label)
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
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.CompAnim.AnimationPercent), -180f, 180f, false, label + " " + angle, "-180", "180");
                if (Widgets.ButtonText(buttonRect, "Add key at " + this.frameLabel))
                {
                    angle = thisFrame.Evaluate(this.CompAnim.AnimationPercent);
                }
            }
            editorRect.y += editorRect.height;

        }

        private void SetAngleShoulder(ref float angle, ref Rect editorRect, string label)
        {
            Rect labelRect = new Rect(editorRect.x, editorRect.y, this.widthLabel, this.defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax, editorRect.y, this.widthButton, this.defaultHeight);

            Widgets.Label(labelRect, label + " " + angle);
            angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle, -180f, 180f));

            editorRect.y += editorRect.height;
        }

        private void SetPosition(ref float? position, ref Rect editorRect, SimpleCurve thisFrame, string label, Dictionary<int, float> keysFloats = null)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.width, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax + this.spacing, editorRect.y, this.widthButton, this.defaultHeight);

            float leftValue = -0.8f;
            float rightValue = 0.8f;

            if (!this.loop && position.HasValue)
            {
                position = Widgets.HorizontalSlider(sliderRect, position.Value, leftValue, rightValue, false, label + " " + position, leftValue.ToString(), rightValue.ToString(), 0.025f);
                if (Widgets.ButtonText(buttonRect, "Remove key at " + this.frameLabel))
                {
                    position = null;
                }
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(this.CompAnim.AnimationPercent), leftValue, rightValue, false, label + " " + position, "-0.4", "0.4");

                if (Widgets.ButtonText(buttonRect, "Add key at " + this.frameLabel))
                {
                    position = thisFrame.Evaluate(this.CompAnim.AnimationPercent);
                }
            }
            editorRect.y += editorRect.height;
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

        private static WalkCycleDef editorWalkcycle = WalkCycleDefOf.Human_Walk;

        public override void PostClose()
        {
            this.CompAnim.AnimatorOpen = false;
            base.PostClose();
        }

        private void FindRandomPawn()
        {
            if (this.pawn == null)
            {
                this.pawn = Find.AnyPlayerHomeMap.FreeColonistsForStoryteller.FirstOrDefault();

                this.pawn.GetCompAnim(out this.compAnim);
                this.CompAnim.AnimatorOpen = true;
                editorWalkcycle = this.CompAnim.walkCycle;
            }

        }

        private static Vector2 portraitSize = new Vector2(240, 240f);

        private float width = 350f;

        private float widthButton = 120f;

        private float widthLabel = 150f;

        private float defaultHeight = 32f;

        private float spacing = 12f;

        private CompBodyAnimator compAnim;
        public CompBodyAnimator CompAnim => this.compAnim;

        private float zoom = 1f;

        private BodyAnimDef bodyAnimDef;

        public static WalkCycleDef EditorWalkcycle => editorWalkcycle;

        public Rect AddPortraitWidget(float left, float top)
        {
            // Portrait
            Rect rect = new Rect(left, top, portraitSize.x, portraitSize.y);
            GUI.DrawTexture(rect, FaceTextures.backgroundAnimTex);
            var bodysize = this.pawn.BodySize;
            // Draw the pawn's portrait
            GUI.BeginGroup(rect);
            Vector2 size = new Vector2(rect.height / 1.4f, rect.height); // 128x180
            Rect position = new Rect(
                rect.width * 0.5f - size.x * 0.5f,
                rect.height * 0.5f - size.y * 0.5f - 10f,
                size.x,
                size.y);

            GUI.DrawTexture(position, PortraitsCache.Get(this.pawn, size, new Vector3(0f, 0f, 0.1f), zoom));

            // GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, default(Vector3)));
            GUI.EndGroup();

            GUI.color = Color.white;
            Widgets.DrawBox(rect, 1);

            return rect;
        }


    }
}

