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

using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs
{
    public class BodyTypeSelectionDto : ASelectionWidgetDto
    {
        #region Public Fields

        public readonly BodyTypeDef OriginalBodyType;

        #endregion Public Fields

        #region Private Fields

        private readonly List<BodyTypeDef> _femaleBodyTypes;
        private readonly List<BodyTypeDef> _maleBodyTypes;
        private List<BodyTypeDef> _bodyTypes;

        #endregion Private Fields

        #region Public Constructors

        public BodyTypeSelectionDto(BodyTypeDef bodyType, Gender gender)
        {
            this.OriginalBodyType = bodyType;

            Array a = Enum.GetValues(typeof(BodyTypeDef));
            this._bodyTypes = new List<BodyTypeDef>(a.Length);
            this._maleBodyTypes = new List<BodyTypeDef>(a.Length - 1);
            this._femaleBodyTypes = new List<BodyTypeDef>(a.Length - 1);
            foreach (BodyTypeDef bt in a)
            {
                if (bt != BodyTypeDefOf.Female)
                {
                    this._maleBodyTypes.Add(bt);
                }

                if (bt != BodyTypeDefOf.Male)
                {
                    this._femaleBodyTypes.Add(bt);
                }
            }

            this._bodyTypes = gender == Gender.Male ? this._maleBodyTypes : this._femaleBodyTypes;
            this.FindIndex(bodyType);
        }

        #endregion Public Constructors

        #region Public Properties

        public override int Count => this._bodyTypes.Count;

        public Gender Gender
        {
            set
            {
                BodyTypeDef bodyType = (BodyTypeDef) this.SelectedItem;
                if (value == Gender.Female)
                {
                    this._bodyTypes = this._femaleBodyTypes;
                    if (bodyType == BodyTypeDefOf.Male)
                    {
                        bodyType = BodyTypeDefOf.Female;
                    }
                }
                else
                {
                    // Male
                    this._bodyTypes = this._maleBodyTypes;
                    if (bodyType == BodyTypeDefOf.Female)
                    {
                        bodyType = BodyTypeDefOf.Male;
                    }
                }

                this.FindIndex(bodyType);
                this.IndexChanged();
            }
        }

        public override object SelectedItem => this._bodyTypes[this.Index];

        public override object SelectedItem2 => null;

        public override string SelectedItemLabel => this._bodyTypes[this.Index].ToString();

        #endregion Public Properties

        #region Public Methods

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalBodyType);
            this.IndexChanged();
        }

        #endregion Public Methods

        #region Private Methods

        private void FindIndex(BodyTypeDef bodyType)
        {
            for (int i = 0; i < this._bodyTypes.Count; ++i)
            {
                if (this._bodyTypes[i] != bodyType)
                {
                    continue;
                }

                this.Index = i;
                break;
            }
        }

        #endregion Private Methods
    }
}