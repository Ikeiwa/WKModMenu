using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using ModMenu.Behaviors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu;

internal sealed class ModMenu
{
    private static RectTransform _settingsMenuTransform;
    private static RectTransform _modMenuPanel;
    private static Dictionary<string, ConfigEntryBase[]> _configs;

    public static void Initialize(Transform target)
    {
        _settingsMenuTransform = target.Search("SettingsParent/Settings Pane")?.GetComponent<RectTransform>();
        _configs = GetModConfigs();

        _modMenuPanel = UI.CreateSettingsPanel(_settingsMenuTransform, "Mods");
        CreateMenuTab();
        CreateModMenus();


        _modMenuPanel.gameObject.SetActive(false);
    }

    private static void CreateMenuTab()
    {
        var tabGroup = _settingsMenuTransform.GetComponentsInChildren<UI_TabGroup>()
            .FirstOrDefault(c => c.gameObject.name == "Tab Selection Hor");

        if (tabGroup == null)
        {
            Plugin.Log.LogError("No tab group found");
            return;
        }

        var tab = tabGroup.AddTab(Templates.MainTabButton, "Mods", _modMenuPanel.gameObject)
            .GetComponent<RectTransform>();
        tab.sizeDelta = new Vector2(80, tab.sizeDelta.y);
    }

    private static void CreateModMenus()
    {
        var scrollRect = UI.CreateScrollRect(_modMenuPanel);
        scrollRect.viewport.GetComponent<RectMask2D>().softness = new Vector2Int(10, 0);
        var scrollTransform = scrollRect.transform as RectTransform;

        scrollTransform.anchorMin = new Vector2(0, 1);
        scrollTransform.anchorMax = new Vector2(1, 1);
        scrollTransform.anchoredPosition = new Vector2(0, -50);
        scrollTransform.sizeDelta = new Vector2(-20, 70);


        var tabGroup = UI.CreateTabGroup(scrollRect.content, scrollRect.content.gameObject, false, false,
            new Color(0.3868f, 0.3868f, 0.3868f, 1));
        foreach (var config in _configs)
        {
            var modMenu = CreateModMenu(config.Value, out var firstElement);
            modMenu.SetActive(false);
            tabGroup.AddTab(Templates.TabButton, config.Key, modMenu, firstElement);
        }
    }

