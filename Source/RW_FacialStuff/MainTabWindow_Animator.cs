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
    using Verse.AI;

    public class MainTabWindow_Animator : MainTabWindow
    {
        public Pawn pawn;

        public bool loop;

        private static int currentFrameInt = 0;

        public static int CurrentFrameInt
        {
            get
            {
                return currentFrameInt;
            }
        }

        public int frameLabel = 1;

        public static bool develop = false;
        public static bool Colored;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1440f, 900f);
                return base.InitialSize;
            }
        }
        private static float animationPercent;

        private void RedistKeys()
        {
        }

        public override void DoWindowContents(Rect inRect)
        {
            this.FindRandomPawn();


            PortraitsCache.SetDirty(this.pawn);

            int count = pawnKeyframes.Count;
            this.frameLabel = CurrentFrameInt + 1;

            Rot4 rotation = BodyRot;

            Rect topEditor = inRect.TopPart(0.52f);
            Rect basics = topEditor.LeftPart(0.375f).ContractedBy(this.spacing);
            basics.width -= 72f;

            GUI.BeginGroup(basics);

            this.DoBasicSettingsMenu(basics);

            GUI.EndGroup();


            Rect bodySetting = topEditor.RightPart(0.375f).ContractedBy(this.spacing);
            bodySetting.xMin += 36f;

            Rect imageRect = new Rect(topEditor.ContractedBy(12f))
            {
                xMin = basics.xMax + 2 * this.spacing,
                xMax = bodySetting.x - 2 * this.spacing
            };

            GUI.BeginGroup(imageRect);

            float curY = 0f;

            this.AddPortraitWidget(portraitSize.y);

            curY += portraitSize.y + spacing * 2;

            this.DrawRotatorBody(curY, portraitSize.y);
            curY += 40f;
            this.DrawRotatorHead(curY, portraitSize.y);

            GUI.EndGroup();


            GUI.BeginGroup(bodySetting);

            this.DrawBodySettingsEditor(rotation);

            GUI.EndGroup();

            Rect bottomEditor = inRect.BottomPart(0.48f);
            Rect timeline = bottomEditor.TopPart(0.25f);
            Rect keyframes = bottomEditor.BottomPart(0.75f);

            GUI.BeginGroup(timeline);

            animSlider = AnimationPercent;

            this.DrawTimelineSlider(count, timeline.width);

            this.DrawTimelineButtons(timeline.width, count);

            GUI.EndGroup();

            this.DrawKeyframeEditor(keyframes, rotation);


            Rect statusRect = inRect.LeftPart(0.25f).ContractedBy(this.spacing);




            //  Widgets.DrawBoxSolid(editor, new Color(1f, 1f, 1f, 0.25f));
            //  Widgets.DrawBoxSolid(upperTop, new Color(0f, 0f, 1f, 0.25f));
            //  Widgets.DrawBoxSolid(bodyEditor, new Color(1f, 0f, 0f, 0.25f));
            //  Widgets.DrawBoxSolid(bottomPart, new Color(1f, 1f, 0f, 0.25f));




            Rect controller = inRect.BottomHalf();
            controller.yMin = statusRect.yMax + this.spacing;



            if (GUI.changed)
            {
                // Close the cycle
                if (!this.loop && animSlider != AnimationPercent)
                {
                    AnimationPercent = animSlider;
                    // Log.Message("current frame: " + this.CurrentFrameInt);
                }

                if (CurrentFrameInt == 0)
                {
                    SynchronizeFrames(this.currentFrame, pawnKeyframes[LastInd]);
                }
                if (CurrentFrameInt == LastInd)
                {
                    SynchronizeFrames(this.currentFrame, pawnKeyframes[0]);
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
            // targetFrame.BodyOffsetVerticalZ = sourceFrame.BodyOffsetVerticalZ;
            targetFrame.BodyOffsetZ = sourceFrame.BodyOffsetZ;
            targetFrame.FootAngle = sourceFrame.FootAngle;
            //  targetFrame.FootPositionVerticalZ = sourceFrame.FootPositionVerticalZ;
            targetFrame.FootPositionX = sourceFrame.FootPositionX;
            targetFrame.FootPositionZ = sourceFrame.FootPositionZ;
            targetFrame.FrontPawAngle = sourceFrame.FrontPawAngle;
            // targetFrame.FrontPawPositionVerticalZ = sourceFrame.FrontPawPositionVerticalZ;
            targetFrame.FrontPawPositionX = sourceFrame.FrontPawPositionX;
            targetFrame.FrontPawPositionZ = sourceFrame.FrontPawPositionZ;
            targetFrame.HandsSwingAngle = sourceFrame.HandsSwingAngle;
            //  targetFrame.HandsSwingPosVertical = sourceFrame.HandsSwingPosVertical;
            targetFrame.HipOffsetHorizontalX = sourceFrame.HipOffsetHorizontalX;
            targetFrame.ShoulderOffsetHorizontalX = sourceFrame.ShoulderOffsetHorizontalX;
        }

        private float sliderWidth = 420f;
        private void DrawBodySettingsEditor(Rot4 rotation)
        {

            Rect sliderRect = new Rect(0, 0, this.sliderWidth, 40f);


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
        private PawnKeyframe currentFrame => pawnKeyframes[CurrentFrameInt];

        private void DrawKeyframeEditor(Rect controller, Rot4 rotation)
        {
            Rect leftController = controller.LeftHalf();
            Rect rightController = controller.RightHalf();
            leftController.xMax -= this.spacing;

            rightController.xMin += this.spacing;

            {
                GUI.BeginGroup(leftController);
                Rect editorRect = new Rect(0f, 0f, leftController.width, 56f);

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

                List<int> framesAt;
                var frames = pawnKeyframes;
                if (rotation.IsHorizontal)
                {

                    framesAt = (from keyframe in frames
                                where keyframe.FootPositionX.HasValue
                                select keyframe.keyIndex).ToList();

                    this.SetPosition(ref currentFrame.FootPositionX, ref editorRect, EditorWalkcycle.FootPositionX, "FootPosX", framesAt);


                    framesAt = (from keyframe in frames
                                where keyframe.FootPositionZ.HasValue
                                select keyframe.keyIndex).ToList();

                    this.SetPosition(
                        ref currentFrame.FootPositionZ,
                        ref editorRect,
                        EditorWalkcycle.FootPositionZ,
                        "FootPosY", framesAt);

                    framesAt = (from keyframe in frames
                                where keyframe.FootAngle.HasValue
                                select keyframe.keyIndex).ToList();

                    this.SetAngle(ref currentFrame.FootAngle, ref editorRect, EditorWalkcycle.FootAngle, "FootAngle", framesAt);

                    framesAt = (from keyframe in frames
                                where keyframe.HipOffsetHorizontalX.HasValue
                                select keyframe.keyIndex).ToList();

                    this.SetPosition(
                        ref currentFrame.HipOffsetHorizontalX,
                        ref editorRect,
                        EditorWalkcycle.HipOffsetHorizontalX,
                        "HipOffsetHorizontalX", framesAt);

                    // Quadruped
                }
                // else
                // {
                //     framesAt = (from keyframe in frames
                //                 where keyframe.FootPositionVerticalZ.HasValue
                //                 select keyframe.keyIndex).ToList();
                //
                //     this.SetPosition(
                //         ref thisFrame.FootPositionVerticalZ,
                //         ref editorRect,
                //         EditorWalkcycle.FootPositionVerticalZ,
                //         "FootPosVerticalY", framesAt);
                // }

                GUI.EndGroup();

                GUI.BeginGroup(rightController);

                editorRect.x = 0f;
                editorRect.y = 0f;






                if (this.CompAnim.Props.bipedWithHands)
                {
                    this.SetAngleShoulder(ref EditorWalkcycle.shoulderAngle, ref editorRect, "ShoulderAngle");

                    framesAt = (from keyframe in frames
                                where keyframe.HandsSwingAngle.HasValue
                                select keyframe.keyIndex).ToList();

                    this.SetAngle(ref currentFrame.HandsSwingAngle, ref editorRect, EditorWalkcycle.HandsSwingAngle, "HandSwing", framesAt);

                    framesAt = (from keyframe in frames
                                where keyframe.ShoulderOffsetHorizontalX.HasValue
                                select keyframe.keyIndex).ToList();
                    this.SetPosition(
                        ref currentFrame.ShoulderOffsetHorizontalX,
                        ref editorRect,
                        EditorWalkcycle.ShoulderOffsetHorizontalX,
                        "ShoulderOffsetHorizontalX", framesAt);
                }

                if (rotation.IsHorizontal)
                {
                    if (this.CompAnim.Props.quadruped)
                    {
                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawPositionX.HasValue
                                    select keyframe.keyIndex).ToList();
                        this.SetPosition(
                            ref currentFrame.FrontPawPositionX,
                            ref editorRect,
                            EditorWalkcycle.FrontPawPositionX,
                            "FrontPawPositionX", framesAt);

                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawPositionZ.HasValue
                                    select keyframe.keyIndex).ToList();

                        this.SetPosition(
                            ref currentFrame.FrontPawPositionZ,
                            ref editorRect,
                            EditorWalkcycle.FrontPawPositionZ,
                            "FrontPawPositionZ", framesAt);

                        framesAt = (from keyframe in frames
                                    where keyframe.FrontPawAngle.HasValue
                                    select keyframe.keyIndex).ToList();

                        this.SetAngle(ref currentFrame.FrontPawAngle, ref editorRect, EditorWalkcycle.FrontPawAngle, "FrontPawAngle", framesAt);

                    }




                    framesAt = (from keyframe in frames
                                where keyframe.BodyAngle.HasValue
                                select keyframe.keyIndex).ToList();

                    this.SetAngle(ref currentFrame.BodyAngle, ref editorRect, EditorWalkcycle.BodyAngle, "BodyAngle", framesAt);
                }
                else
                {
                    if (this.CompAnim.Props.bipedWithHands)
                    {

                        // framesAt = (from keyframe in frames
                        //             where keyframe.HandsSwingPosVertical.HasValue
                        //             select keyframe.keyIndex).ToList();
                        // this.SetPosition(
                        //     ref thisFrame.HandsSwingPosVertical,
                        //     ref editorRect,
                        //     EditorWalkcycle.HandsSwingPosVertical,
                        //     "HandsSwingPosVertical", framesAt);
                    }
                    // framesAt = (from keyframe in frames
                    //             where keyframe.FrontPawPositionVerticalZ.HasValue
                    //             select keyframe.keyIndex).ToList();
                    // if (this.CompAnim.Props.quadruped)
                    // {
                    //     this.SetPosition(
                    //         ref thisFrame.FrontPawPositionVerticalZ,
                    //         ref editorRect,
                    //         EditorWalkcycle.FrontPawPositionVerticalZ,
                    //         "FrontPawPosVerticalY", framesAt);
                    // }
                    // framesAt = (from keyframe in frames
                    //             where keyframe.BodyOffsetVerticalZ.HasValue
                    //             select keyframe.keyIndex).ToList();
                    //
                    // this.SetPosition(
                    //     ref thisFrame.BodyOffsetVerticalZ,
                    //     ref editorRect,
                    //     EditorWalkcycle.BodyOffsetVerticalZ,
                    //     "BodyOffsetVerticalZ", framesAt);


                    framesAt = (from keyframe in frames
                                where keyframe.BodyAngleVertical.HasValue
                                select keyframe.keyIndex).ToList();
                    this.SetAngle(
                        ref currentFrame.BodyAngleVertical,
                        ref editorRect,
                        EditorWalkcycle.BodyAngleVertical,
                        "BodyAngleVertical", framesAt);
                }


                framesAt = (from keyframe in frames
                            where keyframe.BodyOffsetZ.HasValue
                            select keyframe.keyIndex).ToList();

                this.SetPosition(
                    ref currentFrame.BodyOffsetZ,
                    ref editorRect,
                    EditorWalkcycle.BodyOffsetZ,
                    "BodyOffsetZ",
                    framesAt);

                GUI.EndGroup();
            }
        }

        private void DoBasicSettingsMenu(Rect newRect)
        {
            newRect.width -= 36f;
            newRect.xMin += 36f;
            Listing_Standard listing_Standard = new Listing_Standard();

            listing_Standard.Begin(newRect);

            listing_Standard.Label(this.pawn.LabelCap);
            listing_Standard.Label(EditorWalkcycle.LabelCap);
            listing_Standard.Label(this.bodyAnimDef.LabelCap);

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
                                    editorWalkcycle = this.CompAnim.bodyAnim.walkCycles.FirstOrDefault().Value;

                                    this.CompAnim.AnimatorOpen = true;
                                }));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (listing_Standard.ButtonText(EditorWalkcycle.LabelCap))
            {
                var exists = new List<string>();
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                this.bodyAnimDef.walkCycles.Clear();

                foreach (WalkCycleDef walkcycle in (from bsm in DefDatabase<WalkCycleDef>.AllDefs
                                                  orderby bsm.LabelCap
                                                  select bsm).TakeWhile(current => this.bodyAnimDef.WalkCycleType != "None").Where(current => current.WalkCycleType == this.bodyAnimDef.WalkCycleType))
                {
                    list.Add(new FloatMenuOption(walkcycle.LabelCap, delegate { editorWalkcycle = walkcycle; }));
                    exists.Add(walkcycle.locomotionUrgency.ToString());
                    this.bodyAnimDef.walkCycles.Add(walkcycle.locomotionUrgency, walkcycle);
                }
                string[] names = Enum.GetNames(typeof(LocomotionUrgency));
                for (int index = 0; index < names.Length; index++)
                {
                    string name = names[index];
                    LocomotionUrgency myenum = (LocomotionUrgency)Enum.ToObject(typeof(LocomotionUrgency), index);

                    if (exists.Contains(myenum.ToString()))
                    {
                        continue;
                    }
                    list.Add(
                        new FloatMenuOption(
                            "Add new " + this.bodyAnimDef.WalkCycleType + "_" + myenum,
                            delegate
                                {
                                    var newCycle = new WalkCycleDef();
                                    newCycle.defName = newCycle.label = this.bodyAnimDef.WalkCycleType + "_" + name;
                                    newCycle.locomotionUrgency = myenum;
                                    newCycle.WalkCycleType = this.bodyAnimDef.WalkCycleType;
                                    GameComponent_FacialStuff.BuildWalkCycles(newCycle);
                                    editorWalkcycle = newCycle;

                                    this.bodyAnimDef.walkCycles.Add(myenum, newCycle);

                                }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            if (listing_Standard.ButtonText("This pawn is using: " + this.bodyAnimDef.WalkCycleType))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                var listy = DefDatabase<WalkCycleDef>.AllDefsListForReading;

                var stringsy = new List<string>();

                foreach (WalkCycleDef cycleDef in listy)
                {
                    if (!stringsy.Contains(cycleDef.WalkCycleType))
                    {
                        stringsy.Add(cycleDef.WalkCycleType);
                    }
                }
                foreach (string s in stringsy)
                {
                    list.Add(
                        new FloatMenuOption(
                            s,
                            delegate { this.bodyAnimDef.WalkCycleType = s; }));
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

            if (listing_Standard.ButtonText("Add 1 keyframe: "))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                for (int index = 0; index < pawnKeyframes.Count - 1; index++)
                {
                    PawnKeyframe keyframe = pawnKeyframes[index];
                    list.Add(
                        new FloatMenuOption(
                            "Add after " + (keyframe.keyIndex + 1),
                            delegate
                                {
                                    pawnKeyframes.Insert(keyframe.keyIndex + 1, new PawnKeyframe());

                                    this.ReIndexKeyframes();
                                }));
                }

                Find.WindowStack.Add(new FloatMenu(list));

            }
            if (CurrentFrameInt != 0 && CurrentFrameInt != LastInd)
            {
                if (listing_Standard.ButtonText("Remove current keyframe " + (CurrentFrameInt + 1)))
                {
                    pawnKeyframes.RemoveAt(CurrentFrameInt);
                    this.ReIndexKeyframes();
                }
            }
            if (CurrentFrameInt != 0 && CurrentFrameInt != LastInd)
            {

                float shift = this.currentShift;
                shift = Mathf.Round(listing_Standard.Slider(shift, -1f, 1f) * 20f) / 20f;
                if (GUI.changed)
                {
                    this.currentShift = shift;
                }
            }

            listing_Standard.Gap();
            if (listing_Standard.ButtonText("Export BodyDef"))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Confirm overwriting " + this.bodyAnimDef.defName + ".xml", delegate
                    {
                        ExportAnimDefs.Defs animDef = new ExportAnimDefs.Defs(this.bodyAnimDef); ;

                        string configFolder = DefPath;
                        DirectXmlSaver.SaveDataObject(
                            animDef,
                            configFolder + "/BodyAnimDefs/" + this.bodyAnimDef.defName + ".xml");
                    }, true));


                // BodyAnimDef animDef = this.bodyAnimDef;
            }
            if (listing_Standard.ButtonText("Export WalkCycle"))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Confirm overwriting " + EditorWalkcycle.defName + ".xml", delegate
                    {
                        string configFolder = DefPath;
                        ExportCycleDefs.Defs cycle = new ExportCycleDefs.Defs(EditorWalkcycle);
                        DirectXmlSaver.SaveDataObject(cycle,
                            configFolder + "/WalkCycleDefs/" + EditorWalkcycle.defName + ".xml");
                    }, true, null));

            }

            listing_Standard.End();
        }

        public float currentShift
        {
            get
            {
                return this.currentFrame.shift;
            }
            set
            {
                if (Math.Abs(value) < 0.05f)
                {
                    this.currentFrame.status = KeyStatus.Automatic;
                }
                else
                {
                    this.currentFrame.status = KeyStatus.Manual;
                }
                this.currentFrame.shift = value;
                GameComponent_FacialStuff.BuildWalkCycles(EditorWalkcycle);
            }
        }

        private void ReIndexKeyframes()
        {
            for (int index = 0; index < pawnKeyframes.Count; index++)
            {
                PawnKeyframe keyframe = pawnKeyframes[index];
                keyframe.keyIndex = index;
            }
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

            sliderRect.y += sliderRect.height + 8f;

        }

        private void DrawBodyStats(string label, ref bool flipped, ref Rect sliderRect)
        {

            Widgets.CheckboxLabeled(sliderRect, label, ref flipped);

            sliderRect.y += sliderRect.height + 8f;

        }

        private void DrawTimelineButtons(float width, int count)
        {
            Rect buttonRect = new Rect(0f, 48f, (width - (count - 1) * this.spacing) / count, 32f);
            foreach (PawnKeyframe keyframe in pawnKeyframes)
            {
                int keyIndex = keyframe.keyIndex;
                if (keyIndex == CurrentFrameInt)
                {
                    GUI.color = selectedColor;
                }
                if (keyframe.status == KeyStatus.Manual)
                {
                    GUI.color *= Color.cyan;
                }
                if (Widgets.ButtonText(buttonRect, (keyIndex + 1).ToString()))
                {
                    SetCurrentFrame(keyIndex);
                }
                GUI.color = Color.white;
                buttonRect.x += buttonRect.width + this.spacing;
            }
        }

        private void DrawTimelineSlider(int count, float width)
        {
            Rect timeline = new Rect(0f, 0f, width, 40f);
            string label = "Current frame: " + this.frameLabel;
            if (this.loop)
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
                animSlider = Widgets.HorizontalSlider(timeline, AnimationPercent, 0f, 1f, false, label);
            }
        }

        private void DrawRotatorBody(float curY, float width)
        {
            float buttWidth = (width - 4 * this.spacing) / 6;
            Rect butt = new Rect(0f, curY, buttWidth, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                BodyRot = rotation;
            }
            butt.x += butt.width + this.spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Loop", ref this.loop);
        }


        public static Rot4 bodyRot = Rot4.East;
        public static Rot4 headRot = Rot4.East;

        private void DrawRotatorHead(float curY, float width)
        {
            float buttWidth = (width - 4 * this.spacing) / 6;
            Rect butt = new Rect(0f, curY, buttWidth, 32f);

            Rot4 rotation = Rot4.East;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                headRot = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.West;

            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                headRot = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.North;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                headRot = rotation;
            }
            butt.x += butt.width + this.spacing;
            rotation = Rot4.South;
            if (Widgets.ButtonText(butt, rotation.ToStringHuman()))
            {
                headRot = rotation;
            }
            butt.x += butt.width + this.spacing;

            butt.width *= 2;
            Widgets.CheckboxLabeled(butt, "Panic", ref panic);
        }

        private void SetAngle(ref float? angle, ref Rect editorRect, SimpleCurve thisFrame, string label, List<int> framesAt)
        {
            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.sliderWidth, this.defaultHeight);
            Rect buttonRect = new Rect(sliderRect.xMax + this.spacing, editorRect.y, editorRect.width - this.sliderWidth - this.spacing, this.defaultHeight);

            DoActiveKeyframeButtons(framesAt, ref buttonRect);


            if (!this.loop && angle.HasValue)
            {
                angle = Widgets.HorizontalSlider(sliderRect, angle.Value, -180f, 180f, false, label + " " + angle, "-180", "180", 1f);
                GUI.color = this.removeColor;
                if (Widgets.ButtonText(buttonRect, "- " + this.frameLabel))
                {
                    angle = null;
                }
                GUI.color = Color.white;
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(AnimationPercent), -180f, 180f, false, label + " " + angle, "-180", "180");
                GUI.color = this.addColor;
                if (Widgets.ButtonText(buttonRect, "+ " + this.frameLabel))
                {
                    angle = thisFrame.Evaluate(AnimationPercent);
                }
                GUI.color = Color.white;
            }
            editorRect.y += editorRect.height;

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
                            GUI.color = selectedColor;
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

        private void SetAngleShoulder(ref float angle, ref Rect editorRect, string label)
        {
            Rect labelRect = new Rect(editorRect.x, editorRect.y, this.widthLabel, this.defaultHeight);
            Rect sliderRect = new Rect(labelRect.xMax, editorRect.y, this.sliderWidth, this.defaultHeight);

            Widgets.Label(labelRect, label + " " + angle);
            angle = Mathf.FloorToInt(Widgets.HorizontalSlider(sliderRect, angle, 0, 180f));

            editorRect.y += editorRect.height;
        }

        private void SetPosition(ref float? position, ref Rect editorRect, SimpleCurve thisFrame, string label, List<int> framesAt)
        {


            Rect sliderRect = new Rect(editorRect.x, editorRect.y, this.sliderWidth, this.defaultHeight);
            Rect buttonRect = new Rect(
                sliderRect.xMax + this.spacing,
                editorRect.y,
                editorRect.width - this.sliderWidth - this.spacing,
                this.defaultHeight);

            float leftValue = -0.8f;
            float rightValue = 0.8f;

            DoActiveKeyframeButtons(framesAt, ref buttonRect);

            if (!this.loop && position.HasValue)
            {
                position = Widgets.HorizontalSlider(sliderRect, position.Value, leftValue, rightValue, false, label + " " + position, leftValue.ToString(), rightValue.ToString(), 0.025f);
                GUI.color = this.removeColor;
                if (Widgets.ButtonText(buttonRect, "- " + this.frameLabel))
                {
                    position = null;
                }
                GUI.color = Color.white;
            }
            else
            {
                Widgets.HorizontalSlider(sliderRect, thisFrame.Evaluate(AnimationPercent), leftValue, rightValue, false, label + " " + position, "-0.4", "0.4");


                GUI.color = this.addColor;
                if (Widgets.ButtonText(buttonRect, "+ " + this.frameLabel))
                {
                    position = thisFrame.Evaluate(AnimationPercent);
                }
                GUI.color = Color.white;
            }
            editorRect.y += editorRect.height;

        }

        private static WalkCycleDef editorWalkcycle = WalkCycleDefOf.Biped_Walk;

        public override void PostClose()
        {
            this.CompAnim.AnimatorOpen = false;
            this.pawn = null;

            base.PostClose();
        }

        private void FindRandomPawn()
        {
            if (this.pawn == null)
            {
                this.pawn = Find.AnyPlayerHomeMap.FreeColonistsForStoryteller.FirstOrDefault();

                this.pawn.GetCompAnim(out this.compAnim);
                this.CompAnim.AnimatorOpen = true;
                editorWalkcycle = this.CompAnim.bodyAnim.walkCycles.FirstOrDefault().Value;
                //  editorWalkcycle = this.CompAnim.bodyAnim.walkCycles[0];
            }

        }

        private static Vector2 portraitSize = new Vector2(320f, 320f);


        private float widthButton = 120f;

        private float widthLabel = 150f;

        private float defaultHeight = 36f;

        private float spacing = 12f;

        private CompBodyAnimator compAnim;
        public CompBodyAnimator CompAnim => this.compAnim;

        private float zoom = 1f;

        private BodyAnimDef bodyAnimDef => CompAnim.bodyAnim;

        public static WalkCycleDef EditorWalkcycle => editorWalkcycle;

        public static int LastInd
        {
            get
            {
                return pawnKeyframes.Count - 1;
            }
        }

        private static float animSlider;

        public static bool panic;

        private static Color selectedColor = new Color(1f, 0.79f, 0.26f);

        private Color removeColor = new Color(1f, 0.25f, 0.25f);

        private Color addColor = new Color(0.25f, 1f, 0.25f);

        private static List<PawnKeyframe> pawnKeyframes => EditorWalkcycle.keyframes;

        public static Rot4 HeadRot => headRot;

        public static Rot4 BodyRot
        {
            get => bodyRot;
            set
            {
                bodyRot = value;
                headRot = value;
            }
        }

        public static float AnimationPercent
        {
            get
            {
                return animationPercent;
            }
            set
            {
                animationPercent = value;
                int frame = (int)(animationPercent * LastInd);
                currentFrameInt = frame;

            }
        }

        private static void SetCurrentFrame(int frame)
        {
            {
                animSlider = (float)frame / ((float)LastInd);

            }
        }

        public void AddPortraitWidget(float inRectWidth)
        {
            // Portrait
            Rect rect = new Rect(0, 0, inRectWidth, inRectWidth);

            GUI.DrawTexture(rect, FaceTextures.backgroundAnimTex);

            // Draw the pawn's portrait
            Vector2 size = new Vector2(rect.height / 1.4f, rect.height); // 128x180

            Rect position = new Rect(
                rect.width * 0.5f - size.x * 0.5f,
                rect.height * 0.5f - size.y * 0.5f - 10f,
                size.x,
                size.y);

            GUI.DrawTexture(position, PortraitsCache.Get(this.pawn, size, new Vector3(0f, 0f, 0.1f), this.zoom));

            // GUI.DrawTexture(position, PortraitsCache.Get(pawn, size, default(Vector3)));

            Widgets.DrawBox(rect, 1);
        }


    }
}

