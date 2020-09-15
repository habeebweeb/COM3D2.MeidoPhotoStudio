using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class KeyRebindButton : BaseControl
    {
        private Button button;
        private bool listening = false;
        private KeyCode keyCode;
        public KeyCode KeyCode
        {
            get => keyCode;
            set
            {
                keyCode = value;
                button.Label = keyCode.ToString();
            }
        }
        public KeyRebindButton(KeyCode code)
        {
            button = new Button(code.ToString());
            button.ControlEvent += (s, a) => StartListening();
        }

        public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
        {
            GUI.enabled = !listening && !InputManager.Listening;
            button.Draw(buttonStyle, layoutOptions);
            GUI.enabled = true;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            Draw(buttonStyle, layoutOptions);
        }

        private void StartListening()
        {
            listening = true;
            InputManager.StartListening();
            InputManager.KeyChange += KeyChange;
        }

        private void KeyChange(object sender, EventArgs args)
        {
            listening = false;
            KeyCode = InputManager.CurrentKeyCode;
            InputManager.KeyChange -= KeyChange;
            OnControlEvent(EventArgs.Empty);
        }
    }
}
