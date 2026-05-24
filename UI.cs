using System.Collections.Generic;
using System.Linq;
using DarkMachine.UI;
using ModMenu.Behaviors;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModMenu;

public static class UI
{
    public static Button CreateButton(RectTransform parent, string label,
        UnityAction onClick = null, Color? normalColor = null,
        Color? highlightedColor = null,
        Color? pressedColor = null, Color? selectedColor = null, Color? disabledColor = null, Color? labelColor = null,
        Vector2 size = default)
    {
        var buttonObj = Object.Instantiate(Templates.Button);
        var transform = buttonObj.GetComponent<RectTransform>();
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (size != default)
            transform.sizeDelta = size;

        var button = buttonObj.GetComponent<Button>();
        button.colors = new ColorBlock
        {
            normalColor = normalColor ?? Color.white,
            highlightedColor = highlightedColor ?? new Color(1, 0, 0.0651f, 1),
            pressedColor = pressedColor ?? new Color(0.7843f, 0.7843f, 0.7843f, 1),
            selectedColor = selectedColor ?? new Color(1f, 0.0479f, 0f, 1),
            disabledColor = disabledColor ?? new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
            colorMultiplier = button.colors.colorMultiplier,
            fadeDuration = button.colors.fadeDuration,
        };

        button.onClick.m_PersistentCalls.Clear();
        button.onClick.RemoveAllListeners();
        if (onClick != null)
            button.onClick.AddListener(onClick);

        var buttonLabel = buttonObj.GetComponentInChildren<TMP_Text>(true);
        buttonLabel.text = label;
        buttonLabel.fontSizeMin = 0;
        buttonLabel.fontSizeMax = buttonLabel.fontSize;
        buttonLabel.enableAutoSizing = true;
        buttonLabel.color = labelColor ?? Color.white;

        return button;
    }

    public static Toggle CreateToggle(RectTransform parent, string label,
        UnityAction<bool> onClick = null, Color? normalColor = null, Color? highlightedColor = null,
        Color? pressedColor = null, Color? selectedColor = null, Color? disabledColor = null, Vector2 size = default)
    {
        var toggleObj = Object.Instantiate(Templates.Toggle);
        var transform = toggleObj.GetComponent<RectTransform>();
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (size != default)
            transform.sizeDelta = size;

        var toggle = toggleObj.GetComponent<Toggle>();
        toggle.colors = new ColorBlock
        {
            normalColor = normalColor ?? Color.white,
            highlightedColor = highlightedColor ?? new Color(1, 0, 0, 1),
            pressedColor = pressedColor ?? new Color(0.7843f, 0.7843f, 0.7843f, 1.0f),
            selectedColor = selectedColor ?? new Color(1, 0, 0, 1),
            disabledColor = disabledColor ?? new Color(0.7843f, 0.7843f, 0.7843f, 0.502f),
            colorMultiplier = toggle.colors.colorMultiplier,
            fadeDuration = toggle.colors.fadeDuration,
        };

        toggle.onValueChanged.m_PersistentCalls.Clear();
        toggle.onValueChanged.RemoveAllListeners();
        if (onClick != null)
            toggle.onValueChanged.AddListener(onClick);

        var toggleLabel = toggleObj.GetComponentInChildren<TMP_Text>(true);
        toggleLabel.text = label;
        toggleLabel.fontSizeMin = 0;
        toggleLabel.fontSizeMax = toggleLabel.fontSize;
        toggleLabel.enableAutoSizing = true;

        return toggle;
    }

    public static TMP_Dropdown CreateDropdown(RectTransform parent, string label, List<string> options,
        UnityAction<int> onValueChanged = null,
        Vector2 size = default)
    {
        var dropdownObj = Object.Instantiate(Templates.Dropdown);
        var transform = dropdownObj.GetComponent<RectTransform>();
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (size != default)
            transform.sizeDelta = size;

        var dropdown = dropdownObj.GetComponentInChildren<TMP_Dropdown>(true);

        dropdown.onValueChanged.m_PersistentCalls.Clear();
        dropdown.onValueChanged.RemoveAllListeners();
        if (onValueChanged != null)
            dropdown.onValueChanged.AddListener(onValueChanged);

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        
        var dropdownLabel = dropdownObj.GetComponent<TMP_Text>();
        dropdownLabel.text = label;
        dropdownLabel.fontSizeMin = 0;
        dropdownLabel.fontSizeMax = dropdownLabel.fontSize;
        dropdownLabel.enableAutoSizing = true;

        return dropdown;
    }
    
