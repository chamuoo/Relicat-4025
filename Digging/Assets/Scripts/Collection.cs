using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif

public class Collection : MonoBehaviour
{
    public static Collection instance;

    public Inventory Inventory;
    public Player player;

    public GameObject[] Collection_Table;

    public bool[] li_isCollect;
    public bool[] li_isRelicOnTable;

    [SerializeField]
    private Transform slotParent;
    [SerializeField]
    public Slot[] slots;

    private int collectView_idx;
    [SerializeField] private GameObject[] collectUIList;

    [SerializeField] private GameObject[] mouseOverlap_Panel_List;
    [SerializeField] private Button[] rewardButton_List;
    [SerializeField] private Item[] badge_items;

    public int collect_sum;

    public int player_lv;
    public int collect_count;
    public bool[] is_collect_complete;
    public GameObject badgeUI_Icon;
    public TextMeshProUGUI badgeUI_TitleText;

    public Item guessItem;

    [SerializeField] private Button[] resistButton_List;

    [SerializeField] private GameObject MainPanel;
    [SerializeField] private GameObject[] stagePanels;

    //#if UNITY_EDITOR
    //    private void OnValidate()
    //    {
    //        slots = slotParent.GetComponentsInChildren<Slot>();
    //    }
    //#endif

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded1;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        li_isCollect = new bool[player.items.Count];
        li_isRelicOnTable = new bool[player.items.Count];

        is_collect_complete = new bool[5];
        player_lv = 0;
        collect_count = 0;

