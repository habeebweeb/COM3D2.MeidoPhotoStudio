using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class PropsPane : BasePane, IEnumerable<KeyValuePair<PropsPane.PropCategory, BasePane>>
{
    private readonly Dropdown propTypeDropdown;
    private readonly Dictionary<PropCategory, BasePane> propPanes = new(EnumEqualityComparer<PropCategory>.Instance);
    private readonly List<PropCategory> propTypes = new();
    private readonly Toggle paneHeader;

    public PropsPane()
    {
        propTypeDropdown = new(new[] { "PROP TYPES" });
        propTypeDropdown.ControlEvent += OnPropTypeSelectionChanged;

        paneHeader = new(Translation.Get("propsPane", "header"), true);
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
    }

    public BasePane this[PropCategory category]
    {
        get => propPanes[category];
        set => Add(category, value);
    }

    public override void Draw()
    {
        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        GUILayout.BeginHorizontal();

        propTypeDropdown.Draw();

        var arrowLayoutOptions = new[]
        {
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(false),
        };

        if (GUILayout.Button("<", arrowLayoutOptions))
            propTypeDropdown.Step(-1);

        if (GUILayout.Button(">", arrowLayoutOptions))
            propTypeDropdown.Step(1);

        GUILayout.EndHorizontal();

        MpsGui.WhiteLine();

        propPanes[propTypes[propTypeDropdown.SelectedItemIndex]].Draw();
    }

    public IEnumerator<KeyValuePair<PropCategory, BasePane>> GetEnumerator() =>
        propPanes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(PropCategory key, BasePane pane)
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        propPanes[key] = pane;

        propTypes.Add(key);

        updating = true;

        propTypeDropdown.SetDropdownItems(
            propTypes
                .Select(EnumToLower)
                .Select(key => Translation.Get("propTypes", key))
                .ToArray(),
            0);

        updating = false;
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        propTypeDropdown.SetDropdownItemsWithoutNotify(
            propTypes
            .Select(EnumToLower)
            .Select(key => Translation.Get("propTypes", key))
            .ToArray());

        paneHeader.Label = Translation.Get("propsPane", "header");
    }

    private static string EnumToLower<T>(T enumValue)
        where T : Enum
    {
        var enumString = enumValue.ToString();

        return char.ToLower(enumString[0]) + enumString.Substring(1);
    }

    private void OnPropTypeSelectionChanged(object sender, EventArgs e)
    {
        if (updating)
            return;
    }
}
