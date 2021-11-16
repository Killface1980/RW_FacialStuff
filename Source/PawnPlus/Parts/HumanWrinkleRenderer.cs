namespace PawnPlus.Parts
{
    using PawnPlus.Graphics;
    using System.Collections.Generic;
    using UnityEngine;
    using Verse;

    public class HumanWrinkleRenderer : PartRendererBase
    {
        public SimpleCurve ageIntensityCurve;
        public Vector3 additionalOffset = new Vector3(0f, 0f, 0f);

        private MaterialPropertyBlock _matPropBlock = new MaterialPropertyBlock();
        private TextureSet _textureSet;
        private Color _skinColor;

        public override void Initialize(
            Pawn pawn,
            BodyDef bodyDef,
            string defaultTexPath,
            Dictionary<string, string> namedTexPaths,
            BodyPartSignals bodyPartSignals)
        {
            _skinColor = pawn.story.SkinColor;
            float intensity = ageIntensityCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
            _skinColor.a = intensity;
            _textureSet = TextureSet.Create(defaultTexPath);
            _matPropBlock.SetTexture("_MainTex", _textureSet.GetTextureArray());
            _matPropBlock.SetColor("_Color", _skinColor);
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
            Vector3 offset = rootQuat * (renderNodeOffset + additionalOffset);
            _textureSet.GetIndexForRot(rootRot4, out float index);
            if (!portrait)
            {
                _matPropBlock.SetFloat(Shaders.TexIndexPropID, index);
                _matPropBlock.SetColor(Shaders.ColorOnePropID, _skinColor);
                Graphics.DrawMesh(
                    renderNodeMesh,
                    Matrix4x4.TRS(rootPos + offset, rootQuat, Vector3.one),
                    Shaders.FacePart,
                    0,
                    null,
                    0,
                    _matPropBlock);
            }
            else
            {
                Shaders.FacePart.mainTexture = _textureSet.GetTextureArray();
                Shaders.FacePart.SetFloat(Shaders.TexIndexPropID, index);
                Shaders.FacePart.SetColor(Shaders.ColorOnePropID, _skinColor);
                Shaders.FacePart.SetPass(0);
                Graphics.DrawMeshNow(renderNodeMesh, rootPos + offset, rootQuat);
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