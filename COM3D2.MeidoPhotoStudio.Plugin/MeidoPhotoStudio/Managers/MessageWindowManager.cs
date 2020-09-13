using System.Reflection;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MessageWindowManager : IManager, ISerializable
    {
        public const string header = "TEXTBOX";
        public static readonly SliderProp fontBounds = new SliderProp(25f, 60f);
        private static GameObject sysRoot;
        private MessageClass msgClass;
        private MessageWindowMgr msgWnd;
        private UILabel msgLabel;
        private UILabel nameLabel;
        private GameObject msgGameObject;
        public bool ShowingMessage { get; private set; }
        private string messageName;
        private string messageText;

        public MessageWindowManager()
        {
            InputManager.Register(MpsKey.ToggleMessage, KeyCode.M);
            sysRoot = GameObject.Find("__GameMain__/SystemUI Root");
            this.msgWnd = GameMain.Instance.MsgWnd;
            this.msgGameObject = sysRoot.transform.Find("MessageWindowPanel").gameObject;
            this.msgClass = new MessageClass(this.msgGameObject, this.msgWnd);
            this.nameLabel = UTY.GetChildObject(this.msgGameObject, "MessageViewer/MsgParent/SpeakerName/Name", false)
                .GetComponent<UILabel>();
            this.msgLabel = UTY.GetChildObject(this.msgGameObject, "MessageViewer/MsgParent/Message", false)
                .GetComponent<UILabel>();
            Utility.SetFieldValue<MessageClass, UILabel>(this.msgClass, "message_label_", this.msgLabel);
            Utility.SetFieldValue<MessageClass, UILabel>(this.msgClass, "name_label_", this.nameLabel);
        }

        public void Activate()
        {
            SetPhotoMessageWindowActive(true);
        }

        public void Deactivate()
        {
            this.msgWnd.CloseMessageWindowPanel();
            SetPhotoMessageWindowActive(false);
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(ShowingMessage);
            binaryWriter.Write(this.msgLabel.fontSize);
            binaryWriter.WriteNullableString(messageName);
            binaryWriter.WriteNullableString(messageText);
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            CloseMessagePanel();
            bool showingMessage = binaryReader.ReadBoolean();
            this.msgLabel.fontSize = binaryReader.ReadInt32();
            messageName = binaryReader.ReadNullableString();
            messageText = binaryReader.ReadNullableString();
            if (showingMessage) ShowMessage(messageName, messageText);
        }

        public void Update() { }

        private void SetPhotoMessageWindowActive(bool active)
        {
            UTY.GetChildObject(this.msgGameObject, "MessageViewer/MsgParent/MessageBox", false)
                .SetActive(active);
            UTY.GetChildObject(this.msgGameObject, "MessageViewer/MsgParent/Hitret", false)
                .GetComponent<UISprite>().enabled = !active;
            this.nameLabel.gameObject.SetActive(active);
            this.msgLabel.gameObject.SetActive(active);

            Transform transform = sysRoot.transform.Find("MessageWindowPanel/MessageViewer/MsgParent/Buttons");
            MessageWindowMgr.MessageWindowUnderButton[] msgButtons = new[]
            {
                MessageWindowMgr.MessageWindowUnderButton.Skip,
                MessageWindowMgr.MessageWindowUnderButton.Auto,
                MessageWindowMgr.MessageWindowUnderButton.Voice,
                MessageWindowMgr.MessageWindowUnderButton.BackLog,
                MessageWindowMgr.MessageWindowUnderButton.Config
            };
            foreach (var msgButton in msgButtons)
            {
                transform.Find(msgButton.ToString()).gameObject.SetActive(!active);
            }
            if (this.msgClass.subtitles_manager_ != null)
            {
                this.msgClass.subtitles_manager_.visible = false;
                this.msgClass.subtitles_manager_ = null;
            }
        }

        public void ShowMessage(string name, string message)
        {
            messageName = name;
            messageText = message;
            ShowingMessage = true;
            this.msgWnd.OpenMessageWindowPanel();
            this.msgLabel.ProcessText();
            this.msgClass.SetText(name, message, "", 0, AudioSourceMgr.Type.System);
            this.msgClass.FinishChAnime();
        }

        public void SetFontSize(int fontSize)
        {
            this.msgLabel.fontSize = fontSize;
        }

        public void CloseMessagePanel()
        {
            ShowingMessage = false;
            this.msgWnd.CloseMessageWindowPanel();
        }
    }
}
