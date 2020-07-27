using FacialStuff.AnimatorWindows;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using static FacialStuff.Offsets;

namespace FacialStuff
{
    public class ITab_Pawn_Weapons : ITab
    {

        #region Public Fields

        public static bool IgnoreRenderer;

        #endregion Public Fields

        #region Private Fields

        [CanBeNull] private static CompProperties_WeaponExtensions weaponExtensions;

        private readonly string[] _psiToolbarStrings =
        {"North", "East", "South", "West"};

        private bool _leftFront;
        private bool _rightFront = true;

        private bool _useSecondHand =
        weaponExtensions?.RightHandPosition != Vector3.zero;

        private Pawn pawn;

        #endregion Private Fields

        #region Public Constructors

        public ITab_Pawn_Weapons()
        {
            this.labelKey = "TabWeapons";
            this.tutorTag = "FWeapons";
        }

        #endregion Public Constructors

        #region Public Properties

        public override bool IsVisible => this.SelPawn.HasCompFace() && Controller.settings.Develop;

        public bool LeftFront
        {
            set
            {
                if (weaponExtensions != null)
                {
                    {
                        weaponExtensions.LeftHandPosition.y = (value ? 1f : -1f) * YOffset_HandsFeet;
                    }
                }
            }
        }

        public bool RightFront
        {
            set
            {
                if (weaponExtensions != null)
                {
                    weaponExtensions.RightHandPosition.y = (value ? 1f : -1f) * YOffset_HandsFeet;
                }
            }
        }

        public bool UseSecondHand
        {
            set
            {
                if (weaponExtensions == null)
                {
                    return;
                }

                if (value)
                {
                    if (weaponExtensions.LeftHandPosition == Vector3.zero)
                    {
                        this.LeftFront = false;
                    }
                }
                else
                {
                    weaponExtensions.LeftHandPosition = Vector3.zero;
                }
            }
        }

        #endregion Public Properties

        #region Protected Methods

