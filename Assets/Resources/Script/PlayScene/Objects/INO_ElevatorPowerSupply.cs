using System;
using UnityEditor;
using UnityEngine;

public class INO_ElevatorPowerSupply : InteractiveObject {

    public Sprite[] pwSupplySprite;
    private bool IsAble = false;
    private void Awake() {
        conditionText = "주변에 전기 존재";
    }
    public override bool IsActive() {
        return true;
    }
    public override bool IsAvailable() {
        return ExistAroundElectric();
    }

    public bool ExistAroundElectric() {
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int pos = tilePos + new Vector3Int(x, y, 0);
                if (TileMgr.Instance.ExistElectric(pos))
                    return true;
            }
        }
        return false;
    }

    public override void Activate()
    {
        base.Activate();
        INO_Elevator[] elevators = TileMgr.Instance.GetMatchedElevators(tilePos);
        if (!IsAble)
        {
            GetComponent<SpriteRenderer>().sprite = pwSupplySprite[0];
            IsAble = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = pwSupplySprite[1];
            IsAble = false;
        }
        for (int i = 0; i < elevators.Length; i++)
        {
            if (elevators[i] != null)
            {
                if (!elevators[i].IsAble)
                {
                    elevators[i].IsAble = true;
                    elevators[i].GetComponent<Animator>().SetBool("IsActive", true);
                }
                else
                {
                    elevators[i].IsAble = false;
                    elevators[i].GetComponent<Animator>().SetBool("IsActive", false);
                }
            }
        }
    }
}
