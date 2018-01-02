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

// Blatantly stolen from "Change Dresser"

using FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs;
using FacialStuff.FaceEditor.UI.Util;
using Verse;

namespace FacialStuff.FaceEditor.UI.DTO
{
    public class DresserDTO
    {
        private readonly Pawn _pawn;

        private readonly long _originalAgeBioTicks;

        private readonly long _originalAgeChronTicks;

        public DresserDTO(Pawn pawn)
        {
            this._pawn = pawn;
            this._originalAgeBioTicks = pawn.ageTracker.AgeBiologicalTicks;
            this._originalAgeChronTicks = pawn.ageTracker.AgeChronologicalTicks;

            this.BodyTypeSelectionDto = new BodyTypeSelectionDto(this._pawn.story.bodyType, this._pawn.gender);
            this.HeadTypeSelectionDto = new HeadTypeSelectionDto(this._pawn.story.HeadGraphicPath, this._pawn.gender);

            this.GenderSelectionDto = new GenderSelectionDto(this._pawn.gender);
            this.GenderSelectionDto.SelectionChangeListener += delegate
                {
                    this.BodyTypeSelectionDto.Gender = (Gender) this.GenderSelectionDto.SelectedItem;
                    this.HeadTypeSelectionDto.Gender = (Gender) this.GenderSelectionDto.SelectedItem;
                };

            this.SkinColorSliderDto = new SliderWidgetDto(this._pawn.story.melanin, 0, 1);
        }

        public BodyTypeSelectionDto BodyTypeSelectionDto { get; }

        public GenderSelectionDto GenderSelectionDto { get; }

        public HeadTypeSelectionDto HeadTypeSelectionDto { get; }

        public SliderWidgetDto SkinColorSliderDto { get; }

        public void ResetToDefault()
        {
            // Gender must happen first
            this.GenderSelectionDto.ResetToDefault();
            this.BodyTypeSelectionDto.ResetToDefault();
            this.SkinColorSliderDto.ResetToDefault();
            this.HeadTypeSelectionDto.ResetToDefault();
            this._pawn.ageTracker.AgeBiologicalTicks = this._originalAgeBioTicks;
            this._pawn.ageTracker.AgeChronologicalTicks = this._originalAgeChronTicks;
        }

        public void SetUpdatePawnListeners(UpdatePawnListener updatePawn)
        {
            this.BodyTypeSelectionDto.UpdatePawnListener += updatePawn;
            this.GenderSelectionDto.UpdatePawnListener += updatePawn;
            this.SkinColorSliderDto.UpdatePawnListener += updatePawn;
            this.HeadTypeSelectionDto.UpdatePawnListener += updatePawn;
        }
    }
}