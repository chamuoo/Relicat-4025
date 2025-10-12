using System;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class Drill : MonoBehaviour, IWeapon
{
    [SerializeField] private AudioSource diggingAudioSource;

    private bool _isDigging = false;
    public bool isDigging { get => _isDigging; set => _isDigging = value; }
    private bool isDigSound = false;

    // 에너지 카운트
    public float decreaseEnergy = 10f;  // 에너지 감소률 - 1초마다 0.1감소
    public float cooldown = 1f;         // 쿨타임
    [SerializeField] float timer;       // 시간          

    // 애니메이션 카운트
    private float t;
    private float angle;

    private Vector3 pivot;
    Vector2 tileSize;

    [SerializeField] WeaponInstance _instance;
    public WeaponInstance Instance => _instance;

    public Action<Drill, float> OnEnergyChanged; // 이벤트 콜백 - 에너지 충전 - 배터리
    public Action<Drill, float> OnDecreaseEnergy; // 에너지 감소

    public void Use(Vector2 mousePos, Player player, PlayerState state)
    {
        Digging(mousePos, player, state);
    }

    public void SetInstance(WeaponInstance instance)
    {
        _instance = instance;
        Tool.Instance.SetData(instance._id, _instance);
    }

    private void Awake()
    {
        diggingAudioSource = GameObject.FindWithTag("Player")?.GetComponent<AudioSource>();
    }

    private void Start()
    {
        timer = cooldown;
        tileSize = new(1, 1);

        if(_instance._energy >= 100)
            _instance._energy = 100;
    }

    private void Digging(Vector2 worldMousePos, Player player, PlayerState state)
    {
        if(_instance._energy <= 0)
        {
            t = cooldown;
            return;
        }

        // 1. 블록 가져오기 및 사운드 재생 블럭
        List<GameObject> blocks = GetBlocksToDig(worldMousePos, player, state);
        HashSet<int> playedBlockTypes = new();

        // 2. 블록이 없으면 digging 종료
        if(blocks == null || blocks.Count == 0)
        {
            _isDigging = false;
            t = 0;
            return;
        }

        // 3. 블록이 존재하면 digging 시작
        _isDigging = true;

        // 4. 블록 파괴 및 사운드
        foreach(GameObject blockObj in blocks)
        {
            if(blockObj.TryGetComponent(out Block block))
            {
                block.BlockDestroy(_instance._damage * Time.deltaTime, player);

                if(!playedBlockTypes.Contains(block.blockType))
                {
                    PlayDigSound(block.blockType);  // 다른 타입이면 재생
                    playedBlockTypes.Add(block.blockType); // 타입 기록
                }
            }
        }

        // 타이머
        timer -= Time.deltaTime;
        if(timer < 0f)
        {
            timer = cooldown;   // 타이머 리셋
            DecreaseEnergy(decreaseEnergy);
        }

    }

    private void Update()
    {
        AnimateDrill();
    }

    private void PlayDigSound(int blockType)
    {
        //// Dig 사운드
        if(blockType == 0 || blockType == 4 || blockType == 5)
        {
            int idx = UnityEngine.Random.Range(5, 9);
            if(_isDigging && isDigSound == false)
            {
                diggingAudioSource.PlayOneShot(SoundManager.Instance.SFXSounds[idx]);
                isDigSound = true;
            }
            if(!_isDigging && diggingAudioSource.isPlaying == true)
            {
                diggingAudioSource.Stop();
                isDigSound = false;
            }
            if(diggingAudioSource.isPlaying == false)
            {
                isDigSound = false;
            }
        }
        // 광물 블록
        if(blockType == 2 || blockType == 7 || blockType == 8 || blockType == 9 || blockType == 10 || blockType == 11)
        {
            if(_isDigging && isDigSound == false)
            {
                diggingAudioSource.PlayOneShot(SoundManager.Instance.SFXSounds[12]);
                isDigSound = true;
            }
            if(!_isDigging && diggingAudioSource.isPlaying == true)
            {
                diggingAudioSource.Stop();
                isDigSound = false;
            }
            if(diggingAudioSource.isPlaying == false)
            {
                isDigSound = false;
            }
        }
        // 바위 블록
        if(blockType == 3 || blockType == -1)
        {
            int idx = UnityEngine.Random.Range(9, 12);
            if(_isDigging && isDigSound == false)
            {
                diggingAudioSource.PlayOneShot(SoundManager.Instance.SFXSounds[idx]);
                isDigSound = true;
            }
            if(!_isDigging && diggingAudioSource.isPlaying == true)
            {
                diggingAudioSource.Stop();
                isDigSound = false;
            }
            if(diggingAudioSource.isPlaying == false)
            {
                isDigSound = false;
            }
        }
        // 모래 블록
        if(blockType == 6)
        {

            if(_isDigging && isDigSound == false)
            {
                diggingAudioSource.PlayOneShot(SoundManager.Instance.SFXSounds[8]);
                isDigSound = true;
            }
            if(!_isDigging && diggingAudioSource.isPlaying == true)
            {
                diggingAudioSource.Stop();
                isDigSound = false;
            }
            if(diggingAudioSource.isPlaying == false)
            {
                isDigSound = false;
            }
        }
    }

    private void AnimateDrill()
    {
        pivot = transform.parent.Find("Pivot").position;

        if(isDigging)
        {
            t += Time.deltaTime * (_instance._damage / 2);
            t = Mathf.Clamp01(t);

            angle = Mathf.Lerp(60, -30, t);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * 0.1f;
            transform.position = pivot + offset;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if(t >= 1)
                t = 0;
        }
        else
        {
            t = 0;
            transform.position = pivot;
            transform.rotation = Quaternion.Euler(0f, 0f, -30f);
        }
    }

    // 블럭 가져오기
    private List<GameObject> GetBlocksToDig(Vector2 worldMousePos, Player player, PlayerState state)
    {
        // 블럭 가져오기
        List<GameObject> blocks = new List<GameObject>();

        if(state == PlayerState.BURIED)
        {
            // 모래에 갇혔으면 자신의 위치에 있는 블럭만 반환
            GameObject sandBlock = GetCurrentPlayerBlock(player.GetComponent<PlayerController>());

            if(sandBlock != null && !blocks.Contains(sandBlock))
                blocks.Add(sandBlock);
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero, 0f, LayerMask.GetMask("Block"));
            if(hit.collider == null)
                return null;

            var blocksDict = hit.collider.GetComponent<Block>().blocksDictionary;
            blocks = ToGrid(hit, blocksDict);
        }

        return blocks;
    }

    List<GameObject> ToGrid(RaycastHit2D hit, BlocksDictionary blocksDict)
    {
        List<GameObject> blocks = new();
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector2 playerPos = player.transform.position;
        Vector2 PgridPos = new Vector2   // 플레이어 격자 위치
        (
            Mathf.Floor(playerPos.x / tileSize.x) + 0.5f,
            Mathf.Floor(playerPos.y / tileSize.y) + 0.5f
        );
        Vector2 blockPos = hit.collider.transform.position;

        Vector2 direction = blockPos - PgridPos;
        float distance = Vector2.Distance(PgridPos, blockPos);
        //Debug.Log($"Distance between player and block: {distance:F3}");

        // 1. 클릭된 블럭이 플레이어로부터 1칸 또는 대각선 거리 내에 있을 때만 채굴 가능
        if(!(Mathf.Abs(distance - 1f) <= 0.95f || Mathf.Abs(distance) <= 1.5f))
            return blocks;

        bool horizontal = Mathf.Abs(direction.x) >= Mathf.Abs(direction.y);
        int steps = horizontal ? (int)_instance._range.x : (int)_instance._range.y;
        Vector2 stepVector = horizontal ? new Vector2(Mathf.Sign(direction.x), 0)
                                        : new Vector2(0, Mathf.Sign(direction.y));

        // 주축 방향 블록 추가
        for(int i = 0; i < steps; i++)
        {
            Vector2 currentPos = blockPos + stepVector * i;
            if(Vector2.Distance(PgridPos, currentPos) > steps)
                break;

            if(blocksDict.blockPosition.TryGetValue(currentPos, out GameObject obj) && !blocks.Contains(obj))
                blocks.Add(obj);
        }

        TryAddCrossBlocks(blockPos, blocksDict, blocks);

        // 대각선 여부 판단
        bool isDiagonal = Mathf.Abs(direction.x) > 0 && Mathf.Abs(direction.y) > 0
            && Mathf.Abs(distance) > 1.3f && Mathf.Abs(distance) < 1.5f;

        // 대각선 블록 처리
        if(isDiagonal && IsDiagonalAllowed(PgridPos, blockPos, blocksDict))
        {
            if(blocksDict.blockPosition.TryGetValue(blockPos, out GameObject diagBlock) && !blocks.Contains(diagBlock))
                blocks.Add(diagBlock);

            TryAddCrossBlocks(blockPos, blocksDict, blocks);
        }

        return blocks;
    }

    // + 블럭 찾기
    private void TryAddCrossBlocks(Vector2 origin, BlocksDictionary dict, List<GameObject> list)
    {
        Vector2[] offsets = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

        foreach(var offset in offsets)
        {
            Vector2 pos = origin + offset;
            if(dict.blockPosition.TryGetValue(pos, out GameObject block) && !list.Contains(block))
                list.Add(block);
        }
    }

    // 대각선 여부
    bool IsDiagonalAllowed(Vector2 playerPos, Vector2 blockPos, BlocksDictionary blocksDict)
    {
        Vector2Int diff = new Vector2Int(
            (int)Mathf.Sign(blockPos.x - playerPos.x),
            (int)Mathf.Sign(blockPos.y - playerPos.y)
        );

        //float distance = Vector2.Distance(playerPos, blockPos);
        //Debug.Log($"[IsDiagonalAllowed] diff: ({diff.x}, {diff.y}), distance: {distance:F2}, blockPos: {blockPos}, playerPos: {playerPos}");

        if(diff.x == 0 || diff.y == 0)
            return false;

        bool left = blocksDict.blockPosition.ContainsKey(blockPos + Vector2.left);
        bool right = blocksDict.blockPosition.ContainsKey(blockPos + Vector2.right);
        bool up = blocksDict.blockPosition.ContainsKey(blockPos + Vector2.up);
        bool down = blocksDict.blockPosition.ContainsKey(blockPos + Vector2.down);

        if(diff.x < 0 && diff.y > 0) return !right || !down;    // ↖
        if(diff.x > 0 && diff.y > 0) return !left || !down;   // ↗
        if(diff.x < 0 && diff.y < 0) return !right || !up;  // ↙
        if(diff.x > 0 && diff.y < 0) return !left || !up; // ↘

        return false;
    }

    // 현재 방향
    private GameObject GetCurrentPlayerBlock(PlayerController player)
    {
        Vector2 playerCenter = new Vector2(
            Mathf.Floor(player.transform.position.x) + 0.5f,
            Mathf.Floor(player.transform.position.y) + 0.5f
        );

        if(player.blocksDictionary.blockPosition.TryGetValue(playerCenter, out GameObject blockObj))
        {
            return blockObj;
        }

        return null;
    }


    // 에너지 감소
    public void DecreaseEnergy(float amount)
    {
        _instance._energy -= amount;
        SlotManager.Instance.energyClone.GetComponent<EnergyBar>().SetValue(_instance._energy);
    }
}
