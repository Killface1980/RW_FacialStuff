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

    using Verse;

    public class HeadTypeSelectionDTO : ASelectionWidgetDTO
    {
        public readonly CrownType OriginalCrownType;

        public readonly string OriginalHeadType;

        private readonly List<string> femaleHeadTypes = new List<string>();

        private List<string> headTypes;

        private readonly List<string> maleHeadTypes = new List<string>();

        private int savedFemaleIndex;

        private int savedMaleIndex;

        private object selectedItem2;

        public HeadTypeSelectionDTO(string headType, Gender gender)
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

            AddHeadTypesToList("Things/Pawn/Humanlike/Heads/Male", this.maleHeadTypes);
            AddHeadTypesToList("Things/Pawn/Humanlike/Heads/Female", this.femaleHeadTypes);

            this.Gender = gender;
            this.FindIndex(headType);
        }

        public override int Count
        {
            get
            {
                return this.headTypes.Count;
            }
        }

        public Gender Gender
        {
            set
            {
                if (value == Gender.Female)
                {
                    this.savedMaleIndex = this.index;
                    this.headTypes = this.femaleHeadTypes;
                    this.index = this.savedFemaleIndex;
                }
                else
                {
                    // Male
                    this.savedFemaleIndex = this.index;
                    this.headTypes = this.maleHeadTypes;
                    this.index = this.savedMaleIndex;
                }

                this.IndexChanged();
            }
        }

        public override object SelectedItem
        {
            get
            {
                this.selectedItem2 = this.headTypes[this.index].Contains("Narrow")
                                         ? CrownType.Narrow
                                         : CrownType.Average;

                return this.headTypes[this.index];
            }
        }

        public override object SelectedItem2
        {
            get
            {
                return this.selectedItem2;
            }
        }

        public override string SelectedItemLabel
        {
            get
            {
                string[] array = this.headTypes[this.index].Split(new[] { '_' }, StringSplitOptions.None);
                return array[array.Count() - 2] + ", " + array[array.Count() - 1];
            }
        }

        public void FindIndex(string headType)
        {
            for (int i = 0; i < this.headTypes.Count; ++i)
            {
                if (!this.headTypes[i].Equals(headType))
                {
                    continue;
                }

                this.index = i;
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
                string item = source + "/" + current;
                list.Add(item);
            }
        }
    }
}