using UnityEngine;

namespace FacialStuff
{
    using FacialStuff.Genetics;

    using RimWorld;

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
                this.rotationInt = faceComp.FacePawn.Rotation.AsInt;
            }

            faceComp.rotationInt = this.rotationInt;

            bool male = faceComp.FacePawn.gender == Gender.Male;

            if (faceComp.crownType == CrownType.Average)
            {
                switch (faceComp.headType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageNormalOffsetX: " + Controller.settings.MaleAverageNormalOffsetX.ToString("N5"));
                            Controller.settings.MaleAverageNormalOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAverageNormalOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleAverageNormalOffsetY: " + Controller.settings.MaleAverageNormalOffsetY.ToString("N5"));
                            Controller.settings.MaleAverageNormalOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAverageNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageNormalOffsetX: " + Controller.settings.FemaleAverageNormalOffsetX.ToString("N5"));
                            Controller.settings.FemaleAverageNormalOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageNormalOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAverageNormalOffsetY: " + Controller.settings.FemaleAverageNormalOffsetY.ToString("N5"));
                            Controller.settings.FemaleAverageNormalOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAveragePointyOffsetX: " + Controller.settings.MaleAveragePointyOffsetX.ToString("N5"));
                            Controller.settings.MaleAveragePointyOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAveragePointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleAveragePointyOffsetY: " + Controller.settings.MaleAveragePointyOffsetY.ToString("N5"));
                            Controller.settings.MaleAveragePointyOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAveragePointyOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAveragePointyOffsetX: " + Controller.settings.FemaleAveragePointyOffsetX.ToString("N5"));
                            Controller.settings.FemaleAveragePointyOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAveragePointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAveragePointyOffsetY: " + Controller.settings.FemaleAveragePointyOffsetY.ToString("N5"));
                            Controller.settings.FemaleAveragePointyOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAveragePointyOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageWideOffsetX: " + Controller.settings.MaleAverageWideOffsetX.ToString("N5"));
                            Controller.settings.MaleAverageWideOffsetX =
                                GUILayout.HorizontalSlider(Controller.settings.MaleAverageWideOffsetX, -0.4f, 0.4f);
                            GUILayout.Label(
                                "MaleAverageWideOffsetY: " + Controller.settings.MaleAverageWideOffsetY.ToString("N5"));
                            Controller.settings.MaleAverageWideOffsetY =
                                GUILayout.HorizontalSlider(Controller.settings.MaleAverageWideOffsetY, -0.4f, 0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageWideOffsetX: " + Controller.settings.FemaleAverageWideOffsetX.ToString("N5"));
                            Controller.settings.FemaleAverageWideOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageWideOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAverageWideOffsetY: " + Controller.settings.FemaleAverageWideOffsetY.ToString("N5"));
                            Controller.settings.FemaleAverageWideOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageWideOffsetY,
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
                                "MaleNarrowNormalOffsetX: " + Controller.settings.MaleNarrowNormalOffsetX.ToString("N5"));
                            Controller.settings.MaleNarrowNormalOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowNormalOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleNarrowNormalOffsetY: " + Controller.settings.MaleNarrowNormalOffsetY.ToString("N5"));
                            Controller.settings.MaleNarrowNormalOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowNormalOffsetX: " + Controller.settings.FemaleNarrowNormalOffsetX.ToString("N5"));
                            Controller.settings.FemaleNarrowNormalOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowNormalOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowNormalOffsetY: " + Controller.settings.FemaleNarrowNormalOffsetY.ToString("N5"));
                            Controller.settings.FemaleNarrowNormalOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowNormalOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowPointyOffsetX: " + Controller.settings.MaleNarrowPointyOffsetX.ToString("N5"));
                            Controller.settings.MaleNarrowPointyOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowPointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleNarrowPointyOffsetY: " + Controller.settings.MaleNarrowPointyOffsetY.ToString("N5"));
                            Controller.settings.MaleNarrowPointyOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowPointyOffsetY,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowPointyOffsetX: " + Controller.settings.FemaleNarrowPointyOffsetX.ToString("N5"));
                            Controller.settings.FemaleNarrowPointyOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowPointyOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowPointyOffsetY: " + Controller.settings.FemaleNarrowPointyOffsetY.ToString("N5"));
                            Controller.settings.FemaleNarrowPointyOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowPointyOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowWideOffsetX: " + Controller.settings.MaleNarrowWideOffsetX.ToString("N5"));
                            Controller.settings.MaleNarrowWideOffsetX =
                                GUILayout.HorizontalSlider(Controller.settings.MaleNarrowWideOffsetX, -0.4f, 0.4f);
                            GUILayout.Label(
                                "MaleNarrowWideOffsetY: " + Controller.settings.MaleNarrowWideOffsetY.ToString("N5"));
                            Controller.settings.MaleNarrowWideOffsetY =
                                GUILayout.HorizontalSlider(Controller.settings.MaleNarrowWideOffsetY, -0.4f, 0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowWideOffsetX: " + Controller.settings.FemaleNarrowWideOffsetX.ToString("N5"));
                            Controller.settings.FemaleNarrowWideOffsetX = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowWideOffsetX,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowWideOffsetY: " + Controller.settings.FemaleNarrowWideOffsetY.ToString("N5"));
                            Controller.settings.FemaleNarrowWideOffsetY = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowWideOffsetY,
                                -0.4f,
                                0.4f);
                        }

                        break;
                }
            }

            // else
            // {
            // GUILayout.Label("FemaleOffsetY");
            // Controller.settings.FemaleOffsetY =
            // GUILayout.HorizontalSlider(Controller.settings.FemaleOffsetY, -0.4f, 0.4f);
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
