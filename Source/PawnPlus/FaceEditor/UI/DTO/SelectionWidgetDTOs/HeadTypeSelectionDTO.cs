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
using System.IO;
using System.Linq;
using Verse;

namespace FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs
{
    public class HeadTypeSelectionDto : ASelectionWidgetDto
    {
        public readonly CrownType OriginalCrownType;

        public readonly string OriginalHeadType;

        private readonly List<string> _femaleHeadTypes = new List<string>();

        private List<string> _headTypes;

        private readonly List<string> _maleHeadTypes = new List<string>();

        private int _savedFemaleIndex;

        private int _savedMaleIndex;

        private object _selectedItem2;

        public HeadTypeSelectionDto(string headType, Gender gender)
        {
            this.OriginalHeadType = headType;

            if (this.OriginalHeadType.Contains("Narrow"))
            {
                this.OriginalCrownType = CrownType.Narrow;
            }
            else
            {
                this.OriginalCrownType = CrownType.Average;
            }

            AddHeadTypesToList(StringsFS.PathHumanlike +"Heads/Male", this._maleHeadTypes);
            AddHeadTypesToList(StringsFS.PathHumanlike + "Heads/Female", this._femaleHeadTypes);

            this.Gender = gender;
            this.FindIndex(headType);
        }

        public override int Count => this._headTypes.Count;

        public Gender Gender
        {
            set
            {
                if (value == Gender.Female)
                {
                    this._savedMaleIndex = this.Index;
                    this._headTypes = this._femaleHeadTypes;
                    this.Index = this._savedFemaleIndex;
                }
                else
                {
                    // Male
                    this._savedFemaleIndex = this.Index;
                    this._headTypes = this._maleHeadTypes;
                    this.Index = this._savedMaleIndex;
                }

                this.IndexChanged();
            }
        }

        public override object SelectedItem
        {
            get
            {
                this._selectedItem2 = this._headTypes[this.Index].Contains("Narrow")
                                         ? CrownType.Narrow
                                         : CrownType.Average;

                return this._headTypes[this.Index];
            }
        }

        public override object SelectedItem2 => this._selectedItem2;

        public override string SelectedItemLabel
        {
            get
            {
                string[] array = this._headTypes[this.Index].Split(new[] { '_' }, StringSplitOptions.None);
                return array[array.Count() - 2] + ", " + array[array.Count() - 1];
            }
        }

        public void FindIndex(string headType)
        {
            for (int i = 0; i < this._headTypes.Count; ++i)
            {
                if (!this._headTypes[i].Equals(headType))
                {
                    continue;
                }

                this.Index = i;
                break;
            }
        }

        public override void ResetToDefault()
        {
            this.FindIndex(this.OriginalHeadType);
            this.IndexChanged();
        }

        private static void AddHeadTypesToList(string source, List<string> list)
        {
            foreach (string current in GraphicDatabaseUtility.GraphicNamesInFolder(source))
            {
                string item = source + Path.DirectorySeparatorChar + current;
                list.Add(item);
            }
        }
    }
}