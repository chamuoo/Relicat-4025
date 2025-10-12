using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SearchService;
#endif

using System.Collections.ObjectModel;
using Unity.VisualScripting;
using System;
using UnityEngine.PlayerLoop;

public class Player : MonoBehaviour
{
    public static Inventory instance;
    public Inventory Inventory;
    public Collection Collection;
    public Shop Shop;
    public PlayerController player;


    public List<Item> items;
    public List<Item> minerals;
    public List<Item> UseItems;
    public List<Item> UpgradeItems;
    public List<Item> Drill_Items;
    public List<WeaponTemplate> Weapons;

    // 개발자 모드
    private bool isdebugmode = false;

    // 인벤토리
    public GameObject Inventory_obj;
    Vector3 Inventory_StartPos;
    Vector3 Inventory_EndPos;
    public float currentTime = 0f;
    float moveTime = 1f;
    private bool isInventoryMoving = false;
    private bool isOnInventory = false;


    // 상점
    [SerializeField] private GameObject shopUIPanel; // 상점 UI 패널
    private bool isNearShop = false; // 상점 근처에 있는지 체크

    Vector3 Shop_StartPos;
    Vector3 Shop_EndPos;
    private bool isShopMoving = false;
    private bool isOnShop = false;

    // 박물관
    public GameObject CollectUIPanel; // 도감 UI 패널
    private bool isNearMuseum = false;
    public bool isInMuseum = false;
    private bool isNearCollection = false;
    private bool isActiveMuseum = false;

    Vector3 Collect_StartPos;
    Vector3 Collect_EndPos;
    public bool isCollectMoving = false;
    private bool isOnCollect = false;

    // 돌아가기 안내판
    private bool isNearReturnMuseum = false;

    // 진열장
    [SerializeField] private GameObject RelicInfoUIPanel; // 진열장 설명 UI 패널
    [SerializeField] private GameObject RelicInfoImage;
    [SerializeField] private GameObject RelicInfoName;
    [SerializeField] private GameObject RelicInfoText;

    // 체력
    public GameObject[] li_PlayerHearts;           //플레이어 생명 오브젝트 리스트
    public Dictionary<string, bool> DicPlayerHeart = new Dictionary<string, bool>();

    // 일시정지
    [SerializeField] private GameObject PausePanel;
    public bool isPaused = false;

    // 사망
    [SerializeField] private GameObject DiePanel;

    // 지형 리셋
    [SerializeField] private GameObject ResetPanel;
    [SerializeField] private GameObject SavePanel;
    private bool isNearReset;
    private bool isOnResetUI;

    // 튜토리얼
    private bool isNearTutorialExit;

    // 곡괭이 
    public GameObject pick_obj;
    public Sprite[] pick_imgs;

    // 입력 액션
    public System.Action<bool> OnInventoryOpen;
    public Action<float> OnHealthCange;

    // 플레이어 HP
    [SerializeField] PlayerHPBar hPBar;
    public PlayerHPBar HP => hPBar;

    // 로드했는지에 대한 여부
    bool isLoaded = false;

    private string savePath => Application.persistentDataPath + "/SaveData.json";

    private void Awake()
    {
        // 아이템 초기화
        //for (int i = 0; i < items.Count; i++)
        //{
        //    items[i].count = 0;
        //    items[i].accumulation_count = 0;
        //    items[i].ishaveitem = false;
        //    items[i].isalreadySell = false;

        //}

        //for (int i = 0; i < minerals.Count; i++)
        //{
        //    minerals[i].count = 0;
        //}

        //for(int i = 0; i < UseItems.Count; i++)
        //{
        //    UseItems[i].count = 0;
        //}

        //for(int i = 0; i < UpgradeItems.Count; i++)
        //{
        //    UpgradeItems[i].count = 1;
        //    UpgradeItems[i].value = 10;
        //}

        player = GetComponent<PlayerController>();
        //if(!File.Exists(savePath))  // 저장 없을 때
        //    InitItemCount();
    }

    private void OnEnable()
    {
        SlotManager.Instance.OnInventoryOpen = SlotManager.Instance.HandleInventoryOpen;
        OnHealthCange += hPBar.UpdateHP;
    }

