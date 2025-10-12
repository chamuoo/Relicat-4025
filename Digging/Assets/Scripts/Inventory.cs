using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif
using Unity.VisualScripting;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using Spine;
using UnityEngine.SceneManagement;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public Collection collection;

    public List<Item> items;
    public int money;
    public Item money_item;
    public TextMeshProUGUI money_text;

    [SerializeField]
    private Transform slotParent;
    [SerializeField]
    private Slot[] slots;
    public Slot[] Slots { get { return slots; } }

    [SerializeField] private GameObject getItem_LogPanel;
    [SerializeField] private Image getItem_Image;
    [SerializeField] private TextMeshProUGUI getItem_Name;
    [SerializeField] private TextMeshProUGUI getItem_EA;
    private int dupleLog = 0;
    private int dupleMoneyLog = 0;

    public GameObject badgePanel;
    public GameObject moneyPanel;
    public GameObject healthPanel;

    [SerializeField] private GameObject logMessage_Panel;
    [SerializeField] private TextMeshProUGUI logText;

#if UNITY_EDITOR
    private void OnValidate()
    {
        slots = slotParent.GetComponentsInChildren<Slot>();
    }
#endif

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(this.gameObject); // 중복 방지
        }

        FreshSlot();
    }

    public static Inventory Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    // 슬롯 새로고침
    public void FreshSlot()
    {
        int i = 0;
        for(; i < items.Count && i < slots.Length; i++)
        {
            slots[i].item = items[i];

            // UI 텍스트 업데이트
            Transform parent = slots[i].transform.parent;
            TextMeshProUGUI text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = items[i].count.ToString();
        }

        for(; i < slots.Length; i++)
        {
            slots[i].item = null;

            if(slots[i] == null || slots[i].gameObject == null) continue;


            // 빈 슬롯이면 텍스트 비우기
            Transform parent = slots[i].transform.parent;
            TextMeshProUGUI text = parent.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "";
        }

        money_text.text = money_item.count.ToString() + " 냥";

    }

    // addEA: 갯수, _item: Item Object
    public void AddItem(Item _item, int addEA)
    {
        foreach (Item item in items)
        {
            if (item.itemName == _item.itemName)
            {
                _item.count += addEA;
                _item.ishaveitem = true;
                for(int i = 0; i < items.Count; i++)
                {
                    if(slots[i].item.itemName == _item.itemName)
                    {
                        Transform parent = slots[i].transform.parent;
                        TextMeshProUGUI text = parent.GetComponentInChildren<TextMeshProUGUI>();
                        Debug.Log(text);
                        text.text = item.count.ToString();
                        ItemLog(_item, addEA);
                    }
                }

                FreshSlot();
                return;
            }
        }

        // 새로운 아이템 추가 (슬롯 여유가 있을 경우)
        if (items.Count < slots.Length)
        {
            items.Add(_item);
            FreshSlot();
            _item.count += addEA;
            _item.ishaveitem = true;
            for (int i = 0; i < items.Count; i++)
            {
                if (slots[i].item.itemName == _item.itemName)
                {
                    Transform parent = slots[i].transform.parent;
                    TextMeshProUGUI text = parent.GetComponentInChildren<TextMeshProUGUI>();
                    Debug.Log(text);
                    text.text = _item.count.ToString();
                    ItemLog(_item, addEA);
                }
            }
            FreshSlot();
        }
        else
        {
            Debug.Log("슬롯이 가득 차 있습니다.");
        }
    }

    // 보유중인 아이템 초기화
    public void ClearItem()
    {
        foreach (Item item in items)
        {
            item.count = 0;
        }
        for (int i = 0; i < items.Count; i++)
        {
            Transform parent = slots[i].transform.parent;
            TextMeshProUGUI text = parent.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log(text);
            text.text = "";
        }

        items.Clear();
        FreshSlot();

        money = 0;
        
    }

    // 특정 아이템 판매
    public void SellItem(Item _item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == _item.itemName)
            {
                if (items[i].count <= 0)
                {
                    items[i].count = 0;
                    items.RemoveAt(i);
                }
                else
                {
                    items[i].count--;
                    if (items[i].isMineral == true)
                    {
                        money_item.count += _item.value;
                        ItemLog(money_item, _item.value);
                    }
                    else if (items[i].isalreadySell == false)
                    {
                        money_item.count += _item.value;
                        items[i].isalreadySell = true;
                        ItemLog(money_item, _item.value);
                    }
                    else
                    {
                        money_item.count += _item.duplicate_value;
                        ItemLog(money_item, _item.duplicate_value);
                    }
                    if (items[i].count <= 0)
                    {
                        items[i].count = 0;
                        items.RemoveAt(i);
                    }
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[32]);
                }
                   
                
                Debug.Log(money_item.count);


                
                break; // 아이템은 유일하다고 가정
            }
        }
        

        FreshSlot(); // 리스트 변경 후 UI 갱신
    }

    // 특정 아이템 전부 판매
    public void SellAllItem(Item _item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == _item.itemName)
            {
                int idx = items[i].count;
                for (int j = 0; j < idx; j++)
                {
                    if (items[i].count <= 0)
                    {
                        items[i].count = 0;
                        items.RemoveAt(i);
                    }
                    else
                    {
                        items[i].count--;
                        if (items[i].isMineral == true)
                        {
                            money_item.count += _item.value;
                            ItemLog(money_item, _item.value);
                        }
                        else if (items[i].isalreadySell == false)
                        {
                            money_item.count += _item.value;
                            items[i].isalreadySell = true;
                            ItemLog(money_item, _item.value);
                        }
                        else
                        {
                            money_item.count += _item.duplicate_value;
                            ItemLog(money_item, _item.duplicate_value);
                        }
                        if (items[i].count <= 0)
                        {
                            items[i].count = 0;
                            items.RemoveAt(i);
                        }
                    }
                }
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[32]);
                Debug.Log(money_item.count);
                break; // 아이템은 유일하다고 가정
            }
        }
        

        FreshSlot(); // 리스트 변경 후 UI 갱신
    }

    // 아이템 획득 로그
    public void ItemLog(Item _item, int addEA)
    {
        // 같은 아이템 연속 획득 시 
        if(getItem_Name.text == _item.itemName)
        {
            if (getItem_LogPanel.activeSelf == false)
            {
                
                getItem_EA.text = "X " + addEA.ToString();
            }
            else if (_item == money_item)
            {
                
                getItem_EA.text = "X " + (dupleMoneyLog + addEA).ToString();
                dupleMoneyLog += addEA;
            }
            else
            {
                dupleLog++;
                getItem_EA.text = "X " + (addEA + dupleLog).ToString();
            }
            
        }
        // 다른 아이템 획득 시
        else
        {
            if(getItem_Image != null)
            {
                Image image = getItem_Image.GetComponent<Image>();
                if(image != null)
                    image.sprite = _item.itemImage;
            }

            if (_item.isRelic == true)
            {
                for(int i = 0; i < collection.player.items.Count; i++)
                {
                    if (collection.player.items[i].itemName == _item.itemName)
                    {
                        if(collection.li_isRelicOnTable[i] == false)
                        {
                            getItem_Name.text = "???";
                            LogMessage("???" + "을(를) 획득하였습니다." + "\n박물관에 등록하여 목표를 달성하세요!");
                        }
                        else
                        {
                            getItem_Name.text = _item.itemName;
                            LogMessage(_item.itemName + "을(를) 획득하였습니다.");
                        }
                    }
                }
                
            }
            else
            {
                getItem_Name.text = _item.itemName;
            }

            if(_item == money_item)
            {
                dupleMoneyLog += addEA;
            }
            else
            {
                dupleMoneyLog = 0;
            }
            getItem_EA.text = "X " + addEA.ToString();

            dupleLog = 0;
        }

        getItem_LogPanel.SetActive(true);

        CancelInvoke("closeItemLog");
        Invoke("closeItemLog", 3f);
    }

    // 일정 시간 뒤 로그 패널 닫기
    private void closeItemLog()
    {
        getItem_LogPanel.SetActive(false);
        getItem_Name.text = "";
        getItem_EA.text = "";
        dupleLog = 0;
        dupleMoneyLog = 0;
    }

    public void LogMessage(string log)
    {
        logText.text = log;

        logMessage_Panel.SetActive(true);

        CancelInvoke("closeLogMessage");
        Invoke("closeLogMessage", 3f);
    }

    private void closeLogMessage()
    {
        logMessage_Panel.SetActive(false);
        logText.text = "";
    }
}
