using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class INO_Drone : InteractiveObject {
    private const float FLY_SPEED = 1000, FLY_DISTANCE = 1000;
    private const float WING_ROTATE_SPEED = 1000;

    private GameObject wingsObj, wingsMovingObj;
    private GameObject[] movingWingObjs = new GameObject[4];
    private float totalMoveAmount;

    private GameObject floorViewObjectPrefab;

    private enum State {
        IDLE, FLY, END
    };
    private State state = State.IDLE;

    private void Awake() {
        conditionText = "주변에 구조맨 존재";
    }

    protected override void Start() {
        base.Start();

        floorViewObjectPrefab = Resources.Load<GameObject>("Prefabs/Tiles/FloorView");

        // 날개 오브젝트 Load
        wingsObj = transform.Find("Wings").gameObject;
        wingsMovingObj = transform.Find("Wings_Moving").gameObject;

        movingWingObjs[0] = wingsMovingObj.transform.Find("LT").gameObject;
        movingWingObjs[1] = wingsMovingObj.transform.Find("RT").gameObject;
        movingWingObjs[2] = wingsMovingObj.transform.Find("LB").gameObject;
        movingWingObjs[3] = wingsMovingObj.transform.Find("RB").gameObject;

        wingsObj.SetActive(true);
        wingsMovingObj.SetActive(false);
        foreach (GameObject movingWingObj in movingWingObjs)
            movingWingObj.SetActive(true);

    }

    private void Update() {
        if (state == State.FLY) {
            float moveAmount = FLY_SPEED * Time.deltaTime;
            totalMoveAmount += moveAmount;

            transform.position += Vector3.up * moveAmount;
            foreach (GameObject wingObj in movingWingObjs)
                wingObj.transform.Rotate(0, 0, WING_ROTATE_SPEED * Time.deltaTime);

            if (totalMoveAmount >= FLY_DISTANCE) {
                state = State.END;

                GetComponent<SpriteRenderer>().enabled = false;
                wingsMovingObj.SetActive(false);

                INO_DroneFloorView floorView = Instantiate(floorViewObjectPrefab).GetComponent<INO_DroneFloorView>();
                floorView.SetFloor(tilePos.z);

                TileMgr.Instance.RemoveDrone(tilePos);
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

        state = State.FLY;
        totalMoveAmount = 0;

        wingsObj.SetActive(false);
        wingsMovingObj.SetActive(true);
    }
}
