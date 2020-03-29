using System;
using System.IO;
using System.Security.Policy;
using UnityEngine;
using Verse;

namespace FacialStuff.GraphicsFS
{
    public class Graphic_Multi_NaturalEars : Graphic_Multi_NaturalEyes
    {
        public override string GetPartType()
        {
            return "Ear_";
        }
    }
}