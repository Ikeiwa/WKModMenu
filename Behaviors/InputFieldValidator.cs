using System;
using TMPro;
using UnityEngine;

namespace ModMenu.Behaviors;

public class InputFieldValidator : MonoBehaviour
{
    private TMP_InputField _inputField;
    private string _originalText;
    
    private void Start()
    {
        _inputField = GetComponent<TMP_InputField>();
        _inputField.onSelect.AddListener(OnStartEdit);
        _inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnStartEdit(string text)
    {
        _originalText = text;
    }

    private void OnEndEdit(string text)
    {
        if (_inputField.contentType == TMP_InputField.ContentType.DecimalNumber)
        {
            if(!float.TryParse(_inputField.text, out _) && float.TryParse(_originalText, out _))
                _inputField.text = _originalText;
        }
        else if (_inputField.contentType == TMP_InputField.ContentType.IntegerNumber)
        {
            if(!int.TryParse(_inputField.text, out _) && int.TryParse(_originalText, out _))
                _inputField.text = _originalText;
        }
    }
}