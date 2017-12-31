// ReSharper disable StyleCop.SA1401
namespace FacialStuff.Defs
{
    public class PawnKeyframe
    {
        #region Public Fields

        public float? BodyAngle;
        public float? BodyAngleVertical;

        // public float? BodyOffsetVerticalZ;
        public float? BodyOffsetZ;

        public float? FootAngle;

        // public float? FootPositionVerticalZ;
        public float? FootPositionX;

        public float? FootPositionZ;

        // Quadrupeds
        public float? FrontPawAngle;

        // public float? FrontPawPositionVerticalZ;
        public float? FrontPawPositionX;

        public float? FrontPawPositionZ;
        public float? HandsSwingAngle;

        // public float? HandsSwingPosVertical;
        public float? HipOffsetHorizontalX;

        public float? ShoulderOffsetHorizontalX;
        public int keyIndex;

        public KeyStatus status = KeyStatus.Automatic;

        public float shift;

        #endregion Public Fields

        #region Public Constructors

        public PawnKeyframe()
        {
        }

        public PawnKeyframe(int index)
        {
            this.keyIndex = index;
        }

        #endregion Public Constructors
    }
}