using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    [SerializeField] GameObject poisonPrefab;
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] PoisonEffectManager effectManager;
    public List<ParticleSystem> pss = new List<ParticleSystem>();

    // 데미지
    private DamageManager damageManager;
    private int POISON_DAMAGE;

    float timer = 10;   //없어지거나 리젠되기 까지의 시간
    bool isBlockPoison = false;     //블록 파생의 독안개인지 확인, 타이머가 다 지났을 때 이게 true로 되어있다면 없어지게 함
    public bool isOperate = false;

    public float attackTimer = 2.0f;    //여기서의 초기값은 생성후 첫 공격까지의 시간, 공격 쿨타임은 Trigger에서 설정
    bool canAttackPlayer = true;

    Vector2 leftTopPos;
    Vector2 rightBottomPos;
    float SizeX;
    float SizeY;

    private void Start()
    {
        damageManager = new DamageManager();

        POISON_DAMAGE = damageManager.GetDamage(LoadScene.instance.difficulty_level,
                ObstacleType.Poison);
    }

    public void Initialize(Vector2 newLeftTopPos, Vector2 newRightBottomPos, int newSizeX, int newSizeY, float newTimer = 10, bool newIsBlockPoison = false, float newAttackTimer = 3.0f)
    {
        foreach (ParticleSystem ps in pss)
        {
            ps.Stop();
            ps.Clear();
            foreach (ParticleSystem child in ps.GetComponentsInChildren<ParticleSystem>())
            {
                child.Stop();
                child.Clear();
            }
            Destroy(ps.gameObject);
        }
        pss.Clear();

        leftTopPos = newLeftTopPos;
        rightBottomPos = newRightBottomPos;
        SizeX = newSizeX;
        SizeY = newSizeY;
        timer = newTimer;
        isBlockPoison = newIsBlockPoison;
        attackTimer = newAttackTimer;
        
        boxCollider.size = new Vector2(SizeX, SizeY);

        if(!isBlockPoison)
            InitPos();

        float startX = -(SizeX - 1) / 2;
        float startY = -(SizeY - 1) / 2;

        for (int x = 0; x < SizeX; x++)
        {
            for(int y = 0; y < SizeY; y++)
            {
                Vector3 pos = new Vector3(startX + x, startY + y, 0);
                GameObject obj = Instantiate(poisonPrefab, pos + transform.position, Quaternion.identity);
                obj.transform.parent = this.transform;
                pss.Add(obj.GetComponent<ParticleSystem>());
            }
        }

        isOperate = true;
    }

    public void InitPos()   //좌표 재설정
    {
        if (effectManager != null && !isBlockPoison)
        {
            bool isOverlap = false;
            Vector2 newPos;
            while (true)
            {
                newPos = new Vector2(Random.Range(leftTopPos.x + (SizeX / 2), rightBottomPos.x - (SizeX / 2)), Random.Range(leftTopPos.y - (SizeY / 2), rightBottomPos.y + (SizeY / 2)));
                foreach (var anotherEffect in effectManager.poisonEffectPool)
                {
                    Vector2 anotherEffectPos = anotherEffect.transform.position;
                    if (anotherEffect != null && anotherEffect != this.gameObject)
                    {
                        if (anotherEffectPos.x >= (newPos.x - (SizeX / 2)) && anotherEffectPos.x <= (newPos.x + (SizeX / 2))
                            && anotherEffectPos.y >= (newPos.y - (SizeY / 2)) && anotherEffectPos.y <= (newPos.y + (SizeY / 2)))
                        {
                            isOverlap = true;
                            break;
                        }
                    }
                }

                if (isOverlap)
                {
                    isOverlap = false;
                    continue;
                }
                else
                {
                    break;
                }
            }
            this.gameObject.transform.position = newPos;

        }
    }

    void Update()
    {

        if (isOperate)  //작동중 일 때
        {
            if(timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else //timer가 다 됬을 때
            {
                if(isBlockPoison) //독안개에서 나온 놈이면 삭제
                {
                    Destroy(gameObject);
                }
                else //상시 생성되는 놈이면 잠시 정지시키고 2~5초정도 쉬는시간 설정
                {
                    isOperate = false;
                    foreach (var ps in pss)
                    {
                        ps.Stop();
                        ps.Clear();
                        foreach(var child in ps.GetComponentsInChildren<ParticleSystem>())
                        {
                            child.Stop();
                            child.Clear();
                        }
                    }
                    timer = Random.Range(2f, 5f);
                }
            }

            if (!canAttackPlayer)
            {
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }
                else
                {
                    canAttackPlayer = true;
                }
            }
        }
        else //작동중이지 않을 때
        {
            if(timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else //쉬는시간 다 되면 위치 초기화 및 파티클 재시작
            {
                InitPos();
                foreach(var ps in pss)
                {
                    ps.Play();
                    foreach(var child in ps.GetComponentsInChildren<ParticleSystem>())
                    {
                        child.Play();
                    }
                }
                attackTimer = 2.0f; //재생성 후 공격까지의 텀
                canAttackPlayer = false;
                timer = 10f;
                isOperate = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.gameObject.GetComponent<PlayerController>() != null && canAttackPlayer && isOperate)
        {
            canAttackPlayer = false;
            attackTimer = 1.0f;
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(POISON_DAMAGE, this.transform.position);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.gameObject.GetComponent<PlayerController>() != null && canAttackPlayer && isOperate)
        {
            canAttackPlayer = false;
            attackTimer = 1.0f;
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(POISON_DAMAGE, this.transform.position);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.gameObject.GetComponent<PlayerController>() != null && canAttackPlayer && isOperate)
        {
            canAttackPlayer = false;
            attackTimer = 1.0f;
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(POISON_DAMAGE, this.transform.position);
        }
    }
}
