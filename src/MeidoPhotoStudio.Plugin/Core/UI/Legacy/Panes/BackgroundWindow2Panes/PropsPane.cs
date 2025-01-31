using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropsPane : BasePane
{
    private readonly Dropdown<PropCategory> propTypeDropdown;
    private readonly Dictionary<PropCategory, BasePane> propPanes = new(EnumEqualityComparer<PropCategory>.Instance);
    private readonly List<PropCategory> propTypes = [];

    public PropsPane(Translation translation)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));

        translation.Initialized += OnTranslationInitialized;

        propTypeDropdown = new(formatter: CategoryFormatter);

        LabelledDropdownItem CategoryFormatter(PropCategory category, int index) =>
            new(translation["propTypes", category.ToLower()]);

        void OnTranslationInitialized(object sender, EventArgs e) =>
            propTypeDropdown.Reformat();
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
        DrawDropdown(propTypeDropdown);

        UIUtility.DrawBlackLine();

        propPanes[propTypes[propTypeDropdown.SelectedItemIndex]].Draw();
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
