using System.Globalization;
using DarkMachine.UI;
using TMPro;
using UnityEngine;

namespace ModMenu.Behaviors;

public class SliderTextSync : MonoBehaviour
{
    private TMP_Text _text;
    private string _format;

    public void Setup(SubmitSlider slider, string format = "0.00")
    {
        _format = format;
        _text = GetComponent<TMP_Text>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        OnSliderValueChanged(slider.value);
    }

    private void OnSliderValueChanged(float value)
    {
        _text.text = value.ToString(_format, CultureInfo.InvariantCulture);
    }
}