    private static GameObject CreateModMenu(ConfigEntryBase[] entries, out Selectable firstElement)
    {
        var scrollRect = UI.CreateScrollRect(_modMenuPanel);
        scrollRect.viewport.GetComponent<RectMask2D>().softness = new Vector2Int(10, 0);
        var scrollTransform = scrollRect.transform as RectTransform;

        scrollTransform.anchorMin = new Vector2(0, 0);
        scrollTransform.anchorMax = new Vector2(1, 1);
        scrollTransform.anchoredPosition = new Vector2(0, -45);
        scrollTransform.sizeDelta = new Vector2(-20, -70);

        var scrollLayout = scrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
        scrollLayout.childControlHeight = true;
        scrollLayout.childForceExpandHeight = true;
        scrollLayout.spacing = 0;
        scrollLayout.childAlignment = TextAnchor.UpperLeft;

        firstElement = null;
        var groups = entries.GroupBy(entry => entry.Definition.Section);

        foreach (var group in groups)
        {
            var column = CreateEntriesColumn(scrollRect.content);
            float columnFill = 0;
            float maxHeight = scrollRect.content.rect.height;
            float spacing = 5;
            columnFill += ((RectTransform)UI.CreateLabel(column, group.Key).transform).rect.height + spacing;

            foreach (var entry in group)
            {
                var entryName = PrettifyName(entry.Definition.Key);

                switch (entry)
                {
                    case ConfigEntry<bool>:
                        UI.CreateToggle(column, entryName, (bool)entry.BoxedValue,
                            isOn => { entry.BoxedValue = isOn; });
                        columnFill += ((RectTransform)Templates.Toggle.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<float>
                        when entry.Description.AcceptableValues is AcceptableValueRange<float> acceptableFloatValueRange
                        :
                        var minFloat = acceptableFloatValueRange.MinValue;
                        var maxFloat = acceptableFloatValueRange.MaxValue;

                        UI.CreateSlider(column, entryName, (float)entry.BoxedValue,
                            value => { entry.BoxedValue = value; }, minFloat, maxFloat, 0);

                        columnFill += ((RectTransform)Templates.Slider.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<float>:
                        UI.CreateInputField(column, entryName, entry.BoxedValue.ToString(),
                            value =>
                            {
                                if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture,
                                        out var input))
                                    entry.BoxedValue = input;
                            }, TMP_InputField.ContentType.DecimalNumber,
                            TMP_InputField.CharacterValidation.Decimal);

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<int>
                        when entry.Description.AcceptableValues is AcceptableValueRange<int> acceptableIntValueRange:
                        var minInt = acceptableIntValueRange.MinValue;
                        var maxInt = acceptableIntValueRange.MaxValue;

                        UI.CreateSlider(column, entryName, (int)entry.BoxedValue,
                            value => { entry.BoxedValue = (int)value; }, minInt, maxInt, 1, "0");

                        columnFill += ((RectTransform)Templates.Slider.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<int>:
                        UI.CreateInputField(column, entryName, entry.BoxedValue.ToString(),
                            value =>
                            {
                                if (int.TryParse(value, out var input))
                                    entry.BoxedValue = input;
                            }, TMP_InputField.ContentType.IntegerNumber,
                            TMP_InputField.CharacterValidation.Integer);

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<string>
                        when entry.Description.AcceptableValues is AcceptableValueList<string> acceptableValueList:
                        var valueList = acceptableValueList.AcceptableValues.ToList();
                        UI.CreateDropdown(column, entryName, valueList,
                            valueList.IndexOf(entry.BoxedValue.ToString()),
                            value => { entry.BoxedValue = valueList[value]; });

                        columnFill += ((RectTransform)Templates.Dropdown.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<string>:
                        UI.CreateInputField(column, entryName, entry.BoxedValue.ToString(),
                            value => { entry.BoxedValue = value; });

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<KeyCode>:

                        UI.CreateKeyBindingInput(column, entryName, (KeyCode)entry.BoxedValue,
                            key => { entry.BoxedValue = key; }).Setup(_settingsMenuTransform);

                        columnFill += ((RectTransform)Templates.KeyBindingInput.transform).rect.height + spacing;
                        break;

                    case not null when entry.SettingType.IsSubclassOf(typeof(Enum)):
                        var enumType = entry.SettingType;
                        var values = Enum.GetNames(enumType).ToList();

                        UI.CreateDropdown(column, entryName, values,
                            values.IndexOf(entry.BoxedValue.ToString()), value =>
                            {
                                var enumValue = Enum.Parse(enumType, values[value]);
                                entry.BoxedValue = enumValue;
                            });

                        columnFill += ((RectTransform)Templates.Dropdown.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<Color>:
                        var colorValue = (Color)entry.BoxedValue;
                        UI.CreateQuadInputField(column, entryName, colorValue.r.ToString(CultureInfo.InvariantCulture),
                            colorValue.g.ToString(CultureInfo.InvariantCulture),
                            colorValue.b.ToString(CultureInfo.InvariantCulture),
                            colorValue.a.ToString(CultureInfo.InvariantCulture),
                            (val1, val2, val3, val4) =>
                            {
                                if (float.TryParse(val1, out var res1) && float.TryParse(val2, out var res2) &&
                                    float.TryParse(val3, out var res3) && float.TryParse(val4, out var res4))
                                {
                                    entry.BoxedValue = new Color(res1, res2, res3, res4);
                                }
                            }, TMP_InputField.ContentType.DecimalNumber, TMP_InputField.CharacterValidation.Decimal
                            , color1: Color.red, color2: Color.green, color3: Color.blue);

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<Vector2>:
                        var vector2Value = (Vector2)entry.BoxedValue;
                        UI.CreateDualInputField(column, entryName,
                            vector2Value.x.ToString(CultureInfo.InvariantCulture),
                            vector2Value.y.ToString(CultureInfo.InvariantCulture),
                            (val1, val2) =>
                            {
                                if (float.TryParse(val1, out var res1) && float.TryParse(val2, out var res2))
                                {
                                    entry.BoxedValue = new Vector2(res1, res2);
                                }
                            }, TMP_InputField.ContentType.DecimalNumber, TMP_InputField.CharacterValidation.Decimal
                            , color1: Color.red, color2: Color.green);

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<Vector3>:
                        var vector3Value = (Vector3)entry.BoxedValue;
                        UI.CreateTrippleInputField(column, entryName,
                            vector3Value.x.ToString(CultureInfo.InvariantCulture),
                            vector3Value.y.ToString(CultureInfo.InvariantCulture),
                            vector3Value.z.ToString(CultureInfo.InvariantCulture),
                            (val1, val2, val3) =>
                            {
                                if (float.TryParse(val1, out var res1) && float.TryParse(val2, out var res2) &&
                                    float.TryParse(val3, out var res3))
                                {
                                    entry.BoxedValue = new Vector3(res1, res2, res3);
                                }
                            }, TMP_InputField.ContentType.DecimalNumber, TMP_InputField.CharacterValidation.Decimal
                            , color1: Color.red, color2: Color.green, color3: Color.blue);

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<Vector4>:
                        var vector4Value = (Vector4)entry.BoxedValue;
                        UI.CreateQuadInputField(column, entryName,
                            vector4Value.x.ToString(CultureInfo.InvariantCulture),
                            vector4Value.y.ToString(CultureInfo.InvariantCulture),
                            vector4Value.z.ToString(CultureInfo.InvariantCulture),
                            vector4Value.y.ToString(CultureInfo.InvariantCulture),
                            (val1, val2, val3, val4) =>
                            {
                                if (float.TryParse(val1, out var res1) && float.TryParse(val2, out var res2) &&
                                    float.TryParse(val3, out var res3) && float.TryParse(val4, out var res4))
                                {
                                    entry.BoxedValue = new Vector4(res1, res2, res3, res4);
                                }
                            }, TMP_InputField.ContentType.DecimalNumber, TMP_InputField.CharacterValidation.Decimal
                            , color1: Color.red, color2: Color.green, color3: Color.blue);

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<Quaternion>:
                        var quaternionValue = (Quaternion)entry.BoxedValue;
                        UI.CreateQuadInputField(column, entryName,
                            quaternionValue.x.ToString(CultureInfo.InvariantCulture),
                            quaternionValue.y.ToString(CultureInfo.InvariantCulture),
                            quaternionValue.z.ToString(CultureInfo.InvariantCulture),
                            quaternionValue.y.ToString(CultureInfo.InvariantCulture),
                            (val1, val2, val3, val4) =>
                            {
                                if (float.TryParse(val1, out var res1) && float.TryParse(val2, out var res2) &&
                                    float.TryParse(val3, out var res3) && float.TryParse(val4, out var res4))
                                {
                                    entry.BoxedValue = new Quaternion(res1, res2, res3, res4);
                                }
                            }, TMP_InputField.ContentType.DecimalNumber, TMP_InputField.CharacterValidation.Decimal
                            , color1: Color.red, color2: Color.green, color3: Color.blue);

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;

                    case ConfigEntry<Rect>:
                        var rectValue = (Rect)entry.BoxedValue;
                        UI.CreateQuadInputField(column, entryName, rectValue.x.ToString(CultureInfo.InvariantCulture),
                            rectValue.y.ToString(CultureInfo.InvariantCulture),
                            rectValue.width.ToString(CultureInfo.InvariantCulture),
                            rectValue.height.ToString(CultureInfo.InvariantCulture),
                            (val1, val2, val3, val4) =>
                            {
                                if (float.TryParse(val1, out var res1) && float.TryParse(val2, out var res2) &&
                                    float.TryParse(val3, out var res3) && float.TryParse(val4, out var res4))
                                {
                                    entry.BoxedValue = new Rect(res1, res2, res3, res4);
                                }
                            }, TMP_InputField.ContentType.DecimalNumber, TMP_InputField.CharacterValidation.Decimal
                        );

                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;
                }

                if (!firstElement)
                {
                    firstElement = column.GetChild(column.childCount - 1).GetComponentInChildren<Selectable>(true);
                }

                if (columnFill >= maxHeight - 50)
                {
                    column = CreateEntriesColumn(scrollRect.content);
                    columnFill = 0;
                    maxHeight = scrollRect.content.rect.height;
                }
            }
        }

        return scrollRect.gameObject;
    }

    private static RectTransform CreateEntriesColumn(RectTransform parent)
    {
        RectTransform column = new GameObject("column").AddComponent<RectTransform>();
        column.SetParent(parent, false);
        column.anchorMin = new Vector2(0, 0);
        column.anchorMax = new Vector2(0, 1);
        column.anchoredPosition = new Vector2(0, 0);
        column.sizeDelta = new Vector2(300, 0);

        var layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 5;

        return column;
    }

    private static Dictionary<string, ConfigEntryBase[]> GetModConfigs()
    {
        var configs = new Dictionary<string, ConfigEntryBase[]>();

        foreach (var plugin in Chainloader.PluginInfos.Values.OrderBy(p => p.Metadata.Name))
        {
            var configEntries = new List<ConfigEntryBase>();

            foreach (var configEntryBase in plugin.Instance.Config.Select(configEntry => configEntry.Value))
            {
                var tags = configEntryBase.Description?.Tags;

                if (tags != null && tags.Contains("Hidden"))
                    continue;

                configEntries.Add(configEntryBase);
            }

            if (configEntries.Count > 0)
                configs.TryAdd(PrettifyName(plugin.Metadata.Name), configEntries.ToArray());
        }

        return configs;
    }

    private static string PrettifyName(string input)
    {
        input = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        input = Regex.Replace(input, "([A-Z])([A-Z][a-z])", "$1 $2");
        input = Regex.Replace(input, @"\s+", " ");
        input = Regex.Replace(input, @"([A-Z]\.)\s([A-Z]\.)", "$1$2");

        return input.Trim();
    }
}