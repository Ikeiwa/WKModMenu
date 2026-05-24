using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace ModMenu.Behaviors;

public class KeyCodeInput: MonoBehaviour
{
    private static bool _lockInputBinding;
    
    private TMP_Text _text;
    private UI_RebindOverlay _uiRebindOverlay;
    private UI_CloseButtonOnBack _closeButtonOnBack;
    private bool _waitingForNoInput;
    private bool _listeningForKey;
    private KeyCode _key;
    
    public UnityEvent<KeyCode> onValueChanged;

    private void Awake()
    {
        _text = transform.GetChild(0).GetComponentInChildren<TMP_Text>(true);
        GetComponent<Button>().onClick.AddListener(StartListening);
    }

    public void Setup(Transform settingsMenu)
    {
        _uiRebindOverlay = settingsMenu.GetComponentInParent<UI_SettingsMenu>(true)?.GetComponentInChildren<UI_RebindOverlay>(true);
        _closeButtonOnBack = settingsMenu.GetComponentInChildren<UI_CloseButtonOnBack>(true);
    }

    public void StartListening()
    {
        if (_lockInputBinding) return;
        _waitingForNoInput = true;
        _text.text = "Waiting for key press";

        if (_uiRebindOverlay)
        {
            _uiRebindOverlay.gameObject.SetActive(true);
            _uiRebindOverlay.GetComponentInChildren<Text>(true).text = "Waiting for button input.";
        }

        if (_closeButtonOnBack)
            _closeButtonOnBack.enabled = false;
    }

    private void Update()
    {
        if (_waitingForNoInput)
        {
            if (WKLib.API.Input.InputUtility.GetFirstActiveKey() == null)
            {
                _waitingForNoInput = false;
                _listeningForKey = true;
                _lockInputBinding = true;
                return;
            }
        }

        if (_listeningForKey)
        {
            var key = WKLib.API.Input.InputUtility.GetFirstActiveKey();
            if (key == null || key == KeyCode.None) return;
            
            if(key == KeyCode.Escape)
                key = KeyCode.None;

            _key = key.Value;
            _text.text = key.ToString();
            _listeningForKey = false;
            _lockInputBinding = false;
            onValueChanged?.Invoke(_key);
            
            if (_uiRebindOverlay)
            {
                _uiRebindOverlay.gameObject.SetActive(false);
            }
            if (_closeButtonOnBack)
                _closeButtonOnBack.enabled = true;
        }
    }

    public void SetValueWithoutNotify(KeyCode key)
    {
        _key = key;
        _text.text = key.ToString();
    }
}