﻿namespace FacialStuff.ColorPicker
{
    using System;

    using FaceStyling;

    using UnityEngine;

    using Verse;

    public class Dialog_ColorPicker : Window
    {
        #region Fields

        // the color we're going to pass out if requested
        public Color Color = Color.blue;

        public Vector2 initialPosition = Vector2.zero;

        public Vector2 windowSize = Vector2.zero;

        public int numPresets = 0;

        public int pickerSize = 200;

        public int sliderWidth = 15;

        public int alphaBGBlockSize = 10;

        public int previewSize = 90;

        public int // odd multiple of alphaBGblocksize forces alternation of the background texture grid.
                            handleSize = 10;

        // used in the picker only
        public Color tempColor = Color.white;

        private Controls _activeControl = Controls.none;

        private Action _callback;

        private Texture2D _colorPickerBG;

        private Texture2D _huePickerBG;

        private Texture2D _tempPreviewBG;

        private Texture2D _previewBG;

        private string _hexOut;

        private string _hexIn;

        private float _margin = 6f;

        private float _fieldHeight = 30f;

        private float _huePosition;

        private float _alphaPosition;

        private float _unitsPerPixel;

        private float _H;

        private float _S = 1f;

        private float _V = 1f;

        private float _A = 1f;

        private Vector2 _pickerPosition = Vector2.zero;

        private bool _preview = true;

        private bool _autoApply;

        // reference headType containing the in/out parameter
        private ColorWrapper _wrapper;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Call with a ColorWrapper object containing the color to be changed, with an optional callback which is called when Apply or OK are clicked.
        /// Setting draggable = true will break sliders for now.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="callback"></param>
        public Dialog_ColorPicker(ColorWrapper color, Action callback = null, bool preview = true, bool autoApply = false)
        {
            // TODO: figure out if sliders and draggable = true can coexist.
            // using Event.current.Use() prevents further drawing of the tab and closes parent(s).
            this._wrapper = color;
            this._callback = callback;
            this._preview = preview;
            this._autoApply = autoApply;
            this.Color = this._wrapper.Color;
            this.tempColor = this._wrapper.Color;

            this.Notify_RGBUpdated();
        }

        #endregion Constructors

        #region Enums

        private enum Controls
        {
            colorPicker,
            huePicker,
            alphaPicker,
            none
        }

        #endregion Enums

        #region Properties

        public float A
        {
            get
            {
                return this._A;
            }

            set
            {
                this._A = Mathf.Clamp(value, 0f, 1f);
                this.Notify_HSVUpdated();
                this.CreateColorPickerBG();
            }
        }

        public Texture2D ColorPickerBG
        {
            get
            {
                if (this._colorPickerBG == null)
                {
                    this.CreateColorPickerBG();
                }

                return this._colorPickerBG;
            }
        }

        public float H
        {
            get
            {
                return this._H;
            }

            set
            {
                this._H = Mathf.Clamp(value, 0f, 1f);
                this.Notify_HSVUpdated();
                this.CreateColorPickerBG();
            }
        }

        public Texture2D HuePickerBG
        {
            get
            {
                if (this._huePickerBG == null)
                {
                    this.CreateHuePickerBG();
                }

                return this._huePickerBG;
            }
        }

        public Texture2D PreviewBG
        {
            get
            {
                if (this._previewBG == null)
                {
                    this._previewBG = this.CreatePreviewBG(this.Color);
                }

                return this._previewBG;
            }
        }

        public float S
        {
            get
            {
                return this._S;
            }

            set
            {
                this._S = Mathf.Clamp(value, 0f, 1f);
                this.Notify_HSVUpdated();
            }
        }

        public Texture2D TempPreviewBG
        {
            get
            {
                if (this._tempPreviewBG == null)
                {
                    this._tempPreviewBG = this.CreatePreviewBG(this.tempColor);
                }

                return this._tempPreviewBG;
            }
        }

        public float UnitsPerPixel
        {
            get
            {
                if (this._unitsPerPixel == 0.0f)
                {
                    this._unitsPerPixel = 1f / this.pickerSize;
                }

                return this._unitsPerPixel;
            }
        }

        public float V
        {
            get
            {
                return this._V;
            }

            set
            {
                this._V = Mathf.Clamp(value, 0f, 1f);
                this.Notify_HSVUpdated();
            }
        }

        public Vector2 WindowSize
        {
            get
            {
                return this.windowSize;
            }

            set
            {
                this.windowSize = value;
                this.SetWindowSize(this.windowSize);
            }
        }

        #endregion Properties

        #region Methods

        public void AlphaAction(float pos)
        {
            // only changing one value, property should work fine
            this.A = 1 - this.UnitsPerPixel * pos;
            this._alphaPosition = pos;
        }

        public void Apply()
        {
            this._wrapper.Color = this.tempColor;
            this._callback?.Invoke();
        }

        public Texture2D CreatePreviewBG(Color col)
        {
            return SolidColorMaterials.NewSolidColorTexture(col);
        }

        public override void DoWindowContents(Rect inRect)
        {
            // set up rects
            // pickers & sliders
            Rect pickerRect = new Rect(inRect.xMin, inRect.yMin, this.pickerSize, this.pickerSize);
            Rect hueRect = new Rect(pickerRect.xMax + this._margin, inRect.yMin, this.sliderWidth, this.pickerSize);

            // previews
            Rect previewRect = new Rect(hueRect.xMax + this._margin, inRect.yMin, this.previewSize, this.previewSize);
            Rect previewOldRect = new Rect(previewRect.xMax, inRect.yMin, this.previewSize, this.previewSize);

            // buttons and textfields
            Rect okRect = new Rect(hueRect.xMax + this._margin, inRect.yMax - this._fieldHeight, this.previewSize * 2, this._fieldHeight);
            Rect applyRect = new Rect(hueRect.xMax + this._margin, inRect.yMax - 2 * this._fieldHeight - this._margin, this.previewSize - this._margin / 2, this._fieldHeight);
            Rect cancelRect = new Rect(applyRect.xMax + this._margin, applyRect.yMin, this.previewSize - this._margin / 2, this._fieldHeight);
            Rect hexRect = new Rect(hueRect.xMax + this._margin, inRect.yMax - 3 * this._fieldHeight - 2 * this._margin, this.previewSize * 2, this._fieldHeight);

            // move ok/cancel buttons for the simple view with buttons
            if (!this._preview && !this._autoApply)
            {
                cancelRect = new Rect(inRect.xMin, pickerRect.yMax + this._margin, (this.pickerSize - this._margin) / 2, this._fieldHeight);
                okRect = cancelRect;
                okRect.x += (this.pickerSize + this._margin) / 2;
            }

            // draw picker foregrounds
            GUI.DrawTexture(pickerRect, this.ColorPickerBG);
            GUI.DrawTexture(hueRect, this.HuePickerBG);
            if (this._preview)
            {
                GUI.DrawTexture(previewRect, this.TempPreviewBG);
                GUI.DrawTexture(previewOldRect, this.PreviewBG);
            }

            // draw slider handles
            Rect hueHandleRect = new Rect(hueRect.xMin - 3f, hueRect.yMin + this._huePosition - this.handleSize / 2, this.sliderWidth + 6f, this.handleSize);
            Rect pickerHandleRect = new Rect(pickerRect.xMin + this._pickerPosition.x - this.handleSize / 2, pickerRect.yMin + this._pickerPosition.y - this.handleSize / 2, this.handleSize, this.handleSize);
            GUI.DrawTexture(hueHandleRect, this.TempPreviewBG);
            GUI.DrawTexture(pickerHandleRect, this.TempPreviewBG);

            // border on slider handles
            GUI.color = Color.gray;
            Widgets.DrawBox(hueHandleRect);
            Widgets.DrawBox(pickerHandleRect);
            GUI.color = Color.white;

            

            // reset active control on mouseup
            if (Input.GetMouseButtonUp(0))
            {
                this._activeControl = Controls.none;
            }

            // colorpicker interaction
            if (Mouse.IsOver(pickerRect))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    this._activeControl = Controls.colorPicker;
                }

                if (this._activeControl == Controls.colorPicker)
                {
                    Vector2 MousePosition = Event.current.mousePosition;
                    Vector2 PositionInRect = MousePosition - new Vector2(pickerRect.xMin, pickerRect.yMin);

                    this.PickerAction(PositionInRect);
                }
            }

            // hue picker interaction
            if (Mouse.IsOver(hueRect))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    this._activeControl = Controls.huePicker;
                }

                if (Event.current.type == EventType.ScrollWheel)
                {
                    this.H -= Event.current.delta.y * this.UnitsPerPixel;
                    this._huePosition = Mathf.Clamp(this._huePosition + Event.current.delta.y, 0f, this.pickerSize);
                    Event.current.Use();
                }

                if (this._activeControl == Controls.huePicker)
                {
                    float MousePosition = Event.current.mousePosition.y;
                    float PositionInRect = MousePosition - hueRect.yMin;

                    this.HueAction(PositionInRect);
                }
            }

            if (!this._autoApply)
            {
                // buttons and text field
                // for some reason scrolling sometimes changes text size
                Text.Font = GameFont.Small;
                if (Widgets.ButtonText(okRect, "OK"))
                {
                    this.Apply();
                    this.Close();
                }

                if (Widgets.ButtonText(applyRect, "Apply"))
                {
                    this.Apply();
                    this.SetColor();
                }

                if (Widgets.ButtonText(cancelRect, "Cancel"))
                {
                    this.Close();
                }
            }

            if (this._preview)
            {
                if (this._hexIn != this._hexOut)
                {
                    if (ColorHelper.TryHexToRGB(this._hexIn, ref this.tempColor))
                    {
                        this.Notify_RGBUpdated();
                    }
                    else
                    {
                        GUI.color = Color.red;
                    }
                }

                this._hexIn = Widgets.TextField(hexRect, this._hexIn);
            }

            GUI.color = Color.white;

            
        }

        public void HueAction(float pos)
        {
            // only changing one value, property should work fine
            this.H = 1 - this.UnitsPerPixel * pos;
            this._huePosition = pos;
        }

        public void Notify_HSVUpdated()
        {
            this.tempColor = ColorHelper.HSVtoRGB(this.H, this.S, this.V);
            this.tempColor.a = this.A;
            this._tempPreviewBG = this.CreatePreviewBG(this.tempColor);

            if (this._preview)
            {
                this._hexOut = this._hexIn = ColorHelper.RGBtoHex(this.tempColor);
            }

            if (this._autoApply)
            {
                this.Apply();
            }
        }

        public void Notify_RGBUpdated()
        {
            // Set HSV from RGB
            ColorHelper.RGBtoHSV(this.tempColor, out this._H, out this._S, out this._V);
            this._A = this.tempColor.a;

            // rebuild textures
            this.CreateColorPickerBG();
            this.CreateHuePickerBG();

            // set slider positions
            this._huePosition = (1f - this._H) / this.UnitsPerPixel;
            this._pickerPosition.x = this._S / this.UnitsPerPixel;
            this._pickerPosition.y = (1f - this._V) / this.UnitsPerPixel;
            if (this._preview)
            {
                this._alphaPosition = (1f - this._A) / this.UnitsPerPixel;
            }

            // set the color block and update hex fields
            this._tempPreviewBG = this.CreatePreviewBG(this.tempColor);
            if (this._preview)
            {
                this._hexOut = this._hexIn = ColorHelper.RGBtoHex(this.tempColor);
            }

            // call callback for auto-apply
            if (this._autoApply)
            {
                this.Apply();
            }
        }

        public void PickerAction(Vector2 pos)
        {
            // if we set S, V via properties textures will be rebuilt twice.
            this._S = this.UnitsPerPixel * pos.x;
            this._V = 1 - this.UnitsPerPixel * pos.y;

            // rebuild textures
            this.Notify_HSVUpdated();
            this._pickerPosition = pos;
        }

        public override void PostOpen()
        {
            // allow explicit setting of window position
            if (this.initialPosition != Vector2.zero)
            {
                this.windowRect.x = this.initialPosition.x;
                this.windowRect.y = this.initialPosition.y;
            }

            // set the windowsize
            if (this.windowSize == Vector2.zero)
            {
                // not specifically set in construction, calculate size from elements.
                // default window size.
                float width, height;

                // size of main picker + the standard window margins.
                width = height = this.pickerSize + StandardMargin * 2;

                // width of two sliders (hue and alpha) + margins
                width += (this.sliderWidth + this._margin) * 2;

                if (this._preview)
                {
                    // add 2 preview rects
                    width += this._margin * 2 + this.previewSize * 2;
                }
                else
                {
                    if (!this._autoApply)
                    {
                        // if this is not auto applied, we need a place to but the buttons.
                        height += this._fieldHeight + this._margin;
                    }
                }

                // that should do it
                this.SetWindowSize(new Vector2(width, height));
            }
            else
            {
                // allow explicit specification of window size
                // NOTE: elements do not actually adapt to this size.
                this.SetWindowSize(this.windowSize);
            }

            // init sliders
            this.Notify_RGBUpdated();
            this._alphaPosition = this.Color.a / this.UnitsPerPixel;
        }

        public void SetColor()
        {
            this.Color = this.tempColor;
            this._previewBG = this.CreatePreviewBG(this.tempColor);
        }

        public void SetWindowLocation(Vector2 location)
        {
            this.windowRect.xMin = location.x;
            this.windowRect.yMin = location.y;
        }

        public void SetWindowRcet(Rect rect)
        {
            this.windowRect = rect;
        }

        public void SetWindowSize(Vector2 size)
        {
            this.windowRect.width = size.x;
            this.windowRect.height = size.y;
        }


        private void CreateColorPickerBG()
        {
            if (this._colorPickerBG == null)
            {
                this._colorPickerBG = new Texture2D(this.pickerSize, this.pickerSize);
            }

            float S, V;
            int w = this.pickerSize;
            int h = this.pickerSize;
            float wu = this.UnitsPerPixel;
            float hu = this.UnitsPerPixel;

            // HSV colors, H in slider, S horizontal, V vertical.
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    S = x * wu;
                    V = y * hu;
                    this._colorPickerBG.SetPixel(x, y, ColorHelper.HSVtoRGB(this.H, S, V, this.A));
                }
            }

            this._colorPickerBG.Apply();
        }

        private void CreateHuePickerBG()
        {
            if (this._huePickerBG == null)
            {
                this._huePickerBG = new Texture2D(1, this.pickerSize);
            }

            int h = this.pickerSize;
            float hu = this.UnitsPerPixel;

            // HSV colors, S = V = 1
            for (int y = 0; y < h; y++)
            {
                this._huePickerBG.SetPixel(0, y, ColorHelper.HSVtoRGB(hu * y, 1f, 1f));
            }

            this._huePickerBG.Apply();
        }

        #endregion Methods
    }
}