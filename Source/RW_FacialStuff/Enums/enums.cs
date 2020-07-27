namespace FacialStuff
{
    public static class StringsFS
    {
        public const string PathHumanlike = "Things/Pawn/Humanlike/";

    }
    public struct BodyPartStats
    {
        public PartStatus HandLeft;

        public PartStatus HandRight;

        public PartStatus FootLeft;

        public PartStatus FootRight;
    }

    public struct FacePartStats
    {
        public PartStatus EyeLeft;

        public PartStatus EyeRight;

        public PartStatus EarLeft;

        public PartStatus EarRight;

        public PartStatus Jaw;
    }
    public enum HandsToDraw
    {
        Both = 0,
        RightHand = 1,
        LeftHand = 2
    }
    public enum FullHead : byte
    {
        MaleAverageNormal = 0,

        MaleAveragePointy = 1,

        MaleAverageWide = 2,

        MaleNarrowNormal = 3,

        MaleNarrowPointy = 4,

        MaleNarrowWide = 5,

        FemaleAverageNormal = 6,

        FemaleAveragePointy = 7,

        FemaleAverageWide = 8,

        FemaleNarrowNormal = 9,

        FemaleNarrowPointy = 10,

        FemaleNarrowWide = 11,

        Undefined = 12
    }
}