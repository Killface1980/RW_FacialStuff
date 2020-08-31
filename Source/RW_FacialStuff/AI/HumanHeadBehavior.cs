using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FacialStuff.AI
{
	// This class helps simulate head movement so head moves separately from body depending on situation.
	public class HumanHeadBehavior : IHeadBehavior
	{
		#region Configurable Member Variables

		public int socialRecipientDelayTick = 20;
		public int socialDurationTick = 240;
		public float headRotationRate = 10;

		#endregion

		#region Private Member Variable

		private Thing _target;
		private IHeadBehavior.TargetType _curTargetType = IHeadBehavior.TargetType.None;
		// Tick count when the target was set
		private int _targetStartTick;
		// Facing north is 0 degrees. Unit in degrees. Clockwise is positive.
		private float _curAngle;
		
		#endregion
		
		public void Initialize(Pawn pawn)
		{
			_curAngle = pawn.Rotation.AsAngle;
		}

		#region Public Methods

		public void SetTarget(Thing target, IHeadBehavior.TargetType mode)
		{
			// Targets have priorities. Only set the target if new target is of higher priority 
			// than the existing one.
			if(_curTargetType <= mode)
			{
				_curTargetType = mode;
				_target = target;
				_targetStartTick = Find.TickManager.TicksGame;
			}
		}
		
		public void Update(Pawn pawn, PawnState pawnState, out Rot4 headFacing)
		{
			if(!pawnState.alive)
			{
				headFacing = pawn.Rotation;
				_curTargetType = IHeadBehavior.TargetType.None;
				return;
			}
			if(!pawn.Rotation.IsValid)
			{
				Log.Warning(
					"Facial Stuff: invalid body rotation given for PawnHeadRotationAI.Tick() (value: " 
					+ pawn.Rotation.AsInt + ") Pawn:" + pawn?.ToString());
				headFacing = pawn.Rotation;
				return;
			}
			if(pawn == null)
			{
				Log.Warning("Facial Stuff: tried to update head rotation when pawn is null");
				headFacing = Rot4.North;
				return;
			}
			float targetAngle = 0f;
			if(UpdateTargetMode(pawn, pawnState, ref targetAngle) && Mathf.Abs(_curAngle - targetAngle) > 0.1f)
			{
				float curAngle = _curAngle;
				GlobalAngleToLocalAngle(pawn.Rotation, ref targetAngle);
				GlobalAngleToLocalAngle(pawn.Rotation, ref curAngle);
				ClampLocalAngleToAllowedRange(ref targetAngle);
				// Clamping current angle is necessary in case body rotation changes.
				ClampLocalAngleToAllowedRange(ref curAngle);
				float angleDiff = targetAngle - curAngle;
				// If the difference is smaller than the rotation rate per tick...
				if(Mathf.Abs(angleDiff) < headRotationRate)
				{
					// ... then immediately set the angle to the target to avoid overshooting.
					curAngle = targetAngle;
				} else
				{
					// Otherwise move the head as normal. Mathf.Sign() indicates direction of movement
					curAngle += Mathf.Sign(targetAngle - curAngle) * headRotationRate;
				}
				LocalAngleToGlobalAngle(pawn.Rotation, ref curAngle);
				_curAngle = curAngle;
			}
			headFacing = Rot4.FromAngleFlat(_curAngle);

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

		public object Clone()
		{
			return MemberwiseClone();
		}

		public Rot4 GetRotationForPortrait()
		{
			return Rot4.South;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref _target, "target");
			Scribe_Values.Look(ref _curTargetType, "targetType");
			Scribe_Values.Look(ref _targetStartTick, "targetAcquiredTick");
			Scribe_Values.Look(ref _curAngle, "currentAngle");
		}
		
		#endregion

		#region Private Methods

		private bool UpdateTargetMode(Pawn pawn, PawnState pawnState, ref float targetAngle)
		{
			// If pawn is aiming at something, set aim target mode.
			if(pawn.stances.curStance is Stance_Busy stance && !stance.neverAimWeapon && stance.focusTarg.HasThing)
			{
				SetTarget(stance.focusTarg.Thing, IHeadBehavior.TargetType.Aim);
			}
			// If pawn is no longer aiming, reset the target mode.
			else if(_curTargetType == IHeadBehavior.TargetType.Aim)
			{
				_curTargetType = IHeadBehavior.TargetType.None;
			}

			// Do not rotate head when sleeping.
			if(pawnState.sleeping)
			{
				_curTargetType = IHeadBehavior.TargetType.None;
			}
			if(_target != null && _target.Destroyed)
			{
				_curTargetType = IHeadBehavior.TargetType.None;
			}
			switch(_curTargetType)
			{
				case IHeadBehavior.TargetType.None:
					_target = null;
					// Immediately track the body rotation if there is no target when body direction changes.
					_curAngle = pawn.Rotation.AsAngle;
					return false;

				case IHeadBehavior.TargetType.SocialInitiator:
					// Wait for kSocialInteractionRecipientDelayTick before moving head
					if(Find.TickManager.TicksGame - _targetStartTick < socialRecipientDelayTick)
					{
						return false;
					}
					goto case IHeadBehavior.TargetType.SocialRecipient;

				case IHeadBehavior.TargetType.SocialRecipient:
					// End social interaction if enough time has passed
					// or if pawn is unable to see the target
					if(Find.TickManager.TicksGame - _targetStartTick > socialDurationTick ||
						!pawn.CanSee(_target))
					{
						goto case IHeadBehavior.TargetType.None;
					}
					targetAngle = UpdateTargetAngle(pawn);
					return true;

				case IHeadBehavior.TargetType.Aim:
					targetAngle = UpdateTargetAngle(pawn);
					return true;
			}
			// Shouldn't reach here
			return false;
		}

		private float UpdateTargetAngle(Pawn pawn)
		{
			if(_target != null)
			{
				return (_target.Position - pawn.Position).AngleFlat;
			}
			else
			{
				Log.Warning("Facial Stuff: tried to update head angle when there is no target to turn the head towards");
				return 0f;
			}
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
