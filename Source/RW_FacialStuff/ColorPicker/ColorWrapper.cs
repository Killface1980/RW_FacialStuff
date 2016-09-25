using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FaceStyling
{
    /// <summary>
    /// This class exists only to have a reference type for Color.
    /// </summary>
    public class ColorWrapper
    {
        public Color Color { get; set; }

        public ColorWrapper( Color color )
        {
            Color = color;
        }
    }
}
