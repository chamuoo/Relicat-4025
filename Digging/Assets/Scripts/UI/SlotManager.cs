using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.VFX;
//using UnityEngine.Windows;
using System.IO;
#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif

public enum ItemCategory
{
    Weapon = 0b0001_0000, // 16
    Item = 0b0010_0000,   // 32
}

public class SlotManager : Singleton<SlotManager>
{
    #region Field
    public GameObject energy;
    public GameObject energyClone;

    // 액션 입력
    public System.Action<bool> OnInventoryOpen; // 인벤토리 열림
    public Action PickupEnergy; // 드릴 배터리 채우기

    private SlotInfo _selectedSlot;

    // 인벤토리들
    [SerializeField] public QuitSlotUI quitSlotUI;
    [SerializeField] private InventoryUI inventoyUI;
    [SerializeField] private Inventory inventory;

    public bool _isOpen;

    #endregion Field

    #region Event
    private void Awake()
    {
        quitSlotUI = GameObject.FindObjectOfType<QuitSlotUI>();
        inventory = GameObject.FindObjectOfType<Inventory>();
    }

    private void Update()
    {
        // 단일 키가 아니기 때문에 지금은 쓸모가 없음.
        //for(int i = 0; i < quitSlotUI.quickSlots.Count; i++)
        //{
        //    if(UnityEngine.Input.GetKeyDown(KeyCode.Alpha1 + i))
        //    {
        //        // 인벤토리가 열렸을 경우에만 클릭 가능
        //        if(_isOpen && selectedSlot != null)
        //        {
        //            // 데이터 교환 및 슬롯 선택
        //            RouteInputToTarget(i);
        //        }
        //        // 인벤토리가 열리지 않은 상태에서는 무기 세팅
        //        else if(!_isOpen && selectedSlot == null)
        //        {
        //            // 무기 선택
        //            EquipWeapon(quitSlotUI.quickSlots[i]);
        //        }
        //    }
        //}
    }

    #endregion Event

    #region Func
    public void SelectSlot(SlotInfo newSlot)
    {
        if(_selectedSlot != null && _selectedSlot != newSlot)
            _selectedSlot.Deselect(); // 이전 슬롯 비활성화

        _selectedSlot = newSlot;

        _selectedSlot.Select(); // 새로운 슬롯 선택
    }

    public SlotInfo GetSelectedSlot() => _selectedSlot;

    // 레벨에 따른 초기 세팅들
    public void Initialize()
    {
        GiveItem(ItemTypes.Pickaxe, 1);
        Tool.Instance.EquipWeapon(quitSlotUI.FindSlot(ItemTypes.Pickaxe));
        InvenFillSlot();
    }

    // 카테고리에 맞는 인스턴스 생성
    public SlotData CreateInstanceByCategory(ItemTypes type, ScriptableObject template)
    {
        int typeValue = (int)type;

        if((typeValue & (int)ItemCategory.Weapon) == (int)ItemCategory.Weapon)
        {
            if(template is WeaponTemplate weaponTemplate)
            {
                WeaponInstance instance = new WeaponInstance(weaponTemplate);
                return new SlotData
                {
                    weapon = instance,
                    item = null
                };
            }
        }
        else if((typeValue & (int)ItemCategory.Item) == (int)ItemCategory.Item)
        {
            if(template is Item itemTemplate)
            {
                ItemInstance instance = new ItemInstance(itemTemplate);
                return new SlotData
                {
                    weapon = null,
                    item = instance
                };
            }
        }

        return null;
    }

    // 로드된 데이터에 맞는 인스턴스 생성
    public SlotData CreateLoadInstanceByCategory(ItemTypes type, SlotData data)
    {
        int typeValue = (int)type;

        if((typeValue & (int)ItemCategory.Weapon) == (int)ItemCategory.Weapon)
        {
            if(data.weapon != null)
            {
                return new SlotData
                {
                    weapon = data.weapon,
                    item = null
                };
            }
        }
        else if((typeValue & (int)ItemCategory.Item) == (int)ItemCategory.Item)
        {
            if(data.item != null)
            {
                return new SlotData
                {
                    weapon = null,
                    item = data.item
                };
            }
        }

        return null;
    }

