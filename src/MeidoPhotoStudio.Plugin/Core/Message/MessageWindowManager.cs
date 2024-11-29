using System.ComponentModel;

using Alignment = NGUIText.Alignment;

namespace MeidoPhotoStudio.Plugin.Core.Message;

public class MessageWindowManager : INotifyPropertyChanged, IActivateable
{
    public const string Header = "TEXTBOX";

    public static readonly (int Minimum, int Maximum) FontBounds = (25, 60);

    private readonly MessageWindowMgr messageWindowMgr;
    private readonly GameObject subtitlesDisplayPanel;
    private readonly GameObject hitRetSprite;
    private readonly GameObject messageBox;
    private readonly GameObject messageButtons;
    private readonly UILabel messageLabel;
    private readonly UILabel speakerLabel;

    public MessageWindowManager()
    {
        messageWindowMgr = GameMain.Instance.MsgWnd;

        var messageWindowPanel = messageWindowMgr.m_goMessageWindowPanel;
        var msgParent = UTY.GetChildObject(messageWindowPanel, "MessageViewer/MsgParent");

        messageButtons = UTY.GetChildObject(msgParent, "Buttons");
        hitRetSprite = UTY.GetChildObject(msgParent, "Hitret");
        subtitlesDisplayPanel = UTY.GetChildObject(msgParent, "SubtitlesDisplayPanel");

        messageBox = UTY.GetChildObject(msgParent, "MessageBox");
        speakerLabel = UTY.GetChildObject(msgParent, "SpeakerName/Name").GetComponent<UILabel>();
        messageLabel = UTY.GetChildObject(msgParent, "Message").GetComponent<UILabel>();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool ShowingMessage
    {
        get => messageWindowMgr.IsVisibleMessageViewer;
        private set
        {
            if (value == ShowingMessage)
                return;

            if (value)
                messageWindowMgr.OpenMessageWindowPanel();
            else
                messageWindowMgr.CloseMessageWindowPanel();

            RaisePropertyChanged(nameof(ShowingMessage));
        }
    }

    public string MessageName
    {
        get => speakerLabel.text;
        private set
        {
            if (string.Equals(MessageName, value, StringComparison.CurrentCulture))
                return;

            speakerLabel.text = value;

            RaisePropertyChanged(nameof(MessageName));
        }
    }

    public string MessageText
    {
        get => messageLabel.text;
        private set
        {
            if (string.Equals(MessageText, value, StringComparison.CurrentCulture))
                return;

            messageLabel.text = value;

            RaisePropertyChanged(nameof(MessageText));
        }
    }

    public int FontSize
    {
        get => messageLabel.fontSize;
        set
        {
            var newFontSize = Mathf.Clamp(value, FontBounds.Minimum, FontBounds.Minimum);

            if (newFontSize == FontSize)
                return;

            messageLabel.fontSize = newFontSize;

            RaisePropertyChanged(nameof(FontSize));
        }
    }

    public Alignment MessageAlignment
    {
        get => messageLabel.alignment;
        set
        {
            if (MessageAlignment == value)
                return;

            messageLabel.alignment = value;

            RaisePropertyChanged(nameof(MessageAlignment));
        }
    }

    public void ShowMessage(string name, string message)
    {
        MessageName = name;
        MessageText = message;
        ShowingMessage = true;
    }

    public void CloseMessagePanel()
    {
        if (!ShowingMessage)
            return;

        ShowingMessage = false;
    }

    void IActivateable.Activate()
    {
        if (Product.supportMultiLanguage)
            subtitlesDisplayPanel.SetActive(false);

        ResetMessageBoxProperties();

        SetMessageBoxActive(true);

        SetMessageBoxExtrasActive(false);

        CloseMessagePanel();
    }

    void IActivateable.Deactivate()
    {
        if (Product.supportMultiLanguage)
        {
            subtitlesDisplayPanel.SetActive(true);

            SetMessageBoxActive(false);
        }

        ResetMessageBoxProperties();

        SetMessageBoxExtrasActive(true);

        CloseMessagePanel();
    }

    private void SetMessageBoxActive(bool active)
    {
        messageBox.SetActive(active);
        messageLabel.gameObject.SetActive(active);
        speakerLabel.gameObject.SetActive(active);
    }

    private void SetMessageBoxExtrasActive(bool active)
    {
        messageButtons.SetActive(active);
        hitRetSprite.SetActive(active);
    }

    private void ResetMessageBoxProperties()
    {
        FontSize = 25;
        MessageAlignment = Alignment.Left;
        MessageName = string.Empty;
        MessageText = string.Empty;
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
