using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Slider hpBar;
    [SerializeField] private Image HP_fillimage;

    [SerializeField] float targetHP;

    private void Awake()
    {
        // 슬라이더에서 Fill 이미지 찾아서 저장
        HP_fillimage = GetComponentInChildren<Slider>()
            .fillRect.GetComponent<Image>();
    }

    private void Update()
    {
        transform.position = player.transform.position;
        hpBar.value = Mathf.Lerp(hpBar.value, targetHP, Time.deltaTime * 5f);

        float displayRatio = hpBar.value;

        if(displayRatio > 0.7f)
            HP_fillimage.color = Color.green;  // 안정
        else if(displayRatio > 0.4f)
            HP_fillimage.color = Color.yellow; // 경고
        else 
            HP_fillimage.color = Color.red;    // 위험
    }


    // HP Bar Value 변화
    public void UpdateHP(float curHP)
    {
        targetHP = curHP;
    }

}
