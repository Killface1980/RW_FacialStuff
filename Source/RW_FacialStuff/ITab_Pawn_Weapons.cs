using System.Collections.Generic;
using System.Linq;
using FacialStuff.AnimatorWindows;
using FacialStuff.Defs;
using FacialStuff.GraphicsFS;
using RimWorld;
using UnityEngine;
using Verse;

namespace FacialStuff
{
    public class ITab_Pawn_Weapons : ITab
    {
        private readonly string[] _psiToolbarStrings = { "North", "East", "South", "West" };

        private static int _rotation = 2;
        public static Vector3 RightHandPosition;

        public static Vector3 LeftHandPosition;
        private Pawn pawn;
        private bool _rightFront = true;
        private bool _leftFront;

        public ITab_Pawn_Weapons()
        {
            this.labelKey = "TabWeapons";
            this.tutorTag = "FWeapons";
        }

        public override bool IsVisible => this.SelPawn.HasCompFace();

        protected override void FillTab()
        {
            if (this.pawn != this.SelPawn)
            {
                this.pawn = this.SelPawn;
                LeftHandPosition = RightHandPosition = WeaponOffset = AimedWeaponOffset = Vector3.zero;
            }

            if (!this.SelPawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            Rect rect = new Rect(10f, 10f, 330f, 430f);


            GUILayout.BeginArea(rect);
            GUILayout.BeginVertical();


            this.SelPawn.GetCompAnim(out CompBodyAnimator _);


            compFace.IgnoreRenderer = GUILayout.Toggle(compFace.IgnoreRenderer, "Ignore renderer");
            if (GUI.changed)
            {
                IgnoreRenderer = compFace.IgnoreRenderer;
            }

            ThingWithComps primary = this.SelPawn.equipment?.Primary;
            if (primary == null)
            {
                return;
            }

            CompWeaponExtensions extensions = primary?.GetComp<CompWeaponExtensions>();
            if (this.SelPawn.Drafted)
            {
                GUILayout.Label(this.pawn.equipment.Primary.def.defName);

                Vector3 propWeapOffset = Vector3.zero;
                Vector3 propAimOffset = Vector3.zero;
                Vector3 propLeftOffset = Vector3.zero;
                Vector3 propRightOffset = Vector3.zero;

                if (extensions != null)
                {
                    propWeapOffset = extensions.Props.WeaponPositionOffset;
                    propAimOffset = extensions.Props.AimedWeaponPositionOffset;
                    propLeftOffset = extensions.Props.LeftHandPosition;
                    propRightOffset = extensions.Props.RightHandPosition;
                }

                GUILayout.Label("Offset: " +
                                (propWeapOffset.x + WeaponOffset.x).ToString("N2") + " / " +
                                (propWeapOffset.y + WeaponOffset.y).ToString("N2") + " / " +
                                (propWeapOffset.z + WeaponOffset.z).ToString("N2"));

                WeaponOffset.x = GUILayout.HorizontalSlider(WeaponOffset.x, -1f, 1f);
                WeaponOffset.y = GUILayout.HorizontalSlider(WeaponOffset.y, -1f, 1f);
                WeaponOffset.z = GUILayout.HorizontalSlider(WeaponOffset.z, -1f, 1f);

                GUILayout.Label("OffsetAiming: " +
                                (propAimOffset.x + AimedWeaponOffset.x).ToString("N2") + " / " +
                                (propAimOffset.y + AimedWeaponOffset.y).ToString("N2") + " / " +
                                (propAimOffset.z + AimedWeaponOffset.z).ToString("N2"));

                AimedWeaponOffset.x = GUILayout.HorizontalSlider(AimedWeaponOffset.x, -1f, 1f);
                AimedWeaponOffset.y = GUILayout.HorizontalSlider(AimedWeaponOffset.y, -1f, 1f);
                AimedWeaponOffset.z = GUILayout.HorizontalSlider(AimedWeaponOffset.z, -1f, 1f);

                GUILayout.Label("RH: " +
                                (propRightOffset.x + RightHandPosition.x).ToString("N2") + " / " +
                                (propRightOffset.z + RightHandPosition.z).ToString("N2"));
                rightFront = GUILayout.Toggle(rightFront, "front");
                RightHandPosition.x = GUILayout.HorizontalSlider(RightHandPosition.x, -1f, 1f);
                RightHandPosition.z = GUILayout.HorizontalSlider(RightHandPosition.z, -1f, 1f);

                this.UseSecondHand = GUILayout.Toggle(this.UseSecondHand, "Use left hand");
                if (this.UseSecondHand)
                {
                    GUILayout.Label("LH: " +
                                    (propLeftOffset.x + LeftHandPosition.x).ToString("N2") + " / " +
                                    (propLeftOffset.z + LeftHandPosition.z).ToString("N2"));

                    leftFront = GUILayout.Toggle(leftFront, "front");
                    LeftHandPosition.x = GUILayout.HorizontalSlider(LeftHandPosition.x, -1f, 1f);
                    LeftHandPosition.z = GUILayout.HorizontalSlider(LeftHandPosition.z, -1f, 1f);
                }


                if (GUILayout.Button("Export WeaponExtensionDef"))
                {
                    string defName = "WeaponExtensionDef_" + primary?.def.defName;

                    WeaponExtensionDef wepDef;
                    if (!DefDatabase<WeaponExtensionDef>.AllDefsListForReading.Any(x => x.defName == defName))
                    {
                        wepDef = new WeaponExtensionDef
                        {
                            defName = defName,
                            label = defName,
                            weapon = primary.def.defName
                        };
                        DefDatabase<WeaponExtensionDef>.Add(wepDef);
                    }
                    else
                    {
                        wepDef = DefDatabase<WeaponExtensionDef>.GetNamed(defName);
                    }

                    // clear existing offsets
                    propRightOffset.y = propLeftOffset.y = 0f;

                    wepDef.weaponPositionOffset = propWeapOffset + WeaponOffset;
                    wepDef.aimedWeaponPositionOffset = propAimOffset + AimedWeaponOffset;
                    wepDef.firstHandPosition = propRightOffset + RightHandPosition;
                    wepDef.secondHandPosition = propLeftOffset + LeftHandPosition;

                    if (wepDef.firstHandPosition != Vector3.zero)
                    {
                        wepDef.firstHandPosition.y = this.rightY;
                    }
                    if (wepDef.secondHandPosition != Vector3.zero)
                    {
                        wepDef.secondHandPosition.y = this.leftY;
                    }
                    string configFolder = MainTabWindow_BaseAnimator.DefPath;
                    string path = configFolder + "/WeaponExtensionDefs/" + wepDef.defName + ".xml";


                    Find.WindowStack.Add(
                                         Dialog_MessageBox.CreateConfirmation(
                                                                              "Confirm overwriting: " + path,
                                                                              delegate
                                                                              {
                                                                                  ExportWeaponExtensionDefs.Defs cycle =
                                                                                  new ExportWeaponExtensionDefs.
                                                                                  Defs(wepDef);

                                                                                  DirectXmlSaver.SaveDataObject(
                                                                                                                cycle,
                                                                                                                path);
                                                                              },
                                                                              true));
                }
            }


            GUILayout.EndVertical();

            GUILayout.EndArea();

            // NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, this.size.x, this.size.y), base.SelPawn, ref this.thoughtScrollPosition);
        }

        public bool UseSecondHand
        {
            get { return this._useSecondHand; }
            set
            {
                this._useSecondHand = value;
                if (!value)
                {
                    LeftHandPosition=Vector3.zero;
                }
            }
        }

        public bool leftFront
        {
            get { return this._leftFront; }
            set
            {
                this._leftFront = value;
                leftY = (value ? 1f : -1f) * 0.003f;
            }
        }

        public bool rightFront
        {
            get { return this._rightFront; }
            set
            {
                this._rightFront = value;
                rightY = (value ? 1f : -1f) * 0.003f;
            }
        }

        protected override void UpdateSize()
        {
            this.size = new Vector2(350f, 450f);

            // this.size = NeedsCardUtility.GetSize(base.SelPawn);
        }

        public static Vector3 WeaponOffset;
        public static Vector3 AimedWeaponOffset;
        public static bool IgnoreRenderer;
        private float leftY = -0.003f;
        private float rightY = 0.003f;
        private bool _useSecondHand;
    }
}