using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField] GameObject[] chunks;
    [SerializeField] GameObject player;
    [SerializeField] int resetBoxCount = 1;

    float activeDistance = 16.0f;

    private void Awake()
    {
        //리셋 박스 지정 시작
        List<int> numbers = new List<int>();
        for (int i = 0; i < chunks.Length; i++)
        {
            numbers.Add(i);
        }
        for (int i = 0; i < numbers.Count; i++)
        {
            int randIdx = Random.Range(i, numbers.Count);
            (numbers[i], numbers[randIdx]) = (numbers[randIdx], numbers[i]);
        }

        for (int i = 0; i < resetBoxCount; i++)
        {
            Chunk chunkScript = chunks[numbers[i]].GetComponent<Chunk>();
            if (chunkScript != null)
            {
                chunkScript.isResetBox = true;
            }
        }
        //리셋 박스 지정 끝
    }

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }
    void Update()
    {
        if(player != null)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                float distanceToPlayer = Vector2.Distance(new Vector2(chunks[i].transform.position.x + 5f, chunks[i].transform.position.y - 5f), player.transform.position);

                //print($"번호 {i}, 거리{distanceToPlayer}");

                if (chunks[i].activeSelf && distanceToPlayer > activeDistance)
                {
                    chunks[i].SetActive(false);
                }
                else if (!chunks[i].activeSelf && distanceToPlayer <= activeDistance)
                {
                   chunks[i].SetActive(true);
                }

            }
        }
        else
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

    }
}
