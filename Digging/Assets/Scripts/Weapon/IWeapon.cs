
using UnityEngine;
using static PlayerController;

public interface IWeapon
{
    WeaponInstance Instance { get;}

    bool isDigging { get; set; }

    void Use(Vector2 mousePos, Player player, PlayerState state);

    void SetInstance(WeaponInstance weapon);
}
