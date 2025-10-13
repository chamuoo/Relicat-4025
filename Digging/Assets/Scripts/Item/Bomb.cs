using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class Bomb : MonoBehaviour
{
    [SerializeField] AudioSource audio;
    [SerializeField] Player playerScript;
    [SerializeField] private ItemInstance _instance;

    private DamageManager damageManager;
    private int damage;

    // 카운트
    private float timer;
    private float countdown = 4f;

    // 애니메이션
    Animator anim;
    bool hasExploded = false;

    // 폭발 범위
    [SerializeField] Vector2 exploSize = new Vector2(3f, 1f);

    // 사이즈 조절
    Vector3 originalScale;

    private float minScale = 0.5f;
    private float maxScale = 1.5f;
    private float pulseDuration = 1f; // 총 1초 주기

    // 폭탄 던지기
    private Rigidbody2D rb;
    private float throwForceX = 5f;
    private float throwForceY = 5f;

    private bool isGrounded = false;
    private bool isAlreadyDamaged = false;

    private void Awake()
    {
        originalScale = Vector3.one;
        rb = GetComponent<Rigidbody2D>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        anim = GetComponent<Animator>();
        anim.enabled = false;
        audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        timer = countdown;
        damageManager = new DamageManager();

        damage = damageManager.GetDamage(LoadScene.instance.difficulty_level,
                ObstacleType.Bomb);

        // 생성 후, 던지기
        //Throw(GetPlayerDirection());
        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[18]);
    }

    private void Update()
    {
        if(hasExploded)
        {
            transform.localScale = originalScale;
            return;
        }

        timer -= Time.deltaTime;

        if(timer <= 0f)
            Explode();

        // 시간 비율 계산 (0 ~ 1 ~ 0 반복)
        //float t = Mathf.PingPong(Time.time / pulseDuration, 1f);
        //float scale = Mathf.SmoothStep(minScale, maxScale, t);

        //Vector3 newScale = originalScale * scale;
        //transform.localScale = newScale;
    }

    Vector2 GetPlayerDirection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player == null)
            return Vector2.right * throwForceX; // fallback

        Vector2 playerPos = (Vector2) player.transform.position;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorldPos - playerPos).normalized;

        // fallback: 방향이 없다면 바라보는 방향
        if(dir == Vector2.zero)
            dir = new Vector2(Mathf.Sign(player.transform.localScale.x), 0.5f);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f;

        // 8방향
        Vector2[] directions = {
            new Vector2(1, 0.5f),    // 0: →
            new Vector2(1, 1),    // 1: ↗
            new Vector2(0, 1),    // 2: ↑
            new Vector2(-1, 1),   // 3: ↖
            new Vector2(-1, 0.5f),   // 4: ←
            new Vector2(-1, -1),  // 5: ↙
            new Vector2(0, -1),   // 6: ↓
            new Vector2(1, -1)    // 7: ↘
        };

        int index = Mathf.RoundToInt(angle / 45f) % 8;
        Vector2 selectedDir = directions[index];

        float forceX = throwForceX;
        float forceY = throwForceY;

        switch(index)
        {
            case 0: // →
                forceX *= 1.5f;
                break;

            case 1: // ↗
                forceX *= 1.5f;
                forceY *= 1.5f;
                break;

            case 2: // ↑
                break;

            case 3: // ↖
                forceX *= 1.5f;
                forceY *= 1.5f;
                break;

            case 4: // ←
                forceX *= 1.5f;
                break;

            case 5: // ↙
                forceX *= 1.5f;
                forceY = 0;
                break;

            case 6: // ↓
                return Vector2.zero;

            case 7: // ↘
                forceX *= 1.5f;
                forceY = 0;
                break;
        }

        print($"각도: {angle}, X힘: {forceX * selectedDir.x}, Y힘: {forceY * selectedDir.y}\n방향: {selectedDir}, 방향벡터: {dir}\n 플레이어 위치: {playerPos}, 마우스 위치: {mouseWorldPos}");
        return new Vector2(selectedDir.x * forceX, selectedDir.y * forceY);
    }

    public void Throw(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    void Explode()
    {
        hasExploded = true;
        // 애니메이션 재생
        anim.enabled = true;

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[19]);

        // 1. 폭발 크기 지정
        Vector2 explosionSize = new Vector2(1f * exploSize.x, 1f * exploSize.y);
        Vector2 explosionCenter = transform.position;
        Vector2 explosionHalfSize = explosionSize * 0.5f;

        DrawDebugBox(transform.position, explosionSize, Color.red);

        // 3. 충돌 감지 (전체 대상)
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, explosionSize, 0f);

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[19]);

        // 4. 범위 내 중심만 필터링
        foreach(var hit in hits)
        {
            if(hit == null || hit.gameObject == null)
                continue;

            Vector2 hitCenter = hit.bounds.center;

            bool isInsideExplosion =
                Mathf.Abs(hitCenter.x - explosionCenter.x) <= explosionHalfSize.x &&
                Mathf.Abs(hitCenter.y - explosionCenter.y) <= explosionHalfSize.y;

            if(!isInsideExplosion)
                continue;

            string tag = hit.tag;

            switch(tag)
            {
                case "Block":
                    TryDestroyBlock(hit);
                    break;

                case "Enemy":
                    TryDestroyEnemy(hit);
                    break;

                case "Player":
                    TryDamagePlayer(hit);
                    break;

                case "Lamp":
                    TryTriggerLamp(hit);
                    break;
                default:
                    break;
            }
        }
    }

    void TryDestroyBlock(Collider2D hit)
    {
        Block block = hit.GetComponent<Block>();
        if(block == null || block.blockType == -1 || block.blockType == -2 || block.blockType == 1)
            return;
        block.BlockDestroy(10, playerScript);
    }

    void TryDestroyEnemy(Collider2D hit)
    {
        Enemy enemy = hit.GetComponent<Enemy>();
        if(enemy != null)
        {
            SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[25]);
            Destroy(enemy.gameObject);
        }
    }

    void TryDamagePlayer(Collider2D hit)
    {
        if(isAlreadyDamaged) return;

        PlayerController player = hit.GetComponent<PlayerController>();
        if(player != null)
        {
            print("데미지 폭탄: " + damage);
            player.TakeDamage(damage, transform.position);
            isAlreadyDamaged = true;
        }
    }

    void TryTriggerLamp(Collider2D hit)
    {
        Lamp lamp = hit.GetComponent<Lamp>();
        if(lamp != null)
        {
            Tool.Instance.torchObj.Remove(lamp.gameObject);
            Destroy(lamp.gameObject);
        }
    }

    public void OnExplosionFinished()
    {
        print("폭탄 터짐");
        Destroy(gameObject);
    }


    void DrawDebugBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 halfSize = size * 0.5f;

        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!collision.collider.CompareTag("Block")) return;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal.normalized;

        // 현재 이동 방향
        Vector2 moveDir = rb.velocity.normalized;

        // 떨어지는 중일 때만 바닥 충돌 처리
        bool isFalling = rb.velocity.y <= 0f;

        if(!isGrounded )
        {
            if(isFalling && IsGroundContact(contact.normal))
            {
                isGrounded = true;
                rb.velocity = Vector2.zero;
                //print("바닥 충돌 - 정지");
            }
        }
        else if(IsWallContact(normal))
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            //print("벽면 충돌 - 수직 낙하 시작");
        }
        else
        {
            //print("대각선 등 기타 충돌 - 무시");
        }
    }

    bool IsGroundContact(Vector2 normal)
    {
        return Vector2.Dot(normal, Vector2.up) > 0.9f;
    }

    bool IsWallContact(Vector2 normal)
    {
        // 좌우 벽 충돌(왼쪽 또는 오른쪽)
        return Vector2.Dot(normal, Vector2.left) > 0.9f || Vector2.Dot(normal, Vector2.right) > 0.9f;
    }
}
