using UnityEngine;

public interface IItem
{
    void Use(ItemInstance instance, Vector3 spwanPos, bool isGround, Transform context);

}