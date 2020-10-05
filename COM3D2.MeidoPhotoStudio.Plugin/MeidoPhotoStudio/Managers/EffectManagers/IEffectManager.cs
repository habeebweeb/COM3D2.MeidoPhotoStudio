namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public interface IEffectManager : IManager, ISerializable
    {
        bool Ready { get; }
        bool Active { get; }
        void SetEffectActive(bool active);
        void Reset();
    }
}
