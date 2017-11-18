// ReSharper disable All

namespace FacialStuff.FaceEditor.ColorPicker
{
    using UnityEngine;

    /// <summary>
    ///     This class exists only to have a reference headType for Color.
    /// </summary>
    public class ColorWrapper
    {
        public ColorWrapper(Color color)
        {
            this.Color = color;
        }

        public Color Color { get; set; }
    }
}