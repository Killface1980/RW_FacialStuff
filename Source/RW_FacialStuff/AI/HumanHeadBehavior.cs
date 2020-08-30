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

		private Rot4 _curRot;
		private Rot4 _prevBodyRot;
		private Thing _target;
		private IHeadBehavior.TargetType _curTargetMode = IHeadBehavior.TargetType.None;
		// How many ticks have passed since target was set
		private int _curTargetTicks;
		// Facing north is 0 degrees. Unit in degrees. Clockwise is positive.
		private float _curAngle;
		// Facing north is 0 degrees. unit in degrees. Clockwise is positive.
		private float _targetAngle;
		
		#endregion
		
		public void Initialize(Pawn pawn)
		{
			_curRot = pawn.Rotation;
			_curAngle = pawn.Rotation.AsAngle;
			_prevBodyRot = pawn.Rotation;
		}

		#region Public Methods

		public void SetTarget(Thing target, IHeadBehavior.TargetType mode)
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
		
		public void Update(Pawn pawn, PawnState pawnState, out Rot4 headFacing)
		{
			++_curTargetTicks;
			if(!pawnState.alive)
			{
				headFacing = pawn.Rotation;
				_curTargetMode = IHeadBehavior.TargetType.None;
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
			if(UpdateTargetMode(pawn, pawnState) && Mathf.Abs(_curAngle - _targetAngle) > 0.1f)
			{
				float targetAngle = _targetAngle;
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
				_curRot = Rot4.FromAngleFlat(_curAngle);
			}
			headFacing = _curRot;
			_prevBodyRot = pawn.Rotation;

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

		#endregion

		#region Private Methods

		private bool UpdateTargetMode(Pawn pawn, PawnState pawnState)
		{
			// If pawn is aiming at something, set aim target mode.
			if(pawn.stances.curStance is Stance_Busy stance && !stance.neverAimWeapon && stance.focusTarg.HasThing)
			{
				SetTarget(stance.focusTarg.Thing, IHeadBehavior.TargetType.Aim);
			}
			// If pawn is no longer aiming, reset the target mode.
			else if(_curTargetMode == IHeadBehavior.TargetType.Aim)
			{
				_curTargetMode = IHeadBehavior.TargetType.None;
			}

			// Do not rotate head when sleeping.
			if(pawnState.sleeping)
			{
				_curTargetMode = IHeadBehavior.TargetType.None;
			}
			if(_target != null && _target.Destroyed)
			{
				_curTargetMode = IHeadBehavior.TargetType.None;
			}
			switch(_curTargetMode)
			{
				case IHeadBehavior.TargetType.None:
					ResetHeadTarget(pawn.Rotation);
					// Immediately track the body rotation if there is no target when body direction changes.
					if(pawn.Rotation != _prevBodyRot)
					{
						_curAngle = _targetAngle;
						_curRot = pawn.Rotation;
						// Angle is same as target angle. There is no need to move head.
						return false;
					}
					return true;

				case IHeadBehavior.TargetType.SocialInitiator:
					// Wait for kSocialInteractionRecipientDelayTick before moving head
					if(_curTargetTicks < socialRecipientDelayTick)
					{
						return false;
					}
					goto case IHeadBehavior.TargetType.SocialRecipient;

				case IHeadBehavior.TargetType.SocialRecipient:
					// End social interaction if enough time has passed
					// or if pawn is unable to see the target
					if(_curTargetTicks > socialDurationTick ||
						!pawn.CanSee(_target))
					{
						ResetHeadTarget(pawn.Rotation);
						return true;
					}

					UpdateTargetAngle(pawn);
					return true;

				case IHeadBehavior.TargetType.Aim:
					UpdateTargetAngle(pawn);
					return true;
			}
			return true;
		}

		private void UpdateTargetAngle(Pawn pawn)
		{
			if(_target != null)
			{
				_targetAngle = (_target.Position - pawn.Position).AngleFlat;
			}
			else
			{
				Log.Warning("Facial Stuff: tried to update head angle when there is no target to turn the head towards");
			}
		}

		private void ResetHeadTarget(Rot4 bodyRot)
		{
			_curTargetMode = IHeadBehavior.TargetType.None;
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
