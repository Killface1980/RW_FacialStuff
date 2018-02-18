using UnityEngine;

namespace FacialStuff
{
    public struct JointLister
    {
        #region Public Fields

        public JointType jointType;

        public Vector3 LeftJoint;

        public Vector3 RightJoint;

        #endregion Public Fields
    }

    public enum JointType : byte
    {
        Shoulder,
        Hip
    }
}