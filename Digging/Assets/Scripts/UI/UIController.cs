using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class UIRef
{
    public string name;
    public GameObject obj;
}

public class UIController : Singleton<UIController>
{
    /// <게임오브젝트_이름_index>
    /// 0: QuickSlot
    /// 1: Depth
    /// </게임오브젝트_이름_index>
    [SerializeField] List<UIRef> uiReferences;
    
    readonly Dictionary<string, GameObject> uiDict = new();

    public void Initialize()
    {
        foreach(UIRef reference in uiReferences)
        {
            reference.obj = GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == reference.name).gameObject;
            uiDict[reference.name] = reference.obj;
        }

        SetObjectActive(1, true);
        SetObjectActive(0, true);    // 퀵슬롯 
    }

    public void SetText(int index, string message)
    {
        TextMeshProUGUI textComponent = GetComponent<TextMeshProUGUI>(index);

        if(textComponent != null)
            textComponent.text = message;
        else
            Debug.Log($"[UIManager] '{name}' 오브젝트에 TextMeshProUGUI 컴포넌트가 없습니다.");
    }

    // 슬롯의 텍스트 변경
    public void SetSlotText(SlotInfo slot, int amount)
    {
        TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();

        if(amount <= 0)
        {
            slot.ClearSlot();
            text.text = 0.ToString();
            text.enabled = false;
            return;
        }

        int typeValue = (int)slot._type;

        if((typeValue & (int)ItemCategory.Weapon) == (int)ItemCategory.Weapon)
        {
            text.enabled = false;
        }
        else if((typeValue & (int)ItemCategory.Item) == (int)ItemCategory.Item)
        {
            text.enabled = true;
            text.text = amount.ToString();
        }
        else
        {
            text.enabled = false; // 필요하면 기본 처리
        }
    }

    // 해당 인덱스의 게임오브젝트 가져오기
    public GameObject GetGameObject(int index)
    {
        // 아무런 데이터가 없는 경우
        if(!uiDict.TryGetValue(uiReferences[index].name, out GameObject obj) || obj == null)
            return null;

        return obj;
    }

    // 해당 인덱스의 게임오브젝트 타입 가져오기
    public T GetComponent<T>(int index) where T : Component
    {
        // 아무런 데이터가 없는 경우
        if(!uiDict.TryGetValue(uiReferences[index].name, out GameObject obj) || obj == null)
            return null;

        T targetObject = obj.GetComponent<T>();

        return targetObject;
    }

    public void SetObjectActive(int index, bool active)
    {
        if(uiDict.ContainsKey(uiReferences[index].name))
            uiDict[uiReferences[index].name].SetActive(active);
    }  
}
