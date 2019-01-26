using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class DestroyTargetTime : MonoBehaviour
{
    public float destroyTime = 1;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
    void Update()
    {
    }
}
