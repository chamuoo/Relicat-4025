using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.ShaderGraph.Internal;
#endif

using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetValue(float value)
    {
        // 내구도가 깎인 경우만 슬라이더 보이기
        if(slider.value < 1f)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);

        // Fill 이미지 
        Transform fill = transform.GetChild(1).GetChild(0);
        Image image = fill.GetComponent<Image>();

        Color fillColor;

        if(slider.value > 0.5f)
            fillColor = Color.Lerp(Color.yellow, Color.green, (value - 0.5f) * 2f);
        else
            fillColor = Color.Lerp(Color.red, Color.yellow, value * 2f);

        image.color = fillColor;
    }

    public void SetMax(float max)
    {
        slider.maxValue = max / 100f;
    }
}
