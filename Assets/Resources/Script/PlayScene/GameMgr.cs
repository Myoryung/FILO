using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 용
using System.Xml;

public class GameMgr : MonoBehaviour {
    private static GameMgr _instance; // Singleton
    public static GameMgr Instance // 하이어라키에 존재합니다잉
    {
        get {
            if (_instance == null) {
                return null;
            }
            return _instance;
        }
    }

    private Text _mentalText, _stateText, _charNameText;
    private Text startBtnText, timerText = null;
    private Image _fadeImage = null;

    public enum LoadingState { Begin, Stay, End}
    private LoadingState _loadingState;
    public LoadingState CurrentLoadingState{
        get { return _loadingState; }
        set { _loadingState = value; }
    }
    public int GameTurn = 0; // 게임 턴
    private int currTime = 0;

    private List<Player> players = new List<Player>(); // 사용할 캐릭터들의 Components
    private int currPlayerIdx = 0;

    private Dictionary<Vector3Int, Survivor> survivors = new Dictionary<Vector3Int, Survivor>();


    private Canvas selectCanvas, playCanvas;

    private DisasterMgr disasterMgr;
    private GameObject disasterAlaram = null;
    private Text disasterAlaramText = null;

    private GameObject stageEnd = null;
    private Text stageEndText = null;

    private Tablet tablet;
    private Sprite operatorCardNormalSprite, operatorCardSelectedSprite;
    private GameObject[] operatorCards = new GameObject[4];
    private int operatorCardNum;

    private GameObject[] operatorPrefabs = new GameObject[4];
    private GameObject[] operators = new GameObject[4];

    private GoalMgr goalMgr;

    private int _stage = 0;
    public int stage {
        get { return _stage; }
    }

    public enum GameState {
        STAGE_SETUP, SELECT_OPERATOR, STAGE_READY,
        PLAYER_TURN, SURVIVOR_TURN, ENVIRONMENT_TURN, DISASTER_ALARM, DISASTER_TURN, TURN_END,
        STAGE_END
    }
    private GameState _currGameState = GameState.STAGE_SETUP;
    public GameState CurrGameState {
        get { return _currGameState; }
    }

    private bool bStagePlaying = false;
    private bool bSelectOperatorInit = false, bStageStartReady = false, bStageStartBtnClicked = false;
    private bool bTurnEndClicked = false, bStageEndClicked = false;
    private bool bSurvivorActive = false;
    private bool bDisasterAlarmPopup = false, bDisasterAlarmClicked = false;
    private DisasterObject disasterObject = null;
    private bool bDisasterExist = false;
    private bool isAllPlayerInSafetyArea = false;
    private bool bStageEndSetup = false;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update() {
        switch (CurrGameState) {
        case GameState.STAGE_SETUP: StageSetup(); break;
        case GameState.SELECT_OPERATOR: SelectOperator(); break;
        case GameState.STAGE_READY: StageReady(); break;
        case GameState.PLAYER_TURN: PlayerTurn(); break;
        case GameState.SURVIVOR_TURN: SurvivorTurn(); break;
        case GameState.ENVIRONMENT_TURN: EnvironmentTurn(); break;
        case GameState.DISASTER_ALARM: DisasterAlarm(); break;
        case GameState.DISASTER_TURN: DisasterTurn(); break;
        case GameState.TURN_END: TurnEnd(); break;
        case GameState.STAGE_END: StageEnd(); break;
        }

        if (bStagePlaying) {
            Player player = players[currPlayerIdx];
            ChangeMentalText(player);
            ChangeStateText(player);
            ChangeNameText();

            if (goalMgr.IsImpossible())
                _currGameState = GameState.STAGE_END;
        }
    }