    public static KeyCodeInput CreateKeyBindingInput(RectTransform parent, string label,
        UnityAction<KeyCode> onValueChanged = null,
        Vector2 size = default)
    {
        var keyBindingObj = Object.Instantiate(Templates.KeyBindingInput);
        var transform = keyBindingObj.GetComponent<RectTransform>();
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (size != default)
            transform.sizeDelta = size;

        var keyBinding = keyBindingObj.GetComponent<KeyCodeInput>();

        keyBinding.onValueChanged.m_PersistentCalls.Clear();
        keyBinding.onValueChanged.RemoveAllListeners();
        if (onValueChanged != null)
            keyBinding.onValueChanged.AddListener(onValueChanged);
        
        var dropdownLabel = keyBindingObj.GetComponent<TMP_Text>();
        dropdownLabel.text = label;
        dropdownLabel.fontSizeMin = 0;
        dropdownLabel.fontSizeMax = dropdownLabel.fontSize;
        dropdownLabel.enableAutoSizing = true;

        return keyBinding;
    }

    public static SubmitSlider CreateSlider(RectTransform parent, string label,
        UnityAction<float> onValueChanged = null, float minValue = 0.0f,
        float maxValue = 1.0f,
        float stepSize = 0.1f, string displayFormat = "0.00", Vector2 size = default)
    {
        var sliderObj = Object.Instantiate(Templates.Slider);
        var transform = sliderObj.GetComponent<RectTransform>();
        transform.SetParent(parent);
        if (size != default)
            transform.sizeDelta = size;

        var slider = sliderObj.GetComponentInChildren<SubmitSlider>(true);

        slider.onValueChanged.m_PersistentCalls.Clear();
        slider.onValueChanged.RemoveAllListeners();
        if (onValueChanged != null)
            slider.onValueChanged.AddListener(onValueChanged);

        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.stepAmount = stepSize;

        sliderObj.GetComponentInChildren<SliderTextSync>(true).Setup(slider, displayFormat);

        var sliderLabel = transform.GetComponent<TMP_Text>();
        sliderLabel.text = label;
        sliderLabel.fontSizeMin = 0;
        sliderLabel.fontSizeMax = sliderLabel.fontSize;
        sliderLabel.enableAutoSizing = true;
        
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        return slider;
    }
    
    public static TMP_InputField CreateInputField(RectTransform parent, string label,
        UnityAction<string> onValueChanged = null, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
        TMP_InputField.CharacterValidation characterValidation = TMP_InputField.CharacterValidation.None,
        Vector2 size = default)
    {
        var inputFieldObj = Object.Instantiate(Templates.InputField);
        var transform = inputFieldObj.GetComponent<RectTransform>();
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (size != default)
            transform.sizeDelta = size;

        var inputField = inputFieldObj.GetComponentInChildren<TMP_InputField>(true);
        inputField.richText = false;
        inputField.contentType = contentType;
        inputField.characterValidation = characterValidation;
        inputField.selectionColor = new Color(0.6981f, 0.1153f, 0.1153f, 0.75f);

        inputField.onValueChanged.m_PersistentCalls.Clear();
        inputField.onValueChanged.RemoveAllListeners();
        if (onValueChanged != null)
            inputField.onValueChanged.AddListener(onValueChanged);
        
        var dropdownLabel = inputFieldObj.GetComponent<TMP_Text>();
        dropdownLabel.text = label;
        dropdownLabel.fontSizeMin = 0;
        dropdownLabel.fontSizeMax = dropdownLabel.fontSize;
        dropdownLabel.enableAutoSizing = true;

        return inputField;
    }

    public static RectTransform CreateSettingsPanel(RectTransform parent, string label)
    {
        var panelObj = Object.Instantiate(Templates.SettingsPanel, parent, false);
        panelObj.GetComponentInChildren<TMP_Text>(true).text = label;

        return panelObj.GetComponent<RectTransform>();
    }

