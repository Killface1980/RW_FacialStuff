namespace FacialStuff.Drawer
{
    using FacialStuff.Harmony;

    using UnityEngine;

    using Verse;

    public abstract class BasicDrawer
    {
        protected BasicDrawer()
        {
        }

        protected Rot4 bodyFacing;

        protected Rot4 headFacing;

        protected JointLister GetJointPositions(Vector3 vector, float jointWidth, bool carrying = false)
        {
            var rot = this.bodyFacing;
            JointLister joints = new JointLister();
            float leftX = vector.x;
            float rightX = vector.x;
            float leftZ = vector.z;
            float rightZ = vector.z;

            float leftY = HarmonyPatch_PawnRenderer.YOffset_HandsFeet;
            float rightY = HarmonyPatch_PawnRenderer.YOffset_HandsFeet;

            if (carrying)
            {

                leftX -= jointWidth / 2;
                rightX += jointWidth / 2;
                leftZ = -0.025f;
                rightZ = -leftZ;
                if (rot == Rot4.North)
                {
                    leftY = rightY = -HarmonyPatch_PawnRenderer.YOffset_Body;
                }
            }
            else if (rot.IsHorizontal)
            {
                float offsetX = jointWidth / 8;
                float offsetZ = jointWidth * 0.6f;

                if (rot == Rot4.East)
                {

                    leftY = -HarmonyPatch_PawnRenderer.YOffset_Body;
                    leftZ += +offsetZ;

                }
                else
                {

                    rightY = -HarmonyPatch_PawnRenderer.YOffset_Body;
                    rightZ += offsetZ;

                }
                leftX += offsetX;
                rightX -= offsetX;


            }
            else
            {
                leftX = -rightX;

            }
            joints.rightJoint = new Vector3(rightX, rightY, rightZ);
            joints.leftJoint = new Vector3(leftX, leftY, leftZ);

            return joints;
        }

        public virtual Mesh GetPawnMesh(bool wantsBody, bool portrait)
        {
            return wantsBody ? MeshPool.humanlikeBodySet.MeshAt(bodyFacing) : MeshPool.humanlikeHeadSet.MeshAt(headFacing);
        }
    }

    public struct JointLister
    {
        public Vector3 leftJoint;

        public Vector3 rightJoint;
    }
}
