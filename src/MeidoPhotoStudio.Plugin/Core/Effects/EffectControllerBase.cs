using System.ComponentModel;

namespace MeidoPhotoStudio.Plugin.Core.Effects;

public abstract class EffectControllerBase : INotifyPropertyChanged, IActivateable
{
    private bool active;

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual bool Active
    {
        get => active;
        set
        {
            active = value;

            RaisePropertyChanged(nameof(Active));
        }
    }

    public abstract void Reset();

    void IActivateable.Activate() =>
        Activate();

    void IActivateable.Deactivate() =>
        Deactivate();

    protected virtual void Activate() =>
        Active = false;

    protected virtual void Deactivate() =>
        Active = false;

    protected void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
