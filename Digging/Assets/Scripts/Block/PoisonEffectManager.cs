using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEffectManager : MonoBehaviour
{
    public List<GameObject> poisonEffectPool = new List<GameObject>();
    [SerializeField] Vector2 leftTopPos;
    [SerializeField] Vector2 rightBottomPos;
    void Start()
    {
        foreach(var obj in poisonEffectPool)
        {
            PoisonEffect objScript = obj.GetComponent<PoisonEffect>();
            if(objScript != null)
            {
                objScript.Initialize(leftTopPos, rightBottomPos, 3, 3, 5f, false, 1);
            }
        }
    }

    void Update()
    {
        
    }
}
