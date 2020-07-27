using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public class GraphicVectorMeshSet
    {
        #region Public Fields

        public readonly GraphicMeshSet Mesh;

        #endregion Public Fields

        #region Private Fields

        private readonly Vector2 _offSet;

        #endregion Private Fields

        #region Public Constructors

        public GraphicVectorMeshSet(float size, Vector2 offSet)
        {
            this.Mesh = new GraphicMeshSet(size);
            this._offSet = offSet;
        }

        public GraphicVectorMeshSet(float sizeX, float sizeY, Vector2 offSet)
        {
            this.Mesh = new GraphicMeshSet(sizeX, sizeY);
            this._offSet = offSet;
        }

        #endregion Public Constructors

        #region Public Methods

        public Vector3 OffsetAt(Rot4 rotation)
        {
            return rotation.AsInt switch
            {
                1 => new Vector3(this._offSet.x, 0f, -this._offSet.y),
                2 => new Vector3(0f, 0f, -this._offSet.y),
                3 => new Vector3(-this._offSet.x, 0f, -this._offSet.y),
                _ => Vector3.zero
            };
        }

        #endregion Public Methods

        // float z = 1f * Mathf.Cos(num * (this.wheelRotation * 0.1f) % (2 * Mathf.PI));
        // float x = 1f * Mathf.Sin(num * (this.wheelRotation * 0.1f) % (2 * Mathf.PI));
        // Quaternion asQuat = rotation.AsQuat;
        // {
        // public Quaternion RotationAt(Rot4 rotation)
        // asQuat.SetLookRotation(new Vector3(x, 0f, z), Vector3.up);
        // return asQuat;
        // }
    }
}