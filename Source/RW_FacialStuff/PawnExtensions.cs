using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace FacialStuff
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

        private static void CheckFaceForAddedParts(Hediff hediff, CompFace face, BodyPartRecord leftEye,
                                      BodyPartRecord rightEye,
                                      BodyPartRecord jaw)
        {
            if (!face.Pawn.RaceProps.Humanlike || hediff.Part == null)
            {
                return;

            }

            if (face.Props.hasEyes)
            {
                if (hediff.Part == leftEye)
                {
                    face.TexPathEyeLeftPatch = StringsFS.PathHumanlike + "AddedParts/Eye_" + hediff.def.defName + "_Left" + "_"
                                             + face.PawnCrownType;
                }

                if (hediff.Part == rightEye)
                {
                    face.TexPathEyeRightPatch = StringsFS.PathHumanlike + "AddedParts/Eye_" + hediff.def.defName + "_Right" + "_"
                                              + face.PawnCrownType;
                    face.BodyStat.EyeRight = PartStatus.Artificial;

                }
            }

            if (face.Props.hasMouth)
            {
                if (hediff.Part == jaw)
                {
                    face.BodyStat.Jaw = PartStatus.Artificial;
                    face.TexPathJawAddedPart = StringsFS.PathHumanlike + "AddedParts/Mouth_" + hediff.def.defName;
                }
            }
        }

        private static void CheckMissingParts(BodyProps bodyProps)
        {
            Hediff hediff = bodyProps._hediff;

            if (hediff.def != HediffDefOf.MissingBodyPart)
            {
                return;
            }

            if (bodyProps._face != null)
            {
                if (bodyProps._face.Props.hasEyes)
                {
                    if (hediff.Part == bodyProps._leftEye)
                    {
                        bodyProps._face.BodyStat.EyeLeft = PartStatus.Missing;
                    }

                    if (hediff.Part == bodyProps._rightEye)
                    {
                        bodyProps._face.BodyStat.EyeRight = PartStatus.Missing;
                    }
                }
                if (bodyProps._face.Props.hasEars)
                {
                    if (hediff.Part == bodyProps._leftEar)
                    {
                        bodyProps._face.BodyStat.EarLeft = PartStatus.Missing;
                    }

                    if (hediff.Part == bodyProps._rightEar)
                    {
                        bodyProps._face.BodyStat.EarRight = PartStatus.Missing;
                    }
                }
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

            BodyPartRecord leftEye = body.Find(x => x.customLabel == "left eye");
            BodyPartRecord rightEye = body.Find(x => x.customLabel == "right eye");
            BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);


            //BodyPartRecord leftArm = body.Find(x => x.def == BodyPartDefOf.LeftArm);
            //BodyPartRecord rightArm = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("RightShoulder"));
            BodyPartRecord leftHand = body.Find(x => x.customLabel == "left hand");
            BodyPartRecord rightHand = body.Find(x => x.customLabel == "right hand");

            BodyPartRecord leftFoot = body.Find(x => x.customLabel == "left foot");
            BodyPartRecord rightFoot = body.Find(x => x.customLabel == "right foot");

            BodyPartRecord leftArm = body.Find(x => x.customLabel == "left hand");
            BodyPartRecord rightArm = body.Find(x => x.customLabel == "right hand");

            BodyPartRecord leftLeg = body.Find(x => x.customLabel == "left foot");
            BodyPartRecord rightLeg = body.Find(x => x.customLabel == "right foot");

            if (missing)
            {
                CheckMissingParts(new BodyProps(hediff, face, anim, leftEye, rightEye, leftHand, rightHand, leftFoot,
                                                rightFoot));
                return;
            }

            // Missing parts first, hands and feet can be replaced by arms/legs
            //  Log.Message("Checking missing parts.");
            AddedBodyPartProps addedPartProps = hediff.def?.addedPartProps;
            if (addedPartProps == null)
            {
                //    Log.Message("No added parts found.");
                return;
            }

            if (hediff.def?.defName == null)
            {
                return;
            }

            //  Log.Message("Checking face for added parts.");
            if (anim != null && anim.Pawn.RaceProps.Humanlike && face != null)
            {
                CheckFaceForAddedParts(hediff, face, leftEye, rightEye, jaw);
            }

            //  Log.Message("Checking body for added parts.");

            CheckBodyForAddedParts(hediff, anim, leftHand, leftArm, rightHand, rightArm, leftFoot, leftLeg, rightFoot,
                rightLeg);
        }

        public static bool Aiming(this Pawn pawn)
        {
            return pawn.stances.curStance is Stance_Busy stanceBusy && !stanceBusy.neverAimWeapon &&
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
            // if (!pawn.RaceProps.Humanlike)
            // {
            //     return;
            // }

            //   string log = "Checking for parts on " + pawn.LabelShort + " ...";

            if (!Controller.settings.ShowExtraParts)
            {
                //      log += "\n" + "No extra parts in options, return";
                //      Log.Message(log);
                return false;
            }

            // no head => no face
            if (!pawn.health.hediffSet.HasHead)
            {
                //      log += "\n" + "No head, return";
                //      Log.Message(log);
                return false;
            }

            // Reset the stats
            if (pawn.GetCompFace(out CompFace face))
            {
                face.BodyStat.EyeLeft = PartStatus.Natural;
                face.BodyStat.EyeRight = PartStatus.Natural;
                face.BodyStat.EarLeft = PartStatus.Natural;
                face.BodyStat.EarRight = PartStatus.Natural;
                face.BodyStat.Jaw = PartStatus.Natural;
            }

            if (pawn.GetCompAnim(out CompBodyAnimator anim))
            {
                anim.BodyStat.HandLeft = PartStatus.Natural;
                anim.BodyStat.HandRight = PartStatus.Natural;
                anim.BodyStat.FootLeft = PartStatus.Natural;
                anim.BodyStat.FootRight = PartStatus.Natural;
            }

            List<BodyPartRecord> allParts = pawn.RaceProps?.body?.AllParts;
            if (allParts.NullOrEmpty())
            {
                //     log += "\n" + "All parts null or empty, return";
                //     Log.Message(log);
                return false;
            }

            List<Hediff> hediffs = pawn.health?.hediffSet?.hediffs.Where(x => !x.def.defName.NullOrEmpty()).ToList();

            if (hediffs.NullOrEmpty())
            {
                // || hediffs.Any(x => x.def == HediffDefOf.MissingBodyPart && x.Part.def == BodyPartDefOf.Head))
                //     log += "\n" + "Hediffs null or empty, return";
                //     Log.Message(log);
                return false;
            }

            foreach (Hediff diff in hediffs.Where(diff => diff.def == HediffDefOf.MissingBodyPart))
            {
                // Log.Message("Checking missing part "+diff.def.defName);
                CheckPart(allParts, diff, face, anim, true);
            }

            foreach (Hediff diff in hediffs.Where(diff => diff.def.addedPartProps != null))
            {
                //  Log.Message("Checking added part on " + pawn + "--"+diff.def.defName);
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

        public static bool GetPawnFace([NotNull] this Pawn pawn, [CanBeNull] out PawnFace pawnFace)
        {
            pawnFace = null;

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return false;
            }

            PawnFace face = compFace.PawnFace;
            if (face != null)
            {
                pawnFace = face;
                return true;
            }

            return false;
        }

        public static bool HasCompAnimator([NotNull] this Pawn pawn)
        {
            return pawn.def.HasComp(typeof(CompBodyAnimator));
        }

        public static bool HasCompFace([NotNull] this Pawn pawn)
        {
            return pawn.def.HasComp(typeof(CompFace));
        }

        public static bool HasPawnFace([NotNull] this Pawn pawn)
        {
            if (pawn.GetCompFace(out CompFace compFace))
            {
                PawnFace face = compFace.PawnFace;
                return face != null;
            }

            return false;
        }
    }
}