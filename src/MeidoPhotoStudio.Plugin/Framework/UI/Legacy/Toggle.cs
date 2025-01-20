namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Toggle(GUIContent content, bool state = false) : BaseControl
{
    private bool value = state;
    private GUIContent content = content ?? new();

    public Toggle(string label, bool state = false)
        : this(new GUIContent(label ?? string.Empty), state)
    {
    }

    public Toggle(Texture icon, bool state = false)
        : this(new GUIContent(icon), state)
    {
    }

    private event EventHandler<InternalChangeEventArgs> ChangedInternally;

    public static LazyStyle Style { get; } = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.toggle)
        {
            wordWrap = true,
        });

    public string Label
    {
        get => Content.text;
        set => Content.text = value ?? string.Empty;
    }

    public Texture Icon
    {
        get => Content.image;
        set => Content.image = value;
    }

    public GUIContent Content
    {
        get => content;
        set => content = value ?? new();
    }

    public bool Value
    {
        get => value;
        set => SetEnabled(value);
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle toggleStyle, params GUILayoutOption[] layoutOptions)
    {
        var value = GUILayout.Toggle(Value, content, toggleStyle, layoutOptions);

        if (value != Value)
            Value = value;
    }

    public void SetEnabledWithoutNotify(bool enabled) =>
        SetEnabled(enabled, false);

    private void SetEnabled(bool enabled, bool notify = true)
    {
        if (enabled == value)
            return;

        var previousState = value;

        value = enabled;

        ChangedInternally?.Invoke(this, new(notify));

        if (value == previousState)
            return;

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }

    // TODO: Take a look at how this is initialized and used in various places and maybe rework this a little bit to be
    // more comfortable to use. Something to notice is that I'm creating dictionaries to be able to access a particular
    // toggle within the group.
    public class Group : IEnumerable<Toggle>
    {
        private readonly List<Toggle> toggles = [];

        public bool AllowSwitchOff { get; set; }

        public int Count =>
            toggles.Count;

        public Toggle this[int index] =>
            (uint)index >= toggles.Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : toggles[index];

        public void Add(Toggle toggle)
        {
            _ = toggle ?? throw new ArgumentNullException(nameof(toggle));

            if (toggles.Contains(toggle))
                return;

            toggles.Add(toggle);

            toggle.ChangedInternally += OnToggleChanged;
        }

        public void Add(params Toggle[] toggles)
        {
            foreach (var toggle in toggles)
                Add(toggle);
        }

        public void Insert(int index, Toggle toggle)
        {
            if ((uint)index > toggles.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _ = toggle ?? throw new ArgumentNullException(nameof(toggle));

            if (toggles.Contains(toggle))
                return;

            toggles.Insert(index, toggle);

            toggle.ChangedInternally += OnToggleChanged;
        }

        public bool Remove(Toggle toggle)
        {
            _ = toggle ?? throw new ArgumentNullException(nameof(toggle));

            if (!toggles.Remove(toggle))
                return false;

            toggle.ChangedInternally -= OnToggleChanged;

            return toggles.Remove(toggle);
        }

        public IEnumerator<Toggle> GetEnumerator() =>
            toggles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private void OnToggleChanged(object sender, InternalChangeEventArgs e)
        {
            if (sender is not Toggle changedToggle)
                return;

            if (!toggles.Contains(changedToggle))
                return;

            if (!AllowSwitchOff && !toggles.Any(toggle => toggle.Value))
                changedToggle.value = true;

            if (!changedToggle.Value)
                return;

            foreach (var toggle in toggles.Where(toggle => toggle != changedToggle))
                toggle.SetEnabled(false, e.Notify);
        }
    }

    private class InternalChangeEventArgs(bool notify) : EventArgs
    {
        public bool Notify { get; } = notify;
    }
}
