namespace MeidoPhotoStudio.Plugin.Core.Character;

public class ClothingGravityController(CharacterController characterController, TransformWatcher transformWatcher)
    : GravityController(characterController, transformWatcher)
{
    protected override string TypeName =>
        "Clothing";

    protected override void InitializeTransformControl(GravityTransformControl control)
    {
        control.forceRate = 0.1f;

        control.SetTargetSlods(character.Maid.body0.goSlot
            .Where(static slot => slot.obj)
            .Where(static slot => slot.obj.GetComponent<DynamicSkirtBone>())
            .Select(static slot => slot.SlotId)
            .ToArray());
    }
}
