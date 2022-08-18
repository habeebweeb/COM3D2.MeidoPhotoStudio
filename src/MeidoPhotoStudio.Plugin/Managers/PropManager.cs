using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BepInEx.Configuration;
using UnityEngine;

using static MeidoPhotoStudio.Plugin.ModelUtility;

using Object = UnityEngine.Object;

namespace MeidoPhotoStudio.Plugin;

public class PropManager : IManager
{
    public const string Header = "PROP";

    private static readonly ConfigEntry<bool> ModItemsOnlyValue;

    private static bool cubeActive = true;
    private static Dictionary<string, string> modFileToFullPath;
    private static bool cubeSmall;

    private readonly List<DragPointProp> propList = new();
    private readonly MeidoManager meidoManager;

    private int currentPropIndex;

    static PropManager() =>
        ModItemsOnlyValue = Configuration.Config.Bind(
            "Prop", "ModItemsOnly", false, "Disable waiting for and loading base game clothing");

    public PropManager(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        meidoManager.BeginCallMeidos += OnBeginCallMeidos;
        meidoManager.EndCallMeidos += OnEndCallMeidos;

        Activate();
    }

    public event EventHandler PropSelectionChange;

    public event EventHandler FromPropSelect;

    public event EventHandler PropListChange;

    private static event EventHandler CubeActiveChange;

    private static event EventHandler CubeSmallChange;

