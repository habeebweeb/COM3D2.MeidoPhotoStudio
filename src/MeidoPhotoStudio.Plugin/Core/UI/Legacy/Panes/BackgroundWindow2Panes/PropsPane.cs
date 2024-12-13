using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropsPane : BasePane
{
    private readonly Dropdown<PropCategory> propTypeDropdown;
    private readonly Dictionary<PropCategory, BasePane> propPanes = new(EnumEqualityComparer<PropCategory>.Instance);
    private readonly List<PropCategory> propTypes = [];
    private readonly PaneHeader paneHeader;

    public PropsPane()
    {
        propTypeDropdown = new(formatter: CategoryFormatter);

        paneHeader = new(Translation.Get("propsPane", "header"), true);

        static LabelledDropdownItem CategoryFormatter(PropCategory category, int index) =>
            new(Translation.Get("propTypes", category.ToLower()));
    }

    public enum PropCategory
    {
        Game,
        Desk,
        Other,
        Background,
        MyRoom,
        Menu,
        HandItem,
        Favourite,
    }

    public BasePane this[PropCategory category]
    {
        get => propPanes[category];
        set => Add(category, value);
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        DrawDropdown(propTypeDropdown);

        UIUtility.DrawBlackLine();

        propPanes[propTypes[propTypeDropdown.SelectedItemIndex]].Draw();
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        propTypeDropdown.Reformat();

        paneHeader.Label = Translation.Get("propsPane", "header");
    }

    private void Add(PropCategory key, BasePane pane)
    {
        if (propPanes.ContainsKey(key))
            return;

        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        propPanes[key] = pane;

        propTypes.Add(key);
        Add(pane);

        propTypeDropdown.SetItemsWithoutNotify(propTypes, 0);
    }
}
