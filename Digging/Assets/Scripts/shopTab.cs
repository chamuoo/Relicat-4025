using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class shopTab : MonoBehaviour
{
    [Header("탭 UI들 (순서 중요 X)")]
    [SerializeField] private RectTransform[] tabs;

    [Header("탭 버튼들 (선택사항)")]
    [SerializeField] private Button[] tabButtons;

    private int currentTab = 0;

    private void Start()
    {
        // 버튼이 있다면 클릭 이벤트 연결
        //if (tabButtons != null && tabButtons.Length == tabs.Length)
        //{
        //    for (int i = 0; i < tabButtons.Length; i++)
        //    {
        //        int index = i;
        //        tabButtons[i].onClick.AddListener(() => ShowTab(index));
        //    }
        //}

        //ShowTab(0); // 시작 시 첫 번째 탭 표시
    }

    /// <summary>
    /// 지정한 인덱스의 탭을 맨 위로 올림
    /// </summary>
    public void ShowTab(int index)
    {
        if (index < 0 || index >= tabs.Length) return;

        // 선택된 탭을 맨 위로 올림
        tabs[index].SetAsLastSibling();

        currentTab = index;

        Shop.instance.shopView_idx = currentTab;

        Debug.Log($"탭 전환: {tabs[index].name}");
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[30]);
    }

    /// <summary>
    /// 다음 탭으로 순환
    /// </summary>
    public void NextTab()
    {
        int next = (currentTab + 1) % tabs.Length;
        ShowTab(next);
    }
}
