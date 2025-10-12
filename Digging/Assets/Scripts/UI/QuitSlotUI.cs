using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif


public class QuitSlotUI : MonoBehaviour
{
    [SerializeField] public List<SlotInfo> slotInfos { get; set; }

    private static readonly Dictionary<ItemTypes, int> ItemTypeToSlotIndex = new()
    {
        { ItemTypes.Pickaxe, 0 },
        { ItemTypes.Bomb, 1 },
        { ItemTypes.Lamp, 2 },
        { ItemTypes.Teleport, 3 },
        { ItemTypes.Drill, 4 },
    };

    #region Func

    public void InitFillData()
    {
        slotInfos = GetComponentsInChildren<SlotInfo>(true).ToList();

        // 나중에 키 통합이 되면 사용될 것임.
        //for(int i = 0; i < slotInfos.Count; i++)
        //{
        //    slotInfos[i].Initialize(SlotType.QuickSlot, i); // 슬롯 타입과 슬롯 순서 넣기

        //    var item = slotInfos[i].GetComponentInChildren<SlotInteraction>();
        //    // 좌클릭: 선택
        //    item.onLeftClick = SlotManager.Instance.SetSelectedSlot;

        //    // 우클릭: 제거
        //    //item.onRightClick = SlotManager.Instance.RemoveInstance;
        //}
    }

    private void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.RightShift))
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== 슬롯 정보 출력 시작 ===");

            foreach(var slot in slotInfos)
            {
                if(slot == null || slot.slot == null)
                {
                    sb.AppendLine("[Slot] 빈 슬롯입니다.");
                    continue;
                }

                sb.AppendLine($"[Slot Index {slot._index}]");

                if(slot.slot.weapon != null)
                {
                    var weapon = slot.slot.weapon;
                    sb.AppendLine("  [Weapon]");
                    sb.AppendLine($"    ID       : {weapon._id}");
                    sb.AppendLine($"    Level    : {weapon._level}");
                    sb.AppendLine($"    Damage   : {weapon._damage}");
                    sb.AppendLine($"    Template : {weapon._template?.name}");
                }
                else if(slot.slot.item != null)
                {
                    var item = slot.slot.item;
                    sb.AppendLine("  [Item]");
                    sb.AppendLine($"    Name     : {item._item?.itemName}");
                    sb.AppendLine($"    Count    : {item._count}");
                    sb.AppendLine($"    Template : {item._item?.name}");
                }
                else
                {
                    sb.AppendLine("  슬롯에 무기나 아이템이 없습니다.");
                }
            }

            sb.AppendLine("=== 슬롯 정보 출력 끝 ===");

            Debug.Log(sb.ToString());
        }*/
    }

    // 아이템 타입에 맞는 슬롯 찾기
    public SlotInfo FindSlot(ItemTypes type)
    {
        if(!ItemTypeToSlotIndex.TryGetValue(type, out int index))
        {
            Debug.LogWarning($"[Slot] 슬롯 인덱스를 찾을 수 없습니다: {type}");
            return null;
        }

        if(index < 0 || index >= slotInfos.Count)
        {
            Debug.LogError($"[Slot] 슬롯 인덱스 범위 초과: {index}");
            return null;
        }

        return slotInfos[index];
    }

    public List<SlotInfoData> ToData()
    {
        return slotInfos
        .Where(slot => slot != null)
        .Select(slot => slot.ToData())
        .ToList();
    }

    public void ClearAllSlots()
    {
        foreach(var slot in slotInfos)
        {
            slot.slot = null;
            UIController.Instance.SetSlotText(slot, 0);
            slot.ClearSlot();
        }
    }

    public void ClearNullSlots()
    {
        foreach(var slot in slotInfos)
        {
            if(slot.slot.weapon == null && slot.slot.item == null)    // 빈 데이터
            {
                slot.slot = null;
                UIController.Instance.SetSlotText(slot, 0);
                slot.ClearSlot();
            }
        }
    }

    #endregion Func
}

