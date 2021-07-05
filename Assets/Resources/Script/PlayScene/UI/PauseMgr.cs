using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class PauseMgr {
    private enum State { None, Loading, Pause, Sure };

    private GameObject GoalPrefab;
    private const float GOAL_POS_INTERVAL_Y = -75;

    private Canvas pauseCanvas, sureCanvas;
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

        // Paper
        Transform paperTF = pauseCanvas.transform.Find("Paper2");
        Vector3 goalPos = paperTF.transform.position + new Vector3(-200, 180);
        List<Goal> goalList = goalMgr.GetMainGoals();
        goalList.AddRange(goalMgr.GetSubGoals());

        foreach (Goal goal in goalList) {
            GameObject goalObj = GameObject.Instantiate(GoalPrefab, goalPos, Quaternion.identity, paperTF);
            goalObj.GetComponent<Text>().text = goal.GetExplanationText();
            goalObj.transform.Find("Status").GetComponent<Text>().text = goal.GetStatusText();

            goalPos.y += GOAL_POS_INTERVAL_Y;
        }

        // Menu
        menuHeader = pauseCanvas.transform.Find("Menu").Find("Header").gameObject;
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
            // TODO: Loading Animation
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
