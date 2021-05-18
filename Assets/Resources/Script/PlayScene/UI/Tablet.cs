using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tablet {
	private Image tabletCodeTop, tabletCodeBottom;
	private const float TABLET_CODE_MOVE_SPEED = 0.075f;
	private Vector3 tabletCodeInitPos;

	private Image tabletRecord;
	private const float TABLET_RECORD_TURN_PERIOD = 1.0f;
	private float tabletRecordTurnTime = 0.0f;

    private readonly GameObject floorPrefab = Resources.Load<GameObject>("Prefabs/UI/TabletFloor");
    private List<TabletFloor> floors = new List<TabletFloor>();
    private List<OperatorSpawn[]> operatorSpawnsList = new List<OperatorSpawn[]>();
    private int currFloor;

    private List<GameObject> camsWhite = new List<GameObject>();
    private List<GameObject> camsRed = new List<GameObject>();
    private List<int> currCams = new List<int>();

    private readonly Vector3 tabletMiddlePos;

    private readonly Dictionary<int, KeyValuePair<int, int>> operatorCamPairs = new Dictionary<int, KeyValuePair<int, int>>();

    public Tablet() {
        GameObject tablet = GameObject.Find("UICanvas/SelectCanvas/Tablet");
        Transform tabletUI = tablet.transform.Find("UI");

        tabletMiddlePos = tablet.transform.localPosition + tabletUI.localPosition;

		// Code
		tabletCodeTop = tabletUI.Find("CodeTop").GetComponent<Image>();
		tabletCodeBottom = tabletUI.Find("CodeBottom").GetComponent<Image>();
		tabletCodeInitPos = tabletCodeBottom.rectTransform.localPosition;

        // Record
		tabletRecord = tabletUI.Find("Record").GetComponent<Image>();

        // Cam
        Transform camWhite = tabletUI.Find("Cam/White");
        Transform camRed = tabletUI.Find("Cam/Red");

        int camNum = camWhite.childCount;
        for (int i = 0; i < camNum; i++) {
            camsWhite.Add(camWhite.GetChild(i).gameObject);
            camsRed.Add(camRed.GetChild(i).gameObject);
            currCams.Add(0);
        }

        // Floor
        RectTransform tabletFloorsTF = tabletUI.Find("Floors").GetComponent<RectTransform>();
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

        // Cam Operator Pair
        KeyValuePair<int, int> nonePair = new KeyValuePair<int, int>(-1, -1);
        for (int i = 0; i < 4; i++)
            operatorCamPairs.Add(i, nonePair);
    }

	public void Update() {
        // 레코드 점멸
        float currTime = Time.time;
        if (currTime - tabletRecordTurnTime >= TABLET_RECORD_TURN_PERIOD) {
            tabletRecord.enabled = !tabletRecord.enabled;
            tabletRecordTurnTime = currTime;
        }

        // 코드 이동
        float speed = TABLET_CODE_MOVE_SPEED * Time.deltaTime;
        float codeHeight = tabletCodeBottom.rectTransform.rect.height;
        float moveAmount = codeHeight * speed;
        tabletCodeBottom.fillAmount -= speed;
        tabletCodeTop.fillAmount += speed;
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

        int prevFloor = currFloor;
        int prevFloorIdx = prevFloor - TileMgr.Instance.MinFloor;
        int prevFloorCam = currCams[prevFloorIdx];

        camsRed[prevFloorCam].SetActive(false);
        floors[prevFloorIdx].SetNormal();

        SetFloor(floor);
    }
    private void SetFloor(int floor) {
        currFloor = floor;
        int currFloorIdx = currFloor - TileMgr.Instance.MinFloor;

        floors[currFloorIdx].SetHighlight();

        int camNum = camsWhite.Count;
        int activeCamNum = operatorSpawnsList[currFloorIdx].Length;
        int currFloorCam = currCams[currFloorIdx];

        for (int i = 0; i < camNum; i++)
            camsWhite[i].SetActive(i < activeCamNum);
        SetCam(currFloorCam);
    }

    public void ChangeCam(int number) {
        int floorIndex = currFloor - TileMgr.Instance.MinFloor;
        int prevCam = currCams[floorIndex];

        camsWhite[prevCam].SetActive(true);
        camsRed[prevCam].SetActive(false);

        SetCam(number);
	}
    private void SetCam(int number) {
        int currFloorIndex = currFloor - TileMgr.Instance.MinFloor;
        int currCam = currCams[currFloorIndex] = number;

        camsWhite[currCam].SetActive(false);
        camsRed[currCam].SetActive(true);

        Vector3 targetPos = GetCurrCamPos() - tabletMiddlePos;
        Camera.main.GetComponent<FollowCam>().SetPosition(targetPos);
	}

    public Vector3 GetCurrCamPos() {
        int currFloorIndex = currFloor - TileMgr.Instance.MinFloor;
        int currCam = currCams[currFloorIndex];

        OperatorSpawn currSpawn = operatorSpawnsList[currFloorIndex][currCam];
        return currSpawn.transform.position;
    }
    public int GetCurrFloor() {
        return currFloor;
    }
    public KeyValuePair<int, int> GetCurrCam() {
        int currFloorIndex = currFloor - TileMgr.Instance.MinFloor;
        int currCam = currCams[currFloorIndex];

        return new KeyValuePair<int, int>(currFloor, currCam);
	}
    public KeyValuePair<int, int> GetCamPlacedOperator(int operatorNumber) {
        return operatorCamPairs[operatorNumber];
    }
    public int GetOperatorAtCam(KeyValuePair<int, int> cam) {
        for (int i = 0; i < operatorCamPairs.Count; i++) {
            if (operatorCamPairs[i].Equals(cam))
                return i;
		}
        return -1;
    }
    public void SetOperatorPair(int operatorNumber, KeyValuePair<int, int> cam) {
        operatorCamPairs[operatorNumber] = cam;
    }
    public void ClearOperatorPair(int operatorNumber) {
        operatorCamPairs[operatorNumber] = new KeyValuePair<int, int>(-1, -1);
    }
}
