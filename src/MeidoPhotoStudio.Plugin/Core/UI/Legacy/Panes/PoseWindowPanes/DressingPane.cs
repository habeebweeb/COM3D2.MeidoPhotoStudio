using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

using Curling = MeidoPhotoStudio.Plugin.Core.Character.ClothingController.Curling;
using MaskMode = TBody.MaskMode;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class DressingPane : BasePane
{
    private static readonly HashSet<SlotID> ClothingSlots =
    [
        SlotID.wear, SlotID.mizugi, SlotID.onepiece, SlotID.skirt, SlotID.bra, SlotID.panz, SlotID.headset,
        SlotID.megane, SlotID.accHead, SlotID.accUde, SlotID.glove, SlotID.accSenaka, SlotID.stkg, SlotID.shoes,
        SlotID.body,

        SlotID.accAshi, SlotID.accHana, SlotID.accHat, SlotID.accHeso, SlotID.accKamiSubL, SlotID.accKamiSubR,
        SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_, SlotID.accKubi, SlotID.accKubiwa, SlotID.accMiMiL,
        SlotID.accMiMiR, SlotID.accNipL, SlotID.accNipR, SlotID.accShippo, SlotID.accXXX,
    ];

    private static readonly SlotID[] WearSlots = [SlotID.wear, SlotID.mizugi, SlotID.onepiece];

    private static readonly SlotID[] HeadwearSlots =
    [
        SlotID.headset, SlotID.accHat, SlotID.accKamiSubL, SlotID.accKamiSubR, SlotID.accKami_1_, SlotID.accKami_2_,
        SlotID.accKami_3_,
    ];

    private static readonly SlotID[][] SlotGroups =
    [
        [SlotID.wear, SlotID.skirt],
        [SlotID.bra, SlotID.panz],
        [SlotID.headset, SlotID.megane],
        [SlotID.accUde, SlotID.glove, SlotID.accSenaka],
        [SlotID.stkg, SlotID.shoes, SlotID.body],
    ];

    private static readonly SlotID[][] DetailedSlotGroups =
    [
        [SlotID.wear, SlotID.skirt],
        [SlotID.mizugi, SlotID.onepiece],
        [SlotID.bra, SlotID.panz],
        [SlotID.headset, SlotID.megane, SlotID.accHead],
        [SlotID.accUde, SlotID.glove, SlotID.accSenaka],
        [SlotID.stkg, SlotID.shoes, SlotID.body],
        [SlotID.accShippo, SlotID.accHat],
        [SlotID.accKami_1_, SlotID.accKami_2_, SlotID.accKami_3_],
        [SlotID.accKamiSubL, SlotID.accKamiSubR],
        [SlotID.accMiMiL, SlotID.accMiMiR],
        [SlotID.accNipL, SlotID.accNipR],
        [SlotID.accHana, SlotID.accKubi, SlotID.accKubiwa],
        [SlotID.accHeso, SlotID.accAshi, SlotID.accXXX],
    ];

    private static readonly MaskMode[] DressingModes = [MaskMode.None, MaskMode.Underwear, MaskMode.Nude];

    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Dictionary<SlotID, Toggle> clothingToggles;
    private readonly Dictionary<SlotID, bool> loadedSlots;
    private readonly Toggle detailedClothingToggle;
    private readonly Dictionary<MaskMode, Toggle> dressingToggles;
    private readonly Toggle.Group dressingGroup;
    private readonly Toggle curlingFrontToggle;
    private readonly Toggle curlingBackToggle;
    private readonly Toggle underwearShiftToggle;
    private readonly LocalizableGUIContent headsetContent;
    private readonly LocalizableGUIContent headwearContent;

    public DressingPane(Translation translation, SelectionController<CharacterController> characterSelectionController)
    {
        this.characterSelectionController = characterSelectionController
            ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        detailedClothingToggle = new(new LocalizableGUIContent(translation, "dressingPane", "detailedClothing"));
        detailedClothingToggle.ControlEvent += OnDetailedClothingChanged;

        dressingToggles = DressingModes.ToDictionary(mode => mode, CreateDressingModeToggle, EnumEqualityComparer<MaskMode>.Instance);

        dressingGroup = [.. dressingToggles.Values];

        headsetContent = new LocalizableGUIContent(translation, "clothing", SlotID.headset.ToLower());
        headwearContent = new LocalizableGUIContent(translation, "clothing", "headwear");

        clothingToggles = ClothingSlots
            .ToDictionary(slot => slot, CreateSlotToggle, EnumEqualityComparer<SlotID>.Instance);

        loadedSlots = ClothingSlots
            .ToDictionary(slot => slot, _ => false, EnumEqualityComparer<SlotID>.Instance);

        curlingFrontToggle = new(new LocalizableGUIContent(translation, "dressingPane", "curlingFront"));
        curlingFrontToggle.ControlEvent += OnCurlingFrontChanged;

        curlingBackToggle = new(new LocalizableGUIContent(translation, "dressingPane", "curlingBack"));
        curlingBackToggle.ControlEvent += OnCurlingBackChanged;

        underwearShiftToggle = new(new LocalizableGUIContent(translation, "dressingPane", "shiftPanties"));
        underwearShiftToggle.ControlEvent += OnUnderwearShiftChanged;

        Toggle CreateSlotToggle(SlotID slot)
        {
            var content = slot is SlotID.headset
                ? headsetContent
                : new LocalizableGUIContent(translation, "clothing", slot.ToLower());

            var toggle = new Toggle(content);

            toggle.ControlEvent += (_, _) =>
                OnSlotToggleChanged(slot, toggle.Value);

            return toggle;
        }

        Toggle CreateDressingModeToggle(MaskMode mode)
        {
            var key = mode switch
            {
                MaskMode.None => "all",
                MaskMode.Underwear => "underwear",
                MaskMode.Nude => "nude",
                _ => throw new ArgumentOutOfRangeException(nameof(mode)),
            };

            var toggle = new Toggle(new LocalizableGUIContent(translation, "dressingPane", key));

            toggle.ControlEvent += (sender, _) =>
            {
                if (sender is not Toggle { Value: true } || CurrentClothing is not ClothingController controller)
                    return;

                controller.DressingMode = mode;
            };

            return toggle;
        }
    }

    private ClothingController CurrentClothing =>
        characterSelectionController.Current?.Clothing;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        detailedClothingToggle.Draw();

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        foreach (var dressingToggle in dressingGroup)
            dressingToggle.Draw();

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        if (detailedClothingToggle.Value)
        {
            for (var i = 0; i < DetailedSlotGroups.Length; i++)
            {
                DrawSlotGroup(DetailedSlotGroups[i]);

                if (i is 5)
                    UIUtility.DrawBlackLine();
            }
        }
        else
        {
            for (var i = 0; i < SlotGroups.Length; i++)
                DrawSlotGroup(SlotGroups[i]);
        }

        UIUtility.DrawBlackLine();

        DrawCurlingToggles();

        void DrawSlotGroup(SlotID[] slots)
        {
            GUILayout.BeginHorizontal();

            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];

                GUI.enabled = enabled && loadedSlots[slot];
                clothingToggles[slot].Draw();

                if (i < slots.Length - 1)
                    GUILayout.FlexibleSpace();
            }

            GUILayout.EndHorizontal();
        }

        void DrawCurlingToggles()
        {
            GUILayout.BeginHorizontal();

            GUI.enabled = enabled && (CurrentClothing?.SupportsCurlingType(Curling.Front) ?? false);
            curlingFrontToggle.Draw();

            GUILayout.FlexibleSpace();

            GUI.enabled = enabled && (CurrentClothing?.SupportsCurlingType(Curling.Back) ?? false);
            curlingBackToggle.Draw();

            GUILayout.FlexibleSpace();

            GUI.enabled = enabled && (CurrentClothing?.SupportsCurlingType(Curling.Shift) ?? false);
            underwearShiftToggle.Draw();

            GUILayout.EndHorizontal();
        }
    }

    private void UpdateControls()
    {
        UpdateDressingGrid();
        UpdateClothingToggles();
        UpdateCurlingToggles();
    }

    private void UpdateClothingToggles()
    {
        if (CurrentClothing is null)
            return;

        foreach (var slot in ClothingSlots)
            clothingToggles[slot].SetEnabledWithoutNotify(slot switch
            {
                SlotID.wear when !detailedClothingToggle.Value => WearSlots.Any(slot => CurrentClothing[slot]),
                SlotID.megane or SlotID.accHead when !detailedClothingToggle.Value => CurrentClothing[SlotID.megane] || CurrentClothing[SlotID.accHead],
                SlotID.headset when !detailedClothingToggle.Value => HeadwearSlots.Any(slot => CurrentClothing[slot]),
                SlotID.body => CurrentClothing.BodyVisible,
                _ => CurrentClothing[slot],
            });

        clothingToggles[SlotID.headset].Content = detailedClothingToggle.Value ? headsetContent : headwearContent;
    }

    private void UpdateCurlingToggles()
    {
        if (CurrentClothing is null)
            return;

        curlingFrontToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Front]);
        curlingBackToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Back]);
        underwearShiftToggle.SetEnabledWithoutNotify(CurrentClothing[Curling.Shift]);
    }

    private void UpdateDressingGrid()
    {
        if (CurrentClothing is null)
            return;

        dressingToggles[CurrentClothing.DressingMode].SetEnabledWithoutNotify(true);
    }

    private void UpdateLoadedSlots()
    {
        foreach (var slot in ClothingSlots)
            loadedSlots[slot] = slot switch
            {
                SlotID.wear when !detailedClothingToggle.Value => WearSlots.Any(CurrentClothing.SlotLoaded),
                SlotID.megane when !detailedClothingToggle.Value => CurrentClothing.SlotLoaded(SlotID.megane) || CurrentClothing.SlotLoaded(SlotID.accHead),
                SlotID.headset when !detailedClothingToggle.Value => HeadwearSlots.Any(CurrentClothing.SlotLoaded),
                _ => CurrentClothing.SlotLoaded(slot),
            };
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Clothing.PropertyChanged -= OnClothingPropertyChanged;
        e.Selected.Clothing.ClothingChanged -= OnClothingKeyChanged;
        e.Selected.Clothing.CurlingChanged -= OnCurlingKeyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentClothing is null)
            return;

        e.Selected.Clothing.PropertyChanged += OnClothingPropertyChanged;
        e.Selected.Clothing.ClothingChanged += OnClothingKeyChanged;
        e.Selected.Clothing.CurlingChanged += OnCurlingKeyChanged;

        UpdateLoadedSlots();
        UpdateControls();
    }

    private void OnClothingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var clothing = (ClothingController)sender;

        if (e.PropertyName is nameof(ClothingController.BodyVisible))
        {
            clothingToggles[SlotID.body].SetEnabledWithoutNotify(clothing.BodyVisible);
        }
        else if (e.PropertyName is nameof(ClothingController.DressingMode))
        {
            UpdateDressingGrid();
            UpdateClothingToggles();
        }
    }

    private void OnClothingKeyChanged(object sender, KeyedPropertyChangeEventArgs<SlotID> e)
    {
        var clothing = (ClothingController)sender;

        if (!ClothingSlots.Contains(e.Key))
            return;

        if (detailedClothingToggle.Value)
        {
            clothingToggles[e.Key].SetEnabledWithoutNotify(clothing[e.Key]);
            loadedSlots[e.Key] = clothing.SlotLoaded(e.Key);
        }
        else if (HeadwearSlots.Contains(e.Key))
        {
            clothingToggles[SlotID.headset].SetEnabledWithoutNotify(HeadwearSlots.Any(slot => clothing[slot]));
            loadedSlots[SlotID.headset] = HeadwearSlots.Any(clothing.SlotLoaded);
        }
        else if (WearSlots.Contains(e.Key))
        {
            clothingToggles[SlotID.wear].SetEnabledWithoutNotify(WearSlots.Any(slot => clothing[slot]));
            loadedSlots[SlotID.wear] = WearSlots.Any(clothing.SlotLoaded);
        }
        else if (e.Key is SlotID.megane or SlotID.accHead)
        {
            clothingToggles[SlotID.megane].SetEnabledWithoutNotify(clothing[SlotID.megane] || clothing[SlotID.accHead]);
            loadedSlots[SlotID.megane] = clothing.SlotLoaded(SlotID.megane) || clothing.SlotLoaded(SlotID.accHead);
        }
        else
        {
            clothingToggles[e.Key].SetEnabledWithoutNotify(clothing[e.Key]);
            loadedSlots[e.Key] = clothing.SlotLoaded(e.Key);
        }
    }

    private void OnCurlingKeyChanged(object sender, KeyedPropertyChangeEventArgs<Curling> e)
    {
        var clothing = (ClothingController)sender;

        if (e.Key is Curling.Front)
            curlingFrontToggle.SetEnabledWithoutNotify(clothing[e.Key]);
        else if (e.Key is Curling.Back)
            curlingBackToggle.SetEnabledWithoutNotify(clothing[e.Key]);
        else if (e.Key is Curling.Shift)
            underwearShiftToggle.SetEnabledWithoutNotify(clothing[e.Key]);
    }

    private void OnDetailedClothingChanged(object sender, EventArgs e)
    {
        UpdateLoadedSlots();
        UpdateClothingToggles();
    }

    private void OnSlotToggleChanged(SlotID slot, bool value)
    {
        if (CurrentClothing is null)
            return;

        if (slot is SlotID.body)
        {
            CurrentClothing.BodyVisible = value;
        }
        else if (detailedClothingToggle.Value)
        {
            CurrentClothing[slot] = value;
        }
        else if (slot is SlotID.headset)
        {
            foreach (var headwearSlot in HeadwearSlots)
            {
                CurrentClothing[headwearSlot] = value;
                clothingToggles[headwearSlot].SetEnabledWithoutNotify(value);
            }
        }
        else if (slot is SlotID.wear)
        {
            foreach (var wearSlot in WearSlots)
                CurrentClothing[wearSlot] = value;
        }
        else if (slot is SlotID.megane)
        {
            CurrentClothing[SlotID.megane] = value;
            CurrentClothing[SlotID.accHead] = value;
        }
        else
        {
            CurrentClothing[slot] = value;
        }
    }

    private void OnCurlingFrontChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing[Curling.Front] = curlingFrontToggle.Value;

        curlingBackToggle.SetEnabledWithoutNotify(false);
    }

    private void OnCurlingBackChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing[Curling.Back] = curlingBackToggle.Value;

        curlingFrontToggle.SetEnabledWithoutNotify(false);
    }

    private void OnUnderwearShiftChanged(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing[Curling.Shift] = underwearShiftToggle.Value;
    }
}
