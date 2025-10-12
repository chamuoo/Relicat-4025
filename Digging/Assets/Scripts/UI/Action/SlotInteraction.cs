using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotInteraction : MonoBehaviour
{
    [SerializeField] private RectTransform rect;

    [SerializeField] SlotInfo slotInfo;

    // 액션
    public Action<SlotInfo> onLeftClick;
    public Action<SlotInfo> onRightClick;

    [SerializeField] private Image image;
    [HideInInspector] public Transform parentAfterDrag;

    public void Apply(SlotInfo slot)
    {
        // 퀵슬롯인 경우
        object instance = slot.slot.ActiveInstance;

        Sprite icon = null;

        if(instance is ItemInstance itemInstance)
        {
            icon = itemInstance.itemImage;
        }
        else if(instance is WeaponInstance weaponInstance)
        {
            icon = weaponInstance.itemImage;
        }

        if(icon == null)
        {
            Clear();
            return;
        }

        // 이미지 설정
        image.sprite = icon;
        image.color = new Color(1, 1, 1, 1);

        // UI 사이즈 조절
        float width = icon.rect.width;
        float height = icon.rect.height;

        Vector2 size = rect.sizeDelta;

        if(width > height + 10)
        {
            size.x = 80f;
            size.y = height;
        }
        else if(height > width + 10)
        {
            // Ver 1.0
            //size.x = width;

            // Ver 1.1
            float ratio = width / height;
            float turncated = Mathf.Floor(ratio * 100f) / 100f; // 소수점 둘째 자리까지 자르기 
            float sizeX = width * (Mathf.Ceil(ratio * 10f) / 10f);    // 소수점 첫째 자리까지 올림.

            size.x = sizeX;
            size.y = 80f;
        }
        else
        {
            size = new Vector2(80, 80);
        }

        rect.sizeDelta = size;
    }

    // 이미지 없애기
    public void Clear()
    {
        Vector2 size = new Vector2(80, 80);
        rect.sizeDelta = size;
        image.sprite = null;
        image.color = new Color(0, 0, 0, 0);
    }

}
