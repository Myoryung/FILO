using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine;
using System;

public class INO_Elevator : InteractiveObject {
    [SerializeField]
    private bool isUp;
    public bool IsAble = false;

    public override bool IsAvailable() {
        return IsAble;
    }

    public override void Activate() {
        if (!IsAvailable()) return ;

        GameMgr.Instance.ChangeFloorPlayer(isUp);
	}
}
