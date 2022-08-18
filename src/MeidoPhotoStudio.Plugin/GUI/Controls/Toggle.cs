using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class Toggle : BaseControl
{
    private bool value;

    public Toggle(string label, bool state = false)
    {
        Label = label;
        value = state;
    }

    public string Label { get; set; }

    public bool Value
    {
        get => value;
        set
        {
            this.value = value;

            OnControlEvent(EventArgs.Empty);
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(new(GUI.skin.toggle), layoutOptions);

    public void Draw(GUIStyle toggleStyle, params GUILayoutOption[] layoutOptions)
    {
        var value = GUILayout.Toggle(Value, Label, toggleStyle, layoutOptions);

        if (value != Value)
            Value = value;
    }

    public void Draw(Rect rect)
    {
        var value = GUI.Toggle(rect, Value, Label);

        if (value != Value)
            Value = value;
    }
}
