using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class EffectsPane : BasePane
{
    private readonly Dropdown<EffectType> effectTypesDropdown;
    private readonly Dictionary<EffectType, BasePane> effectsPanes = new(EnumEqualityComparer<EffectType>.Instance);
    private readonly PaneHeader paneHeader;

    public EffectsPane(Translation translation)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));

        translation.Initialized += OnTranslationInitialized;

        effectTypesDropdown = new(EffectTypeFormatter);

        paneHeader = new(new LocalizableGUIContent(translation, "effectsPane", "header"), true);

        IDropdownItem EffectTypeFormatter(EffectType type, int index) =>
            new LabelledDropdownItem(translation["effectTypes", type.ToLower()]);

        void OnTranslationInitialized(object sender, EventArgs e) =>
            effectTypesDropdown.Reformat();
    }

    public enum EffectType
    {
        Bloom,
        DepthOfField,
        Vignette,
        Fog,
        SepiaTone,
        Blur,
    }

    public BasePane this[EffectType type]
    {
        get => effectsPanes[type];
        set => Add(type, value);
    }

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        DrawDropdown(effectTypesDropdown);

        effectsPanes[effectTypesDropdown.SelectedItem].Draw();
    }

    private void Add(EffectType type, BasePane pane)
    {
        if (effectsPanes.ContainsKey(type))
            return;

        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        effectsPanes[type] = pane;
        Add(pane);

        var effects = effectTypesDropdown.Concat(new[] { type });

        effectTypesDropdown.SetItemsWithoutNotify(effects, 0);
    }
}
