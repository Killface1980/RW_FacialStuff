using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace FacialStuff.Harmony
{
    [HarmonyPatch(
    typeof(Pawn_InteractionsTracker),
    "TryInteractWith")]
    class HarmonyPatch_Pawn_InteractionsTracker
	{
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
            /*
            // Before running the transpiler:
            ..
            	    }
		            Find.LetterStack.ReceiveLetter(letterLabel, text, letterDef, lookTargets ?? ((LookTargets)pawn));
	            }
                return true;
            }
            // After running the transpiler
            ..
            	    }
		            Find.LetterStack.ReceiveLetter(letterLabel, text, letterDef, lookTargets ?? ((LookTargets)pawn));
	            }
                HarmonyPatch_Pawn_InteractionsTracker.OnSuccessfulInteraction(this.pawn, recipient);
	            return true;
            }
            */
            List<CodeInstruction> instList = instructions.ToList();
            List<CodeInstruction> newInst = new List<CodeInstruction>();
            FieldInfo pawnFieldInfo = typeof(Pawn_InteractionsTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
            if(pawnFieldInfo == null)
			{
                Log.Warning(
                    "Facial Stuff: could not patch Pawn_InterationsTracker.TryInteractWith() method. Pawn_InteractionsTracker.pawn " +
                    "field doesn't exist.");
                return instList;
            }
            newInst.Add(new CodeInstruction(OpCodes.Ldarg_0));
            newInst.Add(new CodeInstruction(OpCodes.Ldfld, pawnFieldInfo));
            newInst.Add(new CodeInstruction(OpCodes.Ldarg_0));
            newInst.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_Pawn_InteractionsTracker), nameof(OnSuccessfulInteraction))));
            int transpilerState = 0;
            int insertIdx = 0;
            for(int i = instList.Count - 1; i >= 0; --i)
			{
                if(instList[i].opcode == OpCodes.Ret && transpilerState == 0)
				{
                    transpilerState = 1;
				}
                else if(instList[i].opcode == OpCodes.Ldc_I4_1 && transpilerState == 1)
				{
                    insertIdx = i;
                    transpilerState = 2;
				}
			}
            if(transpilerState != 2)
			{
                Log.Warning("Facial Stuff: could not patch Pawn_InterationsTracker.TryInteractWith() method. Could not find instructions to patch.");
                return instList;
			}
            instList.InsertRange(insertIdx, newInst);
            return instList;
        }

        public static void OnSuccessfulInteraction(Pawn initiator, Pawn recipient)
		{
            if(initiator == null || recipient == null)
            {
                return;
            }
            if(initiator.GetCompFace(out CompFace compFace))
            {
                if(compFace.Props.canRotateHead)
                {
                    compFace.HeadBehavior.SetTarget(recipient, IHeadBehavior.TargetType.SocialRecipient);
                }
            }
            if(recipient.GetCompFace(out CompFace recipientFace))
            {
                if(recipientFace.Props.canRotateHead)
                {
                    recipientFace.HeadBehavior.SetTarget(initiator, IHeadBehavior.TargetType.SocialInitiator);
                }
            }
        }
    }
}