    private void StageSetup() {
        // Create TileMgr Instance
        TileMgr.CreateInstance(stage);

        // Load XML
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Stage/Stage" + stage);
        doc.LoadXml(textAsset.text);
        XmlNode stageNode = doc.SelectSingleNode("Stage");

        string startTimeStr = stageNode.SelectSingleNode("StartTime").InnerText;
        string[] startTimeTokens = startTimeStr.Split(':');
        currTime = int.Parse(startTimeTokens[0])*60 + int.Parse(startTimeTokens[1]);

        // Stage Content
        string stageNamePreviewText = stageNode.SelectSingleNode("StageNamePreview").InnerText;
        string stageNameText = stageNode.SelectSingleNode("StageName").InnerText;
        string stageContentText = stageNode.SelectSingleNode("StageContent").InnerText;

        string[] stageContentLines = stageContentText.Split('\n');
        for (int i = 1; i < stageContentLines.Length; i++)
            stageContentLines[i] = stageContentLines[i].Trim();
        for (int i = 1; i <= 2 && i < stageContentLines.Length; i++)
            stageContentLines[i] = "          " + stageContentLines[i];

        stageContentText = "";
        for (int i = 1; i < stageContentLines.Length; i++)
            stageContentText += stageContentLines[i] + "\n";

        // Disaster & Goal
        XmlNode disastersNode = stageNode.SelectSingleNode("Disasters");
        XmlNode goalsNode = stageNode.SelectSingleNode("Goals");

        disasterMgr = new DisasterMgr(disastersNode);
        goalMgr = new GoalMgr(goalsNode);

        StartCoroutine(disasterMgr.UpdateWillActiveDisasterArea()); // 다음 턴 재난 지역 타일맵에 동기화

        // Load Canvas
        GameObject canvas = GameObject.Find("UICanvas");
        selectCanvas = canvas.transform.Find("SelectCanvas").GetComponent<Canvas>();
        playCanvas = canvas.transform.Find("PlayCanvas").GetComponent<Canvas>();

        selectCanvas.enabled = true;
        playCanvas.enabled = false;

        // Load Select UI
        Transform operatorCard = selectCanvas.transform.Find("OperatorSelect/OperatorCard");
        operatorCardNum = 0;
        for (int i = 0; i < operatorCard.childCount; i++) {
            GameObject go = operatorCard.GetChild(i).gameObject;
            if (go.activeSelf) {
                operatorCards[i] = go;
                operatorCardNum++;
            }
        }

        selectCanvas.transform.Find("StageGoal/StageNamePreview").GetComponentInChildren<Text>().text = stageNamePreviewText;
		Text stageName = selectCanvas.transform.Find("StageGoal/StageName").GetComponent<Text>();
        stageName.text = stageNameText;
        stageName.rectTransform.sizeDelta = new Vector2(stageName.preferredWidth, stageName.preferredHeight);

        RectTransform stageBar = selectCanvas.transform.Find("StageGoal/StageBar").GetComponent<RectTransform>();
        stageBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, stageName.rectTransform.rect.width + 20);

        GameObject goalPrefab = Resources.Load<GameObject>("Prefabs/UI/SelectUI_Goal");
        Transform goals = selectCanvas.transform.Find("StageGoal/Goals");
        List<Goal> mainGoals = goalMgr.GetMainGoals();

        for (int i = 0; i < mainGoals.Count; i++) {
            string explanation = mainGoals[i].GetExplanationText();
            string status = mainGoals[i].GetStatusText();

            GameObject goal = Instantiate(goalPrefab, goals);
            goal.transform.Find("Explanation").GetComponent<Text>().text = explanation;
            goal.transform.Find("Status").GetComponent<Text>().text = status;
            goal.transform.localPosition = new Vector3(0, -20 + -30 * i);
        }

        RectTransform content = selectCanvas.transform.Find("StageGoal/Content").GetComponent<RectTransform>();
        content.GetComponent<Text>().text = stageContentText;
        content.anchoredPosition += new Vector2(0, -20 + -30*mainGoals.Count + -20);

        startBtnText = selectCanvas.transform.Find("StartBtn").GetComponentInChildren<Text>();
        ChangeStartBtnText();

        // Load Play UI
        _fadeImage = playCanvas.transform.Find("Fade").GetComponent<Image>();

