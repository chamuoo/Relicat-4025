using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif

public class InventoryUI : MonoBehaviour
{
    #region Field

    // 슬롯에 관한 필드
    private WeaponInstance instanceW;
    private ItemInstance instanceI;

    [SerializeField] private List<SlotInfo> slotInfos;

    #endregion Field

    // 인벤토리 데이터 채우기
    public void InitFillData()
    {
        //slotInfos = GetComponentsInChildren<SlotInfo>(true).ToList();

        //for(int i = 0; i < slotInfos.Count; i++)
        //{
        //    slotInfos[i].Initialize(SlotType.Inventory, i); // 슬롯 타입과 슬롯 순서 넣기

        //    var item = slotInfos[i].GetComponentInChildren<SlotInteraction>();
        //    // 좌클릭: 선택
        //    item.onLeftClick = SlotManager.Instance.SetSelectedSlot;

        //    // 우클릭: 제거
        //    item.onRightClick = SlotManager.Instance.RemoveInstance;
        //}
    }

    // 인벤토리 초기화
    public void ResetInventorySlot()
    {
        foreach(var slot in slotInfos)
        {
            slot.GetComponentInChildren<SlotInteraction>().Clear();
        }
    }

    // 빈 슬롯 찾기 -> 나중에 개수 제한이나 무기가 인벤토리까지 가면 그때 사용
    /*public SlotInfo FindEmptySlot(bool isWeapon, string name)
    {
        foreach(var slot in slotInfos)
        {
            // 완전히 비어있는 슬롯에 무기 일 때
            if(slot._instanceW == null && slot._instanceI == null)
                return slot;
            // 아이템이 있는 상황
            else if(!isWeapon && slot._instanceW == null && slot._instanceI != null)
            {
                if(slot._instanceI._item.itemName == name)
                    return slot;
                continue;
            }
            // 무기가 있는 상황
            else if(slot._instanceW != null && slot._instanceI == null)
                continue;
            else
                continue;
        }

        return null;
    }*/

}
