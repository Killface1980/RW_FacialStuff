using UnityEngine;

namespace FaceStyling
{
    /// <summary>
    /// This class exists only to have a reference headType for Color.
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