    private void OnDisable()
    {
        SlotManager.Instance.OnInventoryOpen = null;
        OnHealthCange -= hPBar.UpdateHP;
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        player.EnableControls();
        isLoaded = false;

        // UI 시작/도착 포지션 지정
        Inventory_StartPos = new Vector3(-200f, Screen.height / 2, 0f);
        Inventory_EndPos = new Vector3(0f, Screen.height / 2, 0f);

        Shop_StartPos = new Vector3(2420f, Screen.height / 2, 0f);
        Shop_EndPos = new Vector3(1945f, Screen.height / 2, 0f);

        Collect_StartPos = new Vector3(3420f, Screen.height / 2, 0f);
        Collect_EndPos = new Vector3(1920f, Screen.height / 2, 0f);

        Tool.Instance.torchObj.Clear();
        // 한 프레임 대기 (인스턴스 초기화 대기)
        yield return null;

        // Inventory 스크립트 연결
        Inventory = Inventory.Instance ?? FindObjectOfType<Inventory>();
        Shop = Shop.instance ?? FindObjectOfType<Shop>();
        Collection = Collection.instance ?? FindObjectOfType<Collection>();

        // Inventory 오브젝트 연결
        if(Inventory != null)
        {
            Inventory_obj = Inventory.gameObject;
            shopUIPanel = Shop.gameObject;
            CollectUIPanel = Collection.gameObject;
            Debug.Log("Inventory 연결 완료: " + Inventory_obj.name);
        }
        else
        {
            Debug.LogError("Inventory 연결 실패: Inventory.Instance가 null입니다.");
        }

        if(Inventory != null)
        {
            /*Transform collectionParent = Inventory.transform.Find("HealthPanel");
            //if(collectionParent != null)
            //{
            //    li_PlayerHearts = new GameObject[3];
            //    for(int i = 0; i < li_PlayerHearts.Length; i++)
            //    {
            //        Transform child = collectionParent.Find(i.ToString());
            //        if(child != null)
            //        {
            //            li_PlayerHearts[i] = child.gameObject;
            //            DicPlayerHeart[li_PlayerHearts[i].name] = true;
            //            li_PlayerHearts[i].SetActive(true);
            //        }
            //        else
            //        {
            //            Debug.LogWarning($"li_PlayerHearts[{i}] 오브젝트를 찾을 수 없습니다.");
            //        }
            //    }

            //}
            */
        }

        SlotManager.Instance.quitSlotUI.InitFillData();
        Inventory_obj.transform.position = new Vector3(-200f, Screen.height / 2, 0f);
        shopUIPanel.transform.position = new Vector3(2420f, Screen.height / 2, 0f);
        CollectUIPanel.transform.position = new Vector3(3420f, Screen.height / 2, 0f);
        Inventory_obj.SetActive(true);
        CollectUIPanel.SetActive(true);
        Inventory.badgePanel.gameObject.SetActive(true);
        Inventory.moneyPanel.gameObject.SetActive(true);
        //Inventory.healthPanel.gameObject.SetActive(true);
        LevelManager.instance.stagetargetUI.SetActive(true);
        LevelManager.instance.guide_Button.SetActive(true);
        LevelManager.instance.pause_Button.SetActive(true);
        LevelManager.instance.isRunning = true;

        if(LevelManager.instance.isClickReset)
        {
            LevelManager.instance.isClickReset = false;
        }
        else if(LevelManager.instance.isStageClear)
        {
            LevelManager.instance.isStageClear = false;
            if(LoadScene.instance.isUseContinue == true)
            {
                LoadScene.instance.isUseContinue = false;
                Inventory.FreshSlot();
                SaveSystem.Instance.Load();
                isLoaded = true;
            }
            //SaveSystem.Instance.Save();
        }
        else
        {
            if(File.Exists(savePath))
            {
                SlotManager.Instance.quitSlotUI.ClearAllSlots();
                Inventory.FreshSlot();
                SaveSystem.Instance.Load();
                isLoaded = true;
            }
        }

        // 첫 Level의 아이템 드랍
        if(!isLoaded && SceneManager.GetActiveScene().buildIndex == 3)
        {
            SlotManager.Instance.quitSlotUI.ClearAllSlots();
            LevelManager.instance.GuidePanel.SetActive(true);
            LevelManager.instance.guideView_idx = 0;
            LevelManager.instance.isOnGuide = true;
            LevelManager.instance.Switch_GuideView();
            LevelManager.instance.left_guideButton.SetActive(false);
            LevelManager.instance.right_guideButton.SetActive(true);
            LoadScene.instance.isUseStart = false;

            for(int i = 0; i < UseItems.Count; i++)
            {
                UseItems[i].count = 0;
            }
            Inventory.ClearItem();

            Inventory.AddItem(UseItems[0], 3);
            Inventory.AddItem(UseItems[1], 10);

            //SaveSystem.Instance.Save();
        }

        if(!isLoaded)
        {
            SlotManager.Instance.Initialize();
        }

        if(!File.Exists(savePath))
        {
            print("저장된 파일이 없습니다");
            yield return null;
        }
        else
        {
            string jsonForLoad = File.ReadAllText(savePath);
            SaveData loaded = JsonUtility.FromJson<SaveData>(jsonForLoad);
            SaveSystem.Instance.Save();
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if(UpgradeItems[0].count >= 50)
        {
            Shop.instance.pickImage.sprite = pick_imgs[4];
        }
        else if(UpgradeItems[0].count >= 40)
        {
            Shop.instance.pickImage.sprite = pick_imgs[3];
        }
        else if(UpgradeItems[0].count >= 25)
        {
            Shop.instance.pickImage.sprite = pick_imgs[2];
        }
        else if(UpgradeItems[0].count >= 15)
        {
            Shop.instance.pickImage.sprite = pick_imgs[1];
        }
        else
        {
            Shop.instance.pickImage.sprite = pick_imgs[0];
        }

        if (isPaused == false)
        {
            Interaction_Inventory();
            Interaction_F();
            //GroundAutoHeal();
            Ground_Time_Pause();

            if (Input.GetKeyDown(KeyCode.P))
            {
                
                if(isdebugmode == false)
                {
                    isdebugmode = true;
                    Inventory.LogMessage("개발자모드입니다.");
                }
                else if(isdebugmode == true)
                {
                    isdebugmode = false;
                    Inventory.LogMessage("개발자모드가 해제되었습니다.");
                }
            }

            if(isdebugmode == true)
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    Inventory.AddItem(UseItems[0], 3);
                    Inventory.AddItem(UseItems[1], 10);
                }
                if (Input.GetKeyDown(KeyCode.H))
                {
                    AddPlayerLife(1);
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    Inventory.AddItem(minerals[0], 1);
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    Inventory.AddItem(minerals[3], 100000);
                }

                if (Input.GetKeyDown(KeyCode.C))
                {
                    Inventory.AddItem(items[UnityEngine.Random.Range(0, 20)], 1);
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    Inventory.AddItem(items[5], 1);
                }

                if (Input.GetKeyDown(KeyCode.B))
                {
                    Inventory.AddItem(items[7], 1);
                    Inventory.AddItem(Drill_Items[1], 1);
                    Inventory.AddItem(Drill_Items[2], 1);
                    Inventory.AddItem(Drill_Items[3], 1);
                }

                if (Input.GetKeyDown(KeyCode.L))
                {
                    Collection.player_lv += 1;
                }

                //세이브
                if (Input.GetKeyDown(KeyCode.LeftBracket))
                {
                    SaveSystem.Instance.Save();
                }

                //로드
                if (Input.GetKeyDown(KeyCode.RightBracket))
                {
                    SaveSystem.Instance.Load();
                }

                // 세이브 삭제
                if (Input.GetKeyDown(KeyCode.M))
                {
                    Reset_SaveFile_and_GoMenu();
                }
            }
            
        }

