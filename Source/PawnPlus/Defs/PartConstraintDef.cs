using PawnPlus.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PawnPlus.Defs
{
	public class PartConstraintDef : Def
	{
		public BodyDef raceBodyDef;
		public List<PartClass> partClasses = new List<PartClass>();
	}
}
