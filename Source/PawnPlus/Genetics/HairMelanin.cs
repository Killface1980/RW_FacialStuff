using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawnPlus.Genetics
{
	class HairMelanin
	{
		private float _euMelanin;
		private float _pheoMelanin;

		public float EuMelanin => _euMelanin;
		public float PheoMelanin => _pheoMelanin;

		public HairMelanin(float euMelanin, float pheoMelanin)
		{
			_euMelanin = euMelanin;
			_pheoMelanin = pheoMelanin;
		}
	}
}
