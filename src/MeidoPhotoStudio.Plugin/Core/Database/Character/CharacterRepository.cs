using System.Collections.ObjectModel;

namespace MeidoPhotoStudio.Plugin.Core.Database.Character;

public class CharacterRepository : IEnumerable<CharacterModel>, IActivateable
{
    private ReadOnlyCollection<CharacterModel> characters;

    public int Count =>
        Characters.Count;

    private ReadOnlyCollection<CharacterModel> Characters
    {
        get
        {
            characters ??= Initialize();

            return characters;
        }
    }

    public CharacterModel this[int index] =>
        (uint)index >= Characters.Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : Characters[index];

    public CharacterModel GetByID(string id) =>
        this.FirstOrDefault(character => string.Equals(id, character.ID, StringComparison.OrdinalIgnoreCase));

    public IEnumerator<CharacterModel> GetEnumerator() =>
        Characters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    void IActivateable.Activate() =>
        characters = Initialize();

    void IActivateable.Deactivate()
    {
    }

    private static ReadOnlyCollection<CharacterModel> Initialize() =>
        GameMain.Instance.CharacterMgr.GetStockMaidList()
            .Select(static maid => new CharacterModel(maid))
            .ToList()
            .AsReadOnly();
}
