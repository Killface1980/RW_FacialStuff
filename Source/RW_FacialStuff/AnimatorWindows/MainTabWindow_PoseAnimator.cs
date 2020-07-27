using FacialStuff.Defs;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff.AnimatorWindows
{
    public class MainTabWindow_PoseAnimator : MainTabWindow_BaseAnimator
    {
        #region Public Fields

        public static bool Equipment;
        public static float HorHeadOffset;
        public static float VerHeadOffset;
        public static bool IsPosing;

        #endregion Public Fields

        #region Private Fields

        [CanBeNull] public static PoseCycleDef EditorPoseCycle;

        #endregion Private Fields

        public static bool IsOpen;

        #region Public Properties


        #endregion Private Properties

        #region Public Methods

        // public static float horHeadOffset;
        protected override void DoBasicSettingsMenu(Listing_Standard listing)
        {
            base.DoBasicSettingsMenu(listing);

            GetBodyAnimDef();

            // listing_Standard.CheckboxLabeled("Equipment", ref Equipment);

            // listing_Standard.Label(horHeadOffset.ToString("N2") + " - " + verHeadOffset.ToString("N2"));
            // horHeadOffset = listing_Standard.Slider(horHeadOffset, -1f, 1f);
            // verHeadOffset = listing_Standard.Slider(verHeadOffset, -1f, 1f);
            listing.Label(CompAnim.BodyAnim.offCenterX.ToString("N2"));
            CompAnim.BodyAnim.offCenterX = listing.Slider(CompAnim.BodyAnim.offCenterX, -0.2f, 0.2f);

            if (listing.ButtonText(EditorPoseCycle?.LabelCap))
            {
                List<string> exists = new List<string>();
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                CompAnim.BodyAnim.poseCycles.Clear();

                foreach (PoseCycleDef posecycle in (from bsm in DefDatabase<PoseCycleDef>.AllDefs
                                                    orderby bsm.label
                                                    select bsm)
                                                  .TakeWhile(current => CompAnim.BodyAnim.PoseCycleType != "None")
                                                  .Where(current => current.PoseCycleType ==
                                                                    CompAnim.BodyAnim.PoseCycleType))
                {
                    list.Add(new FloatMenuOption(posecycle.LabelCap, delegate { EditorPoseCycle = posecycle; }));
                    exists.Add(posecycle.pawnPosture.ToString());
                    CompAnim.BodyAnim.poseCycles.Add(posecycle);
                }

                string[] names = Enum.GetNames(typeof(PawnPosture));
                for (int index = 0; index < names.Length; index++)
                {
                    string name = names[index];
                    PawnPosture myenum = (PawnPosture)Enum.ToObject(typeof(PawnPosture), index);

                    if (exists.Contains(myenum.ToString()))
                    {
                        continue;
                    }

                    list.Add(
                             new FloatMenuOption(
                                                 "Add new " + CompAnim.BodyAnim.PoseCycleType + "_" + myenum,
                                                 delegate
                                                 {
                                                     PoseCycleDef newCycle = new PoseCycleDef();
                                                     newCycle.defName =
                                                     newCycle.label =
                                                     CompAnim.BodyAnim.PoseCycleType + "_" + name;
                                                     newCycle.pawnPosture = myenum;
                                                     newCycle.PoseCycleType = CompAnim.BodyAnim.PoseCycleType;
                                                     GameComponent_FacialStuff.BuildPoseCycles(newCycle);
                                                     EditorPoseCycle = newCycle;

                                                     CompAnim.BodyAnim.poseCycles.Add(newCycle);
                                                 }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            listing.Gap();
            string configFolder = DefPath;
            if (listing.ButtonText("Export BodyDef"))
            {
                string filePath = configFolder + "/BodyAnimDefs/" + CompAnim.BodyAnim.defName + ".xml";

                Find.WindowStack.Add(
                                     Dialog_MessageBox.CreateConfirmation(
                                                                          "Confirm overwriting " +
                                                                          filePath,
                                                                          delegate
                                                                          {
                                                                              ExportAnimDefs.Defs animDef =
                                                                              new ExportAnimDefs.Defs(CompAnim.BodyAnim);

                                                                              DirectXmlSaver.SaveDataObject(
                                                                                                            animDef,
                                                                                                            filePath);
                                                                          },
                                                                          true));

                // BodyAnimDef animDef = this.bodyAnimDef;
            }

            if (listing.ButtonText("Export PoseCycle"))
            {
                string path = configFolder + "/PoseCycleDefs/" + EditorPoseCycle?.defName + ".xml";

                Find.WindowStack.Add(
                                     Dialog_MessageBox.CreateConfirmation(
                                                                          "Confirm overwriting " + path,
                                                                          delegate
                                                                          {
                                                                              ExportPoseCycleDefs.Defs cycle =
                                                                              new ExportPoseCycleDefs.Defs(EditorPoseCycle);

                                                                              DirectXmlSaver.SaveDataObject(
                                                                                                            cycle,
                                                                                                            path);
                                                                          },
                                                                          true));
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            if (GUI.changed)
            {
                GameComponent_FacialStuff.BuildPoseCycles();
            }
        }

        protected override void BuildEditorCycle()
        {
            base.BuildEditorCycle();
            GameComponent_FacialStuff.BuildPoseCycles(EditorPoseCycle);
        }

        protected override void DrawBodySettingsEditor(Rot4 rotation)
        {
            Rect sliderRect = new Rect(0, 0, this.SliderWidth, 40f);

            // this.DrawBodyStats("legLength", ref bodyAnimDef.legLength, ref sliderRect);
            // this.DrawBodyStats("hipOffsetVerticalFromCenter",
            // ref bodyAnimDef.hipOffsetVerticalFromCenter, ref sliderRect);
            GetBodyAnimDef();

            Vector3 shoulderOffset = CompAnim.BodyAnim.shoulderOffsets[rotation.AsInt];

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
            // this.DrawBodyStats("shoulderFront",   ref front,            ref sliderRect);

            Vector3 hipOffset = CompAnim.BodyAnim.hipOffsets[rotation.AsInt];
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
            // this.DrawBodyStats("hipFront",   ref hipFront,    ref sliderRect);

            if (GUI.changed)
            {
                this.SetNewVector(rotation, shoulderOffset, CompAnim.BodyAnim.shoulderOffsets, front);
                this.SetNewVector(rotation, hipOffset, CompAnim.BodyAnim.hipOffsets, hipFront);
            }

            this.DrawBodyStats("armLength", ref CompAnim.BodyAnim.armLength, ref sliderRect);
            this.DrawBodyStats("extraLegLength", ref CompAnim.BodyAnim.extraLegLength, ref sliderRect);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        protected override void DrawKeyframeEditor(Rect keyframes, Rot4 rotation)
        {
            if (this.CurrentFrame == null)
            {
                return;
            }

            Rect leftController = keyframes.LeftHalf();
            Rect rightController = keyframes.RightHalf();
            leftController.xMax -= this.Spacing;

            rightController.xMin += this.Spacing;
            {
                GUI.BeginGroup(leftController);
                Rect editorRect = new Rect(0f, 0f, leftController.width, 56f);

                // Dictionary<int, float> keysFloats = new Dictionary<int, float>();

                // // Get the next keyframe
                // for (int i = 0; i < frames.Count; i++)
                // {
                // float? footPositionX = frames[i].FootPositionX;
                // if (!footPositionX.HasValue)
                // {
                // continue;
                // }
                // keysFloats.Add(frames[i].KeyIndex, footPositionX.Value);
                // }
                List<int> framesAt;
                List<PawnKeyframe> frames = PawnKeyframes;
                PoseCycleDef cycleDef = EditorPoseCycle;
                {
                    framesAt = (from keyframe in frames where keyframe.HandPositionX.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.HandPositionX,
                                     ref editorRect,
                                     cycleDef.HandPositionX,
                                     "HandPosX",
                                     framesAt);

                    framesAt = (from keyframe in frames where keyframe.HandPositionZ.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.HandPositionZ,
                                     ref editorRect,
                                     cycleDef.HandPositionZ,
                                     "HandPosZ",
                                     framesAt);

                    framesAt = (from keyframe in frames where keyframe.FootPositionX.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.FootPositionX,
                                     ref editorRect,
                                     cycleDef.FootPositionX,
                                     "FootPosX",
                                     framesAt);

                    framesAt = (from keyframe in frames where keyframe.FootPositionZ.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.FootPositionZ,
                                     ref editorRect,
                                     cycleDef.FootPositionZ,
                                     "FootPosZ",
                                     framesAt);

                    framesAt = (from keyframe in frames where keyframe.FootAngle.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetAngle(
                                  ref this.CurrentFrame.FootAngle,
                                  ref editorRect,
                                  cycleDef.FootAngle,
                                  "FootAngle",
                                  framesAt);

                    framesAt = (from keyframe in frames
                                where keyframe.HipOffsetHorizontalX.HasValue
                                select keyframe.KeyIndex).ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.HipOffsetHorizontalX,
                                     ref editorRect,
                                     cycleDef.HipOffsetHorizontalX,
                                     "HipOffsetHorizontalX",
                                     framesAt);

                    // Quadruped
                }

                GUI.EndGroup();

                GUI.BeginGroup(rightController);

                editorRect.x = 0f;
                editorRect.y = 0f;

                if (this.CompAnim.Props.bipedWithHands)
                {
                    this.SetAngleShoulder(ref cycleDef.shoulderAngle, ref editorRect, "ShoulderAngle");

                    framesAt =
                    (from keyframe in frames where keyframe.HandsSwingAngle.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetAngle(
                                  ref this.CurrentFrame.HandsSwingAngle,
                                  ref editorRect,
                                  cycleDef.HandsSwingAngle,
                                  "HandSwing",
                                  framesAt);
                }

                if (rotation.IsHorizontal)
                {
                    if (this.CompAnim.Props.quadruped)
                    {
                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawPositionX.HasValue
                                    select keyframe.KeyIndex).ToList();
                        this.SetPosition(
                                         ref this.CurrentFrame.FrontPawPositionX,
                                         ref editorRect,
                                         cycleDef.FrontPawPositionX,
                                         "FrontPawPositionX",
                                         framesAt);

                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawPositionZ.HasValue
                                    select keyframe.KeyIndex).ToList();

                        this.SetPosition(
                                         ref this.CurrentFrame.FrontPawPositionZ,
                                         ref editorRect,
                                         cycleDef.FrontPawPositionZ,
                                         "FrontPawPositionZ",
                                         framesAt);

                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawAngle.HasValue
                                    select keyframe.KeyIndex).ToList();

                        this.SetAngle(
                                      ref this.CurrentFrame.FrontPawAngle,
                                      ref editorRect,
                                      cycleDef.FrontPawAngle,
                                      "FrontPawAngle",
                                      framesAt);
                    }

                    framesAt = (from keyframe in frames where keyframe.BodyAngle.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetAngle(
                                  ref this.CurrentFrame.BodyAngle,
                                  ref editorRect,
                                  cycleDef.BodyAngle,
                                  "BodyAngle",
                                  framesAt);
                }
                else
                {
                    framesAt = (from keyframe in frames
                                where keyframe.BodyAngleVertical.HasValue
                                select keyframe.KeyIndex).ToList();
                    this.SetAngle(
                                  ref this.CurrentFrame.BodyAngleVertical,
                                  ref editorRect,
                                  cycleDef.BodyAngleVertical,
                                  "BodyAngleVertical",
                                  framesAt);
                }
                framesAt = (from keyframe in frames
                            where keyframe.ShoulderOffsetHorizontalX.HasValue
                            select keyframe.KeyIndex).ToList();
                this.SetPosition(
                                 ref this.CurrentFrame.ShoulderOffsetHorizontalX,
                                 ref editorRect,
                                 cycleDef.ShoulderOffsetHorizontalX,
                                 "ShoulderOffsetHorizontalX",
                                 framesAt);
                framesAt =
                (from keyframe in frames where keyframe.BodyOffsetZ.HasValue select keyframe.KeyIndex).ToList();

                this.SetPosition(
                                 ref this.CurrentFrame.BodyOffsetZ,
                                 ref editorRect,
                                 cycleDef.BodyOffsetZ,
                                 "BodyOffsetZ",
                                 framesAt);

                GUI.EndGroup();
            }
        }

        protected override void SetCurrentCycle()
        {
            BodyAnimDef anim = this.CompAnim.BodyAnim;
            if (anim != null && anim.poseCycles.Any())
            {
                EditorPoseCycle =
                anim.poseCycles.FirstOrDefault();
            }
        }

        protected override void SetKeyframes()
        {
            PawnKeyframes = EditorPoseCycle?.keyframes;
            this.Label = EditorPoseCycle?.LabelCap;
        }

        protected override void FindRandomPawn()
        {
            base.FindRandomPawn();
            BodyAnimDef anim = this.CompAnim.BodyAnim;
            if (anim != null && anim.poseCycles.Any())
            {
                EditorPoseCycle = anim.poseCycles.FirstOrDefault();
            }
            this.CompAnim.AnimatorPoseOpen = true;
        }

        #endregion Public Methods

        #region Private Methods

        private void DrawBodyStats(string label, ref float value, ref Rect sliderRect)
        {
            float left = -1.5f;
            float right = 1.5f;
            value = Widgets.HorizontalSlider(
                                                   sliderRect,
                                                   value,
                                                   left,
                                                   right,
                                                   false,
                                                   label + ": " + value,
                                                   left.ToString(),
                                                   right.ToString(),
                                                   0.025f);

            sliderRect.y += sliderRect.height + 8f;
        }

        #endregion Private Methods
    }
}