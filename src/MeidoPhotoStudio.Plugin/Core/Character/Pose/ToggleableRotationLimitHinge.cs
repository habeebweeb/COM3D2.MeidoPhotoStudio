using RootMotion.FinalIK;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ToggleableRotationLimitHinge : RotationLimitHinge
{
    public bool Limited { get; set; }

    internal AnimationController Animation { get; set; }

    public override Quaternion LimitRotation(Quaternion rotation) =>
        Limited && !Animation.Playing ? base.LimitRotation(rotation) : rotation;
}
