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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RimWorld;

    using Verse;

    public class BodyTypeSelectionDTO : ASelectionWidgetDTO
    {
        #region Public Fields

        public readonly BodyType OriginalBodyType;

        #endregion Public Fields

        #region Private Fields

        private readonly List<BodyType> femaleBodyTypes;
        private readonly List<BodyType> maleBodyTypes;
        private List<BodyType> bodyTypes;

        #endregion Private Fields

        #region Public Constructors

        public BodyTypeSelectionDTO(BodyType bodyType, Gender gender)
        {
            this.OriginalBodyType = bodyType;

            Array a = Enum.GetValues(typeof(BodyType));
            this.bodyTypes = new List<BodyType>(a.Length);
            this.maleBodyTypes = new List<BodyType>(a.Length - 1);
            this.femaleBodyTypes = new List<BodyType>(a.Length - 1);
            foreach (BodyType bt in a.Cast<BodyType>().Where(bt => bt != BodyType.Undefined))
            {
                if (bt != BodyType.Female)
                {
                    this.maleBodyTypes.Add(bt);
                }

                if (bt != BodyType.Male)
                {
                    this.femaleBodyTypes.Add(bt);
                }
            }

            this.bodyTypes = gender == Gender.Male ? this.maleBodyTypes : this.femaleBodyTypes;
            this.FindIndex(bodyType);
        }

        #endregion Public Constructors

        #region Public Properties

        public override int Count
        {
            get
            {
                return this.bodyTypes.Count;
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
                else
                {
                    // Male
                    this.bodyTypes = this.maleBodyTypes;
                    if (bodyType == BodyType.Female)
                    {
                        bodyType = BodyType.Male;
                    }
                }

                this.FindIndex(bodyType);
                this.IndexChanged();
            }
        }

        public override object SelectedItem
        {
            get
            {
                return this.bodyTypes[this.Index];
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
                return this.bodyTypes[this.Index].ToString();
            }
        }

        #endregion Public Properties

        #region Public Methods

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalBodyType);
            this.IndexChanged();
        }

        #endregion Public Methods

        #region Private Methods

        private void FindIndex(BodyType bodyType)
        {
            for (int i = 0; i < this.bodyTypes.Count; ++i)
            {
                if (this.bodyTypes[i] != bodyType)
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