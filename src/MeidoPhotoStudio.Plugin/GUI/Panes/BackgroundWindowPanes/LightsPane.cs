using System;
using System.Collections.Generic;

using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class LightsPane : BasePane
{
    private static Light mainLight;

    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly Dropdown lightDropdown;
    private readonly SelectionGrid lightTypeGrid;
    private readonly Toggle lightOnToggle;
    private readonly Button addLightButton;
    private readonly Button deleteLightButton;
    private readonly Button clearLightsButton;
    private readonly Slider xRotationSlider;
    private readonly Slider yRotationSlider;
    private readonly Slider intensitySlider;
    private readonly Slider shadowStrengthSlider;
    private readonly Slider rangeSlider;
    private readonly Slider spotAngleSlider;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly Button resetPositionButton;
    private readonly Button resetPropertiesButton;

    private string lightingHeader;
    private string resetHeader;
    private string noLights;

    public LightsPane(LightRepository lightRepository, SelectionController<LightController> lightSelectionController)
    {
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));

        lightRepository.AddedLight += OnAddedLight;
        lightRepository.RemovedLight += OnRemovedLight;

        lightSelectionController.Selecting += OnSelectingLight;
        lightSelectionController.Selected += OnSelectedLight;

        lightingHeader = Translation.Get("lightsPane", "header");
        resetHeader = Translation.Get("lightsPane", "resetLabel");
        noLights = Translation.Get("lightsPane", "noLights");

        lightDropdown = new(new[] { noLights });
        lightDropdown.SelectionChange += LightDropdownSelectionChanged;

        lightTypeGrid = new SelectionGrid(Translation.GetArray("lightType", new[] { "normal", "spot", "point" }));
        lightTypeGrid.ControlEvent += OnLightTypeChanged;

        addLightButton = new Button(Translation.Get("lightsPane", "add"));
        addLightButton.ControlEvent += OnAddLightButtonPressed;

        deleteLightButton = new Button(Translation.Get("lightsPane", "delete"));
        deleteLightButton.ControlEvent += OnDeleteButtonPressed;

        clearLightsButton = new Button(Translation.Get("lightsPane", "clear"));
        clearLightsButton.ControlEvent += OnClearButtonPressed;

        lightOnToggle = new Toggle(Translation.Get("lightsPane", "on"), true);
        lightOnToggle.ControlEvent += OnLightOnToggleChanged;

        var defaultRotation = LightProperties.DefaultRotation.eulerAngles;

        xRotationSlider = new Slider(Translation.Get("lights", "x"), 0f, 360f, defaultRotation.x, defaultRotation.x)
        {
            HasTextField = true,
            HasReset = true,
        };

        xRotationSlider.ControlEvent += OnXRotationSliderChanged;

        yRotationSlider = new Slider(Translation.Get("lights", "y"), 0f, 360f, defaultRotation.y, defaultRotation.y)
        {
            HasTextField = true,
            HasReset = true,
        };

        yRotationSlider.ControlEvent += OnYRotationSliderChanged;

        intensitySlider = new Slider(Translation.Get("lights", "intensity"), 0f, 2f, 0.95f, 0.95f)
        {
            HasTextField = true,
            HasReset = true,
        };

        intensitySlider.ControlEvent += OnIntensitySliderChanged;

        shadowStrengthSlider = new Slider(Translation.Get("lights", "shadow"), 0f, 1f, 0.10f, 0.10f)
        {
            HasTextField = true,
            HasReset = true,
        };

        shadowStrengthSlider.ControlEvent += OnShadowStrenthSliderChanged;

        rangeSlider = new Slider(Translation.Get("lights", "range"), 0f, 150f, 10f, 10f)
        {
            HasTextField = true,
            HasReset = true,
        };

        rangeSlider.ControlEvent += OnRangeSliderChanged;

        spotAngleSlider = new Slider(Translation.Get("lights", "spot"), 0f, 150f, 50f, 50f)
        {
            HasTextField = true,
            HasReset = true,
        };

        spotAngleSlider.ControlEvent += OnSpotAngleSliderChanged;

        redSlider = new Slider(Translation.Get("lights", "red"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        redSlider.ControlEvent += OnRedSliderChanged;

        greenSlider = new Slider(Translation.Get("lights", "green"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        greenSlider.ControlEvent += OnGreenSliderChanged;

        blueSlider = new Slider(Translation.Get("lights", "blue"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        blueSlider.ControlEvent += OnBlueSliderChanged;

        resetPropertiesButton = new Button(Translation.Get("lightsPane", "resetProperties"));
        resetPropertiesButton.ControlEvent += OnResetPropertiesButtonPressed;

        resetPositionButton = new Button(Translation.Get("lightsPane", "resetPosition"));
        resetPositionButton.ControlEvent += OnResetPositionButtonPressed;
    }

    public static Light MainLight =>
        mainLight ? mainLight : mainLight = GameMain.Instance.MainLight.GetComponent<Light>();

    private LightController CurrentLightController =>
        lightSelectionController.Current;

    public override void Draw()
    {
        MpsGui.Header(lightingHeader);
        MpsGui.WhiteLine();

        DrawTopBar();

        if (CurrentLightController == null)
        {
            GUILayout.Label(noLights, GUILayout.ExpandWidth(true));
        }
        else
        {
            DrawLightType();

            var enabled = GUI.enabled;

            GUI.enabled = lightOnToggle.Value;

            if (CurrentLightController.Type is LightType.Directional)
                DrawDirectionalLightControls();
            else if (CurrentLightController.Type is LightType.Spot)
                DrawSpotLightControls();
            else
                DrawPointLightControls();

            MpsGui.BlackLine();

            DrawColourControls();

            DrawReset();

            GUI.enabled = enabled;
        }

        void DrawTopBar()
        {
            GUI.enabled = lightRepository.Count > 0;

            GUILayout.BeginHorizontal();

            lightDropdown.Draw(GUILayout.Width(84f));

            var noExpandWidth = GUILayout.ExpandWidth(false);

            GUI.enabled = true;

            addLightButton.Draw(noExpandWidth);

            GUI.enabled = lightRepository.Count > 0;

            GUILayout.FlexibleSpace();

            deleteLightButton.Draw(noExpandWidth);
            clearLightsButton.Draw(noExpandWidth);

            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        void DrawLightType()
        {
            GUILayout.BeginHorizontal();

            var enabled = GUI.enabled;

            GUI.enabled = lightOnToggle.Value;

            lightTypeGrid.Draw();

            GUI.enabled = enabled;

            GUILayout.FlexibleSpace();

            lightOnToggle.Draw();

            GUILayout.EndHorizontal();
        }

        void DrawDirectionalLightControls()
        {
            xRotationSlider.Draw();
            yRotationSlider.Draw();
            intensitySlider.Draw();
            shadowStrengthSlider.Draw();
        }

        void DrawSpotLightControls()
        {
            xRotationSlider.Draw();
            yRotationSlider.Draw();
            intensitySlider.Draw();
            rangeSlider.Draw();
            spotAngleSlider.Draw();
        }

        void DrawPointLightControls()
        {
            intensitySlider.Draw();
            rangeSlider.Draw();
        }

        void DrawColourControls()
        {
            redSlider.Draw();
            greenSlider.Draw();
            blueSlider.Draw();
        }

        void DrawReset()
        {
            MpsGui.Header(resetHeader);
            MpsGui.WhiteLine();

            GUILayout.BeginHorizontal();

            resetPropertiesButton.Draw();
            resetPositionButton.Draw();

            GUILayout.EndHorizontal();
        }
    }

    public override void UpdatePane()
    {
        base.UpdatePane();

        UpdateControls();
    }

    protected override void ReloadTranslation()
    {
        lightingHeader = Translation.Get("lightsPane", "header");
        resetHeader = Translation.Get("lightsPane", "resetLabel");
        noLights = Translation.Get("lightsPane", "noLights");
        lightTypeGrid.SetItemsWithoutNotify(Translation.GetArray("lightType", new[] { "normal", "spot", "point" }));
        addLightButton.Label = Translation.Get("lightsPane", "add");
        deleteLightButton.Label = Translation.Get("lightsPane", "delete");
        clearLightsButton.Label = Translation.Get("lightsPane", "clear");
        lightOnToggle.Label = Translation.Get("lightsPane", "on");
        xRotationSlider.Label = Translation.Get("lights", "x");
        yRotationSlider.Label = Translation.Get("lights", "y");
        intensitySlider.Label = Translation.Get("lights", "intensity");
        shadowStrengthSlider.Label = Translation.Get("lights", "shadow");
        rangeSlider.Label = Translation.Get("lights", "range");
        spotAngleSlider.Label = Translation.Get("lights", "spot");
        redSlider.Label = Translation.Get("lights", "red");
        greenSlider.Label = Translation.Get("lights", "green");
        blueSlider.Label = Translation.Get("lights", "blue");
        resetPropertiesButton.Label = Translation.Get("lightsPane", "resetProperties");
        resetPositionButton.Label = Translation.Get("lightsPane", "resetPosition");
    }

    private string LightName(Light light) =>
        light == MainLight ? Translation.Get("lightType", "main") : Translation.Get("lightType", "light");

    private void OnSelectingLight(object sender, SelectionEventArgs<LightController> e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.ChangedProperty -= OnChangedLightProperties;
    }

    private void OnSelectedLight(object sender, SelectionEventArgs<LightController> e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.ChangedProperty += OnChangedLightProperties;

        lightDropdown.SetIndexWithoutNotify(e.Index);

        UpdateControls();
    }

    private void OnRemovedLight(object sender, LightRepositoryEventArgs e)
    {
        if (lightRepository.Count is 0)
        {
            lightDropdown.SetDropdownItems(new[] { noLights }, 0);

            return;
        }

        var lightIndex = lightDropdown.SelectedItemIndex >= lightRepository.Count
            ? lightRepository.Count - 1
            : lightDropdown.SelectedItemIndex;

        var lightNameList = new List<string>(lightDropdown.DropdownList);

        lightNameList.RemoveAt(e.LightIndex);

        lightDropdown.SetDropdownItems(lightNameList.ToArray(), lightIndex);
    }

    private void OnAddedLight(object sender, LightRepositoryEventArgs e)
    {
        if (lightRepository.Count is 1)
        {
            lightDropdown.SetDropdownItems(new[] { LightName(e.LightController.Light) }, 0);

            return;
        }

        var lightNameList = new List<string>(lightDropdown.DropdownList);

        lightNameList.Insert(e.LightIndex, GetNewLightName());

        lightDropdown.SetDropdownItems(lightNameList.ToArray(), lightRepository.Count - 1);

        string GetNewLightName()
        {
            var nameSet = new HashSet<string>(lightDropdown.DropdownList);

            var lightName = LightName(e.LightController.Light);
            var newLightName = lightName;
            var index = 1;

            while (nameSet.Contains(newLightName))
            {
                index++;
                newLightName = $"{lightName} ({index})";
            }

            return newLightName;
        }
    }

    private void OnChangedLightProperties(object sender, EventArgs e) =>
        UpdateControls();

    private Color SliderColours() =>
        new(redSlider.Value, greenSlider.Value, blueSlider.Value);

    private void UpdateControls()
    {
        if (CurrentLightController is null)
            return;

        lightTypeGrid.SetValueWithoutNotify(CurrentLightController.Type switch
        {
            LightType.Directional => 0,
            LightType.Spot => 1,
            LightType.Point => 2,
            _ => 0,
        });

        lightOnToggle.SetEnabledWithoutNotify(CurrentLightController.Enabled);

        UpdateSliders();
    }

    private void UpdateSliders()
    {
        if (CurrentLightController is null)
            return;

        var rotation = CurrentLightController.Light.transform.rotation.eulerAngles;

        xRotationSlider.SetValueWithoutNotify(rotation.x);
        yRotationSlider.SetValueWithoutNotify(rotation.y);
        intensitySlider.SetValueWithoutNotify(CurrentLightController.Intensity);
        shadowStrengthSlider.SetValueWithoutNotify(CurrentLightController.ShadowStrength);
        rangeSlider.SetValueWithoutNotify(CurrentLightController.Range);
        spotAngleSlider.SetValueWithoutNotify(CurrentLightController.SpotAngle);
        redSlider.SetValueWithoutNotify(CurrentLightController.Colour.r);
        greenSlider.SetValueWithoutNotify(CurrentLightController.Colour.g);
        blueSlider.SetValueWithoutNotify(CurrentLightController.Colour.b);
    }

    private void LightDropdownSelectionChanged(object sender, EventArgs e)
    {
        if (lightRepository.Count is 0)
            return;

        lightSelectionController.Select(lightDropdown.SelectedItemIndex);
    }

    private void OnLightTypeChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Type = lightTypeGrid.SelectedItemIndex switch
        {
            0 => LightType.Directional,
            1 => LightType.Spot,
            2 => LightType.Point,
            _ => LightType.Directional,
        };

        UpdateSliders();
    }

    private void OnAddLightButtonPressed(object sender, EventArgs e) =>
        lightRepository.AddLight();

    private void OnDeleteButtonPressed(object sender, EventArgs e)
    {
        if (lightRepository.Count is 0)
            return;

        if (CurrentLightController is null)
            return;

        if (CurrentLightController.Light == GameMain.Instance.MainLight.GetComponent<Light>())
            return;

        lightRepository.RemoveLight(lightRepository.IndexOf(CurrentLightController));
    }

    private void OnClearButtonPressed(object sender, EventArgs e)
    {
        for (var i = lightRepository.Count - 1; i > 0; i--)
            lightRepository.RemoveLight(i);
    }

    private void OnLightOnToggleChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        // TODO: LOL
        CurrentLightController.ChangedProperty -= OnChangedLightProperties;
        CurrentLightController.Enabled = lightOnToggle.Value;
        CurrentLightController.ChangedProperty += OnChangedLightProperties;
    }

    private void OnXRotationSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        var z = CurrentLightController.Rotation.eulerAngles.z;

        var newRotation = Quaternion.Euler(xRotationSlider.Value, yRotationSlider.Value, z);

        CurrentLightController.ChangedProperty -= OnChangedLightProperties;
        CurrentLightController.Rotation = newRotation;
        CurrentLightController.ChangedProperty += OnChangedLightProperties;
    }

    private void OnYRotationSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        var z = CurrentLightController.Rotation.eulerAngles.z;

        var newRotation = Quaternion.Euler(xRotationSlider.Value, yRotationSlider.Value, z);

        CurrentLightController.ChangedProperty -= OnChangedLightProperties;
        CurrentLightController.Rotation = newRotation;
        CurrentLightController.ChangedProperty += OnChangedLightProperties;
    }

    private void OnIntensitySliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Intensity = intensitySlider.Value;
    }

    private void OnShadowStrenthSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.ShadowStrength = shadowStrengthSlider.Value;
    }

    private void OnRangeSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Range = rangeSlider.Value;
    }

    private void OnSpotAngleSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.SpotAngle = spotAngleSlider.Value;
    }

    private void OnRedSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Colour = SliderColours();
    }

    private void OnGreenSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Colour = SliderColours();
    }

    private void OnBlueSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Colour = SliderColours();
    }

    private void OnResetPositionButtonPressed(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Position = LightController.DefaultPosition;
    }

    private void OnResetPropertiesButtonPressed(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.ResetCurrentLightProperties();
    }
}
