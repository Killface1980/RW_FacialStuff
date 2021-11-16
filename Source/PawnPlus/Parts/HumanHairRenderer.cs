namespace PawnPlus.Parts
{
    using PawnPlus.Graphics;
    using RimWorld;
    using System.Collections.Generic;
    using UnityEngine;
    using Verse;

    public class HumanHairRenderer : PartRendererBase
    {
        public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);

        private Pawn _pawn;
        private CompFace _compFace;

        public override void Initialize(
            Pawn pawn,
            BodyDef bodyDef,
            string defaultTexPath,
            Dictionary<string, string> namedTexPaths,
            BodyPartSignals bodyPartSignals)
        {
            _pawn = pawn;
            _compFace = pawn.GetComp<CompFace>();
        }

        public override void Update(
            PawnState pawnState,
            BodyPartStatus bodyPartStatus,
            ref bool updatePortrait)
        {
        }

        public override void Render(
            Vector3 rootPos,
            Quaternion rootQuat,
            Rot4 rootRot4,
            Vector3 renderNodeOffset,
            Mesh renderNodeMesh,
            int partIdentifier,
            bool portrait)
        {
            PawnGraphicSet graphics = _pawn.Drawer.renderer.graphics;
            Vector3 hairDrawLoc = rootPos;
            hairDrawLoc += additionalOffset;
            Graphic_Hair hairGraphic = graphics.hairGraphic as Graphic_Hair;
            if (hairGraphic != null)
            {
                Material hairBasemat = hairGraphic.MatAt(rootRot4, _compFace.CurrentHeadCoverage);
                if (!portrait && _pawn.IsInvisible())
                {
                    // Invisibility shader ignores the mask texture used in this mod's custom hair shader, which
                    // cuts out the parts of hair that are poking through headwear.
                    // However, decompiling vanilla invisibility shader and writing an equivalent shader for this mod's
                    // custom shader is rather difficult. The only downside of using the vanilla invisibility shader
                    // is the graphical artifacts, so fixing it will be a low priority task.
                    hairBasemat = InvisibilityMatPool.GetInvisibleMat(hairBasemat);
                }

                // Similar to the invisibility shader, a separate damaged mat shader needs to be written for this
                // mod's custom hair shader. However, the effect is so subtle that taking time to decompile the vanilla
                // shader and writing a custom shader isn't worth the time.
                // graphics.flasher.GetDamagedMat(baseMat);
                Texture2D maskTex = hairBasemat.GetMaskTexture();
                GenDraw.DrawMeshNowOrLater(
                    graphics.HairMeshSet.MeshAt(rootRot4),
                    mat: hairBasemat,
                    loc: hairDrawLoc,
                    quat: rootQuat,
                    drawNow: portrait);
            }
            else
            {
                Log.ErrorOnce(
                    "Pawn Plus: " + _pawn.Name + " has CompFace but doesn't have valid hair graphic of Graphic_Hair class",
                    ("PawnPlus_CompFaceNoValidHair" + _pawn.Name).GetHashCode());
            }
        }

        public override void DoCustomizationGUI(Rect contentRect)
        {
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}