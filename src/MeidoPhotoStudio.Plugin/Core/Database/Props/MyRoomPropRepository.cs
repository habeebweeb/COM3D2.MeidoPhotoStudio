using MeidoPhotoStudio.Plugin.Core.Localization;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class MyRoomPropRepository : IEnumerable<MyRoomPropModel>
{
    private readonly Translation translation;

    private Dictionary<int, IList<MyRoomPropModel>> props;

    public MyRoomPropRepository(Translation translation)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.translation.Initialized += OnReloadedTranslation;
    }

    public IEnumerable<int> CategoryIDs =>
        Props.Keys;

    private Dictionary<int, IList<MyRoomPropModel>> Props =>
        props ??= Initialize();

    public IList<MyRoomPropModel> this[int categoryID] =>
        Props[categoryID];

    public bool TryGetPropList(int categoryID, out IList<MyRoomPropModel> propList) =>
        Props.TryGetValue(categoryID, out propList);

    public bool ContainsCategory(int categoryID) =>
        Props.ContainsKey(categoryID);

    public IEnumerator<MyRoomPropModel> GetEnumerator() =>
        Props.Values.SelectMany(static list => list).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public MyRoomPropModel GetByID(int id) =>
        this.FirstOrDefault(model => model.ID == id);

    private Dictionary<int, IList<MyRoomPropModel>> Initialize()
    {
        var models = new Dictionary<int, List<MyRoomPropModel>>();

        foreach (var data in MyRoomCustom.PlacementData.GetAllDatas(false))
        {
            var assetName = string.IsNullOrEmpty(data.resourceName) ? data.assetName : data.resourceName;
            var model = new MyRoomPropModel(data, translation["myRoomPropNames", assetName]);

            if (!models.ContainsKey(data.categoryID))
                models[data.categoryID] = [];

            models[model.CategoryID].Add(model);
        }

        return models.ToDictionary(static kvp => kvp.Key, static kvp => (IList<MyRoomPropModel>)kvp.Value.AsReadOnly());
    }

    private void OnReloadedTranslation(object sender, EventArgs e)
    {
        foreach (var prop in this)
            prop.Name = translation["myRoomPropNames", prop.AssetName];
    }
}
