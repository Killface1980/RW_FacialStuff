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

namespace FacialStuffEditor.UI.DTO.SelectionWidgetDTOs
{
    using FacialStuffEditor.UI.Util;

    abstract class ASelectionWidgetDTO
    {
        protected int index = 0;

        public event SelectionChangeListener SelectionChangeListener;
        public event UpdatePawnListener UpdatePawnListener;

        public void IncreaseIndex()
        {
            ++this.index;
            if (this.index >= this.Count)
                this.index = 0;
            this.IndexChanged();
        }

        public void DecreaseIndex()
        {
            --this.index;
            if (this.index < 0)
                this.index = this.Count - 1;
            this.IndexChanged();
        }

        public abstract int Count { get; }

        public abstract string SelectedItemLabel { get; }

        public abstract object SelectedItem { get; }

        protected void IndexChanged()
        {
            this.SelectionChangeListener?.Invoke(this);
            this.UpdatePawn(this.SelectedItem);
        }

        protected void UpdatePawn(object item)
        {
            this.UpdatePawnListener?.Invoke(this, item);
        }

        public abstract void ResetToDefault();
    }
}