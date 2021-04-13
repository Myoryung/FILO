using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tablet {
	private Image tabletCodeTop, tabletCodeBottom;
	private const float TABLET_CODE_MOVE_SPEED = 0.005f;
	private Vector3 tabletCodeInitPos;

	private Image tabletRecord;
	private const float TABLET_RECORD_TURN_PERIOD = 1.0f;
	private float tabletRecordTurnTime = 0.0f;

    private readonly GameObject floorPrefab = Resources.Load<GameObject>("Prefabs/UI/TabletFloor");
    private List<TabletFloor> floors = new List<TabletFloor>();
    private List<OperatorSpawn[]> operatorSpawnsList = new List<OperatorSpawn[]>();
    private int currFloor;

	public Tablet() {
		GameObject tabletUI = GameObject.Find("UICanvas/SelectCanvas/Tablet/UI");

        // Code
		tabletCodeTop = tabletUI.transform.Find("CodeTop").GetComponent<Image>();
		tabletCodeBottom = tabletUI.transform.Find("CodeBottom").GetComponent<Image>();
		tabletCodeInitPos = tabletCodeBottom.rectTransform.localPosition;

        // Record
		tabletRecord = tabletUI.transform.Find("Record").GetComponent<Image>();

        // Floor
        RectTransform tabletFloorsTF = tabletUI.transform.Find("Floors").GetComponent<RectTransform>();
        float floorHeight = floorPrefab.GetComponent<RectTransform>().rect.height;
        float y = -tabletFloorsTF.rect.height/2 + 15;

        for (int i = TileMgr.Instance.MinFloor; i <= TileMgr.Instance.MaxFloor; i++) {
            GameObject floorObj = GameObject.Instantiate(floorPrefab, tabletFloorsTF);
            floorObj.transform.localPosition = new Vector3(-2.5f, y);
            y += floorHeight + 10;

            TabletFloor floor = floorObj.GetComponent<TabletFloor>();
            OperatorSpawn[] operatorSpawns = TileMgr.Instance.GetOperatorSpawns(i);

            floor.SetFloorNumber(i);
            if (operatorSpawns.Length == 0)
                floor.SetDisable();

            floors.Add(floor);
            operatorSpawnsList.Add(operatorSpawns);
        }

        SetFloor(TileMgr.Instance.MinFloor);
	}

	public void Update() {
        // 레코드 점멸
        float currTime = Time.time;
        if (currTime - tabletRecordTurnTime >= TABLET_RECORD_TURN_PERIOD) {
            tabletRecord.enabled = !tabletRecord.enabled;
            tabletRecordTurnTime = currTime;
        }

        // 코드 이동
        float codeHeight = tabletCodeBottom.rectTransform.rect.height;
        float moveAmount = codeHeight * TABLET_CODE_MOVE_SPEED;
        tabletCodeBottom.fillAmount -= TABLET_CODE_MOVE_SPEED;
        tabletCodeTop.fillAmount += TABLET_CODE_MOVE_SPEED;
        tabletCodeBottom.rectTransform.localPosition += new Vector3(0, moveAmount);
        tabletCodeTop.rectTransform.localPosition += new Vector3(0, moveAmount);

        if (tabletCodeBottom.fillAmount <= 0) {
            tabletCodeBottom.rectTransform.localPosition = tabletCodeInitPos;
            tabletCodeTop.rectTransform.localPosition = tabletCodeInitPos;
            tabletCodeBottom.fillAmount = 1;
            tabletCodeTop.fillAmount = 0;
        }
    }

    public void ChangeFloor(int floor) {
        if (currFloor == floor) return;

        int currFloorIndex = currFloor - TileMgr.Instance.MinFloor;
        floors[currFloorIndex].SetNormal();

        SetFloor(floor);
    }
    private void SetFloor(int floor) {
        int floorIndex = floor - TileMgr.Instance.MinFloor;

        floors[floorIndex].SetHighlight();

        // TODO: CAM 수정

        currFloor = floor;
    }
}
