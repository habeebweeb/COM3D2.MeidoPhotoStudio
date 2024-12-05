using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class HairGravityController(CharacterController characterController, TransformWatcher transformWatcher)
    : GravityController(characterController, transformWatcher)
{
    protected override string TypeName =>
        "Hair";

    protected override void InitializeTransformControl(GravityTransformControl control)
    {
        control.forceRate = 0.1f;

        control.SetTargetSlods(character.Maid.body0.goSlot
            .Where(static slot => slot.obj)
            .Where(static slot => slot.obj.GetComponent<DynamicBone>())
            .Select(static slot => slot.SlotId)
            .ToArray());
    }
}
