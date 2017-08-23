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

namespace FacialStuff.FaceStyling_Bench.UI.DTO.SelectionWidgetDTOs
{
    using System;
    using System.Collections.Generic;

    using RimWorld;

    using Verse;

    public class BodyTypeSelectionDTO : ASelectionWidgetDTO
    {
        public readonly BodyType OriginalBodyType;

        private List<BodyType> bodyTypes;
        private List<BodyType> maleBodyTypes;
        private List<BodyType> femaleBodyTypes;

        public BodyTypeSelectionDTO(BodyType bodyType, Gender gender) : base()
        {
            this.OriginalBodyType = bodyType;

            Array a = Enum.GetValues(typeof(BodyType));
            this.bodyTypes = new List<BodyType>(a.Length);
            this.maleBodyTypes = new List<BodyType>(a.Length - 1);
            this.femaleBodyTypes = new List<BodyType>(a.Length - 1);
            foreach (BodyType bt in a)
            {
                if (bt != BodyType.Undefined)
                {
                    if (bt != BodyType.Female)
                        this.maleBodyTypes.Add(bt);
                    if (bt != BodyType.Male)
                        this.femaleBodyTypes.Add(bt);
                }
            }

            this.bodyTypes = (gender == Gender.Male) ? this.maleBodyTypes : this.femaleBodyTypes;
            this.FindIndex(bodyType);
        }

        private void FindIndex(BodyType bodyType)
        {
            for (int i = 0; i < this.bodyTypes.Count; ++i)
            {
                if (this.bodyTypes[i] == bodyType)
                {
                    base.index = i;
                    break;
                }
            }
        }

        public Gender Gender
        {
            set
            {
                BodyType bodyType = (BodyType)this.SelectedItem;
                if (value == Gender.Female)
                {
                    this.bodyTypes = this.femaleBodyTypes;
                    if (bodyType == BodyType.Male)
                    {
                        bodyType = BodyType.Female;
                    }
                }
                else // Male
                {
                    this.bodyTypes = this.maleBodyTypes;
                    if (bodyType == BodyType.Female)
                    {
                        bodyType = BodyType.Male;
                    }
                }

                this.FindIndex(bodyType);
                base.IndexChanged();
            }
        }

        public override int Count
        {
            get
            {
                return this.bodyTypes.Count;
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
                return this.bodyTypes[base.index].ToString();
            }
        }

        public override object SelectedItem
        {
            get
            {
                return this.bodyTypes[base.index];
            }
        }

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalBodyType);
            base.IndexChanged();
        }
    }
}