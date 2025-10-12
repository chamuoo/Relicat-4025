using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class Tool : MonoBehaviour
{
    public List<GameObject> torchObj = new List<GameObject>();

    [SerializeField] private GameObject bombPrefab;
    public GameObject lampPrefab;
    [SerializeField] private GameObject teleportPrefab;
    [SerializeField] private Vector2 itemSpawnParent;

    public IWeapon currentWeapon { get; set; }

    Dictionary<long, WeaponInstance> weaponData = new();
    Dictionary<ItemTypes, IItem> itemActions;

    private static Tool _instance;
    public static Tool Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<Tool>();

                if(_instance == null)
                {
                    GameObject obj = new GameObject();
                    GameObject player = GameObject.FindGameObjectWithTag("Player");

                    obj.transform.SetParent(player.transform);

                    obj.name = typeof(Tool).Name;
                    obj.AddComponent<Transform>();
                    obj.AddComponent<SpriteRenderer>();
                    _instance = obj.AddComponent<Tool>();
                }
            }

            return _instance;
        }
    }

    public SpriteRenderer sprite;

    public WeaponInstance GetData(long id)
    {
        if(weaponData.TryGetValue(id, out WeaponInstance data))
            return data;

        return null;
    }

    public void SetData(long id, WeaponInstance newdata)
    {
        weaponData[id] = newdata;
    }   

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        sprite = GetComponent<SpriteRenderer>();

        itemActions = new Dictionary<ItemTypes, IItem>
        {
            { ItemTypes.Bomb, new BombItem(bombPrefab) },
            { ItemTypes.Lamp, new LampItem(lampPrefab) },
            { ItemTypes.Teleport, new TeleportItem(teleportPrefab) },
        };        
    }

    public bool HasWeapon(long id)
    {
        return weaponData.ContainsKey(id);
    }

    public void EquipWeapon(SlotInfo slot)
    {
        ReplaceToolComponent(slot.slot.weapon);
        currentWeapon.SetInstance(slot.slot.weapon);

        SlotManager.Instance.SelectSlot(slot);
    }

    // 무기 사용
    public void UseWeapon(SlotInfo slot, Vector2 mousePos, WeaponInstance weapon, Player player)
    {
        PlayerController _player = player.GetComponent<PlayerController>();

        if(currentWeapon == null)
        {
            ReplaceToolComponent(slot.slot.weapon);
            SetData(slot.slot.weapon._id, slot.slot.weapon);
        }
        else if(currentWeapon.Instance._id != weapon._id)
        {
            ReplaceToolComponent(weapon); // 기존 컴포넌트 제거하고 새 무기 등록

            // 중복 저장 방지
            if(!HasWeapon(weapon._id))
                SetData(weapon._id, weapon);
        }

        currentWeapon.SetInstance(weapon);
        sprite.sprite = currentWeapon.Instance.itemImage;

        // UI
        SlotManager.Instance.SelectSlot(slot);

        currentWeapon?.Use(mousePos, player, _player._state);
    }

    // 아이템 사용
    public void UseItem(SlotInfo slot, bool isGrounded, ItemInstance item)
    {
        itemSpawnParent = GameObject.FindGameObjectWithTag("Player").transform.position;
        Transform playerContext = transform;

        if(!itemActions.TryGetValue(item._item.type, out IItem itemAction))
        {
            print("지원되지 않는 아이템입니다.");
            return;
        }

        itemAction.Use(item, itemSpawnParent, isGrounded, playerContext);
        sprite.sprite = slot.slot.item.itemImage;

        // 개수 감소
        int count = --item._item.count;
        Inventory.Instance.FreshSlot();

        // UI 갱신
        SlotManager.Instance.SelectSlot(slot);
        UIController.Instance.SetSlotText(slot, count);
    }


    // 타입에 맞는 무기 데이터 넣기
    public void ReplaceToolComponent(WeaponInstance weapon)
    {
        // 현재 무기 컴포넌트 찾기
        IWeapon currentWeaponComponent = GetComponent<IWeapon>();

        // 교체할 무기의 컴포넌트 타입 얻기
        Type newWeaponType = GetWeaponTypeFromItemType(weapon._template.type);

        if(currentWeapon != null && currentWeaponComponent.GetType() == newWeaponType)
            return; // 같은 무기가 장착되어 있으면 교체 X

        // 기존 무기 컴포넌트 제거
        if(currentWeaponComponent != null)
            Destroy(currentWeaponComponent as Component);

        currentWeapon = (IWeapon)gameObject.AddComponent(newWeaponType);
    }
    
    private Type GetWeaponTypeFromItemType(ItemTypes itemType)  // 무기 추가 시 여기에 추가
    {
        if(itemType == ItemTypes.Pickaxe)
            return typeof(Pickaxe);
        else if(itemType == ItemTypes.Drill)
            return typeof(Drill);

        // 추가 무기 타입 처리
        throw new NotImplementedException();
    }

    public SlotInfoData GetCurrentWeaponSlotData()
    {
        if(currentWeapon == null)
            return null;

        long id = currentWeapon.Instance._id;

        var slot = SlotManager.Instance.quitSlotUI.slotInfos
            .FirstOrDefault(s => s.slot != null && s.slot.weapon != null && s.slot.weapon._id == id);

        return slot?.ToData();
    }
}
