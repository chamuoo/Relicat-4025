using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.ShaderGraph.Internal;
#endif

using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    private Slider slider;
    private Image fill;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        fill = transform.GetChild(1).GetChild(0).GetComponent<Image>();
    }

    public void SetValue(float value)
    {
        slider.value = value / 100f;
        print("¿¡³ÊÁö: " + value + slider.value);

        slider.gameObject.SetActive(slider.value < 1.0f);

        Color fillColor;
        if(slider.value > 0.5f)
            fillColor = Color.Lerp(Color.yellow, Color.green, (value - 0.5f) * 2f);
        else
            fillColor = Color.Lerp(Color.red, Color.yellow, value * 2f);

        fill.color = fillColor;
    }

    public void SetMax(float max)
    {
        slider.maxValue = max / 100f;
    }
}
