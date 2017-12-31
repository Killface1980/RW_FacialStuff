/*
 * MIT License
 *
 * Copyright (c) [2017] [Travis Offtermatt]
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs
{
    using System.Collections.Generic;

    using Verse;

    public class GenderSelectionDTO : ASelectionWidgetDTO
    {
        public readonly Gender OriginalGender;

        private readonly List<Gender> genders = new List<Gender>(2);

        public GenderSelectionDTO(Gender currentGender)
        {
            this.OriginalGender = currentGender;
            this.genders.Add(Gender.Male);
            this.genders.Add(Gender.Female);
            this.Index = currentGender == Gender.Male ? 0 : 1;
        }

        public override int Count
        {
            get
            {
                return this.genders.Count;
            }
        }

        public override object SelectedItem
        {
            get
            {
                return this.genders[this.Index];
            }
        }

        public override object SelectedItem2
        {
            get
            {
                return null;
            }
        }

        public override string SelectedItemLabel
        {
            get
            {
                return this.genders[this.Index].ToString().Translate().CapitalizeFirst();
            }
        }

        public override void ResetToDefault()
        {
            if (this.OriginalGender == this.genders[this.Index])
            {
                return;
            }

            this.Index = this.OriginalGender == Gender.Male ? 0 : 1;
            this.IndexChanged();
        }
    }
}