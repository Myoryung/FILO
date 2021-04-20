using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine;
using System;

public class INO_Elevator : InteractiveObject {
    [SerializeField]
    private bool isUp;
    
    public override bool IsAvailable() {
        INO_ElevatorPowerSupply powerSupply = TileMgr.Instance.GetMatchedPowerSupply(tilePos);
        return powerSupply.ExistAroundElectric();
    }

    public override void Activate() {
        if (!IsAvailable()) return ;

        GameMgr.Instance.ChangeFloorPlayer(isUp);
	}
}
