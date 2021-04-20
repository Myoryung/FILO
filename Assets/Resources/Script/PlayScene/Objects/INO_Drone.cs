using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class INO_Drone : InteractiveObject {

    private GameObject viewObject;
    private const int ACTIVE_TURN = 3;
    private int activeTurnCount = 0;

    private void Awake() {
        conditionText = "주변에 구조맨 존재";
    }

    protected override void Start() {
        base.Start();

        Transform view = transform.Find("View");
        viewObject = view.gameObject;
        viewObject.SetActive(false);
        
        Vector3 middlePos = TileMgr.Instance.CellToWorld(new Vector3Int(0, 0, tilePos.z));
        view.transform.position = middlePos;

        Vector2 floorSize = TileMgr.Instance.GetFloorSize(tilePos.z);
        view.Find("MainView").GetComponent<SpriteRenderer>().size = floorSize;
        view.Find("SubView").GetComponent<SpriteRenderer>().size = floorSize;
    }

    public void TurnUpdate() {
        if (activeTurnCount > 0) {
            if (--activeTurnCount == 0) {
                viewObject.SetActive(false);
            }
        }
    }

    public override bool IsAvailable() {
        if (!base.IsAvailable()) return false;

        List<Player> players = GameMgr.Instance.GetAroundPlayers(tilePos, 2);
        foreach (Player player in players) {
            if (player.OperatorNumber == Rescuer.OPERATOR_NUMBER)
                return true;
        }
        return false;
    }

    public override void Activate() {
        if (!IsAvailable()) return;
        base.Activate();

        viewObject.SetActive(true);
        activeTurnCount = ACTIVE_TURN;
    }
}
