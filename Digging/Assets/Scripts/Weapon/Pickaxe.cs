using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerController;

public class Pickaxe : MonoBehaviour, IWeapon
{
    [SerializeField] private AudioSource diggingAudioSource;

    private bool _isDigging = false;
    public bool isDigging { get => _isDigging; set => _isDigging = value; }
    private bool isDigSound = false;

    private float t;
    private float angle;

    private Vector3 pivot;

    [SerializeField] WeaponInstance _instance;
    Vector2 tileSize;

    public WeaponInstance Instance => _instance;

    public void Use(Vector2 mousePos, Player player, PlayerState state)
    {
        Digging(mousePos, player, state);
    }

    public void SetInstance(WeaponInstance instance)
    {
        _instance = instance;
    }

    private void Awake()
    {
        diggingAudioSource = GameObject.FindWithTag("Player")?.GetComponent<AudioSource>();
    }

    private void Start()
    {
        tileSize = new (1, 1);
    }

    private void Digging(Vector2 worldMousePos, Player player, PlayerState state)
    {
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

                //Debug.Log($"블록 타입: {block.blockType}, 사운드 재생 여부: {!playedBlockTypes.Contains(block.blockType)}");
            }
        }
    }

    private void Update()
    {
        AnimatePickaxe();
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
            if(hit.collider == null || blocksDict == null)
            {
                return null;
            }

            blocks = ToGrid(hit, blocksDict);
        }

        return blocks;
    }

    private void PlayDigSound(int blockType)
    {
        // Dig 사운드
        if(blockType == 0 || blockType == 4 || blockType == 5 || blockType == 101 || blockType == 102)
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
        if(blockType == 3 || blockType == -1 || blockType == 12 || blockType == 13 || blockType == 14 || blockType == 15 || blockType == 16 || blockType ==17)
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

    private void AnimatePickaxe()
    {
        pivot = transform.parent.Find("Pivot").position;

        if(isDigging)
        {
            // 곡괭이 속도
            t += Time.deltaTime * (_instance._damage / 4);
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

    // Grid격자 형식으로 정규화
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
        float distance = Vector2.Distance(PgridPos , blockPos);
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

        // 대각선 여부 판단
        bool isDiagonal = Mathf.Abs(direction.x) > 0 && Mathf.Abs(direction.y) > 0
            && Mathf.Abs(distance) > 1.3f && Mathf.Abs(distance) < 1.5f;

        // 대각선 블록 처리
        if(isDiagonal && IsDiagonalAllowed(PgridPos, blockPos, blocksDict))
        {
            if(blocksDict.blockPosition.TryGetValue(blockPos, out GameObject diagBlock) && !blocks.Contains(diagBlock))
                blocks.Add(diagBlock);
        }

        return blocks;
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
}

