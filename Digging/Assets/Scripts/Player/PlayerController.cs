using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Rendering;
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Field
    // 플레이어 상태
    public enum PlayerState
    {
        NONE,   // 상태 없음
        BURIED  //모래에 갇힘
    }
    public PlayerState _state;

    float initPosY;

    // Component References
    private InputAction[] _contorl;

    Rigidbody2D rb;
    Collider2D col;
    [SerializeField] Player playerScript;
    SpriteRenderer sr;

    // Movement
    Vector2 input;
    Vector2 velocity;
    float depth = 0.0f; // 깊이

    // 데미지
    [SerializeField] float sandTimer;
    [SerializeField] float sandDamage;
    [SerializeField] int fallDamage;

    bool isFlying;
    private ItemTypes isActiveType;
    bool isGround = false;
    public bool isDie = false;

    [Header("Movement Settings")]
    float gravity = -9.81f;

    [SerializeField] float speed = 8f;
    [SerializeField] float extraAcceleration = 1.2f;

    float MAX_VELOCITY_X = 8;
    float MAX_VELOCITY_Y = 10;

    // 낙하 데미지
    float FALL_MAX_VELOCITY = -5.5f;
    float FALL_MIN_VELOCITY = -9f;

    // Damage System
    int maxHP;
    [SerializeField] int currentHP = 0;

    // Block Detection
    [SerializeField] private LayerMask targetLayer;

    // Animations
    Animator anim;
    [SerializeField] Animator jetpack;

    // AudioSource
    public AudioSource jetpackAudioSourse;
    private bool isplayingjetpack = false;

    public AudioSource footstepAudioSourse01;
    public AudioSource footstepAudioSourse02;
    private bool switchStepSound = false;

    // Particle System
    private GameObject jetpackEffect; // 이펙트 오브젝트

    public BlocksDictionary blocksDictionary;
    #endregion Field

    #region InputAction

    public void SetControl(params string[] names)
    {
        var playerInput = GetComponent<PlayerInput>();
        _contorl = new InputAction[names.Length];

        for(int i = 0; i < names.Length; i++)
        {
            _contorl[i] = playerInput.actions[names[i]];
        }
    }

    public void EnableControls()
    {
        foreach(var action in _contorl)
            action?.Enable();
    }

    public void DisableControls()
    {
        foreach(var action in _contorl)
            action?.Disable();
    }

    public void EnableControl(int index)
    {
        if(_contorl != null && index >= 0 && index < _contorl.Length)
        {
            _contorl[index]?.Enable();
        }
    }

    public void DisableControl(int index)
    {
        if(_contorl != null && index >= 0 && index < _contorl.Length)
        {
            _contorl[index]?.Disable();
        }
    }

    // Input System 이벤트
    public void OnMove(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        var controlName = context.control.name;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if(context.started)
        {
            switch(controlName)
            {
                case "leftButton":
                    isActiveType = ItemTypes.Pickaxe;
                    break;
                case "rightButton":
                    isActiveType = ItemTypes.Drill;
                    break;
                case "q":
                    UseItemByType(ItemTypes.Bomb);
                    break;
                case "e":
                    UseItemByType(ItemTypes.Lamp);
                    break;
                case "r":
                    UseItemByType(ItemTypes.Teleport);
                    break;
                default:
                    break;
            }
        }

        if(context.canceled)
        {
            if(Tool.Instance.currentWeapon != null)
            {
                Tool.Instance.currentWeapon.isDigging = false;
            }
            isActiveType = ItemTypes.Null;
        }
    }

    #endregion // InputAction

    #region UnityEvent
    private void Awake()
    {
        isDie = false;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        playerScript = GetComponent<Player>();

        SetControl("Move", "Action");
        initPosY = transform.position.y;    // 현재 초기 위치

        targetLayer = LayerMask.GetMask("Block", "Floor");

        maxHP = 100;
        currentHP = maxHP;

        anim = GetComponent<Animator>();

        jetpack = transform.Find("jetpack/Anim").GetComponent<Animator>();

        blocksDictionary = GameObject.Find("BlocksDictionary").GetComponent<BlocksDictionary>();
    }

    private void Start()
    {
        _state = PlayerState.NONE;
        jetpackEffect = GameObject.Find("Ef_Jetpack");
    }

    private void Update()
    {
        // 바닥 감지
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, targetLayer);
        //Debug.DrawRay(transform.position, Vector2.down * 0.5f, Color.red);

        if(hit.collider != null)
        {
            isGround = true;
            isFlying = false;
        }
        else
        {
            isGround = false;
            isFlying = true;
        }

        UpdateSandStatus(); // 모래갇힘 판별
        UpdateDepth();  // 깊이 Text 업데이트

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        UseWeaponByType(isActiveType, mouseWorldPos);

        anim.SetBool("IsGround", isGround);
        anim.SetFloat("MoveSpeed", rb.velocity.magnitude);
        anim.SetFloat("FlySpeed", rb.velocity.magnitude);
        anim.SetBool("IsFlying", isFlying);
        anim.SetBool("IsDigging", Tool.Instance.currentWeapon?.isDigging ?? false);
    }
    private void FixedUpdate()
    {
        HandleMovement(input);
    }

    #endregion // Unity Event

    #region Method
    // 타입에 따른 무기 사용
    private void UseWeaponByType(ItemTypes type, Vector3 mouseWorldPos)
    {
        if(isActiveType == ItemTypes.Null)
            return;

        SlotInfo slot = SlotManager.Instance.quitSlotUI.FindSlot(type);

        if(slot?.slot?.weapon != null)
        {
            Tool.Instance.UseWeapon(slot, mouseWorldPos, slot.slot.weapon, playerScript);
        }
    }

    // 타입에 따른 아이템 사용
    private void UseItemByType(ItemTypes type)
    {
        SlotInfo slot = SlotManager.Instance.quitSlotUI.FindSlot(type);

        if(slot.slot == null)
        {
            Tool.Instance.sprite.sprite = null;
            return;
        }

        Tool.Instance.UseItem(slot, isGround, slot.slot.item);
    }

    private void UpdateDepth()
    {
        if(LoadScene.Instance.stage_Level == 0)
        {
            if(transform.position.y == initPosY)
                depth = 0.0f;

            float distance = transform.position.y - initPosY;
            depth = distance;
        }
        else
        {
            depth = transform.position.y;

            if(depth >= 0.0f)
                depth = 0.0f;
        }

        UIController.Instance.SetText(1, Mathf.Abs(depth).ToString("F1") + "m");
    }

    // 모래에 갇혔음을 액션 바인드하기
    private void UpdateSandStatus()
    {
        GameObject target = FindSandBlock();

        if(_state == PlayerState.BURIED)
        {
            input = Vector2.zero;
            velocity = Vector2.zero;
            rb.velocity = velocity;
            rb.gravityScale = 0;
            col.enabled = false;

            // 플레이어 모래 위치로 이동
            transform.position = Vector3.Lerp(transform.position, target.transform.position, 0.01f);
            if(Vector3.Distance(transform.position, target.transform.position) <= 0.1f)
                transform.position = target.transform.position;

            anim.SetBool("SandTrap", true);

            sandTimer += Time.deltaTime;

            if(sandTimer >= 1f)
            {
                TakeDamage(1, Vector3.zero);
                sandTimer = 0;
            }
        }
        else
        {
            _state = PlayerState.NONE;
            sandTimer = 0;

            rb.gravityScale = 1;
            col.enabled = true;

            anim.SetBool("SandTrap", false);
        }
    }

    private GameObject FindSandBlock()
    {
        Vector2 playerPos = new Vector2(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f);

        // 체크할 위치
        Vector2[] directions = new Vector2[]
        {
            Vector2.zero,
            Vector2.up
        };

        foreach(var dir in directions)
        {
            Vector2 neighborPos = playerPos + dir;

            if(blocksDictionary.blockPosition.TryGetValue(neighborPos, out GameObject obj))
            {
                Block block = obj.GetComponent<Block>();

                if(block != null && block.blockType == 6)
                {
                    _state = PlayerState.BURIED;
                    return obj;
                }
            }
            else
            {
                _state = PlayerState.NONE;
            }
        }

        return null;
    }

    // 이동 처리
    private void HandleMovement(Vector2 direction)
    {
        velocity.x = direction.x * speed;

        if(direction.y > 0)
        {
            if(isGround) // 순간 속도
            {
                velocity.y = direction.y * speed * 0.1f;
            }

            // 떨어지는 중일 때 플레이어가 하강속도보다 낮은 속도이기 때문에 속도를 높이기 위해 변수를 곱함.
            velocity.y += direction.y * (velocity.y < 0 ? speed * extraAcceleration : speed) * Time.fixedDeltaTime;
        }
        else if(!isGround)
        {
            if(velocity.y > 0)  // 플레이어가 바로 떨어지기 위해
                velocity.y = 0;

            velocity.y += gravity * Time.fixedDeltaTime;
        }

        // 속도 제한
        velocity.x = Mathf.Clamp(velocity.x, -MAX_VELOCITY_X, MAX_VELOCITY_X);
        velocity.y = Mathf.Clamp(velocity.y, -MAX_VELOCITY_Y, MAX_VELOCITY_Y);

        rb.velocity = velocity;
        jetpack.gameObject.SetActive(isFlying);

        //print($"속도: {velocity} 입력: direction {direction}");

        if(direction.x != 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = direction.x > 0 ? 0.9f : -0.9f;
            transform.localScale = newScale;

            if(switchStepSound && !footstepAudioSourse02.isPlaying && isGround)
            {
                switchStepSound = !switchStepSound;
                footstepAudioSourse01.Play();


            }
            else if(!switchStepSound && !footstepAudioSourse01.isPlaying && isGround)
            {
                switchStepSound = !switchStepSound;
                footstepAudioSourse02.Play();
            }

        }
    }

    // 피해 처리
    public void TakeDamage(int damage, Vector3 attackerPos)
    {
        currentHP -= damage;
        playerScript.OnHealthCange.Invoke(currentHP);

        if(currentHP <= 0)
        {
            //UIManager.Instance.SetObjectActive("HPBar", false);
            playerScript.GameOver();
            return;
        }

        SoundManager.Instance.SFXPlay(SoundManager.Instance.SFXSounds[27]);
        StartCoroutine(DamageEffect(attackerPos));
    }

    // 피격 시 색깔 변화와 넉백
    private IEnumerator DamageEffect(Vector2 targetPos)
    {
        sr.color = Color.red;

        if(targetPos == Vector2.zero)
        {
            yield return new WaitForSeconds(0.5f);
            sr.color = Color.white;
            yield break;
        }

        // 방향 결정 (좌/우)
        int dir = (transform.position.x - targetPos.x) > 0 ? 1 : -1;

        Vector3 knockback = new Vector3(dir, 0.5f, 0f);

        float duration = 0.2f;
        Vector3 start = transform.position;
        Vector3 end = start + knockback;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            Vector2 nextPos = Vector2.Lerp(start, end, elapsed / duration);
            rb.MovePosition(nextPos);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);

        yield return new WaitForSeconds(0.5f);
        sr.color = Color.white;
    }

    // 사망 처리
    public void Die()
    {
        isDie = true;
        transform.position = new Vector3(15.5f, 0.5f, 0f);
        sr.color = Color.white;
        currentHP = maxHP;
        playerScript.OnHealthCange.Invoke(currentHP);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.contacts[0];

        if(collision.gameObject.CompareTag("Block"))
        {
            if(contact.normal.y >= 0.9f)
            {
                fallDamage = 0;

                if(velocity.y <= FALL_MAX_VELOCITY)  // 낙하데미지(speed가 8인 경우 블럭 기준)
                {
                    //print("속도1: " + velocity.y);

                    if(velocity.y <= FALL_MIN_VELOCITY)   // (5칸 이상)
                        fallDamage = 20;
                    else if(velocity.y <= -6f)          // (3 ~ 4칸)
                        fallDamage = 10;
                    else
                        fallDamage = 5;             // (2칸 절반 이상 3칸 이하)
                }

                velocity = Vector2.zero;

                if(LoadScene.Instance.stage_Level == 0)
                    fallDamage = 0;

                if(fallDamage > 0)
                    TakeDamage(fallDamage, Vector3.zero);

                return;
            }
        }

        if(collision.gameObject.CompareTag("Floor"))
        {
            velocity = Vector2.zero;
            return;
        }
    }

    #endregion Method
}