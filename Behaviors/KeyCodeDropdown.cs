using System;
using System.Linq;
using TMPro;
using UnityEngine;
using WKLib.API.Input;

namespace ModMenu.Behaviors;

public class KeyCodeDropdown: MonoBehaviour
{
    private TMP_Dropdown _dropdown;

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnGUI()
    {
        if (!_dropdown.IsExpanded) return;
        if(Event.current.type != EventType.KeyDown) return;

        int index = _dropdown.options.FindIndex(o => o.text == Event.current.keyCode.ToString());
        if (index != -1 && Event.current.keyCode != KeyCode.None && Event.current.keyCode != KeyCode.Escape)
        {
            _dropdown.value = index;
            _dropdown.Hide();
        }
    }
}