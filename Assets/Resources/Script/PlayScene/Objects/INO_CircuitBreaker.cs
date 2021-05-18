using System;
using System.Collections.Generic;
using UnityEngine;

public class INO_CircuitBreaker : InteractiveObject {

    public Sprite[] cbSprite;
    private bool IsAble = false;
    private void Awake() {
        conditionText = "주변에 대원 1명 존재";
    }

    public override bool IsActive() {
        return true;
    }
    public override bool IsAvailable() {
		return true;
	}

	public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        INO_Socket[] sockets = TileMgr.Instance.GetMatchedSockets(tilePos, floor); if (!IsAble)
        {
            GetComponent<SpriteRenderer>().sprite = cbSprite[0];
            IsAble = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = cbSprite[1];
            IsAble = false;
        }
        for (int i=0; i<sockets.Length; i++)
        {
            if (sockets != null)
            {
                sockets[i].Activate();
            }
        }
    }
}