        protected override void FillTab()
        {
            if (this.pawn != this.SelPawn)
            {
                this.pawn = this.SelPawn;
            }

            bool horizontal = this.pawn.Rotation.IsHorizontal;
            if (!this.SelPawn.GetCompFace(out CompFace compFace))
            {
                return;
            }

            Rect rect = new Rect(10f, 10f, 330f, 530f);

            Listing_Standard listing = new Listing_Standard();

            listing.Begin(rect);


            this.SelPawn.GetCompAnim(out CompBodyAnimator _);


            listing.CheckboxLabeled("Ignore renderer", ref pawn.GetCompAnim().IgnoreRenderer);
            if (GUI.changed)
            {
                IgnoreRenderer = pawn.GetCompAnim().IgnoreRenderer;
            }

            ThingWithComps primary = this.SelPawn.equipment?.Primary;
            if (primary == null)
            {
                return;
            }

            weaponExtensions = primary?.def.GetCompProperties<CompProperties_WeaponExtensions>();

            if (weaponExtensions == null)
            {
                weaponExtensions = new CompProperties_WeaponExtensions
                {
                    compClass = typeof(CompWeaponExtensions)
                };

                primary.def.comps?.Add(weaponExtensions);
            }

            if (this.SelPawn.Drafted)
            {
                listing.Label(this.pawn.equipment.Primary.def.defName);

                Vector3 weaponOffset = weaponExtensions.WeaponPositionOffset;

                listing.Label("Offset: " +
                              (weaponOffset.x).ToString("N2") + " / " +
                              (weaponOffset.y).ToString("N2") + " / " +
                              (weaponOffset.z).ToString("N2"));
                if (horizontal)
                {
                    weaponOffset.y = listing.Slider(weaponOffset.y, -1f, 1f, label:"Horizontal x");

                }
                else
                {
                    weaponOffset.x = listing.Slider(weaponOffset.x, -1f, 1f, label: "Vertical x");

                }
                weaponOffset.z = listing.Slider(weaponOffset.z, -1f, 1f, label: "z");

                Vector3 aimedOffset = weaponExtensions.AimedWeaponPositionOffset;
                listing.Label("OffsetAiming: " +
                              (aimedOffset.x).ToString("N2") + " / " +
                              (aimedOffset.y).ToString("N2") + " / " +
                              (aimedOffset.z).ToString("N2"));
                if (horizontal)
                {
                    aimedOffset.y = listing.Slider(aimedOffset.y, -1f, 1f, label: "Horizontal x");

                }
                else
                {
                    aimedOffset.x = listing.Slider(aimedOffset.x, -1f, 1f, label: "Vertical x");

                }
                aimedOffset.z = listing.Slider(aimedOffset.z, -1f, 1f, label: "z");

                Vector3 rightHandPosition = weaponExtensions.RightHandPosition;
                listing.Label("RH: " +
                              (rightHandPosition.x).ToString("N2") + " / " +
                              (rightHandPosition.z).ToString("N2"));

                listing.Gap();
                rightHandPosition.y =
                listing.Slider(rightHandPosition.y, -YOffset_HandsFeet, YOffset_HandsFeet, leftAlignedLabel: "behind",
                               rightAlignedLabel: "front", roundTo: YOffset_HandsFeet);

                rightHandPosition.x = listing.Slider(rightHandPosition.x, -1f, 1f);
                rightHandPosition.z = listing.Slider(rightHandPosition.z, -1f, 1f);

                Vector3 leftHandPosition = weaponExtensions.LeftHandPosition;
                listing.Label("LH: " +
                              (leftHandPosition.x).ToString("N2") + " / " +
                              (leftHandPosition.z).ToString("N2"));

                listing.Gap();
                leftHandPosition.y =
                listing.Slider(leftHandPosition.y, -YOffset_HandsFeet,
                               YOffset_HandsFeet, leftAlignedLabel: "behind",
                               rightAlignedLabel: "front", roundTo: YOffset_HandsFeet);

                leftHandPosition.x = listing.Slider(leftHandPosition.x, -1f, 1f);
                leftHandPosition.z = listing.Slider(leftHandPosition.z, -1f, 1f);

                if (leftHandPosition != Vector3.zero)
                {
                    if (listing.ButtonText("Remove left hand position"))
                    {
                        leftHandPosition = Vector3.zero;
                    }
                }

                listing.Gap();

                if (listing.ButtonText("Export WeaponExtensionDef"))
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

                    wepDef.weaponPositionOffset = weaponOffset;
                    wepDef.aimedWeaponPositionOffset = aimedOffset;
                    wepDef.firstHandPosition = rightHandPosition;
                    wepDef.secondHandPosition = leftHandPosition;

                    string configFolder = MainTabWindow_BaseAnimator.DefPath;
                    string path = configFolder + "/WeaponExtensionDefs/" + wepDef.defName + ".xml";


                    Find.WindowStack.Add(
                                         Dialog_MessageBox.CreateConfirmation(
                                                                              "Confirm overwriting: " + path,
                                                                              delegate
                                                                              {
                                                                                  ExportWeaponExtensionDefs.Defs cycle =
                                                                                  new ExportWeaponExtensionDefs.Defs(wepDef);

                                                                                  DirectXmlSaver.SaveDataObject(
                                                                                                                cycle,
                                                                                                                path);
                                                                              },
                                                                              true));
                }

                if (GUI.changed)
                {
                    weaponExtensions.WeaponPositionOffset = weaponOffset;
                    weaponExtensions.AimedWeaponPositionOffset = aimedOffset;
                    weaponExtensions.LeftHandPosition = leftHandPosition;
                    weaponExtensions.RightHandPosition = rightHandPosition;
                }
            }


            listing.End();
            // NeedsCardUtility.DoNeedsMoodAndThoughts(new Rect(0f, 0f, this.size.x, this.size.y), base.SelPawn, ref this.thoughtScrollPosition);
        }

        protected override void UpdateSize()
        {
            this.size = new Vector2(350f, 550f);

            // this.size = NeedsCardUtility.GetSize(base.SelPawn);
        }

        #endregion Protected Methods

    }
}