    public static bool CubeActive
    {
        get => cubeActive;
        set
        {
            if (value == cubeActive)
                return;

            cubeActive = value;
            CubeActiveChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public static bool CubeSmall
    {
        get => cubeSmall;
        set
        {
            if (value == cubeSmall)
                return;

            cubeSmall = value;
            CubeSmallChange?.Invoke(null, EventArgs.Empty);
        }
    }

    public static bool ModItemsOnly =>
        ModItemsOnlyValue.Value;

    public DragPointProp CurrentProp =>
        PropCount is 0 ? null : propList[CurrentPropIndex];

    public string[] PropNameList =>
        propList.Count is 0
            ? new[] { Translation.Get("systemMessage", "noProps") }
            : propList.Select(prop => prop.Name).ToArray();

    public int PropCount =>
        propList.Count;

    public int CurrentPropIndex
    {
        get => currentPropIndex;
        set
        {
            if (PropCount is 0)
            {
                currentPropIndex = 0;

                return;
            }

            if ((uint)value >= (uint)PropCount)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (currentPropIndex == value)
                return;

            currentPropIndex = value;
            PropSelectionChange?.Invoke(this, EventArgs.Empty);
        }
    }

    private static Dictionary<string, string> ModFileToFullPath
    {
        get
        {
            if (modFileToFullPath is not null)
                return modFileToFullPath;

            var modFiles = Menu.GetModFiles();

            modFileToFullPath = new(modFiles.Length, StringComparer.OrdinalIgnoreCase);

            foreach (var mod in modFiles)
            {
                var key = Path.GetFileName(mod);

                if (!modFileToFullPath.ContainsKey(key))
                    modFileToFullPath[key] = mod;
            }

            return modFileToFullPath;
        }
    }

    public bool AddFromPropInfo(PropInfo propInfo)
    {
        switch (propInfo.Type)
        {
            case PropInfo.PropType.Mod:
                ModItem modItem;

                if (!string.IsNullOrEmpty(propInfo.SubFilename))
                {
                    modItem = ModItem.OfficialMod(ModFileToFullPath[propInfo.Filename]);
                    modItem.BaseMenuFile = propInfo.SubFilename;
                }
                else
                {
                    modItem = ModItem.Mod(propInfo.Filename);
                }

                return AddModProp(modItem);
            case PropInfo.PropType.MyRoom:
                return AddMyRoomProp(new() { ID = propInfo.MyRoomID, PrefabName = propInfo.Filename });
            case PropInfo.PropType.Bg:
                return AddBgProp(propInfo.Filename);
            case PropInfo.PropType.Odogu:
                return AddGameProp(propInfo.Filename);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool AddModProp(ModItem modItem)
    {
        var model = LoadMenuModel(modItem);

        if (!model)
            return false;

        var name = modItem.MenuFile;

        if (modItem.IsOfficialMod)
            name = Path.GetFileName(name) + ".menu"; // add '.menu' for partsedit support

        model.name = name;

        var dragPoint = AttachDragPoint(model);

        dragPoint.Info = PropInfo.FromModItem(modItem);

        AddProp(dragPoint);

        return true;
    }

    public bool AddMyRoomProp(MyRoomItem myRoomItem)
    {
        var model = LoadMyRoomModel(myRoomItem);

        if (!model)
            return false;

        model.name = Translation.Get("myRoomPropNames", myRoomItem.PrefabName);

        var dragPoint = AttachDragPoint(model);

        dragPoint.Info = PropInfo.FromMyRoom(myRoomItem);

        AddProp(dragPoint);

        return true;
    }

    public bool AddBgProp(string assetName)
    {
        var model = LoadBgModel(assetName);

        if (!model)
            return false;

        model.name = Translation.Get("bgNames", assetName);

        var dragPoint = AttachDragPoint(model);

        dragPoint.Info = PropInfo.FromBg(assetName);

        AddProp(dragPoint);

        return true;
    }

    public bool AddGameProp(string assetName)
    {
        var isMenu = assetName.EndsWith(".menu");
        var model = isMenu ? LoadMenuModel(assetName) : LoadGameModel(assetName);

        if (!model)
            return false;

        model.name = Translation.Get("propNames", isMenu ? Utility.HandItemToOdogu(assetName) : assetName, !isMenu);

        var dragPoint = AttachDragPoint(model);

        dragPoint.Info = PropInfo.FromGameProp(assetName);

        AddProp(dragPoint);

        return true;
    }

    public void CopyProp(int propIndex)
    {
        if ((uint)propIndex >= (uint)PropCount)
            throw new ArgumentOutOfRangeException(nameof(propIndex));

        AddFromPropInfo(propList[propIndex].Info);
    }

    public void DeleteAllProps()
    {
        foreach (var prop in propList)
            DestroyProp(prop);

        propList.Clear();
        CurrentPropIndex = 0;

        EmitPropListChange();
    }

    public void RemoveProp(int index)
    {
        if ((uint)index >= (uint)PropCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        DestroyProp(propList[index]);
        propList.RemoveAt(index);
        CurrentPropIndex = Utility.Bound(CurrentPropIndex, 0, PropCount - 1);
        EmitPropListChange();
    }

    public void AttachProp(DragPointProp prop, AttachPoint point, int index)
    {
        if ((uint)index >= (uint)meidoManager.ActiveMeidoList.Count)
            return;

        var meido = meidoManager.ActiveMeidoList[index];

        prop.AttachTo(meido, point);
    }

    public void Update()
    {
    }

    public void Activate()
    {
        CubeSmallChange += OnCubeSmall;
        CubeActiveChange += OnCubeActive;
    }

    public void Deactivate()
    {
        DeleteAllProps();
        CubeSmallChange -= OnCubeSmall;
        CubeActiveChange -= OnCubeActive;
    }

    private DragPointProp AttachDragPoint(GameObject model)
    {
        var dragPoint = DragPoint.Make<DragPointProp>(PrimitiveType.Cube, Vector3.one * 0.12f);

        dragPoint.Initialize(() => model.transform.position, () => Vector3.zero);
        dragPoint.Set(model.transform);
        dragPoint.AddGizmo(0.45f, CustomGizmo.GizmoMode.World);
        dragPoint.ConstantScale = true;
        dragPoint.DragPointScale = CubeSmall ? DragPointGeneral.SmallCube : 1f;
        dragPoint.Delete += OnDeleteProp;
        dragPoint.Select += OnSelectProp;

        return dragPoint;
    }

    private void AddProp(DragPointProp dragPoint)
    {
        propList.Add(dragPoint);
        EmitPropListChange();
    }

    private void DestroyProp(DragPointProp prop)
    {
        if (!prop)
            return;

        prop.Delete -= OnDeleteProp;
        prop.Select -= OnSelectProp;
        Object.Destroy(prop.gameObject);
    }

    private void EmitPropListChange() =>
        PropListChange?.Invoke(this, EventArgs.Empty);

    private void OnBeginCallMeidos(object sender, EventArgs args)
    {
        foreach (var prop in propList.Where(p => p.AttachPointInfo.AttachPoint is not AttachPoint.None))
            prop.DetachTemporary();
    }

    private void OnEndCallMeidos(object sender, EventArgs args)
    {
        foreach (var prop in propList.Where(p => p.AttachPointInfo.AttachPoint is not AttachPoint.None))
        {
            var info = prop.AttachPointInfo;
            var meido = meidoManager.GetMeido(info.MaidGuid);

            prop.AttachTo(meido, info.AttachPoint, meido is null);
        }
    }

    private void OnDeleteProp(object sender, EventArgs args) =>
        RemoveProp(propList.IndexOf((DragPointProp)sender));

    private void OnSelectProp(object sender, EventArgs args)
    {
        CurrentPropIndex = propList.IndexOf((DragPointProp)sender);
        FromPropSelect?.Invoke(this, EventArgs.Empty);
    }

    private void OnCubeSmall(object sender, EventArgs args)
    {
        foreach (var dragPoint in propList)
            dragPoint.DragPointScale = CubeSmall ? DragPointGeneral.SmallCube : 1f;
    }

    private void OnCubeActive(object sender, EventArgs args)
    {
        foreach (var dragPoint in propList)
            dragPoint.gameObject.SetActive(CubeActive);
    }
}
