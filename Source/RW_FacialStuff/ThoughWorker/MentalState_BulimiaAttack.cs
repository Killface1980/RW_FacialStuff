using RimWorld;
using Verse.AI;

namespace RW_FacialStuff
{
    public class MentalState_BulimiaAttack : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }
}