        //collectView_idx = 0;
        //Switch_CollectView();
    }

    private void OnDestroy()
    {
        // 이벤트 제거 (중복 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded1;
    }

    private void OnSceneLoaded1(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드된 후 Player 다시 찾기
        player = FindObjectOfType<Player>();

        if(player != null)
        {
            Debug.Log("씬 전환 후 Player 연결 완료: " + player.name);
        }
        else
        {
            Debug.Log("씬 전환 후 Player를 찾지 못했습니다.");
        }

        GameObject museum = GameObject.Find("Museum");
        if(museum != null)
        {
            Transform collectionParent = museum.transform.Find("CollectionTable");
            if(collectionParent != null)
            {
                Collection_Table = new GameObject[30];
                for(int i = 0; i < 30; i++)
                {
                    Transform child = collectionParent.Find(i.ToString());
                    if(child != null)
                    {
                        Collection_Table[i] = child.gameObject;
                    }
                    else
                    {
                        Debug.Log($"CollectionTable[{i}] 오브젝트를 찾을 수 없습니다.");
                    }
                }

                Debug.Log("Collection_Table 연결 완료.");
            }
            else
            {
                Debug.Log("Museum 하위에 'CollectionTable' 오브젝트가 없습니다.");
            }
        }
        else
        {
            Debug.Log("'Museum' 오브젝트를 찾을 수 없습니다.");
        }



    }

    // Update is called once per frame
    void Update()
    {
        Badge_nameTag_Update();
        ResistButtonActive();
    }

    public void Badge_nameTag_Update()
    {
        switch(player_lv)
        {
            case 0:
                badgeUI_Icon.GetComponent<Image>().sprite = badge_items[0].itemImage;
                badgeUI_TitleText.text = "입문자";
                break;
            case 1:
                badgeUI_Icon.GetComponent<Image>().sprite = badge_items[1].itemImage;
                badgeUI_TitleText.text = "초심자";
                break;
            case 2:
                badgeUI_Icon.GetComponent<Image>().sprite = badge_items[1].itemImage;
                badgeUI_TitleText.text = "수집가";
                break;
            case 3:
                badgeUI_Icon.GetComponent<Image>().sprite = badge_items[2].itemImage;
                badgeUI_TitleText.text = "전문가";
                break;
            case 4:
                badgeUI_Icon.GetComponent<Image>().sprite = badge_items[2].itemImage;
                badgeUI_TitleText.text = "큐레이터";
                break;
            case 5:
                badgeUI_Icon.GetComponent<Image>().sprite = badge_items[3].itemImage;
                badgeUI_TitleText.text = "고고학자";
                break;

        }
    }

    public void collection_Lv_Check()
    {
        collect_count = 0;
        switch(player_lv)
        {
            case 0:
                for(int i = 0; i < 10; i++)
                {
                    if(li_isRelicOnTable[i] == true)
                    {
                        collect_count++;
                    }
                }
                if(collect_count == 10)
                {
                    is_collect_complete[player_lv] = true;
                    rewardButton_List[0].GetComponent<Button>().interactable = true;
                }
                break;
            case 1:
                for(int i = 0; i < 20; i++)
                {
                    if(li_isRelicOnTable[i] == true)
                    {
                        collect_count++;
                    }
                }
                if(collect_count == 20)
                {
                    is_collect_complete[player_lv] = true;
                    rewardButton_List[1].GetComponent<Button>().interactable = true;
                }
                break;
            case 2:
                for(int i = 0; i < 30; i++)
                {
                    if(li_isRelicOnTable[i] == true)
                    {
                        collect_count++;
                    }
                }
                if(collect_count == 30)
                {
                    is_collect_complete[player_lv] = true;
                    rewardButton_List[2].GetComponent<Button>().interactable = true;
                }
                break;
            case 3:

                break;
            case 4:

                break;
            case 5:

                break;

        }
    }

    public void ResistButtonActive()
    {
        if(player != null)
        {
            if(player.isInMuseum)
            {
                for(int i = 0; i < player.items.Count; i++)
                {
                    if(player.items[i].count > 0)
                    {
                        resistButton_List[i].interactable = true;
                    }
                    else
                    {
                        resistButton_List[i].interactable = false;
                    }
                }
            }
        }
    }

    public void Button_ResistRelic(int itemNum)
    {


        if(player.items[itemNum].count > 0)
        {
            li_isRelicOnTable[itemNum] = true;
            player.items[itemNum].accumulation_count += 1;
            Debug.Log(player.items[itemNum].accumulation_count);
            UpdateOnTable(itemNum);
            collect_sum += 1;
            Inventory.SellItem(player.items[itemNum]);

            collection_Lv_Check();
            Inventory.LogMessage("등록 되었습니다.");
        }

    }

    public void UpdateOnTable(int itemNum)
    {

        for(int i = 0; i < li_isRelicOnTable.Length; i++)
        {
            if(li_isRelicOnTable[i] == true)
            {
                Collection_Table[itemNum].GetComponentInChildren<SpriteRenderer>().sprite = player.items[itemNum].itemImage;

            }
        }

    }


    //public void Button_Left()
    //{
    //    collectView_idx -= 1;
    //    if (collectView_idx < 0)
    //    {
    //        collectView_idx = 1;
    //    }
    //    Switch_CollectView();
    //    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[30]);
    //}


    //public void Button_Right()
    //{
    //    collectView_idx += 1;
    //    if (collectView_idx > 1)
    //    {
    //        collectView_idx = 0;
    //    }
    //    Switch_CollectView();
    //    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[30]);
    //}


    //private void Switch_CollectView()
    //{
    //    switch (collectView_idx)
    //    {
    //        case 0:
    //            collectUIList[0].SetActive(true);
    //            collectUIList[1].SetActive(false);

    //            break;
    //        case 1:
    //            collectUIList[0].SetActive(false);
    //            collectUIList[1].SetActive(true);

    //            break;

    //    }
    //}

    public void MouseOverIn_01()
    {
        mouseOverlap_Panel_List[0].SetActive(true);
    }
    public void MouseOverOut_01()
    {
        mouseOverlap_Panel_List[0].SetActive(false);
    }
    public void MouseOverIn_02()
    {
        mouseOverlap_Panel_List[1].SetActive(true);
    }
    public void MouseOverOut_02()
    {
        mouseOverlap_Panel_List[1].SetActive(false);
    }
    public void MouseOverIn_03()
    {
        mouseOverlap_Panel_List[2].SetActive(true);
    }
    public void MouseOverOut_03()
    {
        mouseOverlap_Panel_List[2].SetActive(false);
    }
    public void MouseOverIn_04()
    {
        mouseOverlap_Panel_List[3].SetActive(true);
    }
    public void MouseOverOut_04()
    {
        mouseOverlap_Panel_List[3].SetActive(false);
    }
    public void MouseOverIn_05()
    {
        mouseOverlap_Panel_List[4].SetActive(true);
    }
    public void MouseOverOut_05()
    {
        mouseOverlap_Panel_List[4].SetActive(false);
    }

    public void Button_GetReward(int idx)
    {

        if(idx == 0)
        {
            if(is_collect_complete[idx] == true)
            {
                Inventory.ItemLog(badge_items[0], 1);
                player_lv++;
                rewardButton_List[idx].GetComponent<Button>().interactable = false;
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[36]);
            }

        }
        else if(idx == 1)
        {
            if(is_collect_complete[idx] == true)
            {
                Inventory.ItemLog(Inventory.money_item, 1000);
                Inventory.money += 1000;
                Inventory.FreshSlot();
                player_lv++;
                rewardButton_List[idx].GetComponent<Button>().interactable = false;
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[36]);
            }

        }
        else if(idx == 2)
        {
            if(is_collect_complete[idx] == true)
            {
                Inventory.ItemLog(badge_items[1], 1);
                player_lv++;
                rewardButton_List[idx].GetComponent<Button>().interactable = false;
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[36]);
            }

        }
        else if(idx == 3)
        {
            if(is_collect_complete[idx] == true)
            {
                Inventory.ItemLog(Inventory.money_item, 3000);
                Inventory.money += 3000;
                Inventory.FreshSlot();
                player_lv++;
                rewardButton_List[idx].GetComponent<Button>().interactable = false;
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[36]);
            }
        }
        else if(idx == 4)
        {
            if(is_collect_complete[idx] == true)
            {
                Inventory.ItemLog(badge_items[2], 1);
                player_lv++;
                rewardButton_List[idx].GetComponent<Button>().interactable = false;
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[36]);
            }
        }
        else
        {
            Debug.Log("조건이 충족되지 않았습니다.");
        }

    }

    public void SelectStageButton(int stage)
    {
        switch(stage)
        {
            case 0:
                stagePanels[stage].SetActive(true);
                MainPanel.SetActive(false);
                break;
            case 1:
                stagePanels[stage].SetActive(true);
                MainPanel.SetActive(false);
                break;
            case 2:
                stagePanels[stage].SetActive(true);
                MainPanel.SetActive(false);
                break;
            case 3:
                stagePanels[stage].SetActive(true);
                MainPanel.SetActive(false);
                break;
            case 4:
                stagePanels[stage].SetActive(true);
                MainPanel.SetActive(false);
                break;
        }
    }

    public void BackStageButton(int stage)
    {
        switch(stage)
        {
            case 0:
                stagePanels[stage].SetActive(false);
                MainPanel.SetActive(true);
                break;
            case 1:
                stagePanels[stage].SetActive(false);
                MainPanel.SetActive(true);
                break;
            case 2:
                stagePanels[stage].SetActive(false);
                MainPanel.SetActive(true);
                break;
            case 3:
                stagePanels[stage].SetActive(false);
                MainPanel.SetActive(true);
                break;
            case 4:
                stagePanels[stage].SetActive(false);
                MainPanel.SetActive(true);
                break;
        }
    }

    public void exitCollectionButton()
    {
        player.currentTime = 0f;
        player.isCollectMoving = true;
    }

}
