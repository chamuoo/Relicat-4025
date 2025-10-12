using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private ItemInstance _instance;

    Vector3 SPWAN_POINT = new Vector3(14.5f, 0.5f, 0);

    public void Setup(ItemInstance instance)
    {
        _instance = instance;
    }

    public void Spawn(PlayerController player)
    {
        player.transform.position = SPWAN_POINT;
    }
}
