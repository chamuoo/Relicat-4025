using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lamp : MonoBehaviour
{
    [SerializeField] AudioSource audio;
    [SerializeField] private ItemInstance _instance;

    [SerializeField] GameObject dropItem;
    [SerializeField] Player playerScript;
    Light2D light2D;
    float lightInnerRadius = 0.3f;
    float randFloat;

    bool itemDropped = false;

    public void Setup(ItemInstance instance)
    {
        _instance = instance;
    }

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        light2D = this.gameObject.GetComponent<Light2D>();
        playerScript = playerObj.GetComponent<Player>();
        randFloat = Random.Range(0f, 1f);
        audio = GetComponent<AudioSource>();
    }
    private void Start()
    {
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[14]);
    }

    private void Update()
    {
        CheckBelowAndDrop();

        if(light2D != null)
        {
            float radiusPingpong1 = Mathf.PingPong(Time.time / 3 + randFloat, 0.5f);
            light2D.pointLightInnerRadius = radiusPingpong1;
        }
    }

    public void CheckBelowAndDrop()
    {
        if(itemDropped) return;

        Vector2 origin = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, LayerMask.GetMask("Block"));

        if(hit.collider == null)
        {
            ItemDrop(2, 1, playerScript, 1);
            itemDropped = true;

            if(Tool.Instance.torchObj.Contains(this.gameObject))
            {
                Tool.Instance.torchObj.Remove(this.gameObject);
            }

            Destroy(gameObject);
        }
        else
        {
            itemDropped = false;
        }

    }

    void ItemDrop(int itemType, int itemCode, Player playerScript, int addEA) //itemType 0은 유물, 1은 광물
    {
        GameObject newDropItem = Instantiate(dropItem, this.transform.position, Quaternion.identity);

        DropItem dropItemScript = newDropItem.GetComponent<DropItem>();

        Sprite dropItemSprite = null;

        if(itemType == 2)
        {
            dropItemSprite = playerScript.UseItems[itemCode].itemImage;
        }

        dropItemScript.setDropItem(itemType, itemCode, dropItemSprite, addEA);
    }
}
