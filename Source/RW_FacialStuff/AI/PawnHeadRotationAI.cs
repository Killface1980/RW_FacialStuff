using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff.AI
{
	// This class helps simulate head movement so head moves separately from body depending on situation.
	public class PawnHeadRotationAI
	{
		public enum TargetMode 
		{
			// Value determines the actions priority

			// No particular target to look at 
			None = 0,
			// Ocassionally turn head when sleeping. No target
			Sleep = 1,
			// Look at someone nearby (can be disabled in settings)
			RandomStare = 2,
			// Target is a social interaction initiator. This means this instance is a recipient, which
			// has delay of kSocialInteractionRecipientDelayTick before looking at the initiator.
			SocialInitiator = 3,
			// Target is a social interaction recipient
			SocialRecipient = 4,
			// Look at the aimed Thing
			Aim = 5
		}

		#region Constants

		// The amount of ticks needed for the recipient of social interaction to wait before moving its head
		private const int kSocialInteractionRecipientDelayTick = 30;
		// How long the social interaction lasts
		private const int kSocialInteractionDurationTick = 300;
		// The speed that pawn can rotate head. Unit in deg/tick ( = degrees per 1/60 of second)
		private const float kHeadRotationRate = 10;

		#endregion

		#region Private Member Variable

		private Pawn _pawn;
		private Rot4 _curRot;
		private Rot4 _prevBodyRot;
		private Thing _target;
		private TargetMode _curTargetMode = TargetMode.None;
		// How many ticks have passed since target was set
		private int _curTargetTicks;
		// Facing north is 0 degrees. Unit in degrees. Clockwise is positive.
		private float _curAngle;
		// Facing north is 0 degrees. unit in degrees. Clockwise is positive.
		private float _targetAngle;
		
		#endregion

		public Rot4 CurrentRotation => _curRot;

		public PawnHeadRotationAI(Pawn pawn)
		{
			_pawn = pawn;
			_curRot = pawn.Rotation;
			_curAngle = pawn.Rotation.AsAngle;
			_prevBodyRot = pawn.Rotation;
		}

		#region Public Methods

		public void SetTarget(Thing target, TargetMode mode)
		{
			// Targets have priorities. Only set the target if new target is of higher priority 
			// than the existing one.
			if(_curTargetMode <= mode)
			{
				_curTargetMode = mode;
				_target = target;
				_curTargetTicks = 0;
			}
		}
		
		public void Tick(bool canUpdatePawn, Rot4 bodyRot, PawnState pawnState)
		{
			if(!bodyRot.IsValid)
			{
				Log.Warning(
					"Facial Stuff: invalid body rotation given for PawnHeadRotationAI.Tick() (value: " 
					+ bodyRot.AsInt + ") Pawn:" + _pawn?.ToString());
				return;
			}
			if(_pawn == null)
			{
				Log.Warning("Facial Stuff: tried to update head rotation when pawn is null");
				return;
			}
			if(!canUpdatePawn)
			{
				ResetHeadTarget(bodyRot);
				return;
			}
			if(TickTargetMode(bodyRot, pawnState) && Mathf.Abs(_curAngle - _targetAngle) > 0.1f)
			{
				float targetAngle = _targetAngle;
				float curAngle = _curAngle;
				GlobalAngleToLocalAngle(bodyRot, ref targetAngle);
				GlobalAngleToLocalAngle(bodyRot, ref curAngle);
				ClampLocalAngleToAllowedRange(ref targetAngle);
				// Clamping current angle is necessary in case body rotation changes.
				ClampLocalAngleToAllowedRange(ref curAngle);
				float angleDiff = targetAngle - curAngle;
				// If the difference is smaller than the rotation rate per tick...
				if(Mathf.Abs(angleDiff) < kHeadRotationRate)
				{
					// ... then immediately set the angle to the target to avoid overshooting.
					curAngle = targetAngle;
				} else
				{
					// Otherwise move the head as normal. Mathf.Sign() indicates direction of movement
					curAngle += Mathf.Sign(targetAngle - curAngle) * kHeadRotationRate;
				}
				LocalAngleToGlobalAngle(bodyRot, ref curAngle);
				_curAngle = curAngle;
				_curRot = Rot4.FromAngleFlat(_curAngle);
			}

			_prevBodyRot = bodyRot;
			++_curTargetTicks;

			/*
			Original code for pawns randomly looking at each other. May be performance intensive so leave it as option

			 float rand = Rand.Value;

            // Look at each other
            if (rand > 0.5f)
            {
                IntVec3 position = this._pawn.Position;

                // 8 = 1 field; 24 = 2 fields;
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 intVec = position + GenRadial.RadialPattern[i];
                    if (intVec.InBounds(this._pawn.Map))
                    {
                        Thing thing = intVec.GetThingList(this._pawn.Map)?.Find(x => x is Pawn);

                        if (!(thing is Pawn otherPawn) || otherPawn == this._pawn || !otherPawn.Spawned) // || otherPawn.Dead || otherPawn.Downed)
                        {
                            continue;
                        }
                        
                        if (!this._pawn.CanSee(otherPawn)) continue;

                        // Log.Message(this.pawn + " will look at random pawn " + thing);
                        this._target = otherPawn;
                        return;
                    }
                }
            }
			*/
		}

		public bool TickTargetMode(Rot4 bodyRot, PawnState pawnState)
		{
			// If pawn is aiming at something, set aim target mode.
			if(_pawn.stances.curStance is Stance_Busy stance && !stance.neverAimWeapon && stance.focusTarg.HasThing)
			{
				SetTarget(stance.focusTarg.Thing, TargetMode.Aim);
			}
			// If pawn is no longer aiming, reset the target mode.
			else if(_curTargetMode == TargetMode.Aim)
			{
				_curTargetMode = TargetMode.None;
			}

			// Do not rotate head when sleeping.
			if(pawnState.sleeping)
			{
				_curTargetMode = TargetMode.None;
			}
			if(_target != null && _target.Destroyed)
			{
				_curTargetMode = TargetMode.None;
			}
			switch(_curTargetMode)
			{ 
				case TargetMode.None:
					ResetHeadTarget(bodyRot);
					// Immediately track the body rotation if there is no target when body direction changes.
					if(bodyRot != _prevBodyRot)
					{
						_curAngle = _targetAngle;
						_curRot = bodyRot;
						// Angle is same as target angle. There is no need to move head.
						return false;
					}
					return true;

				case TargetMode.SocialInitiator:
					// Wait for kSocialInteractionRecipientDelayTick before moving head
					if(_curTargetTicks < kSocialInteractionRecipientDelayTick)
					{
						return false;
					}
					goto case TargetMode.SocialRecipient;

				case TargetMode.SocialRecipient:
					// End social interaction if enough time has passed
					// or if pawn is unable to see the target
					if(_curTargetTicks > kSocialInteractionDurationTick ||
						!_pawn.CanSee(_target))
					{
						ResetHeadTarget(bodyRot);
						return true;
					}

					UpdateTargetAngle();
					return true;

				case TargetMode.Aim:
					UpdateTargetAngle();
					return true;
			}
			return true;
		}

		#endregion

		#region Private Methods

		private void UpdateTargetAngle()
		{
			if(_target != null)
			{
				_targetAngle = (_target.Position - _pawn.Position).AngleFlat;
			}
			else
			{
				Log.Warning("Facial Stuff: tried to update head angle when there is no target to turn the head towards");
			}
		}

		private void ResetHeadTarget(Rot4 bodyRot)
		{
			_curTargetMode = TargetMode.None;
			_targetAngle = bodyRot.AsAngle;
			_target = null;
			_curTargetTicks = 0;
		}
		
		// Prevent pawns from looking right behind their back
		private static void ClampLocalAngleToAllowedRange(ref float angle)
		{
			if(angle >= -90f && angle <= 90f)
			{
				return;
			}
			// Find the absolute difference between the current angle, and the max angles for ccw/cw direction,
			// then choose the direction with smaller difference
			float cwMaxDiff = Mathf.Abs(90f - angle);
			float ccwMaxDiff = Mathf.Abs(-90f - angle);
			if(cwMaxDiff < ccwMaxDiff)
			{
				angle = 90f;
			}
			else
			{
				angle = -90f;
			}
		}

		private static void GlobalAngleToLocalAngle(Rot4 basisRot, ref float angle)
		{
			// Global angle is simply angle with respect to north. Local angle is the angle with respect to the 
			// whatever direction given.
			// Ex. 90 degrees angle with basis of north (which is directly point at east) can be thought as 0 degrees for the 
			// basis pointing east.
			// Using localized angle has the benefit of not having to specify 4 different min/max angles for all of directions when
			// clamping the angle.
			angle = angle - basisRot.AsInt * 90f;
		}

		private static void LocalAngleToGlobalAngle(Rot4 basisrot, ref float localAngle)
		{
			// Convert the "localized" angle back to the ordinary angle with basis towards north
			localAngle = localAngle + basisrot.AsInt * 90f;
		}

		#endregion
	}
}
