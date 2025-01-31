using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class FreeLookPane : BasePane
{
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Toggle freeLookToggle;
    private readonly Slider offsetLookXSlider;
    private readonly Slider offsetLookYSlider;
    private readonly Toggle eyeToCameraToggle;
    private readonly Toggle headToCameraToggle;
    private readonly Label bindLabel;

    public FreeLookPane(Translation translation, SelectionController<CharacterController> characterSelectionController)
    {
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        freeLookToggle = new(new LocalizableGUIContent(translation, "freeLookPane", "freeLookToggle"), false);
        freeLookToggle.ControlEvent += OnFreeLookToggleChanged;

        offsetLookXSlider = new(new LocalizableGUIContent(translation, "freeLookPane", "xSlider"), -0.6f, 0.6f)
        {
            HasReset = true,
        };

        offsetLookXSlider.ControlEvent += OnOffsetLookSlidersChanged;

        offsetLookYSlider = new(new LocalizableGUIContent(translation, "freeLookPane", "ySlider"), 0.5f, -0.55f)
        {
            HasReset = true,
        };

        offsetLookYSlider.ControlEvent += OnOffsetLookSlidersChanged;

        eyeToCameraToggle = new(new LocalizableGUIContent(translation, "freeLookPane", "eyeToCamToggle"), true);
        eyeToCameraToggle.ControlEvent += OnBindEyeToggleChanged;

        headToCameraToggle = new(new LocalizableGUIContent(translation, "freeLookPane", "headToCamToggle"), true);
        headToCameraToggle.ControlEvent += OnBindHeadToggleChanged;

        bindLabel = new(new LocalizableGUIContent(translation, "freeLookPane", "bindLabel"));
    }

    private HeadController CurrentHead =>
        characterSelectionController.Current?.Head;

    public override void Draw()
    {
        var enabled = Parent.Enabled && CurrentHead is not null;

        GUI.enabled = enabled;

        var eitherBindingEnabled = eyeToCameraToggle.Value || headToCameraToggle.Value;

        GUI.enabled = enabled && eitherBindingEnabled;

        freeLookToggle.Draw();

        GUI.enabled = enabled && eitherBindingEnabled && freeLookToggle.Value;

        GUILayout.BeginHorizontal(GUILayout.MaxWidth(Parent.WindowRect.width - 10f));

        offsetLookXSlider.Draw();
        offsetLookYSlider.Draw();

        GUILayout.EndHorizontal();

        GUI.enabled = enabled;

        GUILayout.BeginHorizontal();

        bindLabel.Draw(GUILayout.ExpandWidth(false));

        eyeToCameraToggle.Draw();
        headToCameraToggle.Draw();

        GUILayout.EndHorizontal();
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Head.PropertyChanged -= OnHeadPropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Head.PropertyChanged += OnHeadPropertyChanged;

        freeLookToggle.SetEnabledWithoutNotify(CurrentHead.FreeLook);
        offsetLookXSlider.SetValueWithoutNotify(CurrentHead.OffsetLookTarget.x);
        offsetLookYSlider.SetValueWithoutNotify(CurrentHead.OffsetLookTarget.y);
        eyeToCameraToggle.SetEnabledWithoutNotify(CurrentHead.EyeToCamera);
        headToCameraToggle.SetEnabledWithoutNotify(CurrentHead.HeadToCamera);
    }

    private void OnHeadPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var head = (HeadController)sender;

        if (e.PropertyName is nameof(HeadController.FreeLook))
        {
            freeLookToggle.SetEnabledWithoutNotify(head.FreeLook);
        }
        else if (e.PropertyName is nameof(HeadController.OffsetLookTarget))
        {
            offsetLookXSlider.SetValueWithoutNotify(head.OffsetLookTarget.x);
            offsetLookYSlider.SetValueWithoutNotify(head.OffsetLookTarget.y);
        }
        else if (e.PropertyName is nameof(HeadController.EyeToCamera))
        {
            eyeToCameraToggle.SetEnabledWithoutNotify(head.EyeToCamera);
        }
        else if (e.PropertyName is nameof(HeadController.HeadToCamera))
        {
            headToCameraToggle.SetEnabledWithoutNotify(head.HeadToCamera);
        }
    }

    private void OnFreeLookToggleChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.FreeLook = freeLookToggle.Value;
    }

    private void OnOffsetLookSlidersChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.OffsetLookTarget = new(offsetLookXSlider.Value, offsetLookYSlider.Value);
    }

    private void OnBindEyeToggleChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.EyeToCamera = eyeToCameraToggle.Value;

        freeLookToggle.SetEnabledWithoutNotify(CurrentHead.FreeLook);
    }

    private void OnBindHeadToggleChanged(object sender, EventArgs e)
    {
        if (CurrentHead is null)
            return;

        CurrentHead.HeadToCamera = headToCameraToggle.Value;

        freeLookToggle.SetEnabledWithoutNotify(CurrentHead.FreeLook);
    }
}
