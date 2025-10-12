using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Slider hpBar;

    float currentHP = 1;
    public float CurrentHP => currentHP;

    private void Start()
    {
        hpBar.value = CurrentHP;
    }

    private void Update()
    {
        transform.position = player.transform.position;
        hpBar.value = Mathf.Lerp(hpBar.value, currentHP, Time.deltaTime * 5f);
    }

    // HP Bar Value 변화
    public void UpdateHP(float hp)
    {
        hp = Mathf.Round((hp / 100) * 100) / 100f;  // 둘째자리까지 나오게 하기
        currentHP = hp;
    }
    
}
