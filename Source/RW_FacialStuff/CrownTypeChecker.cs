namespace FacialStuff
{
    using FacialStuff.Enums;

    using Verse;

    public class CrownTypeChecker
    {
        private CompFace compFace;

        public CrownTypeChecker(CompFace compFace)
        {
            this.compFace = compFace;
            this.SetHeadOffsets();
        }

        private void SetHeadOffsets()
        {
            switch (this.compFace.pawn.gender)
            {
                case Gender.Male:
                    this.CheckMaleCrownType();
                    break;

                case Gender.Female:
                    this.CheckFemaleCrownType();
                    break;

                default:
                    this.compFace.FullHeadType = FullHead.MaleAverageNormal;
                    break;
            }
        }

        private void CheckFemaleCrownType()
        {
            switch (this.compFace.PawnCrownType)
            {
                case CrownType.Average:
                    this.CheckFemaleCrownTypeAverage();
                    break;

                case CrownType.Narrow:
                    this.CheckFemaleCrownTypeNarrow();
                    break;
            }
        }

        private void CheckFemaleCrownTypeAverage()
        {
            switch (this.compFace.PawnHeadType)
            {
                case HeadType.Normal:
                    this.compFace.FullHeadType = FullHead.FemaleAverageNormal;
                    break;

                case HeadType.Pointy:
                    this.compFace.FullHeadType = FullHead.FemaleAveragePointy;
                    break;

                case HeadType.Wide:
                    this.compFace.FullHeadType = FullHead.FemaleAverageWide;
                    break;
            }
        }

        private void CheckFemaleCrownTypeNarrow()
        {
            switch (this.compFace.PawnHeadType)
            {
                case HeadType.Normal:
                    this.compFace.FullHeadType = FullHead.FemaleNarrowNormal;
                    break;

                case HeadType.Pointy:
                    this.compFace.FullHeadType = FullHead.FemaleNarrowPointy;
                    break;

                case HeadType.Wide:
                    this.compFace.FullHeadType = FullHead.FemaleNarrowWide;
                    break;
            }
        }

        private void CheckMaleCrownType()
        {
            switch (this.compFace.PawnCrownType)
            {
                case CrownType.Average:
                    this.CheckMaleCrownTypeAverage();
                    break;

                case CrownType.Narrow:
                    this.CheckMaleCrownTypeNarrow();
                    break;
            }
        }

        private void CheckMaleCrownTypeAverage()
        {
            switch (this.compFace.PawnHeadType)
            {
                case HeadType.Normal:
                    this.compFace.FullHeadType = FullHead.MaleAverageNormal;
                    break;

                case HeadType.Pointy:
                    this.compFace.FullHeadType = FullHead.MaleAveragePointy;
                    break;

                case HeadType.Wide:
                    this.compFace.FullHeadType = FullHead.MaleAverageWide;
                    break;
            }
        }

        private void CheckMaleCrownTypeNarrow()
        {
            switch (this.compFace.PawnHeadType)
            {
                case HeadType.Normal:
                    this.compFace.FullHeadType = FullHead.MaleNarrowNormal;
                    break;

                case HeadType.Pointy:
                    this.compFace.FullHeadType = FullHead.MaleNarrowPointy;
                    break;

                case HeadType.Wide:
                    this.compFace.FullHeadType = FullHead.MaleNarrowWide;
                    break;
            }
        }
    }
}