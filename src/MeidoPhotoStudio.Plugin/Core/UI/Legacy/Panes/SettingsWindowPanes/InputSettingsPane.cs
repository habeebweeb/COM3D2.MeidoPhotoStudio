using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Input;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class InputSettingsPane : BasePane
{
    private readonly Dictionary<Hotkey, string> hotkeyMapping;
    private readonly Dictionary<Hotkey, string> hotkeyName;
    private readonly InputConfiguration inputConfiguration;
    private readonly InputRemapper inputRemapper;
    private readonly Dictionary<Shortcut, string> shortcutMapping;
    private readonly Dictionary<Shortcut, string> shortcutName;
    private readonly PaneHeader paneHeader;
    private readonly PaneHeader generalControlsHeader;
    private readonly PaneHeader cameraControlsHeader;
    private readonly PaneHeader transformDragHandleControlsHeader;
    private readonly PaneHeader characterControlsHeader;
    private readonly LazyStyle labelStyle = new(13, () => new(GUI.skin.label));
    private readonly LazyStyle leftMargin = new(
        0,
        () => new()
        {
            margin = new(8, 0, 0, 0),
        });

    private readonly LazyStyle buttonStyle = new(
        13,
        () => new(GUI.skin.button)
        {
            wordWrap = true,
        });

    private Hotkey currentHotkey;
    private Shortcut currentShortcut;
    private bool listeningToShortcut;
    private string cancelRebindLabel = "Cancel";
    private string pushAnyKeyLabel = "Push any key combo";

    public InputSettingsPane(InputConfiguration inputConfiguration, InputRemapper inputRemapper)
    {
        this.inputConfiguration = inputConfiguration ?? throw new ArgumentNullException(nameof(inputConfiguration));
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));

        paneHeader = new(Translation.Get("inputSettingsPane", "header"));

        generalControlsHeader = new(Translation.Get("inputSettingsPane", "generalControlsHeader"));
        cameraControlsHeader = new(Translation.Get("inputSettingsPane", "cameraControlsHeader"));
        transformDragHandleControlsHeader = new(Translation.Get("inputSettingsPane", "transformDragHandleControlsHeader"));
        characterControlsHeader = new(Translation.Get("inputSettingsPane", "characterControlsHeader"));

        var shortcutValues = (Shortcut[])Enum.GetValues(typeof(Shortcut));

        shortcutMapping = shortcutValues
            .ToDictionary(
                shortcut => shortcut,
                shortcut => inputConfiguration[shortcut].ToString(),
                EnumEqualityComparer<Shortcut>.Instance);

        shortcutName = shortcutValues
            .ToDictionary(
                shortcut => shortcut,
                shortcut => Translation.Get("controls", shortcut.ToLower()),
                EnumEqualityComparer<Shortcut>.Instance);

        var hotkeyValues = (Hotkey[])Enum.GetValues(typeof(Hotkey));

        hotkeyMapping = hotkeyValues
            .ToDictionary(
                hotkey => hotkey,
                hotkey => inputConfiguration[hotkey].ToString(),
                EnumEqualityComparer<Hotkey>.Instance);

        hotkeyName = hotkeyValues
            .ToDictionary(
                hotkey => hotkey,
                hotkey => Translation.Get("controls", hotkey.ToLower()),
                EnumEqualityComparer<Hotkey>.Instance);

        pushAnyKeyLabel = Translation.Get("inputSettingsPane", "pushAnyKeyLabel");
        cancelRebindLabel = Translation.Get("inputSettingsPane", "cancelRebindLabel");
    }

    public override void Draw()
    {
        GUI.enabled = !inputRemapper.Listening;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUILayout.BeginVertical(leftMargin);

        DrawGeneralControls();
        DrawCameraControls();
        DrawTransformDragHandleControls();
        DrawCharacterDragHandleControls();

        GUILayout.EndVertical();

        GUI.enabled = true;

        void DrawGeneralControls()
        {
            generalControlsHeader.Draw();

            if (!generalControlsHeader.Enabled)
                return;

            for (var shortcut = Shortcut.ActivatePlugin; shortcut <= Shortcut.CyclePreviousAnimation; shortcut++)
                DrawControl(shortcut);
        }

        void DrawCameraControls()
        {
            cameraControlsHeader.Draw();

            if (!cameraControlsHeader.Enabled)
                return;

            for (var shortcut = Shortcut.SaveCamera; shortcut <= Shortcut.ToggleCamera5; shortcut++)
                DrawControl(shortcut);

            for (var hotkey = Hotkey.FastCamera; hotkey <= Hotkey.SlowCamera; hotkey++)
                DrawControl(hotkey);
        }

        void DrawTransformDragHandleControls()
        {
            transformDragHandleControlsHeader.Draw();

            if (!transformDragHandleControlsHeader.Enabled)
                return;

            for (var hotkey = Hotkey.Select; hotkey <= Hotkey.Scale; hotkey++)
                DrawControl(hotkey);
        }

        void DrawCharacterDragHandleControls()
        {
            characterControlsHeader.Draw();

            if (!characterControlsHeader.Enabled)
                return;

            for (var hotkey = Hotkey.DragFinger; hotkey <= Hotkey.MoveLocalY; hotkey++)
                DrawControl(hotkey);
        }

        void DrawControl(Enum key)
        {
            var isShortcut = key.GetType() == typeof(Shortcut);

            DrawShortcutLabel(key, isShortcut);

            GUILayout.BeginHorizontal();

            var buttonWidth = GUILayout.MaxWidth(parent.WindowRect.width - 45f);

            if (CurrentControlIsListening(key, isShortcut))
            {
                GUILayout.Button(pushAnyKeyLabel, buttonStyle, buttonWidth);

                DrawCancelListeningButton();
            }
            else if (DrawControlButton(key, isShortcut))
            {
                ListenForNewKeyCombo(key, isShortcut);
            }

            if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                ClearButtonCombo(key, isShortcut);

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

                return GUILayout.Button(mapping, buttonStyle, buttonWidth);
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

            void SetCombo(Enum key, bool isShortcut, KeyboardInput input)
            {
                if (isShortcut)
                {
                    var shortcut = (KeyboardShortcut)input;

                    inputConfiguration[(Shortcut)key] = shortcut;
                    shortcutMapping[(Shortcut)key] = shortcut.ToString();
                }
                else
                {
                    var hotkey = (KeyboardHotkey)input;

                    inputConfiguration[(Hotkey)key] = hotkey;
                    hotkeyMapping[(Hotkey)key] = hotkey.ToString();
                }
            }

            void DrawCancelListeningButton()
            {
                GUI.enabled = true;

                if (GUILayout.Button(cancelRebindLabel))
                    inputRemapper.Cancel();

                GUI.enabled = false;
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
            shortcutName[shortcut] = Translation.Get("controls", shortcut.ToLower());

        foreach (var hotkey in (Hotkey[])Enum.GetValues(typeof(Hotkey)))
            hotkeyName[hotkey] = Translation.Get("controls", hotkey.ToLower());

        generalControlsHeader.Label = Translation.Get("inputSettingsPane", "generalControlsHeader");
        cameraControlsHeader.Label = Translation.Get("inputSettingsPane", "cameraControlsHeader");
        transformDragHandleControlsHeader.Label = Translation.Get("inputSettingsPane", "transformDragHandleControlsHeader");
        characterControlsHeader.Label = Translation.Get("inputSettingsPane", "characterControlsHeader");

        pushAnyKeyLabel = Translation.Get("inputSettingsPane", "pushAnyKeyLabel");
        cancelRebindLabel = Translation.Get("inputSettingsPane", "cancelRebindLabel");
    }
}
