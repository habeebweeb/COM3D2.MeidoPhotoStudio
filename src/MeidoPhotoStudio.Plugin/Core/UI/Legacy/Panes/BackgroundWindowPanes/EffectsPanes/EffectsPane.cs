using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class EffectsPane : BasePane
{
    private readonly Dropdown<EffectType> effectTypesDropdown;
    private readonly Dictionary<EffectType, BasePane> effectsPanes = new(EnumEqualityComparer<EffectType>.Instance);
    private readonly PaneHeader paneHeader;

    public EffectsPane()
    {
        effectTypesDropdown = new(static (type, _) => new LabelledDropdownItem(Translation.Get("effectTypes", type.ToLower())));

        paneHeader = new(Translation.Get("effectsPane", "header"), true);
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

    protected override void ReloadTranslation()
    {
        effectTypesDropdown.Reformat();

        paneHeader.Label = Translation.Get("effectsPane", "header");
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
