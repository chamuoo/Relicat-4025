using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockBombEffect : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;

    [SerializeField] Sprite blockBombEffect_0;
    [SerializeField] Sprite blockBombEffect_1;
    [SerializeField] Sprite blockBombEffect_2;
    [SerializeField] Sprite blockBombEffect_3;
    [SerializeField] Sprite blockBombEffect_4;
    [SerializeField] Sprite blockBombEffect_5;
    [SerializeField] Sprite blockBombEffect_6;
    [SerializeField] Sprite blockBombEffect_7;
    [SerializeField] Sprite blockBombEffect_8;

    int image_idx = 0;
    float frameCount = 0.03f;

    // 데미지
    private DamageManager damageManager;
    private int BOMBBLOCK_DAMAGE;

    bool isHitPlayer = false;

    List<GameObject> hitBlocks = new List<GameObject>();
    Player playerScript;
    List<GameObject> hitEnemys = new List<GameObject>();

    private void Awake()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    private void Start()
    {
        damageManager = new DamageManager();

        BOMBBLOCK_DAMAGE = damageManager.GetDamage(LoadScene.instance.difficulty_level,
                ObstacleType.BombBlock);

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[19]);
    }

    void Update()
    {
        if (image_idx == 0 && spriteRenderer.sprite != blockBombEffect_0)
        {
            spriteRenderer.sprite = blockBombEffect_0;
        }
        else if (image_idx == 1 && spriteRenderer.sprite != blockBombEffect_1)
        {
            spriteRenderer.sprite = blockBombEffect_1;
        }
        else if (image_idx == 2 && spriteRenderer.sprite != blockBombEffect_2)
        {
            spriteRenderer.sprite = blockBombEffect_2;
        }
        else if (image_idx == 3 && spriteRenderer.sprite != blockBombEffect_3)
        {
            spriteRenderer.sprite = blockBombEffect_3;
        }
        else if (image_idx == 4 && spriteRenderer.sprite != blockBombEffect_4)
        {
            spriteRenderer.sprite = blockBombEffect_4;
        }
        else if (image_idx == 5 && spriteRenderer.sprite != blockBombEffect_5)
        {
            spriteRenderer.sprite = blockBombEffect_5;
        }
        else if (image_idx == 6 && spriteRenderer.sprite != blockBombEffect_6)
        {
            spriteRenderer.sprite = blockBombEffect_6;
        }
        else if (image_idx == 7 && spriteRenderer.sprite != blockBombEffect_7)
        {
            spriteRenderer.sprite = blockBombEffect_7;
        }
        else if (image_idx == 8 && spriteRenderer.sprite != blockBombEffect_8)
        {
            spriteRenderer.sprite = blockBombEffect_8;
        }
    }

    void FixedUpdate()
    {
        frameCount -= Time.deltaTime;
        if (frameCount < 0)
        {
            frameCount = 0.03f;
            switch(image_idx)
            {
                case 0:
                    image_idx = 1;
                    break;
                case 1:
                    image_idx = 2;
                    break;
                case 2:
                    image_idx = 3;
                    break;
                case 3:
                    image_idx = 4;
                    break;
                case 4:
                    image_idx = 5;
                    break;
                case 5:
                    image_idx = 6;
                    break;
                case 6:
                    image_idx = 7;
                    break;
                case 7:
                    image_idx = 8;
                    break;
                case 8:
                    Destroy(this.gameObject);
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isHitPlayer && collision.tag == "Player" && collision.gameObject.GetComponent<PlayerController>() != null)
        {
            isHitPlayer = true;
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(BOMBBLOCK_DAMAGE, this.transform.position);
        }

        if (!(hitBlocks.Contains(collision.gameObject)) && collision.tag == "Block")    //범위 내 블록 파괴 시도, 불필요시 삭제 및 대미지 조정 필요시 조정
        {
            Block blockScript = collision.gameObject.GetComponent<Block>();
            if (blockScript != null)
            {
                blockScript.BlockDestroy(10, playerScript);
                hitBlocks.Add(collision.gameObject);
            }
        }

        if(!(hitEnemys.Contains(collision.gameObject) && collision.tag == "Enemy"))     //범위 내 적 처치 시도, 불필요시 삭제, 블락 파괴를 안할 시 삭제
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.EnemyDie();
                hitEnemys.Add(collision.gameObject);
            }
        }

        if (collision.tag == "Torch")       //범위 내 토치 드랍 시도, 블락 파괴를 안할 시 삭제
        {
            Lamp torchScript = collision.gameObject.GetComponent<Lamp>();
            if (torchScript != null)
            {
                torchScript.CheckBelowAndDrop();
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isHitPlayer && collision.tag == "Player" && collision.gameObject.GetComponent<PlayerController>() != null)
        {
            isHitPlayer = true;
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(BOMBBLOCK_DAMAGE, this.transform.position);
        }

        if (!(hitBlocks.Contains(collision.gameObject)) && collision.tag == "Block")    //범위 내 블록 파괴 시도, 불필요시 삭제 및 대미지 조정 필요시 조정
        {
            Block blockScript = collision.gameObject.GetComponent<Block>();
            if (blockScript != null)
            {
                blockScript.BlockDestroy(10, playerScript);
                hitBlocks.Add(collision.gameObject);
            }
        }

        if (!(hitEnemys.Contains(collision.gameObject) && collision.tag == "Enemy"))     //범위 내 적 처치 시도, 불필요시 삭제, 블락 파괴를 안할 시 삭제
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.EnemyDie();
                hitEnemys.Add(collision.gameObject);
            }
        }

        if (collision.tag == "Torch")       //범위 내 토치 드랍 시도, 블락 파괴를 안할 시 삭제
        {
            Lamp torchScript = collision.gameObject.GetComponent<Lamp>();
            if (torchScript != null)
            {
                torchScript.CheckBelowAndDrop();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isHitPlayer && collision.tag == "Player" && collision.gameObject.GetComponent<PlayerController>() != null)
        {
            isHitPlayer = true;
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(BOMBBLOCK_DAMAGE, this.transform.position);
        }

        if (!(hitBlocks.Contains(collision.gameObject)) && collision.tag == "Block")    //범위 내 블록 파괴 시도, 불필요시 삭제 및 대미지 조정 필요시 조정
        {
            Block blockScript = collision.gameObject.GetComponent<Block>();
            if (blockScript != null)
            {
                blockScript.BlockDestroy(10, playerScript);
                hitBlocks.Add(collision.gameObject);
            }
        }

        if (!(hitEnemys.Contains(collision.gameObject) && collision.tag == "Enemy"))     //범위 내 적 처치 시도, 불필요시 삭제, 블락 파괴를 안할 시 삭제
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.EnemyDie();
                hitEnemys.Add(collision.gameObject);
            }
        }

        if (collision.tag == "Torch")       //범위 내 토치 드랍 시도, 블락 파괴를 안할 시 삭제
        {
            Lamp torchScript = collision.gameObject.GetComponent<Lamp>();
            if (torchScript != null)
            {
                torchScript.CheckBelowAndDrop();
            }
        }
    }
}
