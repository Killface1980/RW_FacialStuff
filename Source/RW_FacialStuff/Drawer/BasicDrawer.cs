using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public abstract class BasicDrawer
    {
        #region Protected Fields

        [NotNull]
        public Pawn Pawn { get; set; }

        [NotNull] protected PawnGraphicSet Graphics;

        protected Rot4 BodyFacing;

        protected Rot4 HeadFacing;

        #endregion Protected Fields

        #region Public Methods

        public virtual Mesh GetPawnMesh(bool wantsBody, bool portrait)
        {
            return wantsBody
                       ? MeshPool.humanlikeBodySet?.MeshAt(this.BodyFacing)
                       : MeshPool.humanlikeHeadSet?.MeshAt(this.HeadFacing);
        }

        #endregion Public Methods

        #region Protected Methods

        protected JointLister GetJointPositions(Vector3 offsets,
                                                float   jointWidth,
                                                bool    carrying          = false)
        {
            Rot4 rot = this.BodyFacing;
            JointLister joints = new JointLister();
            float leftX = offsets.x;
            float rightX = offsets.x;
            float leftZ = offsets.z;
            float rightZ = offsets.z;

            float offsetY =  Offsets.YOffset_HandsFeet;
            float leftY = offsetY;
            float rightY = offsetY;

            if (carrying)
            {
                leftX = -jointWidth / 2;
                rightX = jointWidth / 2;
                leftZ = -0.025f;
                rightZ = -leftZ;
            }
            else if (rot.IsHorizontal)
            {
                float offsetX = jointWidth * 0.1f;
                float offsetZ = jointWidth * 0.2f;

                if (rot == Rot4.East)
                {
                    leftY = -Offsets.YOffset_Behind;
                    leftZ += +offsetZ;
                }
                else
                {
                    rightY = -Offsets.YOffset_Behind;
                    rightZ += offsetZ;
                }

                leftX += offsetX;
                rightX -= offsetX;
            }
            else
            {
                leftX = -rightX;
            }

            if (rot == Rot4.North)
            {
                leftY = rightY = -Offsets.YOffset_Behind;
               // leftX *= -1;
               // rightX *= -1;
            }

            joints.RightJoint = new Vector3(rightX, rightY, rightZ);
            joints.LeftJoint = new Vector3(leftX, leftY, leftZ);

            return joints;
        }

        #endregion Protected Methods
    }
}