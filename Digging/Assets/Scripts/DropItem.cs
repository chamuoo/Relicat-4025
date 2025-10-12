using Spine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DropItem : MonoBehaviour
{
    public int itemType; //0이면 유물, 1이면 광물
    public int itemCode; //아이템 종류
    public int addEA; //아이템 개수
    Rigidbody2D rigidbody;

    [SerializeField] GameObject relicEffect;

    // 액션
    public System.Action<SlotInfo, Item> OnItemPicked;   // 아이템 먹음

    float count = 0;
    bool canTake = false;

    Light2D light2D;
    float lightInnerRadius = 0.1f;
    float lightOuterRadius = 0.3f;

    private bool tryOnce = false;


    void Awake()
    {
        rigidbody = this.gameObject.GetComponent<Rigidbody2D>();
        light2D = this.gameObject.GetComponent<Light2D>();
    }

    void Start()
    {
        float x = Random.Range(-1, 1);
        float y = Random.Range(2, 3);
        rigidbody.velocity = new Vector2(x, y);

        if (itemType == 0)
        {
            relicEffect.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (count < 0.6 && canTake == false)
        {
            count += Time.deltaTime;
        }
        else
        {
            canTake = true;
        }

        if (light2D != null)
        {
            float radiusPingpong = Mathf.PingPong(Time.time / 5f, 0.15f);
            light2D.pointLightInnerRadius = lightInnerRadius + radiusPingpong;
            //light2D.pointLightOuterRadius = lightOuterRadius + radiusPingpong;
        }


    }

    public void setDropItem(int newItemType, int newItemCode, Sprite renderSprite, int newAddEA)
    {
        itemType = newItemType;
        itemCode = newItemCode;
        this.gameObject.GetComponent<SpriteRenderer>().sprite = renderSprite;
        addEA = newAddEA;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
            Physics2D.IgnoreCollision(GetComponent<CircleCollider2D>(), collision.GetComponent<CapsuleCollider2D>(), true);

        if (collision.tag == "Player" && canTake)
        {
            Player playerScript = collision.GetComponent<Player>();

            if (playerScript != null)
            {
                if(itemType == 0) //이 아이템이 유물이면
                {
                    playerScript.Inventory.AddItem(playerScript.items[itemCode], addEA);
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[15]);
                }
                else if (itemType == 1) //이 아이템이 보석이면
                {
                    playerScript.Inventory.AddItem(playerScript.minerals[itemCode], addEA);
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);
                }
                else if(itemType == 2) // 이 아이템이 사용 아이템이라면
                {
                    playerScript.Inventory.AddItem(playerScript.UseItems[itemCode], addEA);
                    SlotManager.Instance.InvenFillSlot();
                    /*int typeValue = (int)playerScript.UseItems[itemCode].type;

                    // 일반적인 값을 가지고 비교연산을 하면 Wepaon도 들어갈 가능성이 있기에 비트 마스크로 함.
                    // 예를 들어 Weapon이 16이고 Item이 10부터 시작하면 값을 비교를 할 때 weapon의 값이 10이상이기 때문에 weapon 타입이 들어갈 수 있음.
                    if((typeValue & (int)ItemCategory.Item) == (int)ItemCategory.Item)
                    {
                        SlotManager.Instance.GiveItem(playerScript.UseItems[itemCode].type, addEA); // 먹은 아이템 보내기
                    }*/

                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);
                }
                else if(itemType == 3) // 이 아이템이 드릴 아이템이라면
                {
                    playerScript.Inventory.AddItem(playerScript.Drill_Items[itemCode], addEA);
                    
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);
                    Inventory.instance.LogMessage("드릴 부품을 획득했습니다.");
                }
                else if (itemType == 4) // 이 아이템이 드릴 배터리 아이템이라면
                {
                    // 드릴이라는 아이템이 퀵슬롯에 있다면 배터리 채우기
                    if(SlotManager.Instance.quitSlotUI.FindSlot(ItemTypes.Drill) && Shop.instance.isCreateDrill == true)
                    {
                        Inventory.instance.LogMessage("드릴의 배터리가 충전되었습니다.");
                        SlotManager.Instance.ChargeEnergy();
                    }
                    else
                    {
                        print("드릴이라는 아이템이 없습니다.");
                        return;
                    }
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);
                }

                Destroy(this.gameObject);

                Debug.Log("enter");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            Physics2D.IgnoreCollision(GetComponent<CircleCollider2D>(), collision.GetComponent<CapsuleCollider2D>(), true);

        if (collision.tag == "Player" && canTake && tryOnce == false)
        {
            Player playerScript = collision.GetComponent<Player>();

            if (playerScript != null)
            {
                if (itemType == 0) //이 아이템이 유물이면
                {
                    playerScript.Inventory.AddItem(playerScript.items[itemCode], addEA);
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[15]);
                }
                else if (itemType == 1) //이 아이템이 보석이면
                {
                    playerScript.Inventory.AddItem(playerScript.minerals[itemCode], addEA);
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);
                }
                else if (itemType == 2) // 이 아이템이 사용 아이템이라면
                {
                    playerScript.Inventory.AddItem(playerScript.UseItems[itemCode], addEA);
                    SlotManager.Instance.InvenFillSlot();

                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);
                }
                else if (itemType == 3) // 이 아이템이 드릴 아이템이라면
                {
                    playerScript.Inventory.AddItem(playerScript.Drill_Items[itemCode], addEA);
                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);
                    Inventory.instance.LogMessage("드릴 부품을 획득했습니다.");
                }
                else if (itemType == 4) // 이 아이템이 드릴 배터리 아이템이라면
                {
                    // 드릴이라는 아이템이 퀵슬롯에 있다면 배터리 채우기
                    if(SlotManager.Instance.quitSlotUI.FindSlot(ItemTypes.Drill) && Shop.instance.isCreateDrill == true)
                    {
                        Inventory.instance.LogMessage("드릴의 배터리가 충전되었습니다.");
                        SlotManager.Instance.ChargeEnergy();
                    }
                    else
                    {
                        print("드릴이라는 아이템이 없습니다.");
                        return;
                    }

                    SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[13]);

                }
                tryOnce = true;
                Destroy(this.gameObject);
                
            }

            Debug.Log("stay");
        }
    }
}

