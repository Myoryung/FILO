using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private float HP; // 체력
    private void Start()
    {
        transform.Translate(0, 0, -1);
    }
}
