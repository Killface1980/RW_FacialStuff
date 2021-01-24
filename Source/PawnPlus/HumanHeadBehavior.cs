using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PawnPlus
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
		private Quaternion _curQuat;
		
		#endregion
		
		public void Initialize(Pawn pawn)
		{
			_curQuat = Quaternion.Euler(0f, pawn.Rotation.AsAngle, 0f);
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
			if(!pawnState.Alive)
			{
				headFacing = pawn.Rotation;
				_curTargetType = IHeadBehavior.TargetType.None;
				return;
			}
			if(!pawn.Rotation.IsValid)
			{
				Log.Warning(
					"Pawn Plus: invalid body rotation given for PawnHeadRotationAI.Tick() (value: " 
					+ pawn.Rotation.AsInt + ") Pawn:" + pawn?.ToString());
				headFacing = pawn.Rotation;
				return;
			}
			if(pawn == null)
			{
				Log.Warning("Pawn Plus: tried to update head rotation when pawn is null");
				headFacing = Rot4.North;
				return;
			}
			Quaternion targetQuat = Quaternion.identity;
			if(UpdateTargetMode(pawn, pawnState, ref targetQuat) && Mathf.Abs(Quaternion.Angle(targetQuat, _curQuat)) > 0.1f)
			{
				float angle = Quaternion.Angle(targetQuat, _curQuat);
				float lerpT = Mathf.Clamp(headRotationRate / angle, 0f, 1f);
				_curQuat = Quaternion.Lerp(_curQuat, targetQuat, lerpT);
			}
			// Make sure that the head can't look back while changing body direction
			float angleDiff = Mathf.Abs(Quaternion.Angle(Quaternion.Euler(0f, pawn.Rotation.AsAngle, 0f), _curQuat));
			if(angleDiff > 90f)
			{
				Quaternion leftQuat = Quaternion.Euler(0f, pawn.Rotation.AsAngle - 90f, 0f);
				Quaternion rightQuat = Quaternion.Euler(0f, pawn.Rotation.AsAngle + 90f, 0f);
				float leftAngle = Mathf.Abs(Quaternion.Angle(leftQuat, _curQuat));
				float rightAngle = Mathf.Abs(Quaternion.Angle(rightQuat, _curQuat));
				if(leftAngle < rightAngle)
				{
					_curQuat = leftQuat;
				} else
				{
					_curQuat = rightQuat;
				}
			}
			headFacing = Rot4.FromAngleFlat(_curQuat.eulerAngles.y);
			
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
			Scribe_Values.Look<Quaternion>(ref _curQuat, "currentAngle");
		}
		
		#endregion

		#region Private Methods

		private bool UpdateTargetMode(Pawn pawn, PawnState pawnState, ref Quaternion targetQuat)
		{
			if(pawnState.Aiming)
			{
				SetTarget(pawnState.Aiming_Target, IHeadBehavior.TargetType.Aim);
			}
			// If pawn is no longer aiming, reset the target mode.
			else if(_curTargetType == IHeadBehavior.TargetType.Aim)
			{
				_curTargetType = IHeadBehavior.TargetType.None;
			}

			// Do not rotate head when laying down.
			if(!pawnState.Standing)
			{
				_curTargetType = IHeadBehavior.TargetType.None;
				_curQuat = FromRot4(pawn.Drawer.renderer.LayingFacing());
				return false;
			}
			if(_target != null && _target.Destroyed)
			{
				_curTargetType = IHeadBehavior.TargetType.None;
			}
			switch(_curTargetType)
			{
				case IHeadBehavior.TargetType.None:
					_target = null;
					targetQuat = Quaternion.Euler(0f, pawn.Rotation.AsAngle, 0f);
					return true;

				case IHeadBehavior.TargetType.SocialInitiator:
					// Wait for kSocialInteractionRecipientDelayTick before moving head
					if((Find.TickManager.TicksGame - _targetStartTick) < socialRecipientDelayTick)
					{
						targetQuat = Quaternion.Euler(0f, pawn.Rotation.AsAngle, 0f);
						return true;
					}
					goto case IHeadBehavior.TargetType.SocialRecipient;

				case IHeadBehavior.TargetType.SocialRecipient:
					// End social interaction if enough time has passed
					// or if pawn is unable to see the target
					if((Find.TickManager.TicksGame - _targetStartTick) > socialDurationTick || 
						!pawn.CanSee(_target))
					{
						_curTargetType = IHeadBehavior.TargetType.None;
						goto case IHeadBehavior.TargetType.None;
					}
					targetQuat = UpdateTargetAngle(pawn);
					return true;

				case IHeadBehavior.TargetType.Aim:
					targetQuat = UpdateTargetAngle(pawn);
					return true;
			}
			// Shouldn't reach here
			return false;
		}

		private Quaternion UpdateTargetAngle(Pawn pawn)
		{
			if(_target != null)
			{
				// Non-zero y component will mess up the angle calculation.
				Vector3 positionDir = new Vector3(
					_target.DrawPos.x - pawn.DrawPos.x,
					0,
					_target.DrawPos.z - pawn.DrawPos.z);
				return Quaternion.LookRotation(Vector3.Normalize(positionDir), Vector3.up);
			}
			else
			{
				Log.Warning("Pawn Plus: tried to update head angle when there is no target to turn the head towards");
				return Quaternion.identity;
			}
		}

		private Quaternion FromRot4(Rot4 rot)
		{
			switch(rot.AsInt)
			{
				case 0: return Quaternion.AngleAxis(0f, new Vector3(0f, 1f, 0f));
				case 1: return Quaternion.AngleAxis(90f, new Vector3(0f, 1f, 0f));
				case 2: return Quaternion.AngleAxis(180f, new Vector3(0f, 1f, 0f));
				case 3: return Quaternion.AngleAxis(270f, new Vector3(0f, 1f, 0f));
				default: return Quaternion.AngleAxis(0f, new Vector3(0f, 1f, 0f));
			}
		}
				
		#endregion
	}
}
