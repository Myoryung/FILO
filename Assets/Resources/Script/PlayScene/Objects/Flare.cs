using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flare : MonoBehaviour {
    private int remainTurn = 4;

    public void TurnUpdate() {
        if (--remainTurn == 0)
            Destroy(gameObject);
    }
}
