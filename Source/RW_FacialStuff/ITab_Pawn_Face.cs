using UnityEngine;

namespace FacialStuff
{
    using FacialStuff.Enums;
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

            Rect checkbox = new Rect(rect.x, rect.y , rect.width, 24f);

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
                this.rotationInt = this.SelPawn.Rotation.AsInt;
            }

            faceComp.rotationInt = this.rotationInt;

            bool male = this.SelPawn.gender == Gender.Male;

            if (faceComp.PawnCrownType == CrownType.Average)
            {
                switch (faceComp.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageNormalOffset: " + Controller.settings.MaleAverageNormalOffset.ToString("N5"));
                            Controller.settings.MaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAverageNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.MaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAverageNormalOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleAverageNormalOffset: " + Controller.settings.EyeMaleAverageNormalOffset.ToString("N5"));
                            Controller.settings.EyeMaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleAverageNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeMaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleAverageNormalOffset.y,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageNormalOffset: " + Controller.settings.FemaleAverageNormalOffset.ToString("N5"));
                            Controller.settings.FemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.FemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageNormalOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAverageNormalOffset: " + Controller.settings.EyeFemaleAverageNormalOffset.ToString("N5"));
                            Controller.settings.EyeFemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleAverageNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeFemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleAverageNormalOffset.y,
                                -0.4f,
                                0.4f);
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAveragePointyOffset: " + Controller.settings.MaleAveragePointyOffset.ToString("N5"));
                            Controller.settings.MaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAveragePointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.MaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.MaleAveragePointyOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleAveragePointyOffset: " + Controller.settings.EyeMaleAveragePointyOffset.ToString("N5"));
                            Controller.settings.EyeMaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleAveragePointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeMaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleAveragePointyOffset.y,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAveragePointyOffset: " + Controller.settings.FemaleAveragePointyOffset.ToString("N5"));
                            Controller.settings.FemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAveragePointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.FemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAveragePointyOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAveragePointyOffset: " + Controller.settings.EyeFemaleAveragePointyOffset.ToString("N5"));
                            Controller.settings.EyeFemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleAveragePointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeFemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleAveragePointyOffset.y,
                                -0.4f,
                                0.4f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageWideOffset.x: " + Controller.settings.MaleAverageWideOffset.ToString("N5"));
                            Controller.settings.MaleAverageWideOffset.x =
                                GUILayout.HorizontalSlider(Controller.settings.MaleAverageWideOffset.x, -0.4f, 0.4f);
                            Controller.settings.MaleAverageWideOffset.y =
                                GUILayout.HorizontalSlider(Controller.settings.MaleAverageWideOffset.y, -0.4f, 0.4f);
                            GUILayout.Label(
                                "MaleAverageWideOffset.x: " + Controller.settings.EyeMaleAverageWideOffset.ToString("N5"));
                            Controller.settings.EyeMaleAverageWideOffset.x =
                                GUILayout.HorizontalSlider(Controller.settings.EyeMaleAverageWideOffset.x, -0.4f, 0.4f);
                            Controller.settings.EyeMaleAverageWideOffset.y =
                                GUILayout.HorizontalSlider(Controller.settings.EyeMaleAverageWideOffset.y, -0.4f, 0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageWideOffset: " + Controller.settings.FemaleAverageWideOffset.ToString("N5"));
                            Controller.settings.FemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageWideOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.FemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleAverageWideOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleAverageWideOffset: " + Controller.settings.EyeFemaleAverageWideOffset.ToString("N5"));
                            Controller.settings.EyeFemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleAverageWideOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeFemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleAverageWideOffset.y,
                                -0.4f,
                                0.4f);
                        }

                        break;
                }
            }
            else
            {
                switch (faceComp.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowNormalOffset.x: " + Controller.settings.MaleNarrowNormalOffset.ToString("N5"));
                            Controller.settings.MaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.MaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowNormalOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleNarrowNormalOffset.x: " + Controller.settings.EyeMaleNarrowNormalOffset.ToString("N5"));
                            Controller.settings.EyeMaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleNarrowNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeMaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleNarrowNormalOffset.y,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowNormalOffset: " + Controller.settings.FemaleNarrowNormalOffset.ToString("N5"));
                            Controller.settings.FemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.FemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowNormalOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowNormalOffset: " + Controller.settings.EyeFemaleNarrowNormalOffset.ToString("N5"));
                            Controller.settings.EyeFemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleNarrowNormalOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeFemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleNarrowNormalOffset.y,
                                -0.4f,
                                0.4f);
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowPointyOffset.x: " + Controller.settings.MaleNarrowPointyOffset.ToString("N5"));
                            Controller.settings.MaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowPointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.MaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.MaleNarrowPointyOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "MaleNarrowPointyOffset.x: " + Controller.settings.EyeMaleNarrowPointyOffset.ToString("N5"));
                            Controller.settings.EyeMaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleNarrowPointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeMaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeMaleNarrowPointyOffset.y,
                                -0.4f,
                                0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowPointyOffset: " + Controller.settings.FemaleNarrowPointyOffset.ToString("N5"));
                            Controller.settings.FemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowPointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.FemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowPointyOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowPointyOffset: " + Controller.settings.EyeFemaleNarrowPointyOffset.ToString("N5"));
                            Controller.settings.EyeFemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleNarrowPointyOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeFemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleNarrowPointyOffset.y,
                                -0.4f,
                                0.4f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowWideOffset.x: " + Controller.settings.MaleNarrowWideOffset.ToString("N5"));
                            Controller.settings.MaleNarrowWideOffset.x =
                                GUILayout.HorizontalSlider(Controller.settings.MaleNarrowWideOffset.x, -0.4f, 0.4f);
                            Controller.settings.MaleNarrowWideOffset.y =
                                GUILayout.HorizontalSlider(Controller.settings.MaleNarrowWideOffset.y, -0.4f, 0.4f);
                            GUILayout.Label(
                                "MaleNarrowWideOffset.x: " + Controller.settings.EyeMaleNarrowWideOffset.ToString("N5"));
                            Controller.settings.EyeMaleNarrowWideOffset.x =
                                GUILayout.HorizontalSlider(Controller.settings.EyeMaleNarrowWideOffset.x, -0.4f, 0.4f);
                            Controller.settings.EyeMaleNarrowWideOffset.y =
                                GUILayout.HorizontalSlider(Controller.settings.EyeMaleNarrowWideOffset.y, -0.4f, 0.4f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowWideOffset: " + Controller.settings.FemaleNarrowWideOffset.ToString("N5"));
                            Controller.settings.FemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowWideOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.FemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.FemaleNarrowWideOffset.y,
                                -0.4f,
                                0.4f);
                            GUILayout.Label(
                                "FemaleNarrowWideOffset: " + Controller.settings.EyeFemaleNarrowWideOffset.ToString("N5"));
                            Controller.settings.EyeFemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleNarrowWideOffset.x,
                                -0.4f,
                                0.4f);
                            Controller.settings.EyeFemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                Controller.settings.EyeFemaleNarrowWideOffset.y,
                                -0.4f,
                                0.4f);
                        }

                        break;
                }
            }

            // else
            // {
            // GUILayout.Label("FemaleOffset.y");
            // Controller.settings.FemaleOffset.y =
            // GUILayout.HorizontalSlider(Controller.settings.FemaleOffset.y, -0.4f, 0.4f);
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