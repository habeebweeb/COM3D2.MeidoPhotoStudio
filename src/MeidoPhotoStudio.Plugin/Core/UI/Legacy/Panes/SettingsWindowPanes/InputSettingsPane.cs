using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Input;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class InputSettingsPane : BasePane
{
    private readonly Dictionary<Hotkey, GUIContent> hotkeyMapping;
    private readonly Dictionary<Hotkey, GUIContent> hotkeyName;
    private readonly InputConfiguration inputConfiguration;
    private readonly InputRemapper inputRemapper;
    private readonly Dictionary<Shortcut, GUIContent> shortcutMapping;
    private readonly Dictionary<Shortcut, GUIContent> shortcutName;
    private readonly PaneHeader generalControlsHeader;
    private readonly PaneHeader cameraControlsHeader;
    private readonly PaneHeader transformDragHandleControlsHeader;
    private readonly PaneHeader characterControlsHeader;
    private readonly GUIContent cancelRebindLabel = new("Cancel");
    private readonly GUIContent pushAnyKeyLabel = new("Push any key combo");
    private readonly GUIContent clearBindingLabel = new("Clear");
    private readonly GUIContent resetBindingLabel = new("Reset");
    private readonly LazyStyle labelStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.label));
    private readonly LazyStyle inputButtonStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.button)
        {
            wordWrap = true,
        });

    private readonly LazyStyle buttonStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.button));

    private Hotkey currentHotkey;
    private Shortcut currentShortcut;
    private bool listeningToShortcut;

    public InputSettingsPane(InputConfiguration inputConfiguration, InputRemapper inputRemapper)
    {
        this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));

        generalControlsHeader = new(Translation.Get("inputSettingsPane", "generalControlsHeader"));
        cameraControlsHeader = new(Translation.Get("inputSettingsPane", "cameraControlsHeader"));
        transformDragHandleControlsHeader = new(Translation.Get("inputSettingsPane", "transformDragHandleControlsHeader"));
        characterControlsHeader = new(Translation.Get("inputSettingsPane", "characterControlsHeader"));

        var shortcutValues = (Shortcut[])Enum.GetValues(typeof(Shortcut));

        shortcutMapping = shortcutValues
            .ToDictionary(
                shortcut => shortcut,
                shortcut => new GUIContent(inputConfiguration[shortcut].ToString()),
                EnumEqualityComparer<Shortcut>.Instance);

        shortcutName = shortcutValues
            .ToDictionary(
                shortcut => shortcut,
                shortcut => new GUIContent(Translation.Get("controls", shortcut.ToLower())),
                EnumEqualityComparer<Shortcut>.Instance);

        var hotkeyValues = (Hotkey[])Enum.GetValues(typeof(Hotkey));

        hotkeyMapping = hotkeyValues
            .ToDictionary(
                hotkey => hotkey,
                hotkey => new GUIContent(inputConfiguration[hotkey].ToString()),
                EnumEqualityComparer<Hotkey>.Instance);

        hotkeyName = hotkeyValues
            .ToDictionary(
                hotkey => hotkey,
                hotkey => new GUIContent(Translation.Get("controls", hotkey.ToLower())),
                EnumEqualityComparer<Hotkey>.Instance);

        pushAnyKeyLabel.text = Translation.Get("inputSettingsPane", "pushAnyKeyLabel");
        cancelRebindLabel.text = Translation.Get("inputSettingsPane", "cancelRebindLabel");
        clearBindingLabel.text = Translation.Get("inputSettingsPane", "clearBindingLabel");
        resetBindingLabel.text = Translation.Get("inputSettingsPane", "resetBindingLabel");
    }

    public override void Draw()
    {
        var parentEnabled = Parent.Enabled;

        DrawGeneralControls();
        DrawCameraControls();
        DrawTransformDragHandleControls();
        DrawCharacterDragHandleControls();

        void DrawGeneralControls()
        {
            if (!DrawHeader(generalControlsHeader))
                return;

            for (var shortcut = Shortcut.ActivatePlugin; shortcut <= Shortcut.CyclePreviousAnimation; shortcut++)
                DrawControl(shortcut);
        }

        void DrawCameraControls()
        {
            if (!DrawHeader(cameraControlsHeader))
                return;

            for (var shortcut = Shortcut.SaveCamera; shortcut <= Shortcut.ToggleCamera5; shortcut++)
                DrawControl(shortcut);

            for (var hotkey = Hotkey.FastCamera; hotkey <= Hotkey.SlowCamera; hotkey++)
                DrawControl(hotkey);
        }

        void DrawTransformDragHandleControls()
        {
            if (!DrawHeader(transformDragHandleControlsHeader))
                return;

            for (var hotkey = Hotkey.Select; hotkey <= Hotkey.Scale; hotkey++)
                DrawControl(hotkey);
        }

        void DrawCharacterDragHandleControls()
        {
            if (!DrawHeader(characterControlsHeader))
                return;

            for (var hotkey = Hotkey.DragFinger; hotkey <= Hotkey.MoveLocalY; hotkey++)
                DrawControl(hotkey);
        }

        bool DrawHeader(PaneHeader header)
        {
            GUI.enabled = parentEnabled && !inputRemapper.Listening;

            header.Draw();

            GUI.enabled = parentEnabled;

            return header.Enabled;
        }

        void DrawControl(Enum key)
        {
            var isShortcut = key.GetType() == typeof(Shortcut);

            GUI.enabled = parentEnabled;

            DrawShortcutLabel(key, isShortcut);

            GUILayout.BeginHorizontal();

            var buttonWidth = GUILayout.MaxWidth(Parent.WindowRect.width - 45f);

            if (CurrentControlIsListening(key, isShortcut))
            {
                GUI.enabled = false;

                GUILayout.Button(pushAnyKeyLabel, inputButtonStyle, buttonWidth);

                GUI.enabled = parentEnabled;

                DrawCancelListeningButton();
            }
            else
            {
                if (DrawControlButton(key, isShortcut))
                    ListenForNewKeyCombo(key, isShortcut);

                GUI.enabled = parentEnabled;

                if (GUILayout.Button(clearBindingLabel, buttonStyle, GUILayout.ExpandWidth(false)))
                    ClearButtonCombo(key, isShortcut);

                if (GUILayout.Button(resetBindingLabel, buttonStyle, GUILayout.ExpandWidth(false)))
                    ResetButtonCombo(key, isShortcut);
            }

            GUILayout.EndHorizontal();

            bool CurrentControlIsListening(Enum key, bool isShortcut) =>
                inputRemapper.Listening && (isShortcut
                    ? listeningToShortcut && (Shortcut)key == currentShortcut
                    : !listeningToShortcut && (Hotkey)key == currentHotkey);

            bool DrawControlButton(Enum key, bool isShortcut)
            {
                var mapping = isShortcut
                    ? shortcutMapping[(Shortcut)key]
                    : hotkeyMapping[(Hotkey)key];

                GUI.enabled = parentEnabled && !inputRemapper.Listening;

                var clicked = GUILayout.Button(mapping, inputButtonStyle, buttonWidth);

                GUI.enabled = parentEnabled;

                return clicked;
            }

            void ListenForNewKeyCombo(Enum key, bool isShortcut)
            {
                listeningToShortcut = isShortcut;

                if (isShortcut)
                {
                    inputRemapper.ListenForShortcut(OnControlRemapped);
                    currentShortcut = (Shortcut)key;
                }
                else
                {
                    inputRemapper.ListenForHotkey(OnControlRemapped);
                    currentHotkey = (Hotkey)key;
                }

                void OnControlRemapped(KeyboardInput input) =>
                    SetCombo(key, isShortcut, input);
            }

            void ClearButtonCombo(Enum key, bool isShortcut) =>
                SetCombo(key, isShortcut, isShortcut ? KeyboardShortcut.Empty : KeyboardHotkey.Empty);

            void ResetButtonCombo(Enum key, bool isShortcut)
            {
                KeyboardInput defaultKey = isShortcut
                    ? inputConfiguration.DefaultBinding((Shortcut)key)
                    : inputConfiguration.DefaultBinding((Hotkey)key);

                SetCombo(key, isShortcut, defaultKey);
            }

            void SetCombo(Enum key, bool isShortcut, KeyboardInput input)
            {
                if (isShortcut)
                {
                    var shortcut = (KeyboardShortcut)input;

                    inputConfiguration[(Shortcut)key] = shortcut;
                    shortcutMapping[(Shortcut)key] = new GUIContent(shortcut.ToString());
                }
                else
                {
                    var hotkey = (KeyboardHotkey)input;

                    inputConfiguration[(Hotkey)key] = hotkey;
                    hotkeyMapping[(Hotkey)key] = new GUIContent(hotkey.ToString());
                }
            }

            void DrawCancelListeningButton()
            {
                GUI.enabled = inputRemapper.Listening;

                if (GUILayout.Button(cancelRebindLabel))
                    inputRemapper.Cancel();
            }

            void DrawShortcutLabel(Enum key, bool isShortcut)
            {
                var keyName = isShortcut ? shortcutName[(Shortcut)key] : hotkeyName[(Hotkey)key];

                GUILayout.Label(keyName, labelStyle, GUILayout.ExpandWidth(false));
            }
        }
    }

    protected override void ReloadTranslation()
    {
        foreach (var shortcut in (Shortcut[])Enum.GetValues(typeof(Shortcut)))
            shortcutName[shortcut].text = Translation.Get("controls", shortcut.ToLower());

        foreach (var hotkey in (Hotkey[])Enum.GetValues(typeof(Hotkey)))
            hotkeyName[hotkey].text = Translation.Get("controls", hotkey.ToLower());

        generalControlsHeader.Label = Translation.Get("inputSettingsPane", "generalControlsHeader");
        cameraControlsHeader.Label = Translation.Get("inputSettingsPane", "cameraControlsHeader");
        transformDragHandleControlsHeader.Label = Translation.Get("inputSettingsPane", "transformDragHandleControlsHeader");
        characterControlsHeader.Label = Translation.Get("inputSettingsPane", "characterControlsHeader");

        pushAnyKeyLabel.text = Translation.Get("inputSettingsPane", "pushAnyKeyLabel");
        cancelRebindLabel.text = Translation.Get("inputSettingsPane", "cancelRebindLabel");
        clearBindingLabel.text = Translation.Get("inputSettingsPane", "clearBindingLabel");
        resetBindingLabel.text = Translation.Get("inputSettingsPane", "resetBindingLabel");
    }
}
