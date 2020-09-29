using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace PawnPlus
{
    public class BodyProps
    {
        public BodyPartRecord _rightFoot;
        public BodyPartRecord _leftFoot;
        public BodyPartRecord _rightHand;
        public BodyPartRecord _leftHand;
        public BodyPartRecord _rightEye;
        public BodyPartRecord _leftEye;
        public BodyPartRecord _rightEar;
        public BodyPartRecord _leftEar;
        public CompBodyAnimator _anim;
        public CompFace _face;
        public Hediff _hediff;

        public BodyProps(Hediff hediff, CompFace face, CompBodyAnimator anim, BodyPartRecord leftEye, BodyPartRecord rightEye, BodyPartRecord leftHand, BodyPartRecord rightHand, BodyPartRecord leftFoot, BodyPartRecord rightFoot)
        {
            this._hediff = hediff;
            this._face = face;
            this._anim = anim;
            this._leftEye = leftEye;
            this._rightEye = rightEye;
            this._leftHand = leftHand;
            this._rightHand = rightHand;
            this._leftFoot = leftFoot;
            this._rightFoot = rightFoot;
        }
    }

    public static class PawnExtensions
    {

        private static void CheckBodyForAddedParts(Hediff hediff, CompBodyAnimator anim, BodyPartRecord leftHand, BodyPartRecord leftArm,
                                      BodyPartRecord rightHand, BodyPartRecord rightArm, BodyPartRecord leftFoot, BodyPartRecord leftLeg, BodyPartRecord rightFoot, BodyPartRecord rightLeg)
        {
            if (anim == null)
            {
                return;
            }

            if (anim.Props.bipedWithHands)
            {
                if (hediff.Part.parts.Contains(leftHand) || hediff.Part.parts.Contains(leftArm))
                {
                    anim.BodyStat.HandLeft = PartStatus.Artificial;
                }

                if (hediff.Part.parts.Contains(rightHand) || hediff.Part.parts.Contains(rightArm))
                {
                    anim.BodyStat.HandRight = PartStatus.Artificial;
                }
            }

            if (hediff.Part.parts.Contains(leftFoot) || hediff.Part.parts.Contains(leftLeg))
            {
                anim.BodyStat.FootLeft = PartStatus.Artificial;
            }

            if (hediff.Part.parts.Contains(rightFoot) || hediff.Part.parts.Contains(rightLeg))
            {
                anim.BodyStat.FootRight = PartStatus.Artificial;
            }
        }
        
        private static void CheckMissingParts(BodyProps bodyProps)
        {
            Hediff hediff = bodyProps._hediff;

            if (hediff.def != HediffDefOf.MissingBodyPart)
            {
                return;
            }
            
            if (bodyProps._anim != null && bodyProps._anim.Props.bipedWithHands)
            {
                if (hediff.Part == bodyProps._leftHand)
                {
                    bodyProps._anim.BodyStat.HandLeft = PartStatus.Missing;
                }

                if (hediff.Part == bodyProps._rightHand)
                {
                    bodyProps._anim.BodyStat.HandRight = PartStatus.Missing;
                }

                if (hediff.Part == bodyProps._leftFoot)
                {
                    bodyProps._anim.BodyStat.FootLeft = PartStatus.Missing;
                }

                if (hediff.Part == bodyProps._rightFoot)
                {
                    bodyProps._anim.BodyStat.FootRight = PartStatus.Missing;
                }
            }
        }

        private static void CheckPart(List<BodyPartRecord> body, Hediff hediff, [CanBeNull] CompFace face,
                                      [CanBeNull]                                           CompBodyAnimator anim, bool missing)
        {
            if (body.NullOrEmpty() || hediff.def == null)
            {
                Log.Message("Body list or hediff.def is null or empty");
                return;
            }

            if (!hediff.Visible)
            {
                return;
            }

            BodyPartRecord leftEye = body.Find(x => x.untranslatedCustomLabel == "left eye");
            BodyPartRecord rightEye = body.Find(x => x.untranslatedCustomLabel == "right eye");
            BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);


            BodyPartRecord leftHand = body.Find(x => x.untranslatedCustomLabel == "left hand");
            BodyPartRecord rightHand = body.Find(x => x.untranslatedCustomLabel == "right hand");

            BodyPartRecord leftFoot = body.Find(x => x.untranslatedCustomLabel == "left foot");
            BodyPartRecord rightFoot = body.Find(x => x.untranslatedCustomLabel == "right foot");

            BodyPartRecord leftArm = body.Find(x => x.untranslatedCustomLabel == "left arm");
            BodyPartRecord rightArm = body.Find(x => x.untranslatedCustomLabel == "right arm");

            BodyPartRecord leftLeg = body.Find(x => x.untranslatedCustomLabel == "left foot");
            BodyPartRecord rightLeg = body.Find(x => x.untranslatedCustomLabel == "right foot");

            if(missing)
            {
                CheckMissingParts(new BodyProps(hediff, face, anim, leftEye, rightEye, leftHand, rightHand, leftFoot,
                                                rightFoot));
                return;
            }
            
            if(hediff.def?.defName == null)
            {
                return;
            }
            
            //  Log.Message("Checking body for added parts.");

            CheckBodyForAddedParts(hediff, anim, leftHand, leftArm, rightHand, rightArm, leftFoot, leftLeg, rightFoot,
                rightLeg);
        }

        public static bool Aiming(this Pawn pawn)
        {
            return 
                pawn.stances != null && 
                pawn.stances.curStance is Stance_Busy stanceBusy && 
                !stanceBusy.neverAimWeapon &&
                stanceBusy.focusTarg.IsValid;
        }

        public static bool ShowWeaponOpenly(this Pawn pawn)
        {
            return pawn.carryTracker?.CarriedThing == null && pawn.equipment?.Primary != null &&
                   (pawn.Drafted ||
                    (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) ||
                    (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon));
        }

        public static bool CheckForAddedOrMissingParts(this Pawn pawn)
        {
            if(!Controller.settings.ShowExtraParts)
            {
                return false;
            }

            // no head => no face
            if(!pawn.health.hediffSet.HasHead)
            {
                return false;
            }
            
            if(pawn.GetCompAnim(out CompBodyAnimator anim))
            {
                anim.BodyStat.HandLeft = PartStatus.Natural;
                anim.BodyStat.HandRight = PartStatus.Natural;
                anim.BodyStat.FootLeft = PartStatus.Natural;
                anim.BodyStat.FootRight = PartStatus.Natural;
            }

            List<BodyPartRecord> allParts = pawn.RaceProps?.body?.AllParts;
            if (allParts.NullOrEmpty())
            {
                return false;
            }

            List<Hediff> hediffs = pawn.health?.hediffSet?.hediffs.Where(x => !x.def.defName.NullOrEmpty()).ToList();

            if (hediffs.NullOrEmpty())
            {
                return false;
            }

            pawn.GetCompFace(out CompFace face);

            foreach (Hediff diff in hediffs.Where(diff => diff.def == HediffDefOf.MissingBodyPart))
            {
                CheckPart(allParts, diff, face, anim, true);
            }

            foreach (Hediff diff in hediffs.Where(diff => diff.def.addedPartProps != null))
            {
                CheckPart(allParts, diff, face, anim, false);
            }

            return true;
        }

        public static bool Fleeing(this Pawn pawn)
        {
            Job job = pawn.CurJob;
            return pawn.MentalStateDef == MentalStateDefOf.PanicFlee
                || (job != null && (job.def == JobDefOf.Flee || job.def == JobDefOf.FleeAndCower));
        }

        [CanBeNull]
        public static CompBodyAnimator GetCompAnim([NotNull] this Pawn pawn)
        {
            return pawn.GetComp<CompBodyAnimator>();
        }

        public static bool GetCompAnim([NotNull] this Pawn pawn, [NotNull] out CompBodyAnimator compAnim)
        {
            compAnim = pawn.GetComp<CompBodyAnimator>();
            return compAnim != null;
        }

        [CanBeNull]
        public static CompFace GetCompFace([NotNull] this Pawn pawn)
        {
            return pawn.GetComp<CompFace>();
        }
        public static bool GetCompFace([NotNull] this Pawn pawn, [NotNull] out CompFace compFace)
        {
            compFace = pawn.GetComp<CompFace>();
            return compFace != null;
        }
        
        public static bool HasCompAnimator([NotNull] this Pawn pawn)
        {
            return pawn.def.HasComp(typeof(CompBodyAnimator));
        }

        public static bool HasCompFace([NotNull] this Pawn pawn)
        {
            return pawn.def.HasComp(typeof(CompFace));
        }
    }
}