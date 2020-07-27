// ReSharper disable StyleCop.SA1401
namespace FacialStuff.Defs
{
    public class PawnKeyframe
    {
        #region Public Fields

        public float? HeadAngleX;
        public float? HeadOffsetZ;
        public float? BodyAngle;
        public float? BodyAngleVertical;

        // public float? BodyOffsetVerticalZ;
        public float? BodyOffsetZ;

        public float? FootAngle;

        // public float? FootPositionVerticalZ;
        public float? FootPositionX;

        public float? FootPositionZ;

        public float? HandPositionX;

        public float? HandPositionZ;

        // Quadrupeds
        public float? FrontPawAngle;

        // public float? FrontPawPositionVerticalZ;
        public float? FrontPawPositionX;

        public float? FrontPawPositionZ;
        public float? HandsSwingAngle;

        // public float? HandsSwingPosVertical;
        public float? HipOffsetHorizontalX;

        public float? ShoulderOffsetHorizontalX;
        public int KeyIndex;

        public KeyStatus Status = KeyStatus.Automatic;

        public float Shift;

        #endregion Public Fields

        #region Public Constructors

        public PawnKeyframe()
        {
        }

        public PawnKeyframe(int index)
        {
            this.KeyIndex = index;
        }

        #endregion Public Constructors
    }
}