using System.Collections.Generic;
using System.Linq;
using FacialStuff.Defs;
using FacialStuff.GraphicsFS;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class ITab_Pawn_Face : ITab
    {
        private readonly string[] _psiToolbarStrings = { "North", "East", "South", "West" };

        private static int _rotation = 2;

        private Pawn _pawn;

        public ITab_Pawn_Face()
        {
            this.labelKey = "TabFace";
            this.tutorTag = "Face";
        }

        public override bool IsVisible => this.SelPawn.HasCompFace() && Controller.settings.Develop;


        protected override void FillTab()
        {

            if (!this.SelPawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            Rect rect = new Rect(10f, 10f, 330f, 330f);

            Rect checkbox = new Rect(rect.x, rect.y, rect.width, 24f);

            Widgets.CheckboxLabeled(checkbox, "Ignore renderer", ref SelPawn.GetCompAnim().IgnoreRenderer);

            Rect pawnRect = new Rect(rect.x, checkbox.yMax, rect.width, 24f);

            foreach (Pawn relatedPawn in this.SelPawn.relations.FamilyByBlood)
            {
                Widgets.Label(pawnRect, relatedPawn + " - " + this.SelPawn.GetRelations(relatedPawn).GetEnumerator());
                pawnRect.y += 24f;
            }

            Rect rect2 = new Rect(rect.x, pawnRect.yMax + 15f, rect.width, rect.height - checkbox.yMax);

            GUILayout.BeginArea(rect2);
            GUILayout.BeginVertical();

           // Listing_Standard list = new Listing_Standard();
           // list.Begin(rect2);

            this.SelPawn.GetCompAnim(out CompBodyAnimator _);
            if (SelPawn.GetCompAnim().IgnoreRenderer)
            {
                _rotation = GUILayout.SelectionGrid(_rotation, this._psiToolbarStrings, 4);
                if (GUI.changed)
                {
                    this.SelPawn.Rotation = new Rot4(_rotation);
                }
            }
            else
            {
                _rotation = this.SelPawn.Rotation.AsInt;
            }


            bool male = this.SelPawn.gender == Gender.Male;

            if (compFace.PawnCrownType == CrownType.Average)
            {
                switch (compFace.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageNormalOffset: " + MeshPoolFS.EyeMaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFS.EyeMaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeMaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleAverageNormalOffset: "
                                + MeshPoolFS.MouthMaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFS.MouthMaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthMaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageNormalOffset: " + MeshPoolFS.EyeFemaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFS.EyeFemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeFemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAverageNormalOffset: "
                                + MeshPoolFS.MouthFemaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFS.MouthFemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthFemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAveragePointyOffset: " + MeshPoolFS.EyeMaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFS.EyeMaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeMaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleAveragePointyOffset: "
                                + MeshPoolFS.MouthMaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFS.MouthMaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthMaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAveragePointyOffset: " + MeshPoolFS.EyeFemaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFS.EyeFemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeFemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAveragePointyOffset: "
                                + MeshPoolFS.MouthFemaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFS.MouthFemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthFemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageWideOffset.x: " + MeshPoolFS.EyeMaleAverageWideOffset.ToString("N5"));
                            MeshPoolFS.EyeMaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeMaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleAverageWideOffset.x: " + MeshPoolFS.MouthMaleAverageWideOffset.ToString("N5"));
                            MeshPoolFS.MouthMaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthMaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageWideOffset: " + MeshPoolFS.EyeFemaleAverageWideOffset.ToString("N5"));
                            MeshPoolFS.EyeFemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeFemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAverageWideOffset: "
                                + MeshPoolFS.MouthFemaleAverageWideOffset.ToString("N5"));
                            MeshPoolFS.MouthFemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthFemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;
                }
            }
            else
            {
                switch (compFace.PawnHeadType)
                {
                    case HeadType.Normal:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowNormalOffset.x: " + MeshPoolFS.EyeMaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFS.EyeMaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeMaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowNormalOffset.x: "
                                + MeshPoolFS.MouthMaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFS.MouthMaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthMaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowNormalOffset: " + MeshPoolFS.EyeFemaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFS.EyeFemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeFemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowNormalOffset: "
                                + MeshPoolFS.MouthFemaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFS.MouthFemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthFemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowPointyOffset.x: " + MeshPoolFS.EyeMaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFS.EyeMaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeMaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeMaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowPointyOffset.x: "
                                + MeshPoolFS.MouthMaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFS.MouthMaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthMaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowPointyOffset: " + MeshPoolFS.EyeFemaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFS.EyeFemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeFemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowPointyOffset: "
                                + MeshPoolFS.MouthFemaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFS.MouthFemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthFemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowWideOffset.x: " + MeshPoolFS.EyeMaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFS.EyeMaleNarrowWideOffset.x =
                                GUILayout.HorizontalSlider(MeshPoolFS.EyeMaleNarrowWideOffset.x, -0.2f, 0.2f);
                            MeshPoolFS.EyeMaleNarrowWideOffset.y =
                                GUILayout.HorizontalSlider(MeshPoolFS.EyeMaleNarrowWideOffset.y, -0.2f, 0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowWideOffset.x: " + MeshPoolFS.MouthMaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFS.MouthMaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthMaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthMaleNarrowWideOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowWideOffset: " + MeshPoolFS.EyeFemaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFS.EyeFemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.EyeFemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.EyeFemaleNarrowWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowWideOffset: " + MeshPoolFS.MouthFemaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFS.MouthFemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFS.MouthFemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFS.MouthFemaleNarrowWideOffset.y,
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
                    list.Add(
                        new FloatMenuOption(
                            localOut.label,
                            delegate
                                {
                                    foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(
                                        x => x.HasCompFace()))
                                    {
                                        Color color = Color.white;
                                        pawn.GetCompFace(out CompFace comp2);
                                        if (comp2.PawnFaceGraphic != null)
                                        {
                                            comp2.PawnFaceGraphic.MouthGraphic =
                                            GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                                                                                                current.texPath,
                                                                                                ShaderDatabase
                                                                                               .CutoutSkin,
                                                                                                Vector2.one,
                                                                                                color) as
                                            Graphic_Multi_NaturalHeadParts;
                                        }
                                    }
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
       // public static bool IgnoreRenderer;
    }

}