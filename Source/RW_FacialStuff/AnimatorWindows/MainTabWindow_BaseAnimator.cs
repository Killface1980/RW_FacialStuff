using FacialStuff.Defs;
using FacialStuff.GraphicsFS;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using FacialStuff.Animator;
using UnityEngine;
using Verse;

namespace FacialStuff.AnimatorWindows
{
    public class MainTabWindow_BaseAnimator : MainTabWindow
    {
        #region Public Fields

        public static bool Colored;

        public static bool Develop;

        public static bool Panic;

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            if (Pawn == null || this.CompAnim == null) { return; }
            CellRect _viewRect = Find.CameraDriver.CurrentViewRect;

            if (_viewRect.Contains(Pawn.Position)) { return; }

            // Execute PostDraw if pawn is not on screen
            this.CompAnim.PostDraw();
        }
        #endregion Public Fields

        #region Protected Fields

        public static Pawn Pawn;
        protected readonly float SliderWidth = 420f;
        protected readonly float Spacing = 12f;
        protected CompBodyAnimator CompAnim;
        protected bool Loop;
        protected float Zoom = 1f;
        #endregion Protected Fields

        #region Private Fields

        [CanBeNull] protected static List<PawnKeyframe> PawnKeyframes;
        private static readonly Color AddColor = new Color(0.25f, 1f, 0.25f);

        private static readonly Color RemoveColor = new Color(1f, 0.25f, 0.25f);

        private static readonly Color SelectedColor = new Color(1f, 0.79f, 0.26f);

        private static float _animationPercent;

        private static float _animSlider;

        private static Rot4 _bodyRot = Rot4.East;

        private static string _defPath;

        private static Vector2 _portraitSize = new Vector2(320f, 320f);
        private readonly float _defaultHeight = 36f;
        private readonly float _widthLabel = 150f;
        private int _frameLabel = 1;

        #endregion Private Fields

        #region Public Properties

        public static float AnimationPercent
        {
            get => _animationPercent;

            set
            {
                _animationPercent = value;
                int frame = (int)(_animationPercent * LastInd);
                CurrentFrameInt = frame;
            }
        }

        public static Rot4 BodyRot
        {
            get => _bodyRot;
            set
            {
                _bodyRot = value;
                HeadRot = value;
            }
        }

        public static Rot4 HeadRot { get; private set; } = Rot4.East;

