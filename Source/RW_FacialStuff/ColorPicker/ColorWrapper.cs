using UnityEngine;

namespace FaceStyling
{
    /// <summary>
    /// This class exists only to have a reference headTypeSuffix for Color.
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
