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
                        var toggle = UI.CreateToggle(column, entryName, isOn =>
                        {
                            entry.BoxedValue = isOn;
                        });
                        toggle.SetIsOnWithoutNotify((bool)entry.BoxedValue);
                        columnFill += ((RectTransform)Templates.Toggle.transform).rect.height + spacing;
                        break;
                    
                    case ConfigEntry<float>:
                        if (entry.Description.AcceptableValues is AcceptableValueRange<float> acceptableFloatValueRange)
                        {
                            var min = acceptableFloatValueRange.MinValue;
                            var max = acceptableFloatValueRange.MaxValue;
                            
                            var slider = UI.CreateSlider(column, entryName, value =>
                            {
                                entry.BoxedValue = value;
                            }, min, max, 0);
                            slider.SetValueWithoutNotify((float)entry.BoxedValue);
                            columnFill += ((RectTransform)Templates.Slider.transform).rect.height + spacing;
                        }
                        else
                        {
                            var floatInputField = UI.CreateInputField(column, entryName, value =>
                            {
                                if(float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture,out var input))
                                    entry.BoxedValue = input;
                            }, TMP_InputField.ContentType.DecimalNumber, TMP_InputField.CharacterValidation.Decimal);
                            floatInputField.SetTextWithoutNotify(entry.BoxedValue.ToString());
                            columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        }
                        break;
                    
                    case ConfigEntry<int>:
                        if (entry.Description.AcceptableValues is AcceptableValueRange<int> acceptableIntValueRange)
                        {
                            var min = acceptableIntValueRange.MinValue;
                            var max = acceptableIntValueRange.MaxValue;
                            
                            var slider = UI.CreateSlider(column, entryName, value =>
                            {
                                entry.BoxedValue = (int)value;
                            }, min, max, 1, "0");
                            slider.SetValueWithoutNotify((int)entry.BoxedValue);
                            columnFill += ((RectTransform)Templates.Slider.transform).rect.height + spacing;
                        }
                        else
                        {
                            var intInputField = UI.CreateInputField(column, entryName, value =>
                            {
                                if(int.TryParse(value, out var input))
                                    entry.BoxedValue = input;
                            }, TMP_InputField.ContentType.IntegerNumber, TMP_InputField.CharacterValidation.Integer);
                            intInputField.SetTextWithoutNotify(entry.BoxedValue.ToString());
                            columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        }
                        break;
                    
                    case ConfigEntry<string> when entry.Description.AcceptableValues is AcceptableValueList<string> acceptableValueList:
                        var valueList = acceptableValueList.AcceptableValues.ToList();
                        var dropDown = UI.CreateDropdown(column, entryName, valueList, value =>
                        {
                            entry.BoxedValue = valueList[value];
                        });
                        dropDown.SetValueWithoutNotify(valueList.IndexOf(entry.BoxedValue.ToString()));
                        columnFill += ((RectTransform)Templates.Dropdown.transform).rect.height + spacing;
                        break;
                    
                    case ConfigEntry<string>:
                        var inputField = UI.CreateInputField(column, entryName, value =>
                        {
                            entry.BoxedValue = value;
                        });
                        inputField.SetTextWithoutNotify(entry.BoxedValue.ToString());
                        columnFill += ((RectTransform)Templates.InputField.transform).rect.height + spacing;
                        break;
                    
                    case ConfigEntry<KeyCode>:
                        
                        var keyBindingInput = UI.CreateKeyBindingInput(column, entryName, key =>
                        {
                            entry.BoxedValue = key;
                        });
                        keyBindingInput.SetValueWithoutNotify((KeyCode)entry.BoxedValue);
                        keyBindingInput.Setup(_settingsMenuTransform);
                        
                        columnFill += ((RectTransform)Templates.KeyBindingInput.transform).rect.height + spacing;
                        break;
                    
                    case not null when entry.SettingType.IsSubclassOf(typeof(Enum)):
                        var enumType = entry.SettingType;
                        var values = Enum.GetNames(enumType).ToList();
                        
                        var enumDropDown = UI.CreateDropdown(column, entryName, values, value =>
                        {
                            var enumValue = Enum.Parse(enumType, values[value]);
                            entry.BoxedValue = enumValue;
                        });
                        enumDropDown.SetValueWithoutNotify(values.IndexOf(entry.BoxedValue.ToString()));

                        if (enumType == typeof(KeyCode))
                            enumDropDown.gameObject.AddComponent<KeyCodeInput>();
                        
                        columnFill += ((RectTransform)Templates.Dropdown.transform).rect.height + spacing;
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