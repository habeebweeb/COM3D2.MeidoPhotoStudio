using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class MyRoomPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly MyRoomPropRepository myRoomPropRepository;
    private readonly IconCache iconCache;
    private readonly Dropdown<int> propCategoryDropdown;

    private Vector2 scrollPosition;
    private IEnumerable<MyRoomPropModel> currentPropList;

    public MyRoomPropsPane(
        PropService propService, MyRoomPropRepository myRoomPropRepository, IconCache iconCache)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.myRoomPropRepository = myRoomPropRepository ?? throw new ArgumentNullException(nameof(myRoomPropRepository));
        this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));

        int[] categories = [-1, .. myRoomPropRepository.CategoryIDs.OrderBy(id => id)];

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        UpdateCurrentPropList();

        static string CategoryFormatter(int category, int index) =>
            Translation.Get("myRoomPropCategories", category.ToString());
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);

        MpsGui.BlackLine();

        DrawPropList();

        static void DrawDropdown<T>(Dropdown<T> dropdown)
        {
            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            const float dropdownButtonWidth = 185f;

            var dropdownLayoutOptions = new[]
            {
                GUILayout.Width(dropdownButtonWidth),
            };

            GUILayout.BeginHorizontal();

            dropdown.Draw(dropdownLayoutOptions);

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.CyclePrevious();

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.CycleNext();

            GUILayout.EndHorizontal();
        }

        void DrawPropList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            const float buttonSize = 70f;

            var buttonStyle = new GUIStyle(GUI.skin.button) { padding = new(0, 0, 0, 0) };
            var buttonLayoutOptions = new GUILayoutOption[]
            {
                GUILayout.Width(buttonSize), GUILayout.Height(buttonSize),
            };

            foreach (var propChunk in currentPropList.Chunk(3))
            {
                GUILayout.BeginHorizontal();

                foreach (var prop in propChunk)
                {
                    var icon = iconCache.GetMyRoomIcon(prop);
                    var clicked = GUILayout.Button(icon, buttonStyle, buttonLayoutOptions);

                    if (clicked)
                        propService.Add(prop);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }

    protected override void ReloadTranslation() =>
        propCategoryDropdown.Reformat();

    private void UpdateCurrentPropList()
    {
        var currentCategory = propCategoryDropdown.SelectedItem;

        if (currentCategory is -1)
        {
            currentPropList = Enumerable.Empty<MyRoomPropModel>();

            return;
        }

        scrollPosition = Vector2.zero;

        currentPropList = myRoomPropRepository[currentCategory];
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        UpdateCurrentPropList();
}