		_mentalText = playCanvas.transform.Find("PlayerCard/CurrentMental").GetComponent<Text>();
        _stateText = playCanvas.transform.Find("PlayerCard/CurrentState").GetComponent<Text>();
        _charNameText = playCanvas.transform.Find("PlayerCard/Player_KorName").GetComponent<Text>();

        disasterAlaram = playCanvas.transform.Find("MiddleUI/DisasterAlarm").gameObject;
        disasterAlaramText = disasterAlaram.transform.Find("Text").GetComponent<Text>();

        stageEnd = playCanvas.transform.Find("MiddleUI/StageEnd").gameObject;
        stageEndText = stageEnd.transform.Find("Text").GetComponent<Text>();

        timerText = GameObject.Find("UICanvas/PlayCanvas/TopUI/TurnEndBtn/TimerText").GetComponent<Text>();
        ChangeTimerText();

        // Load Resources
        operatorPrefabs[0] = Resources.Load<GameObject>("Prefabs/Operator/Captain");
        operatorPrefabs[1] = Resources.Load<GameObject>("Prefabs/Operator/HammerMan");
        operatorPrefabs[2] = Resources.Load<GameObject>("Prefabs/Operator/Rescuers");
        operatorPrefabs[3] = Resources.Load<GameObject>("Prefabs/Operator/Nurse");

        operatorCardNormalSprite = Resources.Load<Sprite>("Sprite/OperatorSelect_UI/Operator/OperatorCardNormal");
        operatorCardSelectedSprite = Resources.Load<Sprite>("Sprite/OperatorSelect_UI/Operator/OperatorCardSelected");