        // 일시정지 esc
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);

        }

        
    }

    public void Reset_SaveFile_and_GoMenu()
    {
        SaveSystem.Instance.DeleteSaveFile();
        LoadScene.instance.isAlreadyWatchStory = false;
        LevelManager.instance.remainingTime = LevelManager.instance.totalTime_1;
        LevelManager.instance.Restart_Go_Menu_Button();
        UIController.Instance.SetObjectActive(0, false);    // 퀵슬롯 
        UIController.Instance.SetObjectActive(1, false);    // 깊이
        Inventory_obj.SetActive(false);
        Inventory.badgePanel.SetActive(false);
        Inventory.moneyPanel.SetActive(false);
        Inventory.healthPanel.SetActive(false);
        LevelManager.instance.stagetargetUI.SetActive(false);
        LevelManager.instance.ClearStagePanel_1.SetActive(false);
        LevelManager.instance.ClearStagePanel_2.SetActive(false);
        LevelManager.instance.ClearStagePanel_3.SetActive(false);
        LevelManager.instance.NextStageButton.SetActive(false);
        LevelManager.instance.EndingButton.SetActive(false);
        LevelManager.instance.isStageClear = false;
        LevelManager.instance.isOnEnding = false;
        Shop.createDrill_textobj.SetActive(true);
        Shop.upgradeDrill_textobj.SetActive(false);
        LoadScene.instance.stage_Level = 0;
        //SlotManager.Instance.quitSlotUI.ResetQuickSlot();
    }

    // 게임 일시정지 / 재개
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            //Time.timeScale = 0f; // 게임 일시정지
            PausePanel.SetActive(true); // UI 표시
            player.DisableControls();
            LevelManager.instance.isRunning = false;
            this.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            this.gameObject.GetComponent<Rigidbody2D>().constraints |= RigidbodyConstraints2D.FreezePositionY;
        }
        else
        {
            //Time.timeScale = 1f; // 게임 재개
            PausePanel.SetActive(false); // UI 숨김
            player.EnableControls();
            LevelManager.instance.isRunning = true;
            this.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
            this.gameObject.GetComponent<Rigidbody2D>().constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            //SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
        }
    }

    // 설정
    public void SettingButton()
    {
        LoadScene.instance.settingsPanel.SetActive(true);
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);

    }

    // 메인메뉴 이동
    public void GoMainMenu()
    {
        FadeEffect.Instance.OnFade(FadeState.FadeInOut);
        isPaused = false;
        //Time.timeScale = 1f; // 게임 재개
        PausePanel.SetActive(false); // UI 숨김
        LevelManager.instance.isRunning = false;
        if (!File.Exists(savePath))
        {
            LevelManager.instance.ResetGame();
        }
        LevelManager.instance.NextStageButton.SetActive(false);
        LevelManager.instance.EndingButton.SetActive(false);

        Invoke("InvokeGoMainMenu", 1.5f);
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
    }
    
    void InvokeGoMainMenu()
    {
        SceneManager.LoadScene("Menu");
        LoadScene.instance.MainMenu.SetActive(true);
        UIController.Instance.SetObjectActive(0, false);    // 퀵슬롯 
        UIController.Instance.SetObjectActive(1, false);    // 깊이
        Inventory_obj.SetActive(false);
        Inventory.badgePanel.SetActive(false);
        Inventory.moneyPanel.SetActive(false);
        Inventory.healthPanel.SetActive(false);
        LevelManager.instance.stagetargetUI.SetActive(false);
        LevelManager.instance.guide_Button.SetActive(false);
        LevelManager.instance.pause_Button.SetActive(false);
    }

    // 인벤토리 상호작용
    private void Interaction_Inventory()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !isInventoryMoving && !isOnShop && !isOnCollect)
        {
            currentTime = 0f;
            isInventoryMoving = true;
            SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
        }

        // 인벤토리 UI 이동 애니메이션
        if (isInventoryMoving && !isOnInventory)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / moveTime);
            Inventory_obj.transform.position = Vector3.Lerp(Inventory_StartPos, Inventory_EndPos, t);

            if (t >= moveTime)
            {
                SlotManager.Instance.OnInventoryOpen.Invoke(true);
                isInventoryMoving = false; // 애니메이션 완료
                isOnInventory = true;
            }
        }
        else if (isInventoryMoving && isOnInventory)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / moveTime);
            Inventory_obj.transform.position = Vector3.Lerp(Inventory_EndPos, Inventory_StartPos, t);

            if (t >= moveTime)
            {
                SlotManager.Instance.OnInventoryOpen.Invoke(false);
                isInventoryMoving = false; // 애니메이션 완료
                isOnInventory = false;
            }
        }
    }

    // F 상호작용
    private void Interaction_F()
    {
        
        // 상점 UI 이동 애니메이션
        if (isShopMoving && !isOnShop)
        {
            player.DisableControl(1);
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / moveTime);
            shopUIPanel.transform.position = Vector3.Lerp(Shop_StartPos, Shop_EndPos, t);

            if (t >= moveTime)
            {
                isShopMoving = false; // 애니메이션 완료
                isOnShop = true;
            }
        }
        else if (isShopMoving && isOnShop)
        {
            player.EnableControl(1);
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / moveTime);
            shopUIPanel.transform.position = Vector3.Lerp(Shop_EndPos, Shop_StartPos, t);

            if (t >= moveTime)
            {
                isShopMoving = false; // 애니메이션 완료
                isOnShop = false;
            }
        }

        // 상점 상호작용
        if (isNearShop && Input.GetKeyDown(KeyCode.F) && !isInventoryMoving && !isShopMoving)
        {
            currentTime = 0f;
            SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
            if (isOnInventory && !isOnShop)
            {
                isShopMoving = true;

                Shop.shop_pickLvText.text = "레벨 : " + UpgradeItems[0].count;
                Shop.shop_lightLvText.text = "레벨 : " + UpgradeItems[1].count;

            }
            else
            {
                isShopMoving = true;
                isInventoryMoving = true;
            }


        }
        else if (!isNearShop && isOnShop && !isInventoryMoving && !isShopMoving)
        {
            currentTime = 0f;
            isInventoryMoving = true;
            isShopMoving = true;

        }

        // 박물관 상호작용
        if (isNearMuseum && Input.GetKeyDown(KeyCode.F))
        {
            if (!isInMuseum && isActiveMuseum == false)
            {
                isActiveMuseum = true;
                FadeEffect.Instance.OnFade(FadeState.FadeInOut);
                Invoke("InvokeInMuseum", 1.5f);
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[16]);

                // 획득한 유물을 도감에 활성화
                for (int i = 0; i < items.Count; i++)
                {
                    Collection.li_isCollect[i] = items[i].ishaveitem;
                    if (Collection.li_isCollect[i] == true)
                    {
                        Collection.slots[i].item = items[i];
                    }
                    if (Collection.li_isRelicOnTable[i] == true)
                    {
                        Collection.Collection_Table[i].GetComponentInChildren<SpriteRenderer>().sprite = items[i].itemImage;
                    }
                }
                

            }
            else if (isInMuseum && isActiveMuseum == false)
            {
                isActiveMuseum = true;
                FadeEffect.Instance.OnFade(FadeState.FadeInOut);
                Invoke("InvokeOutMuseum", 1.5f);
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[17]);

            }

        }

        // 박물관 입구 되돌아가기
        if (isNearReturnMuseum && Input.GetKeyDown(KeyCode.F))
        {
            FadeEffect.Instance.OnFade(FadeState.FadeInOut);
            Invoke("InvokeInMuseum", 1.5f);
            SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
        }

        // 도감 UI 이동 애니메이션
        if (isCollectMoving && !isOnCollect)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / moveTime);
            CollectUIPanel.transform.position = Vector3.Lerp(Collect_StartPos, Collect_EndPos, t);

            if (t >= moveTime)
            {
                isCollectMoving = false; // 애니메이션 완료
                isOnCollect = true;
            }
        }
        else if (isCollectMoving && isOnCollect)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / moveTime);
            CollectUIPanel.transform.position = Vector3.Lerp(Collect_EndPos, Collect_StartPos, t);

            if (t >= moveTime)
            {
                isCollectMoving = false; // 애니메이션 완료
                isOnCollect = false;
            }
        }

        // 도감 상호작용
        if(isNearCollection && Input.GetKeyDown(KeyCode.F) && !isCollectMoving)
        {
            currentTime = 0f;
            SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
            if (isOnInventory && !isOnCollect)
            {
                isCollectMoving = true;

            }
            else
            {
                isCollectMoving = true;
            }

        }
        else if(!isNearCollection && isOnCollect && !isCollectMoving)
        {
            currentTime = 0f;
            //isInventoryMoving = true;
            isCollectMoving = true;

        }

        // 리셋 상호용
        if(isNearReset && Input.GetKeyDown(KeyCode.F))
        {
            if(!isOnResetUI)
            {
                player.DisableControl(1);
                isOnResetUI = true;
                ResetPanel.SetActive(true);
                SavePanel.SetActive(true);
                SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
            }

        }

        // 튜토리얼 탈출
        if(isNearTutorialExit && Input.GetKeyDown(KeyCode.F))
        {
            LoadScene.instance.GoMain();
            SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[29]);
        }
    }

    // 지형 리셋 버튼
    public void Reset_Ground_Button()
    {
        LoadScene.instance.GoMain();        
        isOnResetUI = false;
        ResetPanel.SetActive(false);
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[35]);
        LevelManager.instance.isClickReset = true;
        SaveSystem.Instance.Save();
    }

    public void Reset_Close_Button()
    {
        isOnResetUI = false;
        ResetPanel.SetActive(false);
        SavePanel.SetActive(false);
        player.EnableControl(1);
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[28]);
    }

    // 세이브 버튼
    public void Save_Button()
    {
        SaveSystem.Instance.Save();
        hPBar.UpdateHP(100);
        //GroundAutoHeal();
        Inventory.LogMessage("세이브 되었습니다");
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[34]);
    }

    // 박물관 입장/퇴장 페이드 인아웃
    void InvokeInMuseum()
    {
        gameObject.transform.position = new Vector3(250f, 0.5f, 0f);
        isInMuseum = true;
        isActiveMuseum = false;

        if (SoundManager.Instance.BGMSoundPlay.isPlaying)
        {
            SoundManager.Instance.BGMSoundPlay.Stop();

            SoundManager.Instance.BGMSoundPlay.clip = SoundManager.Instance.BGMs[3];
            SoundManager.Instance.BGMSoundPlay.Play();
        }
    }
    void InvokeOutMuseum()
    {
        gameObject.transform.position = new Vector3(18f, 0.5f, 0f);
        isInMuseum = false;
        isActiveMuseum = false;

        if (SoundManager.Instance.BGMSoundPlay.isPlaying)
        {
            SoundManager.Instance.BGMSoundPlay.Stop();

            SoundManager.Instance.BGMSoundPlay.clip = SoundManager.Instance.BGMs[2];
            SoundManager.Instance.BGMSoundPlay.Play();
        }
    }

    // 플레이어 체력 깎기
    //public void LostPlayerLife(int hp)
    //{
        
    //    for (int i = li_PlayerHearts.Length-1; i >= 0; i--)
    //    {
    //        if (DicPlayerHeart[li_PlayerHearts[i].name] == true)
    //        {
    //            DicPlayerHeart[li_PlayerHearts[i].name] = false;
    //            li_PlayerHearts[i].SetActive(false);
                
    //            if (DicPlayerHeart[li_PlayerHearts[0].name] == false)
    //            {
    //                DiePanel.SetActive(true);
    //                isPaused = true;
    //                LevelManager.instance.isRunning = false;
    //                //Time.timeScale = 0f; // 게임 일시정지
    //                player.contorl.Disable();
    //            }
    //            break;
    //        }
    //    }
        
    //}

    public void GameOver()
    {
        hPBar.gameObject.SetActive(false);
        DiePanel.SetActive(true);
        isPaused = true;
        LevelManager.instance.isRunning = false;
        player.DisableControls();
    }

    // 리스폰 버튼
    public void Respawn_Button()
    {
        // 돈이 있으면 부활
        if(Inventory.money_item.count >= 50)
        {
            Inventory.money_item.count -= 50;
            Inventory.FreshSlot();

            // 플레이어 사망 함수 호출
            player.Die(); // 지상이동, 체력회복
            DiePanel.SetActive(false);
            isPaused = false;
            LevelManager.instance.isRunning = true;
            //Time.timeScale = 1f; // 게임 재개
        }
        else
        {
            Debug.Log("돈이 부족합니다");
        }
    }

    public void Restart_LastSave()
    {
        SlotManager.Instance.quitSlotUI.ClearAllSlots();
        DiePanel.SetActive(false);
        isPaused = false;
        LevelManager.instance.isRunning = true;
        //hPBar.UpdateHP(100);
        //Time.timeScale = 1f; // 게임 재개
        //AddPlayerLife(3);
        
        if (!File.Exists(savePath))
        {
            LevelManager.instance.ResetGame();
        }
        else
        {
            SaveSystem.Instance.Load();
        }

        LoadScene.instance.GoMain();
    }

    // 플레이어 체력 증가
    public void AddPlayerLife(int hp)
    {
        
        for (int i = 0; i < li_PlayerHearts.Length; i++)
        {
            int healed = 0;

            if (DicPlayerHeart[li_PlayerHearts[i].name] == false)
            {
                DicPlayerHeart[li_PlayerHearts[i].name] = true;
                li_PlayerHearts[i].SetActive(true);
                healed++;

                if (healed >= hp) break;
            }
        }

    }
    
    // 지상 위 자동 체력 회복
    public void GroundAutoHeal()
    {
        if (this.gameObject.transform.position.y > 0 && li_PlayerHearts[2] != null && li_PlayerHearts[2].activeSelf == false)
        {
            AddPlayerLife(li_PlayerHearts.Length - 1);
        }
    }

    public void Ground_Time_Pause()
    {
        if(this.gameObject.transform.position.y > 0)
        {
            LevelManager.instance.isRunning = false;
        }
        else
        {
            LevelManager.instance.isRunning = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Shop"))
        {
            isNearShop = true;
            Debug.Log("상점에 접근했습니다. F 키로 상호작용.");
        }
        if(other.CompareTag("Museum"))
        {
            isNearMuseum = true;
            Debug.Log("박물관에 접근했습니다. F 키로 상호작용.");
        }
        if(other.CompareTag("Collect"))
        {
            isNearCollection = true;
            Debug.Log("도감에 접근했습니다. F 키로 상호작용.");
        }
        if(other.CompareTag("Table"))
        {
            Debug.Log(other.gameObject.name);
            int idx = int.Parse(other.gameObject.name);
            if(Collection.li_isRelicOnTable[idx] == true)
            {
                RelicInfoImage.GetComponent<Image>().sprite = items[idx].itemImage;
                RelicInfoName.GetComponent<TextMeshProUGUI>().text = items[idx].itemName;
                RelicInfoText.GetComponent<TextMeshProUGUI>().text = items[idx].Info;
                RelicInfoUIPanel.SetActive(true);
            }


            Debug.Log("진열장에 접근했습니다.");
        }
        if(other.CompareTag("Reset"))
        {
            isNearReset = true;
            Debug.Log("리셋에 접근했습니다. F 키로 상호작용.");
        }
        if(other.CompareTag("ReturnMuseum"))
        {
            isNearReturnMuseum = true;
            Debug.Log("리턴에 접근했습니다. F 키로 상호작용.");
        }
        if(other.CompareTag("tutorialExit"))
        {
            isNearTutorialExit = true;
            Debug.Log("튜토리얼 탈출에 접근했습니다. F 키로 상호작용.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Shop"))
        {
            isNearShop = false;
            Debug.Log("상점에서 벗어났습니다.");
        }
        if(other.CompareTag("Museum"))
        {
            isNearMuseum = false;
            Debug.Log("박물관에서 벗어났습니다.");
        }
        if(other.CompareTag("Collect"))
        {
            isNearCollection = false;
            Debug.Log("도감에서 벗어났습니다.");
        }
        if(other.CompareTag("Table"))
        {
            RelicInfoUIPanel.SetActive(false);
            Debug.Log("진열장에서 벗어났습니다.");
        }
        if(other.CompareTag("Reset"))
        {
            isNearReset = false;
            Debug.Log("리셋에서 벗어났습니다.");
        }
        if(other.CompareTag("ReturnMuseum"))
        {
            isNearReturnMuseum = false;
            Debug.Log("리턴에서 벗어났습니다.");
        }
        if(other.CompareTag("tutorialExit"))
        {
            isNearTutorialExit = false;
            Debug.Log("튜토리얼 탈출에서 벗어났습니다.");
        }
    }

}
