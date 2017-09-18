using UnityEngine;

namespace FacialStuff
{
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Defs;
    using FacialStuff.Enums;
    using FacialStuff.Graphics;

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

            Rect checkbox = new Rect(rect.x, rect.y, rect.width, 24f);

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
                                "MaleAverageNormalOffset: " + Settings.EyeMaleAverageNormalOffset.ToString("N5"));
                            Settings.EyeMaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeMaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeMaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeMaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f); GUILayout.Label(
                                "MouthMaleAverageNormalOffset: " + Settings.MouthMaleAverageNormalOffset.ToString("N5"));
                            Settings.MouthMaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthMaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthMaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthMaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageNormalOffset: " + Settings.EyeFemaleAverageNormalOffset.ToString("N5"));
                            Settings.EyeFemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeFemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAverageNormalOffset: " + Settings.MouthFemaleAverageNormalOffset.ToString("N5"));
                            Settings.MouthFemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthFemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);

                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAveragePointyOffset: " + Settings.EyeMaleAveragePointyOffset.ToString("N5"));
                            Settings.EyeMaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeMaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeMaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeMaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleAveragePointyOffset: " + Settings.MouthMaleAveragePointyOffset.ToString("N5"));
                            Settings.MouthMaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthMaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthMaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthMaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);

                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAveragePointyOffset: " + Settings.EyeFemaleAveragePointyOffset.ToString("N5"));
                            Settings.EyeFemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeFemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAveragePointyOffset: " + Settings.MouthFemaleAveragePointyOffset.ToString("N5"));
                            Settings.MouthFemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthFemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageWideOffset.x: " + Settings.EyeMaleAverageWideOffset.ToString("N5"));
                            Settings.EyeMaleAverageWideOffset.x =
                                GUILayout.HorizontalSlider(Settings.EyeMaleAverageWideOffset.x, -0.2f, 0.2f);
                            Settings.EyeMaleAverageWideOffset.y =
                                GUILayout.HorizontalSlider(Settings.EyeMaleAverageWideOffset.y, -0.2f, 0.2f);
                            GUILayout.Label(
                                "MouthMaleAverageWideOffset.x: " + Settings.MouthMaleAverageWideOffset.ToString("N5"));
                            Settings.MouthMaleAverageWideOffset.x =
                                GUILayout.HorizontalSlider(Settings.MouthMaleAverageWideOffset.x, -0.2f, 0.2f);
                            Settings.MouthMaleAverageWideOffset.y =
                                GUILayout.HorizontalSlider(Settings.MouthMaleAverageWideOffset.y, -0.2f, 0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageWideOffset: " + Settings.EyeFemaleAverageWideOffset.ToString("N5"));
                            Settings.EyeFemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeFemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAverageWideOffset: " + Settings.MouthFemaleAverageWideOffset.ToString("N5"));
                            Settings.MouthFemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthFemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
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
                                "MaleNarrowNormalOffset.x: " + Settings.EyeMaleNarrowNormalOffset.ToString("N5"));
                            Settings.EyeMaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeMaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeMaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeMaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowNormalOffset.x: " + Settings.MouthMaleNarrowNormalOffset.ToString("N5"));
                            Settings.MouthMaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthMaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthMaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthMaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowNormalOffset: " + Settings.EyeFemaleNarrowNormalOffset.ToString("N5"));
                            Settings.EyeFemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeFemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowNormalOffset: " + Settings.MouthFemaleNarrowNormalOffset.ToString("N5"));
                            Settings.MouthFemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthFemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowPointyOffset.x: " + Settings.EyeMaleNarrowPointyOffset.ToString("N5"));
                            Settings.EyeMaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeMaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeMaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeMaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowPointyOffset.x: " + Settings.MouthMaleNarrowPointyOffset.ToString("N5"));
                            Settings.MouthMaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthMaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthMaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthMaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowPointyOffset: " + Settings.EyeFemaleNarrowPointyOffset.ToString("N5"));
                            Settings.EyeFemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeFemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowPointyOffset: " + Settings.MouthFemaleNarrowPointyOffset.ToString("N5"));
                            Settings.MouthFemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthFemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowWideOffset.x: " + Settings.EyeMaleNarrowWideOffset.ToString("N5"));
                            Settings.EyeMaleNarrowWideOffset.x =
                                GUILayout.HorizontalSlider(Settings.EyeMaleNarrowWideOffset.x, -0.2f, 0.2f);
                            Settings.EyeMaleNarrowWideOffset.y =
                                GUILayout.HorizontalSlider(Settings.EyeMaleNarrowWideOffset.y, -0.2f, 0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowWideOffset.x: " + Settings.MouthMaleNarrowWideOffset.ToString("N5"));
                            Settings.MouthMaleNarrowWideOffset.x =
                                GUILayout.HorizontalSlider(Settings.MouthMaleNarrowWideOffset.x, -0.2f, 0.2f);
                            Settings.MouthMaleNarrowWideOffset.y =
                                GUILayout.HorizontalSlider(Settings.MouthMaleNarrowWideOffset.y, -0.2f, 0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowWideOffset: " + Settings.EyeFemaleNarrowWideOffset.ToString("N5"));
                            Settings.EyeFemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.EyeFemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                Settings.EyeFemaleNarrowWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowWideOffset: " + Settings.MouthFemaleNarrowWideOffset.ToString("N5"));
                            Settings.MouthFemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            Settings.MouthFemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                Settings.MouthFemaleNarrowWideOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;
                }
            }

            // else
            // {
            // GUILayout.Label("FemaleOffset.y");
            // Settings.FemaleOffset.y =
            // GUILayout.HorizontalSlider(Settings.FemaleOffset.y, -0.2f, 0.2f);
            // }
            GUILayout.EndVertical();

            if (GUILayout.Button("Mouth"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (MouthDef current in DefDatabase<MouthDef>.AllDefs)
                {
                    MouthDef localOut = current;
                    list.Add(new FloatMenuOption(localOut.label,
                        delegate
                            {
                                foreach (Pawn pawn in Find.VisibleMap.mapPawns.AllPawnsSpawned.Where(x => x.GetComp<CompFace>() != null))
                                {
                                        Color color = Color.white;
                                    CompFace face = pawn.GetComp<CompFace>();
                                    face.faceGraphicPart.MouthGraphic =
                                        GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                            current.texPath,
                                            ShaderDatabase.CutoutSkin,
                                            Vector2.one,
                                            color) as Graphic_Multi_NaturalHeadParts;
                                }

                                ;
                            }));
                }

                Find.WindowStack.Add(new FloatMenu(list));

            }

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