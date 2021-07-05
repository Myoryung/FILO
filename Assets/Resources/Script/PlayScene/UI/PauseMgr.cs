using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class PauseMgr {
    private enum State { None, Loading, Pause, Sure };

    private GameObject GoalPrefab;
    private const float GOAL_POS_INTERVAL_Y = -75;
    private const float LOADING_TIME = 0.3f;

    private Canvas pauseCanvas, sureCanvas;

    private Transform menuTF, paper1TF, paper2TF, radioTF;
    private Vector3 menuInitPos, paper1InitPos, paper2InitPos, radioInitPos;
    private Vector3 menuMoveAmount, paper1MoveAmount, paper2MoveAmount, radioMoveAmount;
    private float loadingAmount;

    private List<Vector3> menuPosList = new List<Vector3>();
    private GameObject menuHeader;
    private int menuHeaderIndex = 0;

    private Action onPauseAction, onResumeAction, onGameExitAction;
    private State state;

    public PauseMgr(GoalMgr goalMgr, Action onPauseAction, Action onResumeAction, Action onGameExitAction) {
        this.onPauseAction = onPauseAction;
        this.onResumeAction = onResumeAction;
        this.onGameExitAction = onGameExitAction;

        GoalPrefab = Resources.Load<GameObject>("Prefabs/UI/Pause_Goal");

        pauseCanvas = GameObject.Find("UICanvas").transform.Find("PauseCanvas").GetComponent<Canvas>();
        pauseCanvas.enabled = false;

        menuTF = pauseCanvas.transform.Find("Menu");
        paper1TF = pauseCanvas.transform.Find("Paper1");
        paper2TF = pauseCanvas.transform.Find("Paper2");
        radioTF = pauseCanvas.transform.Find("Radio");

        menuInitPos = menuTF.position;
        paper1InitPos = paper1TF.position;
        paper2InitPos = paper2TF.position;
        radioInitPos = radioTF.position;

        menuMoveAmount = new Vector3(638, 0);
        paper1MoveAmount = new Vector3(-100, 765);
        paper2MoveAmount = new Vector3(0, 768);
        radioMoveAmount = new Vector3(-271, 0);

        // Paper
        Vector3 goalPos = paper2TF.transform.position + new Vector3(-200, 180);
        List<Goal> goalList = goalMgr.GetMainGoals();
        goalList.AddRange(goalMgr.GetSubGoals());

        foreach (Goal goal in goalList) {
            GameObject goalObj = GameObject.Instantiate(GoalPrefab, goalPos, Quaternion.identity, paper2TF);
            goalObj.GetComponent<Text>().text = goal.GetExplanationText();
            goalObj.transform.Find("Status").GetComponent<Text>().text = goal.GetStatusText();

            goalPos.y += GOAL_POS_INTERVAL_Y;
        }

        // Menu
        menuHeader = menuTF.Find("Header").gameObject;
        Transform itemsTF = pauseCanvas.transform.Find("Menu").Find("Items");
        for (int i = 0; i < itemsTF.childCount; i++)
            menuPosList.Add(itemsTF.GetChild(i).position);

        // Sure
        sureCanvas = pauseCanvas.transform.Find("SureCanvas").GetComponent<Canvas>();
        sureCanvas.enabled = false;

        sureCanvas.transform.Find("Window").Find("Yes").GetComponent<Button>().onClick.AddListener(Sure_Yes);
        sureCanvas.transform.Find("Window").Find("No").GetComponent<Button>().onClick.AddListener(Sure_No);
    }

    public void Update() {
        switch (state) {
        case State.None:
            if (Input.GetKeyDown(KeyCode.Escape))
                Pause();
            break;

        case State.Loading:
            float currTime = Time.unscaledDeltaTime;
            float moveAmount = (loadingAmount + currTime <= LOADING_TIME) ? currTime : LOADING_TIME - loadingAmount;
            float moveRate = moveAmount / LOADING_TIME;

            menuTF.position += menuMoveAmount * moveRate;
            paper1TF.position += paper1MoveAmount * moveRate;
            paper2TF.position += paper2MoveAmount * moveRate;
            radioTF.position += radioMoveAmount * moveRate;

            loadingAmount += moveAmount;
            if (loadingAmount >= LOADING_TIME)
                state = State.Pause;

            if (Input.GetKeyDown(KeyCode.Escape))
                Resume();
            break;

        case State.Pause:
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                if (menuHeaderIndex > 0) {
                    menuHeaderIndex--;

                    Vector3 headerPos = menuHeader.transform.position;
                    headerPos.y = menuPosList[menuHeaderIndex].y;
                    menuHeader.transform.position = headerPos;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                if (menuHeaderIndex+1 < menuPosList.Count) {
                    menuHeaderIndex++;

                    Vector3 headerPos = menuHeader.transform.position;
                    headerPos.y = menuPosList[menuHeaderIndex].y;
                    menuHeader.transform.position = headerPos;
                }
            }

            if (Input.GetKeyDown(KeyCode.Return)) {
                switch (menuHeaderIndex) {
                case 0:
                    Resume();
                    break;
                case 1:
                    sureCanvas.enabled = true;
                    state = State.Sure;
                    break;
                case 2:
                    // TODO: 옵션창
                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                Resume();
            break;

        case State.Sure:
            if (Input.GetKeyDown(KeyCode.Return))
                Sure_Yes();
            if (Input.GetKeyDown(KeyCode.Escape))
                Sure_No();
            break;
        }
    }

    public void Pause() {
        pauseCanvas.enabled = true;
        state = State.Loading;

        menuTF.position = menuInitPos;
        paper1TF.position = paper1InitPos;
        paper2TF.position = paper2InitPos;
        radioTF.position = radioInitPos;
        loadingAmount = 0;

        Time.timeScale = 0;
        onPauseAction();
    }
    public void Resume() {
        pauseCanvas.enabled = false;
        state = State.None;

        Time.timeScale = 1;
        onResumeAction();
    }

    private void Sure_Yes() {
        pauseCanvas.enabled = false;
        sureCanvas.enabled = false;
        state = State.None;

        onGameExitAction();
    }
    private void Sure_No() {
        sureCanvas.enabled = false;
        state = State.Pause;
    }
}