        _currGameState = GameState.SELECT_OPERATOR;
    }
    private void SelectOperator() {
        if (!bSelectOperatorInit) {
            tablet = new Tablet();
            bSelectOperatorInit = true;
        }

        tablet.Update();

        int cnt = 0;
        foreach (GameObject oper in operators) {
            if (oper != null)
                cnt++;
        }
        bStageStartReady = (cnt == operatorCardNum);
        ChangeStartBtnText();

        if (bStageStartReady && bStageStartBtnClicked) {
            bStageStartBtnClicked = false;

            foreach (GameObject oper in operators) {
                if (oper != null)
                    players.Add(oper.GetComponent<Player>());
            }

            tablet = null;
            _currGameState = GameState.STAGE_READY;
        }
    }
    private void StageReady() {
        selectCanvas.enabled = false;
        playCanvas.enabled = true;

        // Operator Spawn Object 삭제
        for (int i = TileMgr.Instance.MinFloor; i <= TileMgr.Instance.MaxFloor; i++) {
            OperatorSpawn[] operatorSpawns = TileMgr.Instance.GetOperatorSpawns(i);
            foreach (OperatorSpawn operatorSpawn in operatorSpawns)
                Destroy(operatorSpawn.gameObject);
        }

        foreach (Player player in players)
            player.StageStartActive();

        bStagePlaying = true;
        players[currPlayerIdx].OnSetMain();
        SetFocusToCurrOperator();

        _currGameState = GameState.PLAYER_TURN;
    }
    private void PlayerTurn() {
        Player currPlayer = players[currPlayerIdx];
        currPlayer.Move();
        currPlayer.Activate();

        if (bTurnEndClicked) {
            // 캐릭터들의 턴 종료 행동 함수 호출
            for (int i = 0; i < players.Count; i++)
                players[i].TurnEndActive();
            _currGameState = GameState.SURVIVOR_TURN;
            bTurnEndClicked = false;
        }
        else if (bStageEndClicked) {
            _currGameState = GameState.STAGE_END;
            bStageEndClicked = false;
        }
    }
    private void SurvivorTurn() {
        if (!bSurvivorActive) {
            foreach (Survivor survivor in survivors.Values)
                survivor.TurnEndActive();
            bSurvivorActive = true;
        }
        else {
            bool moveDone = true;
            foreach (Survivor survivor in survivors.Values) {
                if (!survivor.IsMoveDone)
                    moveDone = false;
            }

            if (moveDone) {
                bSurvivorActive = false;
                _currGameState = GameState.ENVIRONMENT_TURN;
            }
        }
    }
    private void EnvironmentTurn() {
        TileMgr.Instance.SpreadFire();
        TileMgr.Instance.Flaming();
        TileMgr.Instance.MoveGas();

        _currGameState = GameState.DISASTER_ALARM;
    }
    private void DisasterAlarm() {
        if (!bDisasterAlarmPopup) {
            Disaster disaster = disasterMgr.GetWillActiveDisaster();
            if (disaster != null) {
                disasterAlaramText.text = disaster.type.ToString();
                disasterAlaram.SetActive(true);
                bDisasterAlarmPopup = true;
            }
            else
                _currGameState = GameState.TURN_END;
        }
        else {
            if (bDisasterAlarmClicked) {
                disasterAlaram.SetActive(false);
                _currGameState = GameState.DISASTER_TURN;
                bDisasterAlarmClicked = false;
                bDisasterAlarmPopup = false;
            }
        }
    }
    private void DisasterTurn() {
        if (!bDisasterExist) {
            disasterObject = disasterMgr.TurnUpdate();
            if (disasterObject != null)
                bDisasterExist = true;
            else
                _currGameState = GameState.TURN_END;
        }
        else {
            if (disasterObject.IsActive) {
                Destroy(disasterObject.gameObject);
                disasterObject = null;

                StartCoroutine(disasterMgr.UpdateWillActiveDisasterArea()); // 다음 턴 재난 지역 타일맵에 동기화

                bDisasterExist = false;
                _currGameState = GameState.TURN_END;
            }
        }
    }
    private void TurnEnd() {
        GameTurn++;
        currTime += 5;
        ChangeTimerText();
        goalMgr.OnTurnEnd(currTime);

        _currGameState = GameState.PLAYER_TURN;
    }
    private void StageEnd() {
        if (!bStageEndSetup) {
            stageEnd.SetActive(true);
            stageEndText.text = (goalMgr.IsAllSatisfied()) ? "임무 성공" : "임무 실패";
            bStagePlaying = false;
            bStageEndSetup = true;
        }
    }

    public void OnClickStageStartBtn() {
        if (CurrGameState != GameState.SELECT_OPERATOR) return;

        if (bStageStartReady)
            bStageStartBtnClicked = true;
    }
    public void OnClickTurnEnd() {
        if (CurrGameState != GameState.PLAYER_TURN) return;

        if (isAllPlayerInSafetyArea && goalMgr.IsAllSatisfied())
            bStageEndClicked = true;
        else
            bTurnEndClicked = true;
    }
    public void OnClickChangeChar(bool isRight) {
        int prevPlayerIdx = currPlayerIdx;

        if (isRight && currPlayerIdx+1 < players.Count)
            currPlayerIdx++;
        else if (!isRight && currPlayerIdx-1 >= 0)
            currPlayerIdx--;

        if (prevPlayerIdx != currPlayerIdx) {
            players[prevPlayerIdx].OnUnsetMain();
            players[currPlayerIdx].OnSetMain();
            SetFocusToCurrOperator();
        }
    }
    public void OnClickTabletFloor(int floor) {
        if (CurrGameState != GameState.SELECT_OPERATOR) return;

        tablet.ChangeFloor(floor);
	}
    public void OnClickTabletCam(int number) {
        if (CurrGameState != GameState.SELECT_OPERATOR) return;

        tablet.ChangeCam(number);
    }
    public void OnClickOperatorCard(int operatorNumber) {
        if (CurrGameState != GameState.SELECT_OPERATOR) return;

        // Operator 생성 및 삭제
        KeyValuePair<int, int> prevCam = tablet.GetCamPlacedOperator(operatorNumber);
        KeyValuePair<int, int> currCam = tablet.GetCurrCam();

        if (prevCam.Equals(currCam)) {
            tablet.ClearOperatorPair(operatorNumber);
			Destroy(operators[operatorNumber]);
            operators[operatorNumber] = null;
            operatorCards[operatorNumber].GetComponent<Image>().sprite = operatorCardNormalSprite;
        }
        else {
            // 현재 캠에 존재하는 다른 대원 삭제
            int prevOperatorNumber = tablet.GetOperatorAtCam(currCam);
            if (prevOperatorNumber != -1) {
                tablet.ClearOperatorPair(prevOperatorNumber);
                Destroy(operators[prevOperatorNumber]);
                operators[prevOperatorNumber] = null;
                operatorCards[prevOperatorNumber].GetComponent<Image>().sprite = operatorCardNormalSprite;
			}

            if (operators[operatorNumber] == null) {
                operators[operatorNumber] = Instantiate(operatorPrefabs[operatorNumber]);
				operatorCards[operatorNumber].GetComponent<Image>().sprite = operatorCardSelectedSprite;
			}
			operators[operatorNumber].transform.position = tablet.GetCurrCamPos();
            tablet.SetOperatorPair(operatorNumber, currCam);
        }
    }
    public void OnClickDisasterAlarm() {
        if (CurrGameState == GameState.DISASTER_ALARM)
            bDisasterAlarmClicked = true;
	}

    public void SetFocusToCurrOperator() {
        FollowCam cam = Camera.main.GetComponent<FollowCam>();
        Transform currOperatorTransform = players[currPlayerIdx].transform;
        cam.SetPosition(currOperatorTransform.position);
        cam.SetTarget(currOperatorTransform);
    }

    private void ChangeMentalText(Player player) {
        if (_mentalText == null) return;

        switch (player.Mental) {
        case 4:
            _mentalText.text = "아주좋음";
            _mentalText.color = new Color(0, 1, 1);
            break;
        case 3:
            _mentalText.text = "좋    음";
            _mentalText.color = new Color(0.52f, 0.796f, 0.063f);
            break;
        case 2:
            _mentalText.text = "보    통";
            _mentalText.color = new Color(0.992f, 0.82f, 0.02f);
            break;
        case 1:
            _mentalText.text = "나    쁨";
            _mentalText.color = new Color(1, 0.5f, 0);
            break;
        default:
            _mentalText.text = "패    닉";
            _mentalText.color = new Color(0.8f, 0.353f, 0.353f);
            break;
        }
    }
    private void ChangeStateText(Player player) {
        if (_stateText == null) return;

        switch (player.Act) {
        case Player.Action.Carry:
            _stateText.text = "업 는 중";
            _stateText.color = new Color(1, 0.5f, 0);
            break;
        case Player.Action.Rescue:
            _stateText.text = "구 조 중";
            _stateText.color = new Color(1, 0.5f, 0);
            break;
        case Player.Action.Retire:
            _stateText.text = "행동불능";
            _stateText.color = new Color(0.35f, 0.35f, 0.35f);
            break;
        case Player.Action.Panic:
            _stateText.text = "패    닉";
            _stateText.color = new Color(0.8f, 0.35f, 0.35f);
            break;
        default:
            _stateText.text = "정    상";
            _stateText.color = new Color(1, 1, 1);
            break;
        }
    }
    public void ChangeNameText() {
        if (_charNameText == null) return;

        switch (players[currPlayerIdx].OperatorNumber) {
        case Captain.OPERATOR_NUMBER:
            _charNameText.text = "01. 주인공";
            break;
        case HammerMan.OPERATOR_NUMBER:
            _charNameText.text = "02. 빅토르";
            break;
        case Rescuer.OPERATOR_NUMBER:
            _charNameText.text = "03. 레  오";
            break;
        case Nurse.OPERATOR_NUMBER:
            _charNameText.text = "04. 시노에";
            break;
        }
    }
    public void ChangeStartBtnText() {
        if (bStageStartReady)
            startBtnText.text = "임무 시작";
        else
            startBtnText.text = string.Format("{0,2}:{1:00}", (currTime / 60), (currTime % 60));
    }
    public void ChangeTimerText() {
        if (timerText == null) return;

        if (isAllPlayerInSafetyArea && goalMgr.IsAllSatisfied())
            timerText.text = "임무 종료";
        else
            timerText.text = string.Format("{0,2}:{1:00}", (currTime / 60), (currTime % 60));
    }
    public IEnumerator StartLoading(){
        float alpha = 0.0f;
        _fadeImage.enabled = true;
        while(alpha <= 1.0f)
        {
            _fadeImage.color = new Color(1,1,1,alpha);
            alpha += Time.deltaTime;
            yield return null;
        }
        _loadingState = LoadingState.Stay;
        yield return new WaitUntil(() => _loadingState == LoadingState.End);
        while (alpha >= 0.0f)
        {
            _fadeImage.color = new Color(1, 1, 1, alpha);
            alpha -= Time.deltaTime;
            yield return null;
        }
        _fadeImage.enabled = false;
        _loadingState = LoadingState.Begin;
    }

    public List<Player> GetAroundPlayers(Vector3Int pos, int range) {
        List<Player> players = new List<Player>();

        foreach (Player player in this.players) {
            if ((player.currentTilePos - pos).magnitude < range)
                players.Add(player);
        }
        return players;
    }
    public int GetAroundPlayerCount(Vector3Int pos, int range) {
        return GetAroundPlayers(pos, range).Count;
    }
    public List<Player> GetPlayersAt(Vector3Int pos) {
        return GetAroundPlayers(pos, 1);
    }
    public void ChangeFloorPlayer(bool isUp) {
        players[currPlayerIdx].ChangeFloor(isUp);
	}

    public void AddSurvivor(Vector3Int pos, Survivor survivor) {
        survivors.Add(pos, survivor);
        if (survivor.IsImportant)
            goalMgr.OnAddImportantSurvivor();
        else
            goalMgr.OnAddSurvivor();
    }
    public Survivor GetSurvivorAt(Vector3Int pos) {
        if (survivors.ContainsKey(pos))
            return survivors[pos];
        return null;
    }

    public void OnMoveSurvivor(Vector3Int oldPos, Vector3Int newPos) {
        if (survivors.ContainsKey(oldPos)) {
            Survivor survivor = survivors[oldPos];
            survivors.Remove(oldPos);
            survivors.Add(newPos, survivor);
        }
    }
    public void OnMovePlayer(Vector3Int playerTilePos) {
        TileMgr.Instance.MoveEmbers();
        goalMgr.CheckArriveAt(playerTilePos);
    }
    public void OnEnterSafetyArea() {
        bool isAllPlayerInSafetyArea = true;

        foreach (Player player in players) {
            if (!player.IsInSafetyArea) {
                isAllPlayerInSafetyArea = false;
                break;
            }
        }

        if (isAllPlayerInSafetyArea) {
            this.isAllPlayerInSafetyArea = isAllPlayerInSafetyArea;
            ChangeTimerText();
        }
	}
    public void OnExitSafetyArea() {
        isAllPlayerInSafetyArea = false;
        ChangeTimerText();
    }

    public void OnCarrySurvivor(Vector3Int pos) {
        survivors.Remove(pos);
    }
    public void OnRescueSurvivor(Survivor survivor) {
        if (survivor.IsImportant)
            goalMgr.OnRescueImportantSurvivor();
        else
            goalMgr.OnRescueSurvivor();
        Destroy(survivor.gameObject);
    }
    public void OnDieSurvivor(Survivor survivor) {
        Vector3Int tilePos = TileMgr.Instance.WorldToCell(survivor.transform.position);
        survivors.Remove(tilePos);

        if (survivor.IsImportant)
            goalMgr.OnDieImportantSurvivor();
        else
            goalMgr.OnDieSurvivor();

        Destroy(survivor.gameObject);
    }
}
