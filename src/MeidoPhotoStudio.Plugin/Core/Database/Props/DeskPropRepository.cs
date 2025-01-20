using MeidoPhotoStudio.Plugin.Core.Localization;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class DeskPropRepository : IEnumerable<DeskPropModel>
{
    private readonly Translation translation;

    private Dictionary<int, IList<DeskPropModel>> props;

    public DeskPropRepository(Translation translation)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.translation.Initialized += OnTranslationReloaded;
    }

    public IEnumerable<int> CategoryIDs =>
        Props.Keys;

    private Dictionary<int, IList<DeskPropModel>> Props =>
        props ??= Initialize();

    public IList<DeskPropModel> this[int categoryID] =>
        Props[categoryID];

    public bool TryGetPropList(int categoryID, out IList<DeskPropModel> propList) =>
        Props.TryGetValue(categoryID, out propList);

    public bool ContainsCategory(int categoryID) =>
        Props.ContainsKey(categoryID);

    public IEnumerator<DeskPropModel> GetEnumerator() =>
        Props.Values.SelectMany(static list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public DeskPropModel GetByID(int id) =>
        this.FirstOrDefault(model => model.ID == id);

    private Dictionary<int, IList<DeskPropModel>> Initialize()
    {
        var models = new Dictionary<int, List<DeskPropModel>>();

        foreach (var data in DeskManager.item_detail_data_dic.Values)
        {
            var assetName = string.IsNullOrEmpty(data.asset_name) ? data.prefab_name : data.asset_name;
            var model = new DeskPropModel(data, translation["propNames", assetName]);

            if (!models.ContainsKey(data.category_id))
                models[data.category_id] = [];

            models[model.CategoryID].Add(model);
        }

        return models.ToDictionary(static kvp => kvp.Key, static kvp => (IList<DeskPropModel>)kvp.Value.AsReadOnly());
    }

    private void OnTranslationReloaded(object sender, EventArgs e)
    {
        foreach (var prop in this)
        {
            var data = DeskManager.item_detail_data_dic[prop.ID];
            var assetName = string.IsNullOrEmpty(data.asset_name) ? data.prefab_name : data.asset_name;

            prop.Name = translation["propNames", assetName];
        }
    }
}
