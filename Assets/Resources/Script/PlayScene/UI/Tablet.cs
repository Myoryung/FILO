﻿using System.Collections;
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

    private List<GameObject> camsWhite = new List<GameObject>();
    private List<GameObject> camsRed = new List<GameObject>();
    private List<int> currCams = new List<int>();

    public Tablet() {
		GameObject tabletUI = GameObject.Find("UICanvas/SelectCanvas/Tablet/UI");

        // Code
		tabletCodeTop = tabletUI.transform.Find("CodeTop").GetComponent<Image>();
		tabletCodeBottom = tabletUI.transform.Find("CodeBottom").GetComponent<Image>();
		tabletCodeInitPos = tabletCodeBottom.rectTransform.localPosition;

        // Record
		tabletRecord = tabletUI.transform.Find("Record").GetComponent<Image>();

        // Cam
        Transform camWhite = tabletUI.transform.Find("Cam/White");
        Transform camRed = tabletUI.transform.Find("Cam/Red");

        int camNum = camWhite.childCount;
        for (int i = 0; i < camNum; i++) {
            camsWhite.Add(camWhite.GetChild(i).gameObject);
            camsRed.Add(camRed.GetChild(i).gameObject);
            currCams.Add(0);
        }

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
    }
}