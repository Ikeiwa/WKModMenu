using ModMenu.Behaviors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ModMenu;

public static class Templates
{
    public static GameObject Button;
    public static GameObject MainTabButton;
    public static GameObject Toggle;
    public static GameObject Dropdown;
    public static GameObject Slider;
    public static GameObject TabButton;
    public static GameObject SettingsPanel;
    public static GameObject InputField;
    public static UI_TabGroup TabGroup;
    public static ScrollRect ScrollRect;
    public static TMP_Text Label;
    public static KeyCodeInput KeyBindingInput;

    private static RectTransform _settingsRoot;
    private static GameObject _templateContainer;
    private static bool _loaded;

    public static void LoadTemplates(Transform target)
    {
        if (_loaded) return;

        _templateContainer = new GameObject("[TemplateContainer]");
        Object.DontDestroyOnLoad(_templateContainer);
        _templateContainer.SetActive(false);

        _settingsRoot = target.Search("SettingsParent/Settings Pane")?.GetComponent<RectTransform>();
        if (!CheckValidity(_settingsRoot, "settingsRoot")) return;

        Button = MakeTemplate(_settingsRoot.Search("Save And Close"));
        Button.name = "Button";
        if (!CheckValidity(Button, "button")) return;
        Object.Destroy(Button.GetComponent<UI_CloseButtonOnBack>());
        Button.GetComponent<UT_ButtonAudio>().onSubmit = false;

        MainTabButton = MakeTemplate(_settingsRoot.Search("Tab Selection Hor/Accessibility"));
        if (!CheckValidity(MainTabButton, "mainTabButton")) return;
        MainTabButton.name = "MainTabButton";
        MainTabButton.GetComponent<Button>().onClick.RemoveAllListeners();

        Toggle = MakeTemplate(_settingsRoot.Search("Video Settings/Options Tab/Video/Fullscreen Toggle"));
        if (!CheckValidity(Toggle, "toggle")) return;
        Toggle.name = "Toggle";
        Object.Destroy(Toggle.GetComponent<ToggleSettingsBinder>());

        Dropdown = MakeTemplate(_settingsRoot.Search("Video Settings/Options Tab/Video/Screen Resolution"));
        if (!CheckValidity(Dropdown, "dropdown")) return;
        Dropdown.name = "Dropdown";
        Object.Destroy(Dropdown.GetComponent<Settings_Resolution>());
        var dropdownComp = Dropdown.GetComponentInChildren<TMP_Dropdown>(true);
        dropdownComp.ClearOptions();
        
        // There is no vanilla inputField, so we'll build one from a dropdown
        InputField = MakeTemplate(Dropdown);
        if (!CheckValidity(InputField, "InputField")) return;
        InputField.name = "InputField";
        var inputFieldRoot = InputField.transform.GetChild(0);
        inputFieldRoot.name = "Text Area";
        inputFieldRoot.gameObject.AddComponent<RectMask2D>();
        Object.Destroy(inputFieldRoot.GetComponent<TMP_Dropdown>());
        Object.Destroy(inputFieldRoot.Search("Arrow"));
        Object.Destroy(inputFieldRoot.Search("Template"));
        Object.Destroy(InputField.GetComponent<Settings_Resolution>());
        var inputFieldText = inputFieldRoot.Search("Label");
        var inputPlaceholder = Object.Instantiate(inputFieldText, inputFieldText.transform.parent, false)
            .GetComponent<TMP_Text>();
        inputPlaceholder.color = new Color(1,1,1,0.5f);
        var inputFieldComp = InputField.AddComponent<TMP_InputField>();
        inputFieldComp.textViewport = inputFieldRoot.transform as RectTransform;
        inputFieldComp.textComponent = inputFieldText.GetComponent<TMP_Text>();
        inputFieldComp.placeholder = inputPlaceholder;
        inputFieldComp.fontAsset = inputPlaceholder.font;
        inputFieldComp.pointSize = inputPlaceholder.fontSize;
        
        KeyBindingInput = MakeTemplate(Dropdown).AddComponent<KeyCodeInput>();
        if (!CheckValidity(KeyBindingInput, "KeyBindingInput")) return;
        KeyBindingInput.gameObject.name = "KeyBindingInput";
        var keyBindingRoot = KeyBindingInput.transform.GetChild(0);
        keyBindingRoot.gameObject.AddComponent<RectMask2D>();
        Object.Destroy(keyBindingRoot.GetComponent<TMP_Dropdown>());
        Object.Destroy(keyBindingRoot.Search("Arrow"));
        Object.Destroy(keyBindingRoot.Search("Template"));
        Object.Destroy(KeyBindingInput.GetComponent<Settings_Resolution>());
        var keyBindingButton = KeyBindingInput.gameObject.AddComponent<Button>();
        keyBindingButton.targetGraphic = keyBindingRoot.GetComponent<Graphic>();
        keyBindingButton.colors = dropdownComp.colors;

        Slider = MakeTemplate(_settingsRoot.Search("Video Settings/Options Tab/Video/SliderAsset - Brightness"));
        if (!CheckValidity(Slider, "slider")) return;
        Slider.name = "Slider";
        Object.Destroy(Slider.GetComponentInChildren<SliderSettingBinder>(true));
        var sliderText = Slider.GetComponentInChildren<TextSettingsBinder>(true);
        sliderText.gameObject.AddComponent<SliderTextSync>();
        Object.Destroy(sliderText);

        TabButton = MakeTemplate(_settingsRoot.Search("Controls Page/Controls Page Tab Selector/Options"));
        if (!CheckValidity(TabButton, "tabButton")) return;
        TabButton.gameObject.name = "TabButton";

        Label = MakeTemplate(_settingsRoot.Search("Video Settings/Options Tab/Video/Video Settings")).GetComponent<TMP_Text>();
        if (!CheckValidity(Label, "Label")) return;
        Label.gameObject.name = "Label";

        SettingsPanel = MakeTemplate(_settingsRoot.Search("Video Settings"));
        if (!CheckValidity(SettingsPanel, "SettingsPanel")) return;
        SettingsPanel.name = "SettingsPanel";
        Object.Destroy(SettingsPanel.Search("Options Tab"));

        TabGroup = MakeTemplate(_settingsRoot.Search("Tab Selection Hor")).GetComponent<UI_TabGroup>();
        if (!CheckValidity(TabGroup, "TabGroup")) return;
        TabGroup.gameObject.name = "TabGroup";
        
        ScrollRect = MakeTemplate(target.parent.parent.Search("Canvas - Screen - Play/Play Menu/Play Pane/Tab Objects/Play Pane - Scroll View Tab - Endless Variant")).GetComponent<ScrollRect>();
        if (!CheckValidity(ScrollRect, "ScrollRect")) return;
        ScrollRect.gameObject.name = "ScrollRect";
        var scrollRectContent = ScrollRect.content;
        for (int i = scrollRectContent.childCount - 1; i >= 0; i--)
            Object.Destroy(scrollRectContent.GetChild(i).gameObject);
        scrollRectContent.GetComponent<ContentSizeFitter>().enabled = true;

        _loaded = true;
    }

    private static bool CheckValidity(Object obj, string name)
    {
        if (!obj)
        {
            Plugin.Log.LogError(name + " is null");
            return false;
        }

        return true;
    }

    private static GameObject MakeTemplate(GameObject source)
    {
        if (!source) return null;
        var template = Object.Instantiate(source, _templateContainer.transform, false);
        template.SetActive(true);
        return template;
    }
}