    // 카테고리에 맞는 인스턴스 넣기
    public void SetInstanceData(SlotInfo slot, SlotData data)
    {
        slot.slot = CreateLoadInstanceByCategory(slot._type, data);
    }

    public void GiveItem(ItemTypes type, int amount)
    {
        SlotInfo selectSlot = quitSlotUI.FindSlot(type);    // 넣을 슬롯 찾아오기

        // 1. 슬롯이 이미 존재하고, 인스턴스가 있음 -> 수량만 증가
        if(selectSlot.slot != null)
        {
            object instance = selectSlot.slot.ActiveInstance;

            if(instance is ItemInstance itemInstance)
            {
                itemInstance._item.count = amount;
                selectSlot.SetSlotImage();
                UIController.Instance.SetSlotText(selectSlot, itemInstance._item.count);
            }
            else if(instance is WeaponInstance weaponInstance)
                return;

            return;
        }
        else
        {
            // 2. 슬롯이 비어 있다면 -> 템플릿 가져와서 인스턴스 생성
            ScriptableObject temp = Repository.Instance.GetTemplate(type);  // 해당 타입에 맞는 데이터 가져오기

            selectSlot.slot = CreateInstanceByCategory(type, temp);  // 해당 슬롯의 인스턴스 생성
            selectSlot.SetSlotImage();
            UIController.Instance.SetSlotText(selectSlot, amount); // 해당 슬롯의 텍스트 업데이트

            if(type == ItemTypes.Drill)
            {
                Transform lastChild = selectSlot.transform.GetChild(selectSlot.transform.childCount - 1);
                energyClone = Instantiate(energy, lastChild);
                var rect = energyClone.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); rect.anchoredPosition = new Vector2(0, -40f); rect.sizeDelta = new Vector2(80, 30);
            }
        }
    }

    // 인벤토리 열었는 지 여부
    public void HandleInventoryOpen(bool isOpen)
    {
        _isOpen = isOpen;
    }


    /* 슬롯들 채우기(아이템)[아이템 최대 개수, 인벤토리에 다른 아이템을 집어 넣을 때 사용]
    public void FillSlot(Item template, int addEA)
    {
        int remaining = addEA;

        for(int i = 0; i < addEA; i++)
        {
            if(remaining <= 0)
                break;

            // 1. 퀵슬롯
            SlotInfo slot = quitSlotUI.FindEmptySlot(false, template.itemName);

            // 2. 인벤토리
            if(slot == null)
            {
                inventory.AddItem(template, remaining);
                Debug.Log($"[퀵슬롯] 꽉 참. 인벤토리로 {addEA}개 이동.");
                return;
            }           
            int added = TryFillSlot(slot, template, remaining);
            remaining -= added;

            UpdateText(slot);
        }
        inventory.ItemLog(template, addEA);
    }*/

    // 초기 슬롯들 아이템 채우기(아이템)
    public void InvenFillSlot()
    {
        // 인벤토리에 있는 아이템들 가져오기
        foreach(Item item in inventory.items)
        {
            if(item == null || !item.name.StartsWith("Item_Use"))
                continue;

            string suffix = item.name.Substring("Item_Use".Length);
            if(suffix != "01" && suffix != "02" && suffix != "03")
                continue;

            GiveItem(item.type, item.count);

            // 나중에 아이템에 대한 제한 개수가 있다면 쓸 예정
            /*if(item.item.count > item.item.stackLimit) // 아이템의 개수가 최대치보다 많다면
            {
                slot._instanceI._count = item.item.stackLimit;  // 최대치로 계수 저장
                item.item.count -= slot._instanceI._count;
            }
            else if(item.item.count <= item.item.stackLimit) // 아이템이 최대치보다 작다면
            {
                slot._instanceI._count = item.item.count;
                item.item.count -= slot._instanceI._count;  // 현재 개수를 넣기
            }*/
        }
    }

    public void LoadInvenFillSlot()
    {
        // 인벤토리에 있는 아이템들 가져오기
        foreach(Item item in inventory.items)
        {
            if(item == null || !item.name.StartsWith("Item_Use"))
                return;

            // UI
            GiveItem(item.type, item.count);
        }
    }

    // 무기 업그레이드 이미지 가져오기
    public void UpgradeWeapon(SlotInfo slot, float newDamage)
    {
        var instance = slot.slot.ActiveInstance;

        if(instance is WeaponInstance weaponInstance)
        {
            long id = weaponInstance._id;

            // 1. Tool에 저장된 인스턴스 가져오기
            WeaponInstance storedInstance = Tool.Instance.GetData(id);

            if(storedInstance != null)
            {
                // 2. 데미지 수정
                storedInstance._level += 1;
                storedInstance._damage = newDamage;
                storedInstance.itemImage = storedInstance.GetSprite();

                // 3. Tool에 다시 저장
                Tool.Instance.SetData(id, storedInstance);

                // 4. 장착 중인 무기 컴포넌트에 반영
                slot.slot.weapon = storedInstance;
                Tool.Instance.sprite.sprite = slot.slot.weapon.itemImage;

                // UI
                slot.SetSlotImage();
            }
        }
    }

    // 에너지 충전
    public void ChargeEnergy()
    {
        if(energyClone == null)
            return;

        energyClone.GetComponent<EnergyBar>().SetValue(100);
        SlotInfo slot = quitSlotUI.FindSlot(ItemTypes.Drill);

        slot.slot.weapon._energy = 100;
    }

    // 드릴 에너지 감소 및 충전
    public void BindDrillEnergy(Drill drill, float energy)
    {
        // 1. 슬롯에 드릴이 있는지 확인
        SlotInfo slot = quitSlotUI.FindSlot(ItemTypes.Drill);

        if(slot == null)
        {
            print("드릴이 없습니다.");
            return;
        }

        EnergyBar bar = slot.GetComponentInChildren<EnergyBar>(true);
        //bar.SetMax(slot._instanceW._energy); // 나중에 강화 중에서 energy 증가를 위해 남겨둠.

        bar.SetValue(energy);
    }

    public void LoadQuickSlots(SaveData loaded)
    {
        // 1. 저장된 슬롯 데이터 복원
        foreach(var saved in loaded.quickSlotInfoData.slots)
        {
            var slot = quitSlotUI.slotInfos.FirstOrDefault(s => s._index == saved.index);
            if(slot == null) continue;

            slot._index = saved.index;
            slot._typeS = saved.type;

            // 무기 인스턴스 복원
            if(saved.data.weapon != null && saved.data.weapon._template != null)
            {
                SetInstanceData(slot, saved.data);

                slot.SetSlotImage();
                UIController.Instance.SetSlotText(slot, 1);

                // Tool 딕셔너리에도 등록
                Tool.Instance.SetData(saved.data.weapon._id, saved.data.weapon); 
            }
            // 아이템 인스턴스 복원
            else if(saved.data.item != null && saved.data.item._item != null)
            {
                SetInstanceData(slot, saved.data);
            }
        }

        LoadInvenFillSlot();

        // 3. 현재 장착 무기 복원
        if(loaded.quickSlotInfoData.currentWeapon != null)
        {
            int weaponIndex = loaded.quickSlotInfoData.currentWeapon.index;
            var weaponSlot = quitSlotUI.slotInfos.FirstOrDefault(s => s._index == weaponIndex);

            if(weaponSlot != null)
            {
                Tool.Instance.EquipWeapon(weaponSlot);
            }
        }
    }

    #endregion Func
}
