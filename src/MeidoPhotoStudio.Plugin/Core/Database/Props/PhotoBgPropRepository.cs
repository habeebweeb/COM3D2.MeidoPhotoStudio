using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class PhotoBgPropRepository : IEnumerable<PhotoBgPropModel>
{
    private Dictionary<string, IList<PhotoBgPropModel>> props;

    public PhotoBgPropRepository() =>
        Translation.ReloadTranslationEvent += OnReloadedTranslation;

    public IEnumerable<string> Categories =>
        Props.Keys;

    private Dictionary<string, IList<PhotoBgPropModel>> Props =>
        props ??= Initialize();

    public IList<PhotoBgPropModel> this[string category] =>
        Props[category];

    public bool TryGetPropList(string category, out IList<PhotoBgPropModel> propList) =>
        Props.TryGetValue(category, out propList);

    public bool ContainsCategory(string category) =>
        Props.ContainsKey(category);

    public IEnumerator<PhotoBgPropModel> GetEnumerator() =>
        Props.Values.SelectMany(static list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public PhotoBgPropModel GetByID(long id) =>
        this.FirstOrDefault(model => model.ID == id);

    private static Dictionary<string, IList<PhotoBgPropModel>> Initialize()
    {
        PhotoBGObjectData.Create();

        Dictionary<string, List<PhotoBgPropModel>> props = [];

        foreach (var (category, propList) in PhotoBGObjectData.category_list)
        {
            if (!props.ContainsKey(category))
                props[category] = [];

            foreach (var prop in propList)
            {
                var assetName = string.IsNullOrEmpty(prop.create_asset_bundle_name)
                    ? prop.create_prefab_name
                    : prop.create_asset_bundle_name;

                if (string.IsNullOrEmpty(assetName))
                    continue;

                props[category].Add(new(prop, Translation.Get("propNames", assetName)));
            }
        }

        return props.ToDictionary(
            static kvp => kvp.Key,
            static kvp => (IList<PhotoBgPropModel>)kvp.Value.AsReadOnly());
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var prop in this)
        {
            var assetName = string.IsNullOrEmpty(prop.AssetName) ? prop.PrefabName : prop.AssetName;

            if (string.IsNullOrEmpty(assetName))
                continue;

            prop.Name = Translation.Get("propNames", assetName);
        }
    }
}
