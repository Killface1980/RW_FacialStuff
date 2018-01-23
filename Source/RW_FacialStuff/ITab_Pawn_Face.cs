using System.Collections.Generic;
using System.Linq;
using FacialStuff.AnimatorWindows;
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
        public static Vector3 RightHandPosition;

        public static Vector3 LeftHandPosition;
        private Pawn pawn;

        public ITab_Pawn_Face()
        {
            this.labelKey = "TabFace";
            this.tutorTag = "Face";
        }

        public override bool IsVisible => this.SelPawn.HasCompFace();


        protected override void FillTab()
        {
            if (this.pawn != this.SelPawn)
            {
                this.pawn = this.SelPawn;
                LeftHandPosition = RightHandPosition = WeaponOffset = AimedWeaponOffset = Vector3.zero;
            }
            if (!this.SelPawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            Rect rect = new Rect(10f, 10f, 330f, 330f);

            Rect checkbox = new Rect(rect.x, rect.y, rect.width, 24f);

            Widgets.CheckboxLabeled(checkbox, "Ignore renderer", ref compFace.IgnoreRenderer);
            if (GUI.changed)
            {
                IgnoreRenderer = compFace.IgnoreRenderer;
            }

            Rect pawnRect = new Rect(rect.x, checkbox.yMax, rect.width, 24f);

            foreach (Pawn relatedPawn in this.SelPawn.relations.FamilyByBlood)
            {
                Widgets.Label(pawnRect, relatedPawn + " - " + this.SelPawn.GetRelations(relatedPawn).GetEnumerator());
                pawnRect.y += 24f;
            }

            Rect rect2 = new Rect(rect.x, pawnRect.yMax + 15f, rect.width, rect.height - checkbox.yMax);

            GUILayout.BeginArea(rect2);
            GUILayout.BeginVertical();

            this.SelPawn.GetCompAnim(out CompBodyAnimator _);
            if (compFace.IgnoreRenderer)
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

            CompWeaponExtensions extensions = this.SelPawn.equipment?.Primary.GetComp<CompWeaponExtensions>();
            if (this.SelPawn.Drafted && extensions != null)
            {
                GUILayout.Label(this.pawn.equipment.Primary.def.defName);
                GUILayout.Label("Offset: " + WeaponOffset.x.ToString("N2") + " / " + WeaponOffset.y.ToString("N2") + " / " + WeaponOffset.z.ToString("N2"));
                WeaponOffset.x = GUILayout.HorizontalSlider(WeaponOffset.x, -1f, 1f);
                WeaponOffset.y = GUILayout.HorizontalSlider(WeaponOffset.y, -1f, 1f);
                WeaponOffset.z = GUILayout.HorizontalSlider(WeaponOffset.z, -1f, 1f);
                GUILayout.Label("OffsetAiming: " + AimedWeaponOffset.x.ToString("N2") + " / " + AimedWeaponOffset.y.ToString("N2") + " / " + AimedWeaponOffset.z.ToString("N2"));
                AimedWeaponOffset.x = GUILayout.HorizontalSlider(AimedWeaponOffset.x, -1f, 1f);
                AimedWeaponOffset.y = GUILayout.HorizontalSlider(AimedWeaponOffset.y, -1f, 1f);
                AimedWeaponOffset.z = GUILayout.HorizontalSlider(AimedWeaponOffset.z, -1f, 1f);

                GUILayout.Label("RH: " + RightHandPosition.x.ToString("N2") + " / " + RightHandPosition.z.ToString("N2"));
                RightHandPosition.x = GUILayout.HorizontalSlider(RightHandPosition.x, -1f, 1f);
                RightHandPosition.z = GUILayout.HorizontalSlider(RightHandPosition.z, -1f, 1f);

                GUILayout.Label("LH: " + LeftHandPosition.x.ToString("N2") + " / " + LeftHandPosition.z.ToString("N2"));
                LeftHandPosition.x = GUILayout.HorizontalSlider(LeftHandPosition.x, -1f, 1f);
                LeftHandPosition.z = GUILayout.HorizontalSlider(LeftHandPosition.z, -1f, 1f);

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
                                "MaleAverageNormalOffset: " + MeshPoolFs.EyeMaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFs.EyeMaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeMaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleAverageNormalOffset: "
                                + MeshPoolFs.MouthMaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFs.MouthMaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthMaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageNormalOffset: " + MeshPoolFs.EyeFemaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFs.EyeFemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeFemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAverageNormalOffset: "
                                + MeshPoolFs.MouthFemaleAverageNormalOffset.ToString("N5"));
                            MeshPoolFs.MouthFemaleAverageNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleAverageNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthFemaleAverageNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleAverageNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAveragePointyOffset: " + MeshPoolFs.EyeMaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFs.EyeMaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeMaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleAveragePointyOffset: "
                                + MeshPoolFs.MouthMaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFs.MouthMaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthMaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAveragePointyOffset: " + MeshPoolFs.EyeFemaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFs.EyeFemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeFemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAveragePointyOffset: "
                                + MeshPoolFs.MouthFemaleAveragePointyOffset.ToString("N5"));
                            MeshPoolFs.MouthFemaleAveragePointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleAveragePointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthFemaleAveragePointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleAveragePointyOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleAverageWideOffset.x: " + MeshPoolFs.EyeMaleAverageWideOffset.ToString("N5"));
                            MeshPoolFs.EyeMaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeMaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleAverageWideOffset.x: " + MeshPoolFs.MouthMaleAverageWideOffset.ToString("N5"));
                            MeshPoolFs.MouthMaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthMaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleAverageWideOffset: " + MeshPoolFs.EyeFemaleAverageWideOffset.ToString("N5"));
                            MeshPoolFs.EyeFemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeFemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleAverageWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleAverageWideOffset: "
                                + MeshPoolFs.MouthFemaleAverageWideOffset.ToString("N5"));
                            MeshPoolFs.MouthFemaleAverageWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleAverageWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthFemaleAverageWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleAverageWideOffset.y,
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
                                "MaleNarrowNormalOffset.x: " + MeshPoolFs.EyeMaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFs.EyeMaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeMaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowNormalOffset.x: "
                                + MeshPoolFs.MouthMaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFs.MouthMaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthMaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowNormalOffset: " + MeshPoolFs.EyeFemaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFs.EyeFemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeFemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowNormalOffset: "
                                + MeshPoolFs.MouthFemaleNarrowNormalOffset.ToString("N5"));
                            MeshPoolFs.MouthFemaleNarrowNormalOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleNarrowNormalOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthFemaleNarrowNormalOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleNarrowNormalOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Pointy:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowPointyOffset.x: " + MeshPoolFs.EyeMaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFs.EyeMaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeMaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeMaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowPointyOffset.x: "
                                + MeshPoolFs.MouthMaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFs.MouthMaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthMaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowPointyOffset: " + MeshPoolFs.EyeFemaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFs.EyeFemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeFemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowPointyOffset: "
                                + MeshPoolFs.MouthFemaleNarrowPointyOffset.ToString("N5"));
                            MeshPoolFs.MouthFemaleNarrowPointyOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleNarrowPointyOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthFemaleNarrowPointyOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleNarrowPointyOffset.y,
                                -0.2f,
                                0.2f);
                        }

                        break;

                    case HeadType.Wide:
                        if (male)
                        {
                            GUILayout.Label(
                                "MaleNarrowWideOffset.x: " + MeshPoolFs.EyeMaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFs.EyeMaleNarrowWideOffset.x =
                                GUILayout.HorizontalSlider(MeshPoolFs.EyeMaleNarrowWideOffset.x, -0.2f, 0.2f);
                            MeshPoolFs.EyeMaleNarrowWideOffset.y =
                                GUILayout.HorizontalSlider(MeshPoolFs.EyeMaleNarrowWideOffset.y, -0.2f, 0.2f);
                            GUILayout.Label(
                                "MouthMaleNarrowWideOffset.x: " + MeshPoolFs.MouthMaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFs.MouthMaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthMaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthMaleNarrowWideOffset.y,
                                -0.2f,
                                0.2f);
                        }
                        else
                        {
                            GUILayout.Label(
                                "FemaleNarrowWideOffset: " + MeshPoolFs.EyeFemaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFs.EyeFemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.EyeFemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.EyeFemaleNarrowWideOffset.y,
                                -0.2f,
                                0.2f);
                            GUILayout.Label(
                                "MouthFemaleNarrowWideOffset: " + MeshPoolFs.MouthFemaleNarrowWideOffset.ToString("N5"));
                            MeshPoolFs.MouthFemaleNarrowWideOffset.x = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleNarrowWideOffset.x,
                                -0.2f,
                                0.2f);
                            MeshPoolFs.MouthFemaleNarrowWideOffset.y = GUILayout.HorizontalSlider(
                                MeshPoolFs.MouthFemaleNarrowWideOffset.y,
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
                                    foreach (Pawn pawn in Find.VisibleMap.mapPawns.AllPawnsSpawned.Where(
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
        public static Vector3 WeaponOffset;
        public static Vector3 AimedWeaponOffset;
        public static bool IgnoreRenderer;
    }

}