    public static UI_TabGroup CreateTabGroup(RectTransform parent, GameObject target = null, bool vertical = false,
        bool supportGamepad = true, Color? closeColor = null, Color? openColor = null, string tabLeftButton = "TabMainLeft", string tabRightButton = "TabMainRight")
    {
        GameObject tabGroupObj = target;
        if (!tabGroupObj)
        {
            tabGroupObj = new GameObject("TabGroup");
            tabGroupObj.AddComponent<RectTransform>().SetParent(parent, false);
        }

        tabGroupObj.AddComponent<CanvasGroup>();
        var tabGroup = tabGroupObj.AddComponent<UI_TabGroup>();
        var transform = tabGroupObj.transform as RectTransform;
        tabGroup.switchTabSound = Templates.TabGroup.switchTabSound;
        tabGroup.openColor = openColor ?? new Color(0.6981f, 0.1153f, 0.1153f, 1);
        tabGroup.closeColor = closeColor ?? new Color(0.2642f, 0.2642f, 0.2642f, 1);
        tabGroup.EventOnChangeTab = new UnityEvent();

        HorizontalOrVerticalLayoutGroup layout = tabGroupObj.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (vertical && !layout)
            layout = tabGroupObj.AddComponent<VerticalLayoutGroup>();
        else if (!layout)
            layout = tabGroupObj.AddComponent<HorizontalLayoutGroup>();

        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleLeft;

        if (supportGamepad)
        {
            tabGroup.tabLeftButton = tabLeftButton;
            tabGroup.tabRightButton = tabRightButton;
            Object.Instantiate(Templates.TabGroup.gameObject.Search("LB"), transform);
            Object.Instantiate(Templates.TabGroup.gameObject.Search("RB.02"), transform);
        }
        else
        {
            tabGroup.tabLeftButton = "";
            tabGroup.tabRightButton = "";
        }

        return tabGroup;
    }

    public static Button AddTab(this UI_TabGroup tabGroup, GameObject template, string text, GameObject targetObject,
        Selectable firstSelect = null)
    {
        var tabGameObject = Object.Instantiate(template, tabGroup.transform, false);
        tabGameObject.name = text;
        if (tabGroup.tabs == null || tabGroup.tabs.Count == 0)
        {
            if (tabGroup.transform.childCount > 0)
                tabGameObject.transform.SetSiblingIndex(tabGroup.transform.GetChild(0).GetSiblingIndex() + 1);
        }
        else
            tabGameObject.transform.SetSiblingIndex(tabGroup.tabs.Last().button.transform.GetSiblingIndex() + 1);

        tabGameObject.GetComponentInChildren<TMP_Text>(true).text = text;
        var tabButton = tabGameObject.GetComponent<Button>();

        var tab = new UI_TabGroup.Tab
        {
            name = text,
            button = tabButton,
            tabObject = targetObject,
            firstSelect = firstSelect,
        };

        tabGroup.tabs ??= new List<UI_TabGroup.Tab>();
        tabGroup.tabs.Add(tab);
        tabButton.onClick.RemoveAllListeners();
        tabButton.onClick.AddListener(() => tabGroup.SelectTab(tab.name));

        return tabButton;
    }

    public static ScrollRect CreateScrollRect(RectTransform parent, bool supportGamepad = false)
    {
        var rectObj = Object.Instantiate(Templates.ScrollRect, parent, false);
        var scrollRect = rectObj.GetComponent<ScrollRect>();
        if (!supportGamepad)
        {
            Object.Destroy(rectObj.gameObject.Search("Scroll Holder/Scroll Right Button/RB"));
            Object.Destroy(rectObj.gameObject.Search("Scroll Holder/Scroll Left Button/LB"));
            Object.Destroy(rectObj.gameObject.Search("Scroll Holder/Scroll Right Button").GetComponent<UI_PressButtonOnInput>());
            Object.Destroy(rectObj.gameObject.Search("Scroll Holder/Scroll Left Button").GetComponent<UI_PressButtonOnInput>());
        }

        scrollRect.content.anchorMin = new Vector2(0, 0);
        scrollRect.content.anchorMax = new Vector2(0, 1);
        scrollRect.content.anchoredPosition = new Vector2(0, 0);
        scrollRect.content.sizeDelta = Vector2.zero;

        return scrollRect;
    }

    public static TMP_Text CreateLabel(RectTransform parent, string text)
    {
        var labelObj = Object.Instantiate(Templates.Label, parent, false);
        labelObj.text = text;

        return labelObj;
    }
}