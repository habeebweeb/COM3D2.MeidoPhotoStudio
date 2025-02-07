using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class ComboBox : DropdownBase<string>
{
    private static readonly Func<string, int, IDropdownItem> DefaultFormatter = static (string item, int index) =>
        new LabelledDropdownItem(string.IsNullOrEmpty(item) ? string.Empty : item);

    private static readonly GUILayoutOption[] ButtonLayoutOptions = new GUILayoutOption[1];

    private readonly SearchBar<string> searchBar;

    private bool clickedWhileOpen;
    private bool buttonClicked;

    public ComboBox(IEnumerable<string> items, Func<string, int, IDropdownItem> formatter = null)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));

        base.Formatter = formatter ?? DefaultFormatter;

        searchBar = new(SearchSelector, formatter ?? DefaultFormatter);
        searchBar.SelectedValue += OnValueSelected;

        SetItems(items);

        IEnumerable<string> SearchSelector(string query) =>
            this.Where(item => item.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public event EventHandler SelectedValue;

    public event EventHandler ChangedValue
    {
        add => searchBar.ChangedValue += value;
        remove => searchBar.ChangedValue -= value;
    }

    public override Func<string, int, IDropdownItem> Formatter
    {
        get => base.Formatter;
        set
        {
            base.Formatter = value;
            searchBar.Formatter = value;
        }
    }

    public string Value
    {
        get => searchBar.Query;
        set => searchBar.Query = value;
    }

    public string Placeholder
    {
        get => searchBar.Placeholder;
        set => searchBar.Placeholder = value;
    }

    public GUIContent PlaceholderContent
    {
        get => searchBar.PlaceholderContent;
        set => searchBar.PlaceholderContent = value;
    }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        ButtonLayoutOptions[0] = GUILayout.Width(UIUtility.Scaled(20));

        Draw(TextField.Style, Button.Style, layoutOptions, ButtonLayoutOptions);
    }

    public void Draw(
        GUIStyle textFieldStyle,
        GUIStyle buttonStyle,
        GUILayoutOption[] comboBoxLayoutOptions,
        GUILayoutOption[] buttonLayoutOptions)
    {
        GUILayout.BeginHorizontal();

        searchBar.Draw(textFieldStyle, comboBoxLayoutOptions);

        var clicked = GUILayout.Button(Symbols.DownChevron, buttonStyle, buttonLayoutOptions);

        if (clicked)
        {
            buttonClicked = !clickedWhileOpen;
            clickedWhileOpen = false;
        }

        if (buttonClicked && Event.current.type is EventType.Repaint)
        {
            buttonClicked = false;

            DropdownHelper.OpenDropdown(this, GUILayoutUtility.GetLastRect());
        }

        GUILayout.EndHorizontal();
    }

    protected override void OnItemSelected(int index)
    {
        base.OnItemSelected(index);

        searchBar.SetQueryWithoutShowingResults(this[SelectedItemIndex]);

        SelectedValue?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnDropdownClosed(bool clickedButton) =>
        clickedWhileOpen = clickedButton;

    private void OnValueSelected(object sender, SearchBarSelectionEventArgs<string> e)
    {
        searchBar.SetQueryWithoutShowingResults(e.Item);

        SelectedValue?.Invoke(this, EventArgs.Empty);
    }
}
