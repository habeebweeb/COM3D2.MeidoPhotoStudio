using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class AnimationPane : BasePane
{
    private const string PlayBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAQElEQVQ4y2NgGF7g////R/7//+9CiQEwQJ5B/zEBaQb9xw2IM+g/YYBhEBO1Y4HqXiA7EMmOxgP///+3ZxhZAAAgvb6MbsgO0gAAAABJRU5ErkJggg==";
    private const string PauseBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAJklEQVQ4y2NgGPKAEV3g/////+GSjIyMuMRggIlSF4waMGrAMAEAsQQIFCtKuVwAAAAASUVORK5CYII=";
    private const string StepBackwardBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAXUlEQVQ4y93RsQmAQAxA0UN0AMdwD/dxGgtncRR3cALh2YYr5DjOQn+X4n9IktK/wYAtzKBUHrFHoTiACUcuFAUw4xQoDmDBJeMp0L1x+foVmhyxyRuD0GOtDnyDG80KxesR6EkbAAAAAElFTkSuQmCC";
    private const string StepForwardBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAU0lEQVQ4y93RMQ2AQBBE0QsBAcjAB35ODQVakIIHFJA8GorrB5r73Rbzk50ppS+8NPeOKRHAgTkRwIklEcCFNRHAjdpmhr9X+OyFqMRoxg1j6ZsHiw3F6zNQ9OgAAAAASUVORK5CYII=";

    private static GUIContent playContent;
    private static GUIContent pauseContent;

    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly Slider animationSlider;
    private readonly Toggle playPauseButton;
    private readonly RepeatButton stepLeftButton;
    private readonly RepeatButton stepRightButton;
    private readonly Label stepAmountLabel;
    private readonly NumericalTextField stepAmountField;
    private readonly RepeatButton decreaseStepAmountButton;
    private readonly RepeatButton increaseStepAmountButton;
    private readonly Button resetStepAmountButton;

    public AnimationPane(
        Translation translation,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        animationSlider = new Slider(0f, 1f)
        {
            HasTextField = true,
        };

        animationSlider.ControlEvent += OnAnimationSliderChange;
        animationSlider.StartedInteraction += OnAnimationSliderInteractionStarted;
        animationSlider.EndedInteraction += OnAnimationSliderInteractionEnded;

        playPauseButton = new(PauseIcon, true);
        playPauseButton.ControlEvent += OnPlayPauseButtonPushed;

        stepLeftButton = new(new GUIContent(UIUtility.LoadTextureFromBase64(16, 16, StepBackwardBase64)), 3f);
        stepLeftButton.ControlEvent += OnStepLeftButtonPushed;

        stepRightButton = new(new GUIContent(UIUtility.LoadTextureFromBase64(16, 16, StepForwardBase64)), 3f);
        stepRightButton.ControlEvent += OnStepRightButtonPushed;

        stepAmountLabel = new(new LocalizableGUIContent(translation, "characterAnimationPane", "stepAmountLabel"));

        stepAmountField = new(0.01f);
        stepAmountField.ControlEvent += OnStepAmountFieldChanged;

        decreaseStepAmountButton = new(Symbols.Minus, 3f);
        decreaseStepAmountButton.ControlEvent += OnDecreaseStepAmountButtonPushed;

        increaseStepAmountButton = new(Symbols.Plus, 3f);
        increaseStepAmountButton.ControlEvent += OnIncreaseStepAmountButtonPushed;

        resetStepAmountButton = new("|");
        resetStepAmountButton.ControlEvent += OnResetStepAmountButtonPushed;
    }

    private static GUIContent PlayIcon =>
        playContent ??= new(UIUtility.LoadTextureFromBase64(16, 16, PlayBase64));

    private static GUIContent PauseIcon =>
        pauseContent ??= new(UIUtility.LoadTextureFromBase64(16, 16, PauseBase64));

    private CharacterUndoRedoController CharacterUndoRedo =>
        Character is null ? null : characterUndoRedoService[Character];

    private CharacterController Character =>
        characterSelectionController.Current;

    private AnimationController CurrentAnimation =>
        characterSelectionController.Current?.Animation;

    private bool Playing =>
        playPauseButton.Value;

    public override void Draw()
    {
        var currentAnimation = CurrentAnimation;
        var guiEnabled = Parent.Enabled && currentAnimation is not null;

        GUI.enabled = guiEnabled;

        if (currentAnimation is not null)
        {
            if (currentAnimation.Playing)
            {
                animationSlider.SetValueWithoutNotify(currentAnimation.Time);
            }
            else
            {
                if (Playing)
                {
                    playPauseButton.SetEnabledWithoutNotify(false);
                    UpdatePlayPauseButtonIcon();
                    animationSlider.SetValueWithoutNotify(currentAnimation.Time);
                }
            }
        }

        var animationStopped = !currentAnimation?.Playing ?? false;
        var animationValid = currentAnimation?.Length is > 0f;

        GUI.enabled = guiEnabled && animationStopped && animationValid;

        animationSlider.Draw();

        GUI.enabled = guiEnabled;

        GUILayout.BeginHorizontal();

        var height = GUILayout.Height(Mathf.Max(21, UIUtility.Scaled(StyleSheet.TextSize) + 10));
        var noExpandWidth = GUILayout.ExpandWidth(false);

        playPauseButton.Draw(Symbols.IconButtonStyle, GUILayout.Width(UIUtility.Scaled(45)), height);

        GUI.enabled = guiEnabled && animationValid && animationStopped;

        stepLeftButton.Draw(noExpandWidth, height);
        stepRightButton.Draw(noExpandWidth, height);

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        var buttonSize = GUILayout.Width(UIUtility.Scaled(25));

        stepAmountLabel.Draw(noExpandWidth);

        decreaseStepAmountButton.Draw(Symbols.IconButtonStyle, buttonSize, height);
        increaseStepAmountButton.Draw(Symbols.IconButtonStyle, buttonSize, height);

        stepAmountField.Draw(GUILayout.Width(UIUtility.Scaled(65)), height);

        resetStepAmountButton.Draw(noExpandWidth, height);

        GUILayout.EndHorizontal();

        GUI.enabled = guiEnabled;
    }

    public override void UpdatePane()
    {
        if (CurrentAnimation is null)
            return;

        playPauseButton.SetEnabledWithoutNotify(CurrentAnimation.Playing);

        UpdatePlayPauseButtonIcon();

        animationSlider.SetValueWithoutNotify(CurrentAnimation.Time);
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentAnimation is null)
            return;

        CurrentAnimation.PropertyChanged -= OnAnimationpropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (CurrentAnimation is null)
            return;

        CurrentAnimation.PropertyChanged += OnAnimationpropertyChanged;

        UpdateSliderRange();

        UpdatePane();
    }

    private void OnAnimationpropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var animation = (AnimationController)sender;

        if (e.PropertyName is nameof(AnimationController.Playing))
        {
            playPauseButton.SetEnabledWithoutNotify(animation.Playing);

            UpdatePlayPauseButtonIcon();
        }
        else if (e.PropertyName is nameof(AnimationController.Time))
        {
            animationSlider.SetValueWithoutNotify(animation.Time);
        }
        else if (e.PropertyName is nameof(AnimationController.Animation))
        {
            UpdateSliderRange();
            UpdatePane();
        }
    }

    private void UpdateSliderRange()
    {
        if (CurrentAnimation is null)
            return;

        var length = CurrentAnimation.Length - 0.0001f;

        if (length < 0f)
            length = 0f;

        animationSlider.SetRightBoundWithoutNotify(length);
    }

    private void OnPlayPauseButtonPushed(object sender, EventArgs e)
    {
        if (CurrentAnimation is null)
            return;

        UpdatePlayPauseButtonIcon();

        if (Character is CharacterController character)
        {
            if (character.IK.Dirty && !character.Animation.Playing)
            {
                CharacterUndoRedo.StartPoseChange();
                character.Animation.Playing = Playing;
                CharacterUndoRedo.EndPoseChange();
            }
            else
            {
                character.Animation.Playing = Playing;
            }
        }

        UpdatePane();
    }

    private void OnAnimationSliderInteractionStarted(object sender, EventArgs e)
    {
        if (CurrentAnimation is null)
            return;

        if (Character.IK.Dirty)
            CharacterUndoRedo.StartPoseChange();
    }

    private void OnAnimationSliderChange(object sender, EventArgs e)
    {
        if (CurrentAnimation is null)
            return;

        CurrentAnimation.Time = animationSlider.Value;
    }

    private void OnAnimationSliderInteractionEnded(object sender, EventArgs e)
    {
        if (CurrentAnimation is null)
            return;

        CharacterUndoRedo.EndPoseChange();
    }

    private void OnStepRightButtonPushed(object sender, EventArgs e)
    {
        if (Character.IK.Dirty)
            CharacterUndoRedo.StartPoseChange();

        animationSlider.Value += stepAmountField.Value;

        CharacterUndoRedo.EndPoseChange();
    }

    private void OnStepLeftButtonPushed(object sender, EventArgs e)
    {
        if (Character.IK.Dirty)
            CharacterUndoRedo.StartPoseChange();

        animationSlider.Value -= stepAmountField.Value;

        CharacterUndoRedo.EndPoseChange();
    }

    private void OnStepAmountFieldChanged(object sender, EventArgs e)
    {
        if (stepAmountField.Value < 0f)
            stepAmountField.SetValueWithoutNotify(0f);
    }

    private void OnDecreaseStepAmountButtonPushed(object sender, EventArgs e) =>
        stepAmountField.Value -= 0.01f;

    private void OnIncreaseStepAmountButtonPushed(object sender, EventArgs e) =>
        stepAmountField.Value += 0.01f;

    private void OnResetStepAmountButtonPushed(object sender, EventArgs e) =>
        stepAmountField.Value = 0.01f;

    private void UpdatePlayPauseButtonIcon() =>
        playPauseButton.Content = Playing ? PauseIcon : PlayIcon;
}
