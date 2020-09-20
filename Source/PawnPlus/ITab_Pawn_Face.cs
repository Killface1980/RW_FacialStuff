using System.Collections.Generic;
using System.Linq;
using PawnPlus.Defs;
using PawnPlus.Graphics;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnPlus
{
    public class ITab_Pawn_Face : ITab
    {
        private readonly string[] _psiToolbarStrings = { "North", "East", "South", "West" };

        private static int _rotation = 2;

        // private Pawn _pawn;

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

            Widgets.CheckboxLabeled(checkbox, "Ignore renderer", ref this.SelPawn.GetCompAnim().IgnoreRenderer);

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
            if (this.SelPawn.GetCompAnim().IgnoreRenderer)
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

            FullHead fullHead = MeshPoolFS.GetFullHeadType(SelPawn.gender, compFace.PawnCrownType, compFace.PawnHeadType);
            GUILayout.Label("Eye" + fullHead.ToString() + "Offset: NOT USED");
            float x = GUILayout.HorizontalSlider(0f, -0.2f, 0.2f);
            float y = GUILayout.HorizontalSlider(0f, -0.2f, 0.2f);

            GUILayout.Label("Mouth" + fullHead.ToString() + "Offset: " + MeshPoolFS.mouthOffsetsHeadType[(int)fullHead].ToString("N5"));
            Vector3 mouthOffset = MeshPoolFS.mouthOffsetsHeadType[(int)fullHead];
            mouthOffset.x = GUILayout.HorizontalSlider(mouthOffset.x, -0.2f, 0.2f);
            mouthOffset.y = GUILayout.HorizontalSlider(mouthOffset.y, -0.2f, 0.2f);
            MeshPoolFS.mouthOffsetsHeadType[(int)fullHead] = mouthOffset;
        
            GUILayout.EndVertical();

            if (GUILayout.Button("Mouth"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                // TODO: don't know what this is for. disable because I'm removing PawnFaceGraphic.MouthGraphic member variable
                /*foreach (MouthDef current in DefDatabase<MouthDef>.AllDefs)
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
                }*/

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