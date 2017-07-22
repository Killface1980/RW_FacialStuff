using UnityEngine;

namespace FacialStuff
{
    using FacialStuff.Genetics;

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
                CompFace faceComp = this.SelPawn.TryGetComp<CompFace>();
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

            // this.thoughtScrollPosition = default(Vector2);
        }

        private int rotationInt = 2;

        protected override void FillTab()
        {
            CompFace faceComp = this.SelPawn.TryGetComp<CompFace>();

            Rect rect = new Rect(10f, 10f, 330f, 330f);
            Rect colRect = new Rect(rect.x, rect.y, rect.width / 3, 25f);

            Widgets.DrawBoxSolid(colRect, this.SelPawn.story.SkinColor);
            colRect.x += colRect.width;
            Widgets.DrawBoxSolid(colRect, HairMelanin.gradient_mel1.Evaluate(faceComp.melanin1));
            colRect.x += colRect.width;
            Widgets.DrawBoxSolid(colRect, HairMelanin.gradient_mel2.Evaluate(faceComp.melanin2));

            Rect checkbox = new Rect(rect.x, colRect.yMax + 15f, rect.width, 24f);

            Widgets.CheckboxLabeled(checkbox, "Ignore renderer", ref faceComp.IgnoreRenderer);

            Rect pawnRect = new Rect(rect.x, checkbox.yMax, rect.width, 24f);

            foreach (Pawn relatedPawn in this.SelPawn.relations.FamilyByBlood)
            {

                Widgets.Label(pawnRect, relatedPawn.ToString() + " - " + this.SelPawn.GetRelations(relatedPawn).GetEnumerator());
                pawnRect.y += 24f;
            }

            Rect rect2 = new Rect(rect.x, pawnRect.yMax + 15f, rect.width, rect.height - checkbox.yMax);

            GUILayout.BeginArea(rect2);
            GUILayout.BeginVertical();


            if (faceComp.IgnoreRenderer)
            {
                this.rotationInt = GUILayout.SelectionGrid(this.rotationInt, this.psiToolbarStrings, 4);
            }
            else
            {
                this.rotationInt = faceComp.pawn.Rotation.AsInt;
            }

            faceComp.rotationInt = this.rotationInt;

            bool male = faceComp.pawn.gender == Gender.Male;

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
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleAverageNormalOffsetY: " + FS_Settings.MaleAverageNormalOffsetY.ToString("N5"));
                            FS_Settings.MaleAverageNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAverageNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageNormalOffsetX: " + FS_Settings.FemaleAverageNormalOffsetX.ToString("N5"));
                            FS_Settings.FemaleAverageNormalOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageNormalOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAverageNormalOffsetY: " + FS_Settings.FemaleAverageNormalOffsetY.ToString("N5"));
                            FS_Settings.FemaleAverageNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAveragePointyOffsetX: " + FS_Settings.MaleAveragePointyOffsetX.ToString("N5"));
                            FS_Settings.MaleAveragePointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAveragePointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleAveragePointyOffsetY: " + FS_Settings.MaleAveragePointyOffsetY.ToString("N5"));
                            FS_Settings.MaleAveragePointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleAveragePointyOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAveragePointyOffsetX: " + FS_Settings.FemaleAveragePointyOffsetX.ToString("N5"));
                            FS_Settings.FemaleAveragePointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAveragePointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAveragePointyOffsetY: " + FS_Settings.FemaleAveragePointyOffsetY.ToString("N5"));
                            FS_Settings.FemaleAveragePointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAveragePointyOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageWideOffsetX: " + FS_Settings.MaleAverageWideOffsetX.ToString("N5"));
                            FS_Settings.MaleAverageWideOffsetX =
                                GUILayout.HorizontalSlider(FS_Settings.MaleAverageWideOffsetX, -0.4f, 0.4f);
                            GUILayout.Label(
                                "MaleAverageWideOffsetY: " + FS_Settings.MaleAverageWideOffsetY.ToString("N5"));
                            FS_Settings.MaleAverageWideOffsetY =
                                GUILayout.HorizontalSlider(FS_Settings.MaleAverageWideOffsetY, -0.4f, 0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageWideOffsetX: " + FS_Settings.FemaleAverageWideOffsetX.ToString("N5"));
                            FS_Settings.FemaleAverageWideOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageWideOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAverageWideOffsetY: " + FS_Settings.FemaleAverageWideOffsetY.ToString("N5"));
                            FS_Settings.FemaleAverageWideOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleAverageWideOffsetY,
                                -0.4f,
                                0.4f);
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
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleNarrowNormalOffsetY: " + FS_Settings.MaleNarrowNormalOffsetY.ToString("N5"));
                            FS_Settings.MaleNarrowNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowNormalOffsetX: " + FS_Settings.FemaleNarrowNormalOffsetX.ToString("N5"));
                            FS_Settings.FemaleNarrowNormalOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowNormalOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowNormalOffsetY: " + FS_Settings.FemaleNarrowNormalOffsetY.ToString("N5"));
                            FS_Settings.FemaleNarrowNormalOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowPointyOffsetX: " + FS_Settings.MaleNarrowPointyOffsetX.ToString("N5"));
                            FS_Settings.MaleNarrowPointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowPointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleNarrowPointyOffsetY: " + FS_Settings.MaleNarrowPointyOffsetY.ToString("N5"));
                            FS_Settings.MaleNarrowPointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.MaleNarrowPointyOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowPointyOffsetX: " + FS_Settings.FemaleNarrowPointyOffsetX.ToString("N5"));
                            FS_Settings.FemaleNarrowPointyOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowPointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowPointyOffsetY: " + FS_Settings.FemaleNarrowPointyOffsetY.ToString("N5"));
                            FS_Settings.FemaleNarrowPointyOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowPointyOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowWideOffsetX: " + FS_Settings.MaleNarrowWideOffsetX.ToString("N5"));
                            FS_Settings.MaleNarrowWideOffsetX =
                                GUILayout.HorizontalSlider(FS_Settings.MaleNarrowWideOffsetX, -0.4f, 0.4f);
                            GUILayout.Label(
                                "MaleNarrowWideOffsetY: " + FS_Settings.MaleNarrowWideOffsetY.ToString("N5"));
                            FS_Settings.MaleNarrowWideOffsetY =
                                GUILayout.HorizontalSlider(FS_Settings.MaleNarrowWideOffsetY, -0.4f, 0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowWideOffsetX: " + FS_Settings.FemaleNarrowWideOffsetX.ToString("N5"));
                            FS_Settings.FemaleNarrowWideOffsetX = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowWideOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowWideOffsetY: " + FS_Settings.FemaleNarrowWideOffsetY.ToString("N5"));
                            FS_Settings.FemaleNarrowWideOffsetY = GUILayout.HorizontalSlider(
                                FS_Settings.FemaleNarrowWideOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                }
            }

            // else
            // {
            // GUILayout.Label("FemaleOffsetY");
            // FS_Settings.FemaleOffsetY =
            // GUILayout.HorizontalSlider(FS_Settings.FemaleOffsetY, -0.4f, 0.4f);
            // }
            GUILayout.EndVertical();
            GUILayout.EndArea();

            // NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, this.size.x, this.size.y), base.SelPawn, ref this.thoughtScrollPosition);
        }

        protected override void UpdateSize()
        {
            this.size = new Vector2(350f, 350f);

            // this.size = NeedsCardUtility.GetSize(base.SelPawn);
        }
    }
}
