using System;
using UnityEngine;

namespace FacialStuff
{
    using RimWorld;

    using RW_FacialStuff;

    using Verse;

    public class ITab_Pawn_Face : ITab
    {
        private readonly string[] psiToolbarStrings =
            {
                "North",
                "East",
                "South",
                "West"
            };

        public override bool IsVisible
        {
            get
            {
                var faceComp = this.SelPawn.TryGetComp<CompFace>();
                return faceComp != null;
            }
        }

        public ITab_Pawn_Face()
        {
            this.labelKey = "TabFace";
            this.tutorTag = "Face";
        }

        public override void OnOpen()
        {

            //      this.thoughtScrollPosition = default(Vector2);
        }

        private int rotationInt = 2;

        protected override void FillTab()
        {
            var faceComp = this.SelPawn.TryGetComp<CompFace>();

            Rect rect = new Rect(10f, 10f, 330f, 330f);
            GUILayout.BeginArea(rect);
            GUILayout.BeginVertical();

            faceComp.ignoreRenderer = true;
            faceComp.ignoreRenderer = GUILayout.Toggle(faceComp.ignoreRenderer, "Ignore renderer");

            if (faceComp.ignoreRenderer)
            {
                rotationInt = GUILayout.SelectionGrid(rotationInt, psiToolbarStrings, 4);
            }
            else
            {
                rotationInt = faceComp.pawn.Rotation.AsInt;
            }
            faceComp.rotationInt = this.rotationInt;

            var male = faceComp.pawn.gender == Gender.Male;

            if (faceComp.crownType == CrownType.Average)
            {
                switch (faceComp.headType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageNormalOffsetX: " + FS_Settings.MaleAverageNormalOffsetX.ToString("N5"));
                            FS_Settings.MaleAverageNormalOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAverageNormalOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "MaleAverageNormalOffsetY: " + FS_Settings.MaleAverageNormalOffsetY.ToString("N5"));
                            FS_Settings.MaleAverageNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAverageNormalOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageNormalOffsetX: " + FS_Settings.FemaleAverageNormalOffsetX.ToString("N5"));
                            FS_Settings.FemaleAverageNormalOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageNormalOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "FemaleAverageNormalOffsetY: " + FS_Settings.FemaleAverageNormalOffsetY.ToString("N5"));
                            FS_Settings.FemaleAverageNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageNormalOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        break;
                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAveragePointyOffsetX: " + FS_Settings.MaleAveragePointyOffsetX.ToString("N5"));
                            FS_Settings.MaleAveragePointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAveragePointyOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "MaleAveragePointyOffsetY: " + FS_Settings.MaleAveragePointyOffsetY.ToString("N5"));
                            FS_Settings.MaleAveragePointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAveragePointyOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAveragePointyOffsetX: " + FS_Settings.FemaleAveragePointyOffsetX.ToString("N5"));
                            FS_Settings.FemaleAveragePointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAveragePointyOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "FemaleAveragePointyOffsetY: " + FS_Settings.FemaleAveragePointyOffsetY.ToString("N5"));
                            FS_Settings.FemaleAveragePointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAveragePointyOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        break;
                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageWideOffsetX: " + FS_Settings.MaleAverageWideOffsetX.ToString("N5"));
                            FS_Settings.MaleAverageWideOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAverageWideOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "MaleAverageWideOffsetY: " + FS_Settings.MaleAverageWideOffsetY.ToString("N5"));
                            FS_Settings.MaleAverageWideOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAverageWideOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageWideOffsetX: " + FS_Settings.FemaleAverageWideOffsetX.ToString("N5"));
                            FS_Settings.FemaleAverageWideOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageWideOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "FemaleAverageWideOffsetY: " + FS_Settings.FemaleAverageWideOffsetY.ToString("N5"));
                            FS_Settings.FemaleAverageWideOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageWideOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        break;
                }
            }

            else
            {
                switch (faceComp.headType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowNormalOffsetX: " + FS_Settings.MaleNarrowNormalOffsetX.ToString("N5"));
                            FS_Settings.MaleNarrowNormalOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowNormalOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "MaleNarrowNormalOffsetY: " + FS_Settings.MaleNarrowNormalOffsetY.ToString("N5"));
                            FS_Settings.MaleNarrowNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowNormalOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowNormalOffsetX: " + FS_Settings.FemaleNarrowNormalOffsetX.ToString("N5"));
                            FS_Settings.FemaleNarrowNormalOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowNormalOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "FemaleNarrowNormalOffsetY: " + FS_Settings.FemaleNarrowNormalOffsetY.ToString("N5"));
                            FS_Settings.FemaleNarrowNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowNormalOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        break;
                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowPointyOffsetX: " + FS_Settings.MaleNarrowPointyOffsetX.ToString("N5"));
                            FS_Settings.MaleNarrowPointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowPointyOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "MaleNarrowPointyOffsetY: " + FS_Settings.MaleNarrowPointyOffsetY.ToString("N5"));
                            FS_Settings.MaleNarrowPointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowPointyOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowPointyOffsetX: " + FS_Settings.FemaleNarrowPointyOffsetX.ToString("N5"));
                            FS_Settings.FemaleNarrowPointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowPointyOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "FemaleNarrowPointyOffsetY: " + FS_Settings.FemaleNarrowPointyOffsetY.ToString("N5"));
                            FS_Settings.FemaleNarrowPointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowPointyOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        break;
                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowWideOffsetX: " + FS_Settings.MaleNarrowWideOffsetX.ToString("N5"));
                            FS_Settings.MaleNarrowWideOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowWideOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "MaleNarrowWideOffsetY: " + FS_Settings.MaleNarrowWideOffsetY.ToString("N5"));
                            FS_Settings.MaleNarrowWideOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowWideOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowWideOffsetX: " + FS_Settings.FemaleNarrowWideOffsetX.ToString("N5"));
                            FS_Settings.FemaleNarrowWideOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowWideOffsetX,
                                -0.15f,
                                0.15f);
                            GUILayout.Label(
                                "FemaleNarrowWideOffsetY: " + FS_Settings.FemaleNarrowWideOffsetY.ToString("N5"));
                            FS_Settings.FemaleNarrowWideOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowWideOffsetY,
                                -0.15f,
                                0.15f);
                        }
                        break;
                }

            }

            //  else
            //  {
            //      GUILayout.Label("FemaleOffsetY");
            //      FS_Settings.FemaleOffsetY =
            //          GUILayout.HorizontalSlider(FS_Settings.FemaleOffsetY, -0.15f, 0.15f);
            //  }

            GUILayout.EndVertical();
            GUILayout.EndArea();
            //      NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, this.size.x, this.size.y), base.SelPawn, ref this.thoughtScrollPosition);
        }

        protected override void UpdateSize()
        {
            size = new Vector2(350f, 350f);

            //      this.size = NeedsCardUtility.GetSize(base.SelPawn);
        }
    }
}
