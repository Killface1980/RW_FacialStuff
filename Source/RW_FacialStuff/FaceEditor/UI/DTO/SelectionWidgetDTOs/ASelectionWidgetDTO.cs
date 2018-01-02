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

using System.Diagnostics.CodeAnalysis;
using FacialStuff.FaceEditor.UI.Util;

namespace FacialStuff.FaceEditor.UI.DTO.SelectionWidgetDTOs
{
    [SuppressMessage("ReSharper", "StyleCop.SA1215")]
    [SuppressMessage("ReSharper", "StyleCop.SA1401")]
    public abstract class ASelectionWidgetDto
    {
        #region Protected Fields

        protected int Index;

        #endregion Protected Fields

        #region Public Events

        public event SelectionChangeListener SelectionChangeListener;

        public event UpdatePawnListener UpdatePawnListener;

        #endregion Public Events

        #region Public Properties

        public abstract int Count { get; }

        public abstract object SelectedItem { get; }

        public abstract object SelectedItem2 { get; }

        public abstract string SelectedItemLabel { get; }

        #endregion Public Properties

        #region Public Methods

        public void DecreaseIndex()
        {
            --this.Index;
            if (this.Index < 0)
            {
                this.Index = this.Count - 1;
            }

            this.IndexChanged();
        }

        public void IncreaseIndex()
        {
            ++this.Index;
            if (this.Index >= this.Count)
            {
                this.Index = 0;
            }

            this.IndexChanged();
        }

        public abstract void ResetToDefault();

        #endregion Public Methods

        #region Protected Methods

        protected void IndexChanged()
        {
            this.SelectionChangeListener?.Invoke(this);
            this.UpdatePawn(this.SelectedItem, this.SelectedItem2);
        }

        protected void UpdatePawn(object item, object item2 = null)
        {
            this.UpdatePawnListener?.Invoke(this, item, item2);
        }

        #endregion Protected Methods
    }
}