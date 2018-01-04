using System;
using System.Collections.Generic;
using System.Linq;
using FacialStuff.DefOfs;
using FacialStuff.Defs;
using FacialStuff.GraphicsFS;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff
{
    public class MainTabWindow_Animator : MainTabWindow
    {
        #region Public Fields

        public static bool  Colored;
        public static bool  Develop;
        public static bool  Equipment;
        public static bool isOpen;
        public static float HorHeadOffset;
        public static bool  Panic;
        public static float VerHeadOffset;
        public        int   FrameLabel = 1;

        public Pawn Pawn;

        #endregion Public Fields

        #region Private Fields

        private bool             Loop;
        private CompBodyAnimator _compAnim;

        private static readonly Color _addColor     = new Color(0.25f, 1f,    0.25f);
        private static readonly Color _removeColor  = new Color(1f,    0.25f, 0.25f);
        private static readonly Color SelectedColor = new Color(1f,    0.79f, 0.26f);

        private          float _zoom          = 1f;
        private readonly float _defaultHeight = 36f;
        private readonly float _sliderWidth   = 420f;
        private readonly float _spacing       = 12f;
        private readonly float _widthLabel    = 150f;
        private static   float _animationPercent;
        private static   float _animSlider;

        private static Rot4 _bodyRot = Rot4.East;
        private static Rot4 _headRot = Rot4.East;

        private static string  _defPath;
        private static Vector2 _portraitSize = new Vector2(320f, 320f);
        public static bool isMoving = true;

        #endregion Private Fields

        #region Public Properties

        public static float AnimationPercent
        {
            get => _animationPercent;

            set
            {
                _animationPercent = value;
                int frame         = (int) (_animationPercent * LastInd);
                CurrentFrameInt   = frame;
            }
        }

        public static Rot4 BodyRot
        {
            get => _bodyRot;
            set
            {
                _bodyRot = value;
                _headRot = value;
            }
        }

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

        [NotNull]
        public static WalkCycleDef EditorWalkcycle { get; private set; } = WalkCycleDefOf.Biped_Walk;

        public static Rot4 HeadRot => _headRot;

        public static int LastInd => PawnKeyframes.Count - 1;

        public CompBodyAnimator CompAnim => this._compAnim;

        public float CurrentShift
        {
            get => this.CurrentFrame.Shift;

            set
            {
                if (Math.Abs(value) < 0.05f)
                {
                    this.CurrentFrame.Status = KeyStatus.Automatic;
                }
                else
                {
                    this.CurrentFrame.Status = KeyStatus.Manual;
                }

                this.CurrentFrame.Shift = value;
                GameComponent_FacialStuff.BuildWalkCycles(EditorWalkcycle);
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1440f, 900f);
                return base.InitialSize;
            }
        }

        #endregion Public Properties

        #region Private Properties

        private static int CurrentFrameInt { get; set; }
        // public static float horHeadOffset;

        // public static float verHeadOffset;
        private static List<PawnKeyframe> PawnKeyframes => EditorWalkcycle.keyframes;

        private BodyAnimDef BodyAnimDef => this.CompAnim.BodyAnim;

        private PawnKeyframe CurrentFrame => PawnKeyframes[CurrentFrameInt];

        #endregion Private Properties

        #region Public Methods

        public void AddPortraitWidget(float inRectWidth)
        {
            // Portrait
            Rect rect = new Rect(0, 0, inRectWidth, inRectWidth);

            GUI.DrawTexture(rect, FaceTextures.BackgroundAnimTex);

            // Draw the pawn's portrait
            Vector2 size = new Vector2(rect.height / 1.4f, rect.height); // 128x180

            Rect position = new Rect(
                                     rect.width  * 0.5f - size.x * 0.5f,
                                     rect.height * 0.5f - size.y * 0.5f - 10f,
                                     size.x,
                                     size.y);

            GUI.DrawTexture(position, PortraitsCache.Get(this.Pawn, size, new Vector3(0f, 0f, 0.1f), this._zoom));

            // GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, default(Vector3)));
            Widgets.DrawBox(rect);
        }

        public override void DoWindowContents(Rect inRect)
        {
            this.FindRandomPawn();

            PortraitsCache.SetDirty(this.Pawn);

            int count       = PawnKeyframes.Count;
            this.FrameLabel = CurrentFrameInt + 1;

            Rot4 rotation = BodyRot;

            Rect topEditor = inRect.TopPart(0.52f);
            Rect basics    = topEditor.LeftPart(0.375f).ContractedBy(this._spacing);
            basics.width   -= 72f;

            GUI.BeginGroup(basics);

            this.DoBasicSettingsMenu(basics);

            GUI.EndGroup();

            Rect bodySetting = topEditor.RightPart(0.375f).ContractedBy(this._spacing);
            bodySetting.xMin += 36f;

            Rect imageRect =
            new Rect(topEditor.ContractedBy(12f))
            {
            xMin = basics.xMax   + 2 * this._spacing,
            xMax = bodySetting.x - 2 * this._spacing
            };

            GUI.BeginGroup(imageRect);

            float curY = 0f;

            this.AddPortraitWidget(_portraitSize.y);

            curY += _portraitSize.y + this._spacing * 2;

            this.DrawRotatorBody(curY, _portraitSize.y);
            curY += 40f;
            this.DrawRotatorHead(curY, _portraitSize.y);

            GUI.EndGroup();

            GUI.BeginGroup(bodySetting);

            this.DrawBodySettingsEditor(rotation);

            GUI.EndGroup();

            Rect bottomEditor = inRect.BottomPart(0.48f);
            Rect timeline     = bottomEditor.TopPart(0.25f);
            Rect keyframes    = bottomEditor.BottomPart(0.75f);

            GUI.BeginGroup(timeline);

            _animSlider = AnimationPercent;

            this.DrawTimelineSlider(count, timeline.width);

            this.DrawTimelineButtons(timeline.width, count);

            GUI.EndGroup();

            this.DrawKeyframeEditor(keyframes, rotation);

            Rect statusRect = inRect.LeftPart(0.25f).ContractedBy(this._spacing);

            // Widgets.DrawBoxSolid(editor, new Color(1f, 1f, 1f, 0.25f));
            // Widgets.DrawBoxSolid(upperTop, new Color(0f, 0f, 1f, 0.25f));
            // Widgets.DrawBoxSolid(bodyEditor, new Color(1f, 0f, 0f, 0.25f));
            // Widgets.DrawBoxSolid(bottomPart, new Color(1f, 1f, 0f, 0.25f));
            Rect controller = inRect.BottomHalf();
            controller.yMin = statusRect.yMax + this._spacing;

            if (GUI.changed)
            {
                // Close the cycle
                if (!this.Loop && Math.Abs(_animSlider - AnimationPercent) > 0.01f)
                {
                    AnimationPercent = _animSlider;

                    // Log.Message("current frame: " + this.CurrentFrameInt);
                }

                if (CurrentFrameInt == 0)
                {
                    SynchronizeFrames(this.CurrentFrame, PawnKeyframes[LastInd]);
                }

                if (CurrentFrameInt == LastInd)
                {
                    SynchronizeFrames(this.CurrentFrame, PawnKeyframes[0]);
                }

                if (!this.Loop)
                {
                    GameComponent_FacialStuff.BuildWalkCycles();
                }
            }

            // HarmonyPatch_PawnRenderer.Prefix(this.pawn.Drawer.renderer, Vector3.zero, Rot4.East.AsQuat, true, Rot4.East, Rot4.East, RotDrawMode.Fresh, false, false);
            base.DoWindowContents(inRect);
        }

        public override void PostClose()
        {
            this.CompAnim.AnimatorOpen = false;
            this.Pawn                  = null;
            isOpen = false;
            base.PostClose();
        }

        #endregion Public Methods

        #region Private Methods

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
                _animSlider = frame / (float) LastInd;
            }
        }

        private static void SynchronizeFrames(PawnKeyframe sourceFrame, PawnKeyframe targetFrame)
        {
            targetFrame.BodyAngle         = sourceFrame.BodyAngle;
            targetFrame.BodyAngleVertical = sourceFrame.BodyAngleVertical;

            // targetFrame.BodyOffsetVerticalZ = sourceFrame.BodyOffsetVerticalZ;
            targetFrame.BodyOffsetZ = sourceFrame.BodyOffsetZ;
            targetFrame.FootAngle   = sourceFrame.FootAngle;

            // targetFrame.FootPositionVerticalZ = sourceFrame.FootPositionVerticalZ;
            targetFrame.FootPositionX = sourceFrame.FootPositionX;
            targetFrame.FootPositionZ = sourceFrame.FootPositionZ;
            targetFrame.FrontPawAngle = sourceFrame.FrontPawAngle;

            // targetFrame.FrontPawPositionVerticalZ = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.FrontPawPositionX = sourceFrame.FrontPawPositionX;
            targetFrame.FrontPawPositionZ = sourceFrame.FrontPawPositionZ;
            targetFrame.HandsSwingAngle   = sourceFrame.HandsSwingAngle;

            // targetFrame.HandsSwingPosVertical = sourceFrame.HandsSwingPosVertical;
            targetFrame.HipOffsetHorizontalX      = sourceFrame.HipOffsetHorizontalX;
            targetFrame.ShoulderOffsetHorizontalX = sourceFrame.ShoulderOffsetHorizontalX;
        }

        private void DoBasicSettingsMenu(Rect newRect)
        {
            newRect.width                    -= 36f;
            newRect.xMin                     += 36f;
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(newRect);
            string label = this.Pawn.LabelCap + " - " + EditorWalkcycle.LabelCap + " - " + this.BodyAnimDef.LabelCap;

            listingStandard.Label(label);

            this._zoom = listingStandard.Slider(this._zoom, 0.5f, 1.5f);

            listingStandard.CheckboxLabeled("Develop", ref Develop);
            listingStandard.CheckboxLabeled("Colored", ref Colored);
            listingStandard.CheckboxLabeled("Moving", ref isMoving);

            // listing_Standard.CheckboxLabeled("Equipment", ref Equipment);

            // listing_Standard.Label(horHeadOffset.ToString("N2") + " - " + verHeadOffset.ToString("N2"));
            // horHeadOffset = listing_Standard.Slider(horHeadOffset, -1f, 1f);
            // verHeadOffset = listing_Standard.Slider(verHeadOffset, -1f, 1f);
            listingStandard.Label(this.BodyAnimDef.offCenterX.ToString("N2"));
            this.BodyAnimDef.offCenterX = listingStandard.Slider(this.BodyAnimDef.offCenterX, -0.2f, 0.2f);

            if (listingStandard.ButtonText(this.Pawn.LabelCap))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn current in from bsm in this.Pawn.Map.mapPawns.AllPawnsSpawned
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

                                                     this.Pawn = smLocal;
                                                     smLocal.GetCompAnim(out this._compAnim);

                                                     // editorWalkcycle = this.CompAnim.Props.defaultCycleWalk;
                                                     if (this.CompAnim.BodyAnim.walkCycles.Any())
                                                     {
                                                         EditorWalkcycle =
                                                         this.CompAnim.BodyAnim.walkCycles.FirstOrDefault().Value;
                                                     }

                                                     this.CompAnim.AnimatorOpen = true;
                                                 }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (listingStandard.ButtonText(EditorWalkcycle.LabelCap))
            {
                List<string>          exists = new List<string>();
                List<FloatMenuOption> list   = new List<FloatMenuOption>();
                this.BodyAnimDef.walkCycles.Clear();

                foreach (WalkCycleDef walkcycle in (from bsm in DefDatabase<WalkCycleDef>.AllDefs
                                                    orderby bsm.LabelCap
                                                    select bsm)
                                                  .TakeWhile(current => this.BodyAnimDef.WalkCycleType != "None")
                                                  .Where(current => current.WalkCycleType              ==
                                                                    this.BodyAnimDef.WalkCycleType))
                {
                    list.Add(new FloatMenuOption(walkcycle.LabelCap, delegate { EditorWalkcycle = walkcycle; }));
                    exists.Add(walkcycle.locomotionUrgency.ToString());
                    this.BodyAnimDef.walkCycles.Add(walkcycle.locomotionUrgency, walkcycle);
                }

                string[] names = Enum.GetNames(typeof(LocomotionUrgency));
                for (int index = 0; index < names.Length; index++)
                {
                    string            name   = names[index];
                    LocomotionUrgency myenum = (LocomotionUrgency) Enum.ToObject(typeof(LocomotionUrgency), index);

                    if (exists.Contains(myenum.ToString()))
                    {
                        continue;
                    }

                    list.Add(
                             new FloatMenuOption(
                                                 "Add new " + this.BodyAnimDef.WalkCycleType + "_" + myenum,
                                                 delegate
                                                 {
                                                     WalkCycleDef newCycle = new WalkCycleDef();
                                                     newCycle.defName      =
                                                     newCycle.label        =
                                                     this.BodyAnimDef.WalkCycleType + "_" + name;
                                                     newCycle.locomotionUrgency = myenum;
                                                     newCycle.WalkCycleType     = this.BodyAnimDef.WalkCycleType;
                                                     GameComponent_FacialStuff.BuildWalkCycles(newCycle);
                                                     EditorWalkcycle = newCycle;

                                                     this.BodyAnimDef.walkCycles.Add(myenum, newCycle);
                                                 }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (listingStandard.ButtonText("This pawn is using: " + this.BodyAnimDef.WalkCycleType))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                List<WalkCycleDef> listy = DefDatabase<WalkCycleDef>.AllDefsListForReading;

                List<string> stringsy = new List<string>();

                foreach (WalkCycleDef cycleDef in listy)
                {
                    if (!stringsy.Contains(cycleDef.WalkCycleType))
                    {
                        stringsy.Add(cycleDef.WalkCycleType);
                    }
                }

                foreach (string s in stringsy)
                {
                    list.Add(new FloatMenuOption(s, delegate { this.BodyAnimDef.WalkCycleType = s; }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            /*
                        if (listing_Standard.ButtonText("Walk cycle only for: " + EditorWalkcycle.WalkCycleType))
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();

                            string[] names = Enum.GetNames(typeof(WalkCycleType));
                            for (int index = 0; index < names.Length; index++)
                            {
                                string name = names[index];
                                WalkCycleType myenum = (WalkCycleType)Enum.ToObject(typeof(WalkCycleType), index);
                                list.Add(
                                    new FloatMenuOption(
                                        name,
                                        delegate { EditorWalkcycle.WalkCycleType = myenum; }));
                            }

                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                        if (listing_Standard.ButtonText("Locomotion Urgency: " + EditorWalkcycle.locomotionUrgency))
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();

                            string[] names = Enum.GetNames(typeof(LocomotionUrgency));
                            for (int index = 0; index < names.Length; index++)
                            {
                                string name = names[index];
                                LocomotionUrgency myenum = (LocomotionUrgency)Enum.ToObject(typeof(LocomotionUrgency), index);
                                list.Add(
                                    new FloatMenuOption(
                                        name,
                                        delegate { EditorWalkcycle.locomotionUrgency = myenum; }));
                            }

                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                        */
            if (listingStandard.ButtonText("Add 1 keyframe: "))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                for (int index = 0; index < PawnKeyframes.Count - 1; index++)
                {
                    PawnKeyframe keyframe = PawnKeyframes[index];
                    list.Add(
                             new FloatMenuOption(
                                                 "Add after " + (keyframe.KeyIndex + 1),
                                                 delegate
                                                 {
                                                     PawnKeyframes.Insert(keyframe.KeyIndex + 1, new PawnKeyframe());

                                                     this.ReIndexKeyframes();
                                                 }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (CurrentFrameInt != 0 && CurrentFrameInt != LastInd)
            {
                if (listingStandard.ButtonText("Remove current keyframe " + (CurrentFrameInt + 1)))
                {
                    PawnKeyframes.RemoveAt(CurrentFrameInt);
                    this.ReIndexKeyframes();
                }
            }

            if (CurrentFrameInt != 0 && CurrentFrameInt != LastInd)
            {
                float shift = this.CurrentShift;
                shift       = Mathf.Round(listingStandard.Slider(shift, -1f, 1f) * 20f) / 20f;
                if (GUI.changed)
                {
                    this.CurrentShift = shift;
                }
            }

            listingStandard.Gap();
            string configFolder = DefPath;
            if (listingStandard.ButtonText("Export BodyDef"))
            {
                string filePath = configFolder + "/BodyAnimDefs/" + this.BodyAnimDef.defName + ".xml";

                Find.WindowStack.Add(
                                     Dialog_MessageBox.CreateConfirmation(
                                                                          "Confirm overwriting " +
                                                                          filePath,
                                                                          delegate
                                                                          {
                                                                              ExportAnimDefs.Defs animDef =
                                                                              new ExportAnimDefs.Defs(this.BodyAnimDef);

                                                                              DirectXmlSaver.SaveDataObject(
                                                                                                            animDef,
                                                                                                            filePath);
                                                                          },
                                                                          true));

                // BodyAnimDef animDef = this.bodyAnimDef;
            }

            if (listingStandard.ButtonText("Export WalkCycle"))
            {
                string path = configFolder + "/WalkCycleDefs/" + EditorWalkcycle.defName + ".xml";

                Find.WindowStack.Add(
                                     Dialog_MessageBox.CreateConfirmation(
                                                                          "Confirm overwriting " + path,
                                                                          delegate
                                                                          {
                                                                              ExportCycleDefs.Defs cycle =
                                                                              new ExportCycleDefs.
                                                                              Defs(EditorWalkcycle);

                                                                              DirectXmlSaver.SaveDataObject(
                                                                                                            cycle,
                                                                                                            path);
                                                                          },
                                                                          true));
            }

            listingStandard.End();
        }

        private void DrawBodySettingsEditor(Rot4 rotation)
        {
            Rect sliderRect = new Rect(0, 0, this._sliderWidth, 40f);

            // this.DrawBodyStats("legLength", ref bodyAnimDef.legLength, ref sliderRect);
            // this.DrawBodyStats("hipOffsetVerticalFromCenter",
            // ref bodyAnimDef.hipOffsetVerticalFromCenter, ref sliderRect);
            Vector3 shoulderOffset = this.BodyAnimDef.shoulderOffsets[rotation.AsInt];

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

            Vector3 hipOffset = this.BodyAnimDef.hipOffsets[rotation.AsInt];
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
            if (rotation                == Rot4.West)
            {
                hipFront = hipOffset.y < 0;
            }

            this.DrawBodyStats("hipOffsetX", ref hipOffset.x, ref sliderRect);
            this.DrawBodyStats("hipOffsetZ", ref hipOffset.z, ref sliderRect);
            // this.DrawBodyStats("hipFront",   ref hipFront,    ref sliderRect);

            if (GUI.changed)
            {
                this.SetNewVector(rotation, shoulderOffset, this.BodyAnimDef.shoulderOffsets, front);
                this.SetNewVector(rotation, hipOffset,      this.BodyAnimDef.hipOffsets,      hipFront);
            }

            this.DrawBodyStats("armLength",      ref this.BodyAnimDef.armLength,      ref sliderRect);
            this.DrawBodyStats("extraLegLength", ref this.BodyAnimDef.extraLegLength, ref sliderRect);
        }

        private void DrawBodyStats(string label, ref float value, ref Rect sliderRect)
        {
            float left  = -1.5f;
            float right = 1.5f;
            value       = Widgets.HorizontalSlider(
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

        private void DrawBodyStats(string label, ref bool flipped, ref Rect sliderRect)
        {
            Widgets.CheckboxLabeled(sliderRect, label, ref flipped);

            sliderRect.y += sliderRect.height + 8f;
        }

        private void DrawKeyframeEditor(Rect controller, Rot4 rotation)
        {
            Rect leftController  = controller.LeftHalf();
            Rect rightController = controller.RightHalf();
            leftController.xMax  -= this._spacing;

            rightController.xMin += this._spacing;
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
                List<int>          framesAt;
                List<PawnKeyframe> frames = PawnKeyframes;
                if (rotation.IsHorizontal)
                {
                    framesAt = (from keyframe in frames where keyframe.FootPositionX.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.FootPositionX,
                                     ref editorRect,
                                     EditorWalkcycle.FootPositionX,
                                     "FootPosX",
                                     framesAt);

                    framesAt = (from keyframe in frames where keyframe.FootPositionZ.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.FootPositionZ,
                                     ref editorRect,
                                     EditorWalkcycle.FootPositionZ,
                                     "FootPosY",
                                     framesAt);

                    framesAt = (from keyframe in frames where keyframe.FootAngle.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetAngle(
                                  ref this.CurrentFrame.FootAngle,
                                  ref editorRect,
                                  EditorWalkcycle.FootAngle,
                                  "FootAngle",
                                  framesAt);

                    framesAt = (from keyframe in frames
                                where keyframe.HipOffsetHorizontalX.HasValue
                                select keyframe.KeyIndex).ToList();

                    this.SetPosition(
                                     ref this.CurrentFrame.HipOffsetHorizontalX,
                                     ref editorRect,
                                     EditorWalkcycle.HipOffsetHorizontalX,
                                     "HipOffsetHorizontalX",
                                     framesAt);

                    // Quadruped
                }

                // else
                // {
                // framesAt = (from keyframe in frames
                // where keyframe.FootPositionVerticalZ.HasValue
                // select keyframe.KeyIndex).ToList();
                // this.SetPosition(
                // ref thisFrame.FootPositionVerticalZ,
                // ref editorRect,
                // EditorWalkcycle.FootPositionVerticalZ,
                // "FootPosVerticalY", framesAt);
                // }
                GUI.EndGroup();

                GUI.BeginGroup(rightController);

                editorRect.x = 0f;
                editorRect.y = 0f;

                if (this.CompAnim.Props.bipedWithHands)
                {
                    this.SetAngleShoulder(ref EditorWalkcycle.shoulderAngle, ref editorRect, "ShoulderAngle");

                    framesAt =
                    (from keyframe in frames where keyframe.HandsSwingAngle.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetAngle(
                                  ref this.CurrentFrame.HandsSwingAngle,
                                  ref editorRect,
                                  EditorWalkcycle.HandsSwingAngle,
                                  "HandSwing",
                                  framesAt);

                    framesAt = (from keyframe in frames
                                where keyframe.ShoulderOffsetHorizontalX.HasValue
                                select keyframe.KeyIndex).ToList();
                    this.SetPosition(
                                     ref this.CurrentFrame.ShoulderOffsetHorizontalX,
                                     ref editorRect,
                                     EditorWalkcycle.ShoulderOffsetHorizontalX,
                                     "ShoulderOffsetHorizontalX",
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
                                         EditorWalkcycle.FrontPawPositionX,
                                         "FrontPawPositionX",
                                         framesAt);

                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawPositionZ.HasValue
                                    select keyframe.KeyIndex).ToList();

                        this.SetPosition(
                                         ref this.CurrentFrame.FrontPawPositionZ,
                                         ref editorRect,
                                         EditorWalkcycle.FrontPawPositionZ,
                                         "FrontPawPositionZ",
                                         framesAt);

                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawAngle.HasValue
                                    select keyframe.KeyIndex).ToList();

                        this.SetAngle(
                                      ref this.CurrentFrame.FrontPawAngle,
                                      ref editorRect,
                                      EditorWalkcycle.FrontPawAngle,
                                      "FrontPawAngle",
                                      framesAt);
                    }

                    framesAt = (from keyframe in frames where keyframe.BodyAngle.HasValue select keyframe.KeyIndex)
                   .ToList();

                    this.SetAngle(
                                  ref this.CurrentFrame.BodyAngle,
                                  ref editorRect,
                                  EditorWalkcycle.BodyAngle,
                                  "BodyAngle",
                                  framesAt);
                }
                else
                {
                    if (this.CompAnim.Props.bipedWithHands)
                    {
                        // framesAt = (from keyframe in frames
                        // where keyframe.HandsSwingPosVertical.HasValue
                        // select keyframe.KeyIndex).ToList();
                        // this.SetPosition(
                        // ref thisFrame.HandsSwingPosVertical,
                        // ref editorRect,
                        // EditorWalkcycle.HandsSwingPosVertical,
                        // "HandsSwingPosVertical", framesAt);
                    }

                    // framesAt = (from keyframe in frames
                    // where keyframe.FrontPawPositionVerticalZ.HasValue
                    // select keyframe.KeyIndex).ToList();
                    // if (this.CompAnim.Props.quadruped)
                    // {
                    // this.SetPosition(
                    // ref thisFrame.FrontPawPositionVerticalZ,
                    // ref editorRect,
                    // EditorWalkcycle.FrontPawPositionVerticalZ,
                    // "FrontPawPosVerticalY", framesAt);
                    // }
                    // framesAt = (from keyframe in frames
                    // where keyframe.BodyOffsetVerticalZ.HasValue
                    // select keyframe.KeyIndex).ToList();
                    // this.SetPosition(
                    // ref thisFrame.BodyOffsetVerticalZ,
                    // ref editorRect,
                    // EditorWalkcycle.BodyOffsetVerticalZ,
                    // "BodyOffsetVerticalZ", framesAt);
                    framesAt = (from keyframe in frames
                                where keyframe.BodyAngleVertical.HasValue
                                select keyframe.KeyIndex).ToList();
                    this.SetAngle(
                                  ref this.CurrentFrame.BodyAngleVertical,
                                  ref editorRect,
                                  EditorWalkcycle.BodyAngleVertical,
                                  "BodyAngleVertical",
                                  framesAt);
                }

                framesAt =
                (from keyframe in frames where keyframe.BodyOffsetZ.HasValue select keyframe.KeyIndex).ToList();

                this.SetPosition(
                                 ref this.CurrentFrame.BodyOffsetZ,
                                 ref editorRect,
                                 EditorWalkcycle.BodyOffsetZ,
                                 "BodyOffsetZ",
                                 framesAt);

                GUI.EndGroup();
            }
        }

        private void DrawRotatorBody(float curY, float width)
        {
            float buttWidth = (width - 4 * this._spacing) / 6;
            Rect  butt      = new Rect(0f, curY, buttWidth, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x   += butt.width + this._spacing;
            rotation =  Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x   += butt.width + this._spacing;
            rotation =  Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x   += butt.width + this._spacing;
            rotation =  Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }

            butt.x += butt.width + this._spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Loop", ref this.Loop);
        }

        private void DrawRotatorHead(float curY, float width)
        {
            float buttWidth = (width - 4 * this._spacing) / 6;
            Rect  butt      = new Rect(0f, curY, buttWidth, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                _headRot = rotation;
            }

            butt.x   += butt.width + this._spacing;
            rotation =  Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                _headRot = rotation;
            }

            butt.x   += butt.width + this._spacing;
            rotation =  Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                _headRot = rotation;
            }

            butt.x   += butt.width + this._spacing;
            rotation =  Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                _headRot = rotation;
            }

            butt.x += butt.width + this._spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Panic", ref Panic);
        }

        private void DrawTimelineButtons(float width, int count)
        {
            Rect buttonRect = new Rect(0f, 48f, (width - (count - 1) * this._spacing) / count, 32f);
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

                GUI.color    =  Color.white;
                buttonRect.x += buttonRect.width + this._spacing;
            }
        }

        private void DrawTimelineSlider(int count, float width)
        {
            Rect   timeline = new Rect(0f, 0f, width, 40f);
            string label    = "Current frame: " + this.FrameLabel;
            if (this.Loop)
            {
                AnimationPercent += 0.01f;
                if (AnimationPercent > 1f)
                {
                    AnimationPercent = 0f;
                }

                Widgets.HorizontalSlider(timeline, AnimationPercent, 0f, 1f, false, label);
            }
            else
            {
                _animSlider = Widgets.HorizontalSlider(timeline, AnimationPercent, 0f, 1f, false, label);
            }
        }

        private void FindRandomPawn()
        {
            if (this.Pawn == null)
            {
                this.Pawn = Find.AnyPlayerHomeMap.FreeColonistsForStoryteller.FirstOrDefault();

                this.Pawn.GetCompAnim(out this._compAnim);
                this.CompAnim.AnimatorOpen = true;
                if (this.CompAnim.BodyAnim.walkCycles.Any())
                {
                    EditorWalkcycle = this.CompAnim.BodyAnim.walkCycles.FirstOrDefault().Value;
                }

                isOpen = true;
                // editorWalkcycle = this.CompAnim.bodyAnim.walkCycles[0];
            }
        }

        private void RedistKeys()
        {
        }

        private void ReIndexKeyframes()
        {
            for (int index = 0; index < PawnKeyframes.Count; index++)
            {
                PawnKeyframe keyframe = PawnKeyframes[index];
                keyframe.KeyIndex     = index;
            }
        }

        private void SetAngle(
        ref float?  angle,
        ref Rect    editorRect,
        SimpleCurve thisFrame,
        string      label,
        List<int>   framesAt)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this._sliderWidth, this._defaultHeight);
            Rect buttonRect = new Rect(
                                       sliderRect.xMax + this._spacing,
                                       editorRect.y,
                                       editorRect.width - this._sliderWidth - this._spacing, this._defaultHeight);

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
                GUI.color = _removeColor;
                if (Widgets.ButtonText(buttonRect, "- " + this.FrameLabel))
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
                GUI.color = _addColor;
                if (Widgets.ButtonText(buttonRect, "+ " + this.FrameLabel))
                {
                    angle = thisFrame.Evaluate(AnimationPercent);
                }

                GUI.color = Color.white;
            }

            editorRect.y += editorRect.height;
        }

        private void SetAngleShoulder(ref float angle, ref Rect editorRect, string label)
        {
            Rect labelRect  = new Rect(editorRect.x,   editorRect.y, this._widthLabel,  this._defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this._sliderWidth, this._defaultHeight);

            Widgets.Label(labelRect, label + " " + angle);
            angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle, 0, 180f));

            editorRect.y += editorRect.height;
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

        private void SetPosition(
        ref float?  position,
        ref Rect    editorRect,
        SimpleCurve thisFrame,
        string      label,
        List<int>   framesAt)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this._sliderWidth, this._defaultHeight);
            Rect buttonRect = new Rect(
                                       sliderRect.xMax + this._spacing,
                                       editorRect.y,
                                       editorRect.width - this._sliderWidth - this._spacing, this._defaultHeight);

            float leftValue  = -0.8f;
            float rightValue = 0.8f;

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
                GUI.color = _removeColor;
                if (Widgets.ButtonText(buttonRect, "- " + this.FrameLabel))
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

                GUI.color = _addColor;
                if (Widgets.ButtonText(buttonRect, "+ " + this.FrameLabel))
                {
                    position = thisFrame.Evaluate(AnimationPercent);
                }

                GUI.color = Color.white;
            }

            editorRect.y += editorRect.height;
        }

        #endregion Private Methods
    }
}