using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CopyPosePane : BasePane
{
    private readonly Dropdown<CharacterController> otherCharacterDropdown;
    private readonly Button copyPoseButton;
    private readonly Button copyBothHandsButton;
    private readonly Button copyLeftHandToLeftButton;
    private readonly Button copyLeftHandToRightButton;
    private readonly Button copyRightHandToLeftButton;
    private readonly Button copyRightHandToRightButton;
    private readonly CharacterService characterService;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Header copyHandHeader;

    public CopyPosePane(
        Translation translation,
        CharacterService characterService,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;

        otherCharacterDropdown = new(formatter: OtherCharacterFormatter);

        copyPoseButton = new(new LocalizableGUIContent(translation, "copyPosePane", "copyButton"));
        copyPoseButton.ControlEvent += OnCopyPoseButtonPushed;

        copyBothHandsButton = new(new LocalizableGUIContent(translation, "copyPosePane", "copyBothHands"));
        copyBothHandsButton.ControlEvent += OnCopyBothHandsButtonPushed;

        copyLeftHandToLeftButton = new(new LocalizableGUIContent(translation, "copyPosePane", "copyLeftHandToLeft"));
        copyLeftHandToLeftButton.ControlEvent += OnCopyLefHandToLeftButtonPushed;

        copyLeftHandToRightButton = new(new LocalizableGUIContent(translation, "copyPosePane", "copyLeftHandToRight"));
        copyLeftHandToRightButton.ControlEvent += OnCopyLefHandToRightButtonPushed;

        copyRightHandToLeftButton = new(new LocalizableGUIContent(translation, "copyPosePane", "copyRightHandToLeft"));
        copyRightHandToLeftButton.ControlEvent += OnCopyRightHandToLeftButtonPushed;

        copyRightHandToRightButton = new(new LocalizableGUIContent(translation, "copyPosePane", "copyRightHandToRight"));
        copyRightHandToRightButton.ControlEvent += OnCopyRightHandToRightButtonPushed;

        copyHandHeader = new(new LocalizableGUIContent(translation, "copyPosePane", "copyHandHeader"));

        static LabelledDropdownItem OtherCharacterFormatter(CharacterController character, int index) =>
            new($"{character.Slot + 1}: {character.CharacterModel.FullName()}");
    }

    private CharacterController OtherCharacter =>
        characterService.Count > 0
            ? characterService[otherCharacterDropdown.SelectedItemIndex]
            : null;

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled && CurrentCharacter is not null;

        DrawDropdown(otherCharacterDropdown);

        if (CurrentCharacter != OtherCharacter)
        {
            UIUtility.DrawBlackLine();

            copyPoseButton.Draw();

            UIUtility.DrawBlackLine();
        }

        copyHandHeader.Draw();

        if (CurrentCharacter != OtherCharacter)
        {
            copyBothHandsButton.Draw();

            GUILayout.BeginHorizontal();

            copyRightHandToRightButton.Draw();
            copyRightHandToLeftButton.Draw();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            copyLeftHandToRightButton.Draw();
            copyLeftHandToLeftButton.Draw();

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();

            copyRightHandToLeftButton.Draw();
            copyLeftHandToRightButton.Draw();

            GUILayout.EndHorizontal();
        }
    }

    private void OnCharactersCalled(object sender, EventArgs e) =>
        otherCharacterDropdown.SetItemsWithoutNotify(characterService, 0);

    private void OnCopyPoseButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        characterUndoRedoService[CurrentCharacter].StartPoseChange();
        CurrentCharacter.IK.CopyPoseFrom(OtherCharacter);
        characterUndoRedoService[CurrentCharacter].EndPoseChange();
    }

    private void OnCopyBothHandsButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        characterUndoRedoService[CurrentCharacter].StartPoseChange();
        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandRight, HandOrFootType.HandRight);
        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandLeft, HandOrFootType.HandLeft);
        characterUndoRedoService[CurrentCharacter].EndPoseChange();
    }

    private void OnCopyRightHandToRightButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandRight, HandOrFootType.HandRight);

    private void OnCopyRightHandToLeftButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandRight, HandOrFootType.HandLeft);

    private void OnCopyLefHandToLeftButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandLeft, HandOrFootType.HandLeft);

    private void OnCopyLefHandToRightButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandLeft, HandOrFootType.HandRight);

    private void CopyHands(HandOrFootType copyFrom, HandOrFootType copyTo)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        characterUndoRedoService[CurrentCharacter].StartPoseChange();
        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, copyFrom, copyTo);
        characterUndoRedoService[CurrentCharacter].EndPoseChange();
    }
}
