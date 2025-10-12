using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using static UnityEngine.UI.Image;

[System.Serializable]
public class BombItem : IItem
{
    private GameObject _bombPrefab;

    public BombItem(GameObject prefab)
    {
        _bombPrefab = prefab;
    }

    public void Use(ItemInstance instance, Vector3 playerPos, bool isGround, Transform context)
    {
        GameObject.Instantiate(_bombPrefab, context.position, Quaternion.identity);
    }
}

[System.Serializable]
public class LampItem : IItem
{
    private GameObject _lampPrefab;

    public LampItem(GameObject prefab)
    {
        _lampPrefab = prefab;
    }

    public void Use(ItemInstance instance, Vector3 playerPos, bool isGround, Transform context)
    {
        if(!isGround)
        {
            Debug.Log("램프는 공중에서 설치할 수 없습니다.");
            return;
        }

        // 횟불 설치 위치
        float playerSize = context.GetComponentInParent<PlayerController>().
            GetComponent<SpriteRenderer>().size.y;
        float torchSize = _lampPrefab.GetComponent<SpriteRenderer>().size.y;
        Vector3 placePos = new Vector3(
           Mathf.Floor(playerPos.x * 10f) / 10f,    // 소수 첫번재 자리까지
           (playerPos.y - (playerSize - torchSize)) - 0.1f
        );

        GameObject lamp = GameObject.Instantiate(_lampPrefab, placePos, Quaternion.identity);
        Tool.Instance.torchObj.Add(lamp);
    }
}

[System.Serializable]
public class TeleportItem : IItem
{
    private GameObject _teleportPrefab;

    public TeleportItem(GameObject prefab)
    {
        _teleportPrefab = prefab;
    }

    public void Use(ItemInstance instance, Vector3 playerPos, bool isGround, Transform context)
    {
        if(!isGround)
        {
            Debug.Log("Teleport를 사용할 수 없습니다.");
            return;
        }

        PlayerController player = context.GetComponentInParent<PlayerController>();
        Teleport teleport = new Teleport();
        teleport.Spawn(player);
    }
}