        public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight - 35f);

        #endregion Public Properties

        #region Protected Properties

        public static string DefPath
        {
            get
            {
                if (!_defPath.NullOrEmpty())
                {
                    return _defPath;
                }

                ModMetaData mod = ModLister.AllInstalledMods.FirstOrDefault(
                                                                            x => x?.Name != null && x.Active &&
                                                                                 x.Name
                                                                                  .StartsWith("Facial Stuff"));
                if (mod != null)
                {
                    _defPath = mod.RootDir + "/Defs";
                }

                return _defPath;
            }
        }

        protected static int CurrentFrameInt { get; set; }
        protected static int LastInd
        {
            get
            {
                if (PawnKeyframes != null)
                {
                    return PawnKeyframes.Count - 1;
                }

                return 0;
            }
        }

        protected BodyAnimDef BodyAnimDef
        {
            get
            {
                BodyAnimDef compAnimBodyAnim = this.CompAnim.BodyAnim;
                if (compAnimBodyAnim != null && compAnimBodyAnim.thingTarget.NullOrEmpty())
                {
                    // ReSharper disable once PossibleNullReferenceException
                    this.CompAnim.BodyAnim.thingTarget = Pawn.def.ToString();
                    this.CompAnim.BodyAnim.bodyDrawers = this.CompAnim.Props.bodyDrawers;
                    this.CompAnim.BodyAnim.handType = this.CompAnim.Props.handType;
                    //this.CompAnim.BodyAnim.footType = this.CompAnim.Props.footType;
                    //this.CompAnim.BodyAnim.pawType = this.CompAnim.Props.pawType;
                    this.CompAnim.BodyAnim.quadruped = this.CompAnim.Props.quadruped;
                    this.CompAnim.BodyAnim.bipedWithHands = this.CompAnim.Props.bipedWithHands;
                }
                return this.CompAnim.BodyAnim;
            }
        }

        [CanBeNull]
        protected PawnKeyframe CurrentFrame => PawnKeyframes?[CurrentFrameInt];

        #endregion Protected Properties

        #region Public Methods

        public override void DoWindowContents(Rect inRect)
        {
            this.SetKeyframes();
            this._frameLabel = CurrentFrameInt + 1;

            Rot4 rotation = BodyRot;

            Rect topEditor = inRect.TopPart(0.52f);
            Rect basics = topEditor.LeftPart(0.375f).ContractedBy(this.Spacing);
            basics.width -= 72f;

            GUI.BeginGroup(basics);
            basics.width -= 36f;
            basics.xMin += 36f;

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(basics);
            this.DoBasicSettingsMenu(listing);
            listing.End();

            GUI.EndGroup();

            Rect bodySetting = topEditor.RightPart(0.375f).ContractedBy(this.Spacing);
            bodySetting.xMin += 36f;

            Rect imageRect =
            new Rect(topEditor.ContractedBy(12f))
            {
                xMin = basics.xMax + 2 * this.Spacing,
                xMax = bodySetting.x - 2 * this.Spacing
            };

            GUI.BeginGroup(imageRect);

            float curY = 0f;

            this.AddPortraitWidget(_portraitSize.y);

            curY += _portraitSize.y + this.Spacing * 2;

            this.DrawRotatorBody(curY, _portraitSize.y);
            curY += 40f;
            this.DrawRotatorHead(curY, _portraitSize.y);

            GUI.EndGroup();

            GUI.BeginGroup(bodySetting);

            this.DrawBodySettingsEditor(rotation);

            GUI.EndGroup();

            Rect bottomEditor = inRect.BottomPart(0.48f);
            Rect timeline = bottomEditor.TopPart(0.25f);
            Rect keyframes = bottomEditor.BottomPart(0.75f);

            GUI.BeginGroup(timeline);

            _animSlider = AnimationPercent;

            if (PawnKeyframes != null)
            {
                int count = PawnKeyframes.Count;
                this.DrawTimelineSlider(count, timeline.width);

                this.DrawTimelineButtons(timeline.width, count);
            }

            GUI.EndGroup();

            this.DrawKeyframeEditor(keyframes, rotation);

            Rect statusRect = inRect.LeftPart(0.25f).ContractedBy(this.Spacing);

            // Widgets.DrawBoxSolid(editor, new Color(1f, 1f, 1f, 0.25f));
            // Widgets.DrawBoxSolid(upperTop, new Color(0f, 0f, 1f, 0.25f));
            // Widgets.DrawBoxSolid(bodyEditor, new Color(1f, 0f, 0f, 0.25f));
            // Widgets.DrawBoxSolid(bottomPart, new Color(1f, 1f, 0f, 0.25f));
            Rect controller = inRect.BottomHalf();
            controller.yMin = statusRect.yMax + this.Spacing;

            if (GUI.changed)
            {
                // Close the cycle
                if (!this.Loop && Math.Abs(_animSlider - AnimationPercent) > 0.01f)
                {
                    AnimationPercent = _animSlider;

                    // Log.Message("current frame: " + this.CurrentFrameInt);
                }
                if (PawnKeyframes != null)
                {
                    if (CurrentFrameInt == 0)
                    {
                        SynchronizeFrames(this.CurrentFrame, PawnKeyframes[LastInd]);
                    }

                    if (CurrentFrameInt == LastInd)
                    {
                        SynchronizeFrames(this.CurrentFrame, PawnKeyframes[0]);
                    }
                }
            }

            // HarmonyPatch_PawnRenderer.Prefix(this.pawn.Drawer.renderer, Vector3.zero, Rot4.East.AsQuat, true, Rot4.East, Rot4.East, RotDrawMode.Fresh, false, false);
            base.DoWindowContents(inRect);
        }

        public override void PostClose()
        {
            CompAnim = null;
            Pawn = null;
            base.PostClose();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            this.FindRandomPawn();
            PortraitsCache.SetDirty(Pawn);
        }
        protected virtual void SetKeyframes()
        {
            PawnKeyframes = new List<PawnKeyframe> { new PawnKeyframe() };
        }
        #endregion Public Methods

        #region Protected Methods

        public string Label;

        public float CurrentShift
        {
            get
            {
                PawnKeyframe currentFrame = this.CurrentFrame;
                if (currentFrame != null)
                {
                    return currentFrame.Shift;
                }

                return 0f;
            }

            set
            {
                if (this.CurrentFrame == null)
                {
                    return;
                }

                if (Math.Abs(value) < 0.05f)
                {
                    this.CurrentFrame.Status = KeyStatus.Automatic;
                }
                else
                {
                    this.CurrentFrame.Status = KeyStatus.Manual;
                }

                this.CurrentFrame.Shift = value;
                this.BuildEditorCycle();
            }
        }

        protected virtual void BuildEditorCycle()
        {
        }

        protected virtual void DoBasicSettingsMenu(Listing_Standard listing)
        {
            string label = Pawn.LabelCap + " - " + this.Label + " - " + this.BodyAnimDef.LabelCap;

            listing.Label(label);

            if (listing.ButtonText(Pawn.LabelCap))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn current in from bsm in Pawn.Map.mapPawns.AllPawnsSpawned
                                         where bsm.HasCompAnimator()
                                         orderby bsm.LabelCap
                                         select bsm)
                {
                    Pawn smLocal = current;
                    list.Add(
                             new FloatMenuOption(
                                                 smLocal.LabelCap,
                                                 delegate
                                                 {
                                                     Find.WindowStack.WindowOfType<MainTabWindow_WalkAnimator>().Close(false);
                                                     Find.Selector.ClearSelection();
                                                     Find.Selector.Select(smLocal, false);
                                                     Find.MainTabsRoot.ToggleTab(DefDatabase<MainButtonDef>.GetNamed("WalkAnimator"));
                                                 }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }
            this.Zoom = listing.Slider(this.Zoom, 0.5f, 1.5f);

            listing.CheckboxLabeled("Develop", ref Develop);
            listing.CheckboxLabeled("Colored", ref Colored);

            if (listing.ButtonText("Add 1 keyframe: "))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                if (PawnKeyframes != null)
                {
                    for (int index = 0; index < PawnKeyframes.Count - 1; index++)
                    {
                        PawnKeyframe keyframe = PawnKeyframes[index];
                        list.Add(
                                 new FloatMenuOption(
                                                     "Add after " + (keyframe.KeyIndex + 1),
                                                     delegate
                                                     {
                                                         PawnKeyframes.Insert(keyframe.KeyIndex + 1,
                                                                              new PawnKeyframe());

                                                         this.ReIndexKeyframes();
                                                     }));
                    }
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (CurrentFrameInt != 0 && CurrentFrameInt != LastInd)
            {
                if (listing.ButtonText("Remove current keyframe " + (CurrentFrameInt + 1)))
                {
                    PawnKeyframes?.RemoveAt(CurrentFrameInt);
                    this.ReIndexKeyframes();
                }
            }

            if (CurrentFrameInt != 0 && CurrentFrameInt != LastInd)
            {
                float shift = this.CurrentShift;
                shift = Mathf.Round(listing.Slider(shift, -1f, 1f) * 20f) / 20f;
                if (GUI.changed)
                {
                    this.CurrentShift = shift;
                }
            }
        }
        protected virtual void DrawBodySettingsEditor(Rot4 rotation)
        {
        }

        protected virtual void DrawKeyframeEditor(Rect keyframes, Rot4 rotation)
        {
        }

        protected virtual void FindRandomPawn()
        {
            Thing selectedThing = Find.Selector.SingleSelectedThing;
            if (selectedThing != null && selectedThing is Pawn pawn && pawn.HasCompAnimator())
            {
                Pawn = pawn;
            }
            else
            {
                Pawn = Find.AnyPlayerHomeMap.PlayerPawnsForStoryteller.FirstOrDefault(x => x.HasCompAnimator());
            }

            Pawn?.GetCompAnim(out this.CompAnim);
        }

        protected void ReIndexKeyframes()
        {
            if (PawnKeyframes == null)
            {
                return;
            }

            for (int index = 0; index < PawnKeyframes.Count; index++)
            {
                PawnKeyframe keyframe = PawnKeyframes[index];
                keyframe.KeyIndex = index;
            }
        }

        protected void SetAngle(
        ref float? angle,
        ref Rect editorRect,
        SimpleCurve thisFrame,
        string label,
        List<int> framesAt)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.SliderWidth, this._defaultHeight);
            Rect buttonRect = new Rect(
                                       sliderRect.xMax + this.Spacing,
                                       editorRect.y,
                                       editorRect.width - this.SliderWidth - this.Spacing, this._defaultHeight);

            DoActiveKeyframeButtons(framesAt, ref buttonRect);

            if (!this.Loop && angle.HasValue)
            {
                angle = Widgets.HorizontalSlider(
                                                 sliderRect,
                                                 angle.Value,
                                                 -180f,
                                                 180f,
                                                 false,
                                                 label + " " + angle,
                                                 "-180",
                                                 "180",
                                                 1f);
                GUI.color = RemoveColor;
                if (Widgets.ButtonText(buttonRect, "- " + this._frameLabel))
                {
                    angle = null;
                }

                GUI.color = Color.white;
            }
            else
            {
                Widgets.HorizontalSlider(
                                         sliderRect,
                                         thisFrame.Evaluate(AnimationPercent),
                                         -180f,
                                         180f,
                                         false,
                                         label + " " + angle,
                                         "-180",
                                         "180");
                GUI.color = AddColor;
                if (Widgets.ButtonText(buttonRect, "+ " + this._frameLabel))
                {
                    angle = thisFrame.Evaluate(AnimationPercent);
                }

                GUI.color = Color.white;
            }

            editorRect.y += editorRect.height;
        }

        protected void SetAngleShoulder(ref float angle, ref Rect editorRect, string label)
        {
            Rect labelRect = new Rect(editorRect.x, editorRect.y, this._widthLabel, this._defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this.SliderWidth, this._defaultHeight);

            Widgets.Label(labelRect, label + " " + angle);
            angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle, 0, 180f));

            editorRect.y += editorRect.height;
        }

        protected virtual void SetCurrentCycle()
        {
        }
        protected void SetNewVector(Rot4 rotation, Vector3 newOffset, List<Vector3> offset, bool front)
        {
            newOffset.y = (front ? 1 : -1) * 0.025f;
            if (rotation == Rot4.West)
            {
                newOffset.y *= -1;
            }

            // Set new offset
            offset[rotation.AsInt] = newOffset;

            Vector3 opposite = newOffset;

            float oppY = offset[rotation.Opposite.AsInt].y;
            float oppZ = offset[rotation.Opposite.AsInt].z;

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

        protected void SetPosition(
        ref float? position,
        ref Rect editorRect,
        SimpleCurve thisFrame,
        string label,
        List<int> framesAt, float sliderFactor = 1f)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.SliderWidth, this._defaultHeight);
            Rect buttonRect = new Rect(
                                       sliderRect.xMax + this.Spacing,
                                       editorRect.y,
                                       editorRect.width - this.SliderWidth - this.Spacing, this._defaultHeight);

            float leftValue = -0.8f * sliderFactor;
            float rightValue = 0.8f * sliderFactor;

            DoActiveKeyframeButtons(framesAt, ref buttonRect);

            if (!this.Loop && position.HasValue)
            {
                position = Widgets.HorizontalSlider(
                                                    sliderRect,
                                                    position.Value,
                                                    leftValue,
                                                    rightValue,
                                                    false,
                                                    label + " " + position,
                                                    leftValue.ToString(),
                                                    rightValue.ToString(),
                                                    0.025f);
                GUI.color = RemoveColor;
                if (Widgets.ButtonText(buttonRect, "- " + this._frameLabel))
                {
                    position = null;
                }

                GUI.color = Color.white;
            }
            else
            {
                Widgets.HorizontalSlider(
                                         sliderRect,
                                         thisFrame.Evaluate(AnimationPercent),
                                         leftValue,
                                         rightValue,
                                         false,
                                         label + " " + position,
                                         "-0.4",
                                         "0.4");

                GUI.color = AddColor;
                if (Widgets.ButtonText(buttonRect, "+ " + this._frameLabel))
                {
                    position = thisFrame.Evaluate(AnimationPercent);
                }

                GUI.color = Color.white;
            }

            editorRect.y += editorRect.height;
        }

        #endregion Protected Methods

        #region Private Methods

        public float lastTime = 0f;

        protected virtual void DrawBackground(Rect rect)
        {
            GUI.DrawTexture(rect, FaceTextures.BackgroundAnimTex);
        }

        private static void DoActiveKeyframeButtons(List<int> framesAt, ref Rect buttonRect)
        {
            if (!framesAt.NullOrEmpty())
            {
                buttonRect.width /= LastInd + 2;

                for (int i = 0; i < LastInd + 1; i++)
                {
                    if (framesAt.Contains(i))
                    {
                        if (i == CurrentFrameInt)
                        {
                            GUI.color = SelectedColor;
                        }

                        if (Widgets.ButtonText(buttonRect, (i + 1).ToString()))
                        {
                            SetCurrentFrame(i);
                        }

                        GUI.color = Color.white;
                    }

                    buttonRect.x += buttonRect.width;
                }
            }
        }

        private static void SetCurrentFrame(int frame)
        {
            {
                _animSlider = frame / (float)LastInd;
            }
        }

        private static void SynchronizeFrames(PawnKeyframe sourceFrame, PawnKeyframe targetFrame)
        {
            targetFrame.BodyAngle = sourceFrame.BodyAngle;
            targetFrame.BodyAngleVertical = sourceFrame.BodyAngleVertical;

            // targetFrame.BodyOffsetVerticalZ = sourceFrame.BodyOffsetVerticalZ;
            targetFrame.BodyOffsetZ = sourceFrame.BodyOffsetZ;
            targetFrame.FootAngle = sourceFrame.FootAngle;

            // targetFrame.FootPositionVerticalZ = sourceFrame.FootPositionVerticalZ;
            targetFrame.HandPositionX = sourceFrame.HandPositionX;
            targetFrame.HandPositionZ = sourceFrame.HandPositionZ;

            targetFrame.FootPositionX = sourceFrame.FootPositionX;
            targetFrame.FootPositionZ = sourceFrame.FootPositionZ;
            targetFrame.FrontPawAngle = sourceFrame.FrontPawAngle;

            // targetFrame.FrontPawPositionVerticalZ = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.FrontPawPositionX = sourceFrame.FrontPawPositionX;
            targetFrame.FrontPawPositionZ = sourceFrame.FrontPawPositionZ;
            targetFrame.HandsSwingAngle = sourceFrame.HandsSwingAngle;

            // targetFrame.HandsSwingPosVertical = sourceFrame.HandsSwingPosVertical;
            targetFrame.HipOffsetHorizontalX = sourceFrame.HipOffsetHorizontalX;
            targetFrame.ShoulderOffsetHorizontalX = sourceFrame.ShoulderOffsetHorizontalX;
        }

        private void AddPortraitWidget(float inRectWidth)
        {
            // Portrait
            Rect rect = new Rect(0, 0, inRectWidth, inRectWidth);

            if (false)
            {

            var skeleton = new FS_Skeleton {joints = new List<FS_Joint>()};
            FS_Joint head = new FS_Joint {color = Color.red};
            FS_Joint neck = new FS_Joint {color = Color.cyan};
            FS_Joint hipCenter = new FS_Joint { color = Color.cyan };

            FS_Joint leftShoulder = new FS_Joint();
            FS_Joint leftElbow = new FS_Joint();
            FS_Joint leftWrist = new FS_Joint();
            FS_Joint leftHand = new FS_Joint();

            FS_Joint rightShoulder = new FS_Joint();
            FS_Joint rightElbow = new FS_Joint();
            FS_Joint rightWrist = new FS_Joint();
            FS_Joint rightHand = new FS_Joint();

            FS_Joint leftHip = new FS_Joint();
            FS_Joint leftKnee = new FS_Joint();
            FS_Joint leftAnkle = new FS_Joint();
            FS_Joint leftFoot = new FS_Joint();
      
            FS_Joint rightHip = new FS_Joint();
            FS_Joint rightKnee = new FS_Joint();
            FS_Joint rightAnkle = new FS_Joint();
            FS_Joint rightFoot = new FS_Joint();


            skeleton.joints.Add(head);
            }

            this.DrawBackground(rect);

            // Draw the pawn's portrait
            Vector2 size = new Vector2(rect.height / 1.4f, rect.height); // 128x180

            Rect position = new Rect(
                                     rect.width * 0.5f - size.x * 0.5f,
                                     rect.height * 0.5f - size.y * 0.5f - 10f,
                                     size.x,
                                     size.y);

            Vector3 cameraOffset = new Vector3(0f, 0f, 0.1f);
            //   RenderTexture image = PortraitsCache.Get(Pawn, size, cameraOffset, this.Zoom);
            {
                RenderTexture renderTexture = new RenderTexture((int)size.x, (int)size.y, 24);
                Find.PortraitRenderer.RenderPortrait(Pawn, renderTexture, cameraOffset, this.Zoom);
                GUI.DrawTexture(position, renderTexture);
                renderTexture.Release();
            }

            // GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, default(Vector3)));
            Widgets.DrawBox(rect);
        }
        private void DrawRotatorBody(float curY, float width)
        {
            float buttWidth = (width - 4 * this.Spacing) / 6;
            Rect butt = new Rect(0f, curY, buttWidth, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x += butt.width + this.Spacing;
            rotation = Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x += butt.width + this.Spacing;
            rotation = Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x += butt.width + this.Spacing;
            rotation = Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x += butt.width + this.Spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Loop", ref this.Loop);
        }

        private void DrawRotatorHead(float curY, float width)
        {
            float buttWidth = (width - 4 * this.Spacing) / 6;
            Rect butt = new Rect(0f, curY, buttWidth, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                HeadRot = rotation;
            }

            butt.x += butt.width + this.Spacing;
            rotation = Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                HeadRot = rotation;
            }

            butt.x += butt.width + this.Spacing;
            rotation = Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                HeadRot = rotation;
            }

            butt.x += butt.width + this.Spacing;
            rotation = Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                HeadRot = rotation;
            }

            butt.x += butt.width + this.Spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Panic", ref Panic);
        }

        private void DrawTimelineButtons(float width, int count)
        {
            if (PawnKeyframes == null)
            {
                return;
            }
            Rect buttonRect = new Rect(0f, 48f, (width - (count - 1) * this.Spacing) / count, 32f);
            foreach (PawnKeyframe keyframe in PawnKeyframes)
            {
                int keyIndex = keyframe.KeyIndex;
                if (keyIndex == CurrentFrameInt)
                {
                    GUI.color = SelectedColor;
                }

                if (keyframe.Status == KeyStatus.Manual)
                {
                    GUI.color *= Color.cyan;
                }

                if (Widgets.ButtonText(buttonRect, (keyIndex + 1).ToString()))
                {
                    SetCurrentFrame(keyIndex);
                }

                GUI.color = Color.white;
                buttonRect.x += buttonRect.width + this.Spacing;
            }
        }
        private void DrawTimelineSlider(int count, float width)
        {
            Rect timeline = new Rect(0f, 0f, width, 40f);
            string label = "Current frame: " + this._frameLabel;
            if (this.Loop)
            {

                AnimationPercent = Time.realtimeSinceStartup - lastTime;
                if (AnimationPercent >= 1f)
                {
                    lastTime = Time.realtimeSinceStartup;
                    AnimationPercent = 0f;
                }


                Widgets.HorizontalSlider(timeline, AnimationPercent, 0f, 1f, false, label);
            }
            else
            {
                _animSlider = Widgets.HorizontalSlider(timeline, AnimationPercent, 0f, 1f, false, label);
            }

        }

        #endregion Private Methods
    }
}