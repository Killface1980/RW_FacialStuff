using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnPlus.Genetics
{
	public class HairMelanin : IExposable
    {
		private float _euMelanin;
		private float _pheoMelanin;
        // TODO move this to rendering code instead
        private float _greyness;
        private Color _hairColor;
        private Color _beardColor;
        private bool _hasSameBeardColor;
        // TODO move this to rendering code instead
        private float _wrinkleIntensity;

        public float EuMelanin => _euMelanin;

		public float PheoMelanin => _pheoMelanin;

        public Color HairColor => _hairColor;

        public Color BeardColor => _beardColor;
        
		public void GenerateHairDNA(Pawn pawn, bool ignoreRelative = false, bool newPawn = true)
        {
            _hasSameBeardColor = Rand.Value > 0.3f;
            _wrinkleIntensity = Mathf.InverseLerp(45f, 80f, pawn.ageTracker.AgeBiologicalYearsFloat) - pawn.story.melanin / 2;
            HairDNA hairDna =
                HairMelaninUtil.GenerateHairMelaninAndCuticula(pawn, _hasSameBeardColor, ignoreRelative);
            _euMelanin = hairDna.HairColorRequest.EuMelanin;
            _pheoMelanin = hairDna.HairColorRequest.PheoMelanin;
            _greyness = hairDna.HairColorRequest.Greyness;

            if(newPawn)
            {
                _hairColor = hairDna.HairColor;
                _beardColor = hairDna.BeardColor;
            } else
            {
                _hairColor = pawn.story.hairColor;
                _beardColor = HairMelaninUtil.ShuffledBeardColor(_hairColor);
            }
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }
    }
}
