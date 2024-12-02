namespace MeidoPhotoStudio.Plugin.Core.Character;

public class GravityDragHandleSet
{
    public GravityDragHandleController HairDragHandle { get; init; }

    public GravityDragHandleController ClothingDragHandle { get; init; }

    public void Deconstruct(out GravityDragHandleController hairDragHandle, out GravityDragHandleController clothingDragHandle) =>
        (hairDragHandle, clothingDragHandle) = (HairDragHandle, ClothingDragHandle);
}
