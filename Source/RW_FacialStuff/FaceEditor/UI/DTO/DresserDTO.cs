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

namespace FacialStuff.FaceEditor.UI.DTO
{
    using FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs;
    using FacialStuff.FaceEditor.UI.Util;

    using Verse;

    public class DresserDTO
    {
        private readonly Pawn pawn;

        private readonly long originalAgeBioTicks;

        private readonly long originalAgeChronTicks;

        public DresserDTO(Pawn pawn)
        {
            this.pawn = pawn;
            this.originalAgeBioTicks = pawn.ageTracker.AgeBiologicalTicks;
            this.originalAgeChronTicks = pawn.ageTracker.AgeChronologicalTicks;

            this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.pawn.story.bodyType, this.pawn.gender);
            this.HeadTypeSelectionDto = new HeadTypeSelectionDTO(this.pawn.story.HeadGraphicPath, this.pawn.gender);

            this.GenderSelectionDto = new GenderSelectionDTO(this.pawn.gender);
            this.GenderSelectionDto.SelectionChangeListener += delegate
                {
                    this.BodyTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                    this.HeadTypeSelectionDto.Gender = (Gender)this.GenderSelectionDto.SelectedItem;
                };

            this.SkinColorSliderDto = new SliderWidgetDTO(this.pawn.story.melanin, 0, 1);
        }

        public BodyTypeSelectionDTO BodyTypeSelectionDto { get; }

        public GenderSelectionDTO GenderSelectionDto { get; }

        public HeadTypeSelectionDTO HeadTypeSelectionDto { get; }

        public SliderWidgetDTO SkinColorSliderDto { get; }

        public void ResetToDefault()
        {
            // Gender must happen first
            this.GenderSelectionDto.ResetToDefault();
            this.BodyTypeSelectionDto.ResetToDefault();
            this.SkinColorSliderDto.ResetToDefault();
            this.HeadTypeSelectionDto.ResetToDefault();
            this.pawn.ageTracker.AgeBiologicalTicks = this.originalAgeBioTicks;
            this.pawn.ageTracker.AgeChronologicalTicks = this.originalAgeChronTicks;
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