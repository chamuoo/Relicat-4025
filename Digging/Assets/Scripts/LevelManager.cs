using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public Collection collection;
    public Shop shop;
    public Player player;

    // 스테이지 목표 유물 개수
    public GameObject stagetargetUI;
    public TextMeshProUGUI stagetargetNumText;
    private Color originalTargetTextColor;
    public int[] stagetargetNum = new int[2];

    // 타이머
    [SerializeField] private TextMeshProUGUI stagetimerText;
    private Color originalTimerTextColor;

    public float totalTime_1 = 900.9f;
    public float totalTime_2 = 1800.9f;
    public float remainingTime;
    public bool isRunning = true;
    public GameObject TimeOutUI;

    public bool isOnEnding = false;

    // 모래시계
    public Animator SandAni_0;
    public Animator SandAni_1;

    // 가이드 UI
    public GameObject GuidePanel;
    public bool isOnGuide = false;
    public GameObject[] Guide_list;
    public int guideView_idx;
    public GameObject left_guideButton;
    public GameObject right_guideButton;

    // 버튼
    public GameObject guide_Button;
    public GameObject pause_Button;

    public bool isClickReset;

    // 레벨
    //public int stage_Level = 2;

    // 다음 스테이지
    public GameObject ClearStagePanel_1;
    public GameObject ClearStagePanel_2;
    public GameObject ClearStagePanel_3;
    public bool isStageClear = false;
    public GameObject NextStageButton;
    public GameObject EndingButton;

    public static LevelManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded3;
        }
        else
        {
            Destroy(this.gameObject); // 중복 방지
        }

    }

    void Start()
    {
        if (LoadScene.instance.stage_Level == 0)
        {
            remainingTime = totalTime_1;
        }
        else if (LoadScene.instance.stage_Level == 1)
        {
            remainingTime = totalTime_2;
        }
        else if(LoadScene.instance.stage_Level == 2)
        {
            remainingTime = totalTime_2;
        }
        else
        {
            remainingTime = totalTime_1;
        }
        
        //SaveSystem.Instance.DeleteSaveFile();
        //SaveSystem.Instance.Load();
        originalTargetTextColor = stagetargetNumText.color;
        originalTimerTextColor = stagetimerText.color;

        guideView_idx = 0;
        Guide_list[0].SetActive(true);

    }

    private void OnDestroy()
    {
        // 이벤트 제거 (중복 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded3;
    }

    // 매 씬이 로드가 될 때마다
    private void OnSceneLoaded3(Scene scene, LoadSceneMode mode)
    {
        //if (scene.buildIndex != 2) return;
        //if(scene.buildIndex != 3) return;

        if(scene.buildIndex > 1 && scene.buildIndex < 6)
        {
            UIController.Instance.Initialize();
        }

        if(scene.buildIndex == 3)
        {
            Toggle_GuidePanel();
            player = FindObjectOfType<Player>();
        }

        if(scene.buildIndex != 3) return;
        if(scene.buildIndex == 3)
        {
            Toggle_GuidePanel();
        }

        // 씬이 로드된 후 Player 다시 찾기
        player = FindObjectOfType<Player>();

        if (player != null)
        {
            Debug.Log("씬 전환 후 Player 연결 완료: " + player.name);
        }
        else
        {
            Debug.Log("씬 전환 후 Player를 찾지 못했습니다.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (LoadScene.instance.stage_Level == 0)
        //{
        //    stagetargetNumText.text = collection.collect_sum.ToString() + " / 10";
        //}
        //else if (LoadScene.instance.stage_Level == 1)
        //{
        //    stagetargetNumText.text = collection.collect_sum.ToString() + " / 20";
        //}
        //else if(LoadScene.instance.stage_Level == 2)
        //{
        //    stagetargetNumText.text = collection.collect_sum.ToString() + " / 30";
        //}

        if(collection.collect_sum >= stagetargetNum[LoadScene.instance.stage_Level] && isStageClear == false)
        {

        }

        if(SceneManager.GetActiveScene().buildIndex > 2 && stagetargetUI.activeSelf == true && !isStageClear)
        {
            if (!isRunning)
            {
                SandAni_0.speed = 0f;
                SandAni_1.speed = 0f;

                return;
            }

            if (remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                UpdateTimerUI();
                if (remainingTime < 60)
                {
                    stagetimerText.color = Color.red;
                }
                else if (remainingTime < totalTime_1 / 2)
                {
                    stagetimerText.color = Color.yellow;
                }

                SandAni_0.speed = 1f;
                SandAni_1.speed = 1f;

            }
            else
            {
                remainingTime = 0f;
                isRunning = false;
                UpdateTimerUI(); // 마지막 00:00 표시
                Debug.Log("타이머 종료");

                TimeOutEvent();
            }
        }
    }

    public void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        stagetimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 타임 오버
    void TimeOutEvent()
    {
        player.GetComponent<PlayerController>().DisableControls();

        if(remainingTime <= 0)
        {
            TimeOutUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    
    // 레벨 클리어
    public void stageClear()
    {
        stagetargetNumText.color = Color.green;
        stagetimerText.color = Color.green;

        isRunning = false;
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[37]);
        isStageClear = true;

        if (LoadScene.instance.stage_Level == 0)
        {
            ClearStagePanel_1.SetActive(true);
        }
        else if (LoadScene.instance.stage_Level == 1)
        {
            ClearStagePanel_2.SetActive(true);
        }
        else if (LoadScene.instance.stage_Level == 2)
        {
            ClearStagePanel_3.SetActive(true);
        }

        player.GetComponent<PlayerController>().DisableControls();
    }

    public void EndingSequence()
    {
        isOnEnding = true;

        SaveSystem.Instance.DeleteSaveFile();
        LoadScene.instance.isAlreadyWatchStory = false;
        ResetGame();
        shop.createDrill_textobj.SetActive(true);
        shop.upgradeDrill_textobj.SetActive(false);
        Collection.instance.player.Inventory_obj.SetActive(false);
        Collection.instance.player.CollectUIPanel.SetActive(false);
        //Inventory.badgePanel.SetActive(false);
        //Inventory.moneyPanel.SetActive(false);
        //Inventory.healthPanel.SetActive(false);
        stagetargetUI.SetActive(false);
        isOnEnding = false;
        guide_Button.SetActive(false);
        pause_Button.SetActive(false);

        //SaveSystem.Instance.Save();

        LoadScene.instance.GoEnding();

        LoadScene.instance.stage_Level = 0;
        ClearStagePanel_3.SetActive(false);
        EndingButton.SetActive(false);
    }

    public void GoNextStageButton()
    {
        LoadScene.instance.stage_Level += 1;
        if (LoadScene.instance.stage_Level == 0)
        {
            remainingTime = totalTime_1;
        }
        else if (LoadScene.instance.stage_Level == 1)
        {
            remainingTime = totalTime_2;
            ClearStagePanel_1.SetActive(false);
        }
        else if(LoadScene.instance.stage_Level == 2)
        {
            remainingTime = totalTime_2;
            ClearStagePanel_2.SetActive(false);
        }
        stagetargetNumText.color = originalTargetTextColor;
        stagetimerText.color = originalTimerTextColor;

        collection.collect_sum = 0;
        collection.collect_count = 0;
        stagetargetNumText.text = collection.collect_count.ToString() + " / 10";

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
        //isStageClear = false;

        //ClearStagePanel_1.SetActive(false);
        NextStageButton.SetActive(false);

        SaveSystem.Instance.Save();
        LoadScene.instance.GoMain();
    }

    public void ContinueStageButton1()
    {
        player.GetComponent<PlayerController>().EnableControls();

        ClearStagePanel_1.SetActive(false);
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
        SaveSystem.Instance.Save();
        NextStageButton.SetActive(true);
    }

    public void ContinueStageButton2()
    {
        player.GetComponent<PlayerController>().EnableControls();

        ClearStagePanel_2.SetActive(false);
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
        SaveSystem.Instance.Save();
        NextStageButton.SetActive(true);
    }

    public void ContinueStageButton3()
    {
        player.GetComponent<PlayerController>().EnableControls();

        ClearStagePanel_3.SetActive(false);
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
        SaveSystem.Instance.Save();
        EndingButton.SetActive(true);
    }

    // 레벨 클리어 닫음
    public void CloseClearPanel_1()
    {
        //LoadScene.instance.stage_Level = 1;
        if (LoadScene.instance.stage_Level == 0)
        {
            remainingTime = totalTime_1;
        }
        else if (LoadScene.instance.stage_Level == 1)
        {
            remainingTime = totalTime_2;
        }
        else if(LoadScene.instance.stage_Level == 2)
        {
            remainingTime = totalTime_2;
        }
        stagetargetNumText.color = originalTargetTextColor;
        stagetimerText.color = originalTimerTextColor;

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
        isStageClear = false;

        ClearStagePanel_1.SetActive(false);

        LoadScene.instance.GoMenu();
        SaveSystem.Instance.Save();

        stagetargetUI.SetActive(false);
        guide_Button.SetActive(false);
        pause_Button.SetActive(false);
        NextStageButton.SetActive(false);

        Collection.instance.player.Inventory_obj.SetActive(false);
        Collection.instance.player.CollectUIPanel.SetActive(false);
    }

    public void CloseClearPanel_2()
    {
        //LoadScene.instance.stage_Level = 1;
        if (LoadScene.instance.stage_Level == 0)
        {
            remainingTime = totalTime_1;
        }
        else if (LoadScene.instance.stage_Level == 1)
        {
            remainingTime = totalTime_2;
        }
        else if(LoadScene.instance.stage_Level == 2)
        {
            remainingTime = totalTime_2;
        }
        stagetargetNumText.color = originalTargetTextColor;
        stagetimerText.color = originalTimerTextColor;

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
        isStageClear = false;

        ClearStagePanel_2.SetActive(false);

        LoadScene.instance.GoMenu();
        SaveSystem.Instance.Save();

        stagetargetUI.SetActive(false);
        guide_Button.SetActive(false);
        pause_Button.SetActive(false);

        Collection.instance.player.Inventory_obj.SetActive(false);
        Collection.instance.player.CollectUIPanel.SetActive(false);
    }

    public void CloseClearPanel_3()
    {
        //LoadScene.instance.stage_Level = 1;
        if(LoadScene.instance.stage_Level == 0)
        {
            remainingTime = totalTime_1;
        }
        else if(LoadScene.instance.stage_Level == 1)
        {
            remainingTime = totalTime_2;
        }
        else if(LoadScene.instance.stage_Level == 2)
        {
            remainingTime = totalTime_2;
        }
        stagetargetNumText.color = originalTargetTextColor;
        stagetimerText.color = originalTimerTextColor;

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
        isStageClear = false;

        ClearStagePanel_3.SetActive(false);

        LoadScene.instance.GoMenu();
        SaveSystem.Instance.Save();

        stagetargetUI.SetActive(false);
        guide_Button.SetActive(false);
        pause_Button.SetActive(false);

        Collection.instance.player.Inventory_obj.SetActive(false);
        Collection.instance.player.CollectUIPanel.SetActive(false);
    }

    // 스테이지 리셋 버튼(다시 씬 로드)
    public void Restart_this_Stage_Button()
    {
        SaveSystem.Instance.DeleteSaveFile();
        // 아이템 초기화
        for (int i = 0; i < collection.player.items.Count; i++)
        {
            collection.player.items[i].count = 0;
            collection.player.items[i].accumulation_count = 0;
            collection.player.items[i].ishaveitem = false;
            collection.player.items[i].isalreadySell = false;
        }
        for (int i = 0; i < collection.player.minerals.Count; i++)
        {
            collection.player.minerals[i].count = 0;
        }

        for (int i = 0; i < collection.player.UseItems.Count; i++)
        {
            collection.player.UseItems[i].count = 0;
        }

        
        collection.player.UpgradeItems[0].count = 1;
        collection.player.UpgradeItems[0].value = 10;
        collection.player.UpgradeItems[1].count = 1;
        collection.player.UpgradeItems[1].value = 50;
        collection.player.UpgradeItems[2].count = 1;
        collection.player.UpgradeItems[2].value = 50;

        collection.Inventory.money_item.count = 0;
        collection.Inventory.ClearItem();

        collection.li_isCollect = new bool[collection.player.items.Count];
        collection.li_isRelicOnTable = new bool[collection.player.items.Count];
        for (int i = 0; i < collection.player.items.Count; i++)
        {
            collection.slots[i].item = collection.guessItem;
            collection.Collection_Table[i].GetComponentInChildren<SpriteRenderer>().sprite = null;
        }
        collection.collect_sum = 0;

        shop.pick_damage = 6;
        shop.lightRadius = 2f;
        shop.playerlight.GetComponent<Light2D>().pointLightOuterRadius = shop.lightRadius;
        shop.shop_pickLvText.text = "레벨 : 1";
        shop.shop_lightLvText.text = "레벨 : 1";
        shop.shop_pickUpdateText.text = "-" + Collection.instance.player.UpgradeItems[0].value.ToString();
        shop.shop_lightUpdateText.text = "-" + Collection.instance.player.UpgradeItems[1].value.ToString();

        if(LoadScene.instance.stage_Level == 1)
        {
            remainingTime = totalTime_1;
        }
        else if(LoadScene.instance.stage_Level == 2)
        {
            remainingTime = totalTime_2;
        }
        
        stagetargetNumText.color = originalTargetTextColor;
        stagetimerText.color = originalTimerTextColor;
        isRunning = false;
        Time.timeScale = 1f;

        //SaveSystem.Instance.Save();
        LoadScene.instance.GoMain();
        TimeOutUI.SetActive(false);
    }
    
    // 타임오버(나가기 버튼)
    public void Restart_Go_Menu_Button()
    {
        SaveSystem.Instance.DeleteSaveFile();
        // 아이템 초기화
        for (int i = 0; i < collection.player.items.Count; i++)
        {
            collection.player.items[i].count = 0;
            collection.player.items[i].accumulation_count = 0;
            collection.player.items[i].ishaveitem = false;
            collection.player.items[i].isalreadySell = false;
        }
        for (int i = 0; i < collection.player.minerals.Count; i++)
        {
            collection.player.minerals[i].count = 0;
        }

        for (int i = 0; i < collection.player.UseItems.Count; i++)
        {
            collection.player.UseItems[i].count = 0;
        }


        collection.player.UpgradeItems[0].count = 1;
        collection.player.UpgradeItems[0].value = 10;
        collection.player.UpgradeItems[1].count = 1;
        collection.player.UpgradeItems[1].value = 50;
        collection.player.UpgradeItems[2].count = 1;
        collection.player.UpgradeItems[2].value = 50;

        collection.player.Drill_Items[1].count = 0;
        collection.player.Drill_Items[2].count = 0;
        collection.player.Drill_Items[3].count = 0;

        collection.Inventory.money_item.count = 0;
        collection.Inventory.ClearItem();

        collection.li_isCollect = new bool[collection.player.items.Count];
        collection.li_isRelicOnTable = new bool[collection.player.items.Count];
        for (int i = 0; i < collection.player.items.Count; i++)
        {
            collection.slots[i].item = collection.guessItem;
            collection.Collection_Table[i].GetComponentInChildren<SpriteRenderer>().sprite = null;
        }
        collection.collect_sum = 0;

        shop.pick_damage = 6;
        shop.lightRadius = 2f;
        shop.playerlight.GetComponent<Light2D>().pointLightOuterRadius = shop.lightRadius;
        shop.shop_pickLvText.text = "레벨 : 1";
        shop.shop_lightLvText.text = "레벨 : 1";
        shop.drill_Lv_Text.text = "레벨 : 0";
        shop.shop_pickUpdateText.text = "-" + Collection.instance.player.UpgradeItems[0].value.ToString();
        shop.shop_lightUpdateText.text = "-" + Collection.instance.player.UpgradeItems[1].value.ToString();
        shop.isCreateDrill = false;

        remainingTime = totalTime_1;

        stagetargetNumText.color = originalTargetTextColor;
        stagetimerText.color = originalTimerTextColor;
        isRunning = false;
        Time.timeScale = 1f;

        //SaveSystem.Instance.Save();
        LoadScene.instance.GoMenu();
        TimeOutUI.SetActive(false);

        stagetargetUI.SetActive(false);
        guide_Button.SetActive(false);
        pause_Button.SetActive(false);

        Collection.instance.player.Inventory_obj.SetActive(false);
    }

    // 게임 초기화
    public void ResetGame()
    {
        SlotManager.Instance.quitSlotUI.ClearAllSlots();

        // 아이템 초기화
        for(int i = 0; i < collection.player.items.Count; i++)
        {
            collection.player.items[i].count = 0;
            collection.player.items[i].accumulation_count = 0;
            collection.player.items[i].ishaveitem = false;
            collection.player.items[i].isalreadySell = false;
        }
        for (int i = 0; i < collection.player.minerals.Count; i++)
        {
            collection.player.minerals[i].count = 0;
        }

        for (int i = 0; i < collection.player.UseItems.Count; i++)
        {
            collection.player.UseItems[i].count = 0;
        }


        collection.player.UpgradeItems[0].count = 1;
        collection.player.UpgradeItems[0].value = 10;
        collection.player.UpgradeItems[1].count = 1;
        collection.player.UpgradeItems[1].value = 50;
        collection.player.UpgradeItems[2].count = 1;
        collection.player.UpgradeItems[2].value = 50;

        collection.player.Drill_Items[1].count = 0;
        collection.player.Drill_Items[2].count = 0;
        collection.player.Drill_Items[3].count = 0;

        collection.Inventory.money_item.count = 0;
        collection.Inventory.ClearItem();

        collection.li_isCollect = new bool[collection.player.items.Count];
        collection.li_isRelicOnTable = new bool[collection.player.items.Count];
        for (int i = 0; i < collection.player.items.Count; i++)
        {
            collection.slots[i].item = collection.guessItem;
            collection.Collection_Table[i].GetComponentInChildren<SpriteRenderer>().sprite = null;
        }
        collection.collect_sum = 0;

        shop.pick_damage = 6;
        shop.lightRadius = 2f;
        shop.playerlight.GetComponent<Light2D>().pointLightOuterRadius = shop.lightRadius;
        shop.shop_pickLvText.text = "레벨 : 1";
        shop.shop_lightLvText.text = "레벨 : 1";
        shop.drill_Lv_Text.text = "레벨 : 0";
        shop.shop_pickUpdateText.text = "-" + Collection.instance.player.UpgradeItems[0].value.ToString();
        shop.shop_lightUpdateText.text = "-" + Collection.instance.player.UpgradeItems[1].value.ToString();
        shop.isCreateDrill = false;

        stagetargetNumText.color = originalTargetTextColor;
        stagetimerText.color = originalTimerTextColor;
        isRunning = false;
        remainingTime = totalTime_1;
        
        LoadScene.instance.stage_Level = 0;

        Time.timeScale = 1f;

        //SaveSystem.Instance.Save();
        //LoadScene.instance.GoMenu();
        TimeOutUI.SetActive(false);
    }

    // 버튼 입력에 의한 일시정지 ||
    public void Toggle_Pause_Panel()
    {
        player.GetComponent<PlayerController>().DisableControls();
        collection.player.TogglePause();
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
    }

    public void Toggle_GuidePanel()
    {
        if (!isOnGuide)
        {
            GuidePanel.SetActive(true);
            isOnGuide = true;
        }
        else if(isOnGuide)
        {
            GuidePanel.SetActive(false);
            isOnGuide = false;
            
        }
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
    }
    
    // 가이드
    public void Button_Left()
    {
        guideView_idx -= 1;
        if (guideView_idx <= 0)
        {
            guideView_idx = 0;
            left_guideButton.SetActive(false);
        }
        right_guideButton.SetActive(true);
        Switch_GuideView();
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[30]);
    }
    public void Button_Right()
    {
        guideView_idx += 1;
        if (guideView_idx >= Guide_list.Length - 1)
        {
            guideView_idx = Guide_list.Length - 1;
            right_guideButton.SetActive(false);
        }
        left_guideButton.SetActive(true);
        Switch_GuideView();
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[30]);

    }


    public void Switch_GuideView()
    {
        switch (guideView_idx)
        {
            case 0:
                Guide_list[0].SetActive(true);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 1:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(true);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 2:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(true);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 3:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(true);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 4:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(true);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 5:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(true);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 6:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(true);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 7:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(true);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(false);

                break;
            case 8:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(true);
                Guide_list[9].SetActive(false);

                break;
            case 9:
                Guide_list[0].SetActive(false);
                Guide_list[1].SetActive(false);
                Guide_list[2].SetActive(false);
                Guide_list[3].SetActive(false);
                Guide_list[4].SetActive(false);
                Guide_list[5].SetActive(false);
                Guide_list[6].SetActive(false);
                Guide_list[7].SetActive(false);
                Guide_list[8].SetActive(false);
                Guide_list[9].SetActive(true);

                break;

        }
    }

    
}
