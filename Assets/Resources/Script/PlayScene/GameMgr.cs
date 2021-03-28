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

    public int GameTurn = 0; // 게임 턴
    private List<Player> Players = new List<Player>(); // 사용할 캐릭터들의 Components
    private int _currentChar = 0; // 현재 사용중인 캐릭터의 번호

    private Dictionary<Vector3Int, Survivor> survivors = new Dictionary<Vector3Int, Survivor>();

    public int CurrentChar {
        get { return _currentChar; }
        set { _currentChar = value; }
    } // CurrentChar Property
   
    private GameObject disasterAlaram = null;
    private Text disasterAlaramText = null;

    private GameObject stageEnd = null;
    private Text stageEndText = null;

    private int _stage = 0;
    private DisasterMgr disasterMgr;
    private GoalMgr goalMgr;

    public int stage {
        get { return _stage; }
    }

    public enum GameState {
        STAGE_SETUP, SELECT_CHAR, STAGE_READY,
        PLAYER_TURN, SURVIVOR_TURN, SPREAD_FIRE, DISASTER_ALARM, DISASTER_TURN, TURN_END,
        STAGE_END
    }
    private GameState _currGameState = GameState.STAGE_SETUP;
    public GameState CurrGameState {
        get { return _currGameState; }
    }

    private bool bStagePlaying = false;
    private bool bTurnEndClicked = false;
    private bool bSurvivorActive = false;
    private bool bDisasterAlarmPopup = false, bDisasterAlarmClicked = false;
    private DisasterObject disasterObject = null;
    private bool bDisasterExist = false;

    private GameObject selectCanvas, playCanvas;

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
        case GameState.SELECT_CHAR: SelectChar(); break;
        case GameState.STAGE_READY: StageReady(); break;
        case GameState.PLAYER_TURN: PlayerTurn(); break;
        case GameState.SURVIVOR_TURN: SurvivorTurn(); break;
        case GameState.SPREAD_FIRE: SpreadFire(); break;
        case GameState.DISASTER_ALARM: DisasterAlarm(); break;
        case GameState.DISASTER_TURN: DisasterTurn(); break;
        case GameState.TURN_END: TurnEnd(); break;
        case GameState.STAGE_END: StageEnd(); break;
        }

        if (bStagePlaying) {
            if (CurrentChar < Players.Count) {
                Player player = Players[CurrentChar];
                ChangeMentalText(player);
                ChangeStateText(player);
                ChangeNameText();
            }

            if (TileMgr.Instance.IsChangedFire())
                goalMgr.CheckFireInArea();

            if (goalMgr.IsAllSatisfied() || goalMgr.IsImpossible()) {
                _currGameState = GameState.STAGE_END;
                stageEnd.SetActive(true);
                stageEndText.text = (goalMgr.IsAllSatisfied()) ? "스테이지 클리어" : "스테이지 실패";
                bStagePlaying = false;
            }
        }
    }

    private void StageSetup() {
        // Load Canvas Object
        GameObject canvas = GameObject.Find("UICanvas");
        selectCanvas = canvas.transform.Find("SelectCanvas").gameObject;
        playCanvas = canvas.transform.Find("PlayCanvas").gameObject;

        selectCanvas.SetActive(true);
        playCanvas.GetComponent<Canvas>().enabled = false;

        // Load UI
        _mentalText = playCanvas.transform.Find("PlayerCard/CurrentMental").GetComponent<Text>();
        _stateText = playCanvas.transform.Find("PlayerCard/CurrentState").GetComponent<Text>();
        _charNameText = playCanvas.transform.Find("PlayerCard/Player_KorName").GetComponent<Text>();

        disasterAlaram = playCanvas.transform.Find("MiddleUI/DisasterAlarm").gameObject;
        disasterAlaramText = disasterAlaram.transform.Find("Text").GetComponent<Text>();

        stageEnd = playCanvas.transform.Find("MiddleUI/StageEnd").gameObject;
        stageEndText = stageEnd.transform.Find("Text").GetComponent<Text>();

        // Load XML
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Stage/Stage" + stage);
        doc.LoadXml(textAsset.text);

        XmlNode disastersNode = doc.SelectSingleNode("Stage/Disasters");
        XmlNode goalsNode = doc.SelectSingleNode("Stage/Goals");

        disasterMgr = new DisasterMgr(disastersNode);
        goalMgr = new GoalMgr(goalsNode);

        StartCoroutine(disasterMgr.UpdateWillActiveDisasterArea()); // 다음 턴 재난 지역 타일맵에 동기화

        _currGameState = GameState.SELECT_CHAR;
    }
    private void SelectChar() {
        //TileMgr.Instance.ExistPlayerSpawn()
        if (Input.GetMouseButtonUp(0)) {

		}

        Players.Add(GameObject.Find("Captain").GetComponent<Player>());
        Players.Add(GameObject.Find("HammerMan").GetComponent<Player>());
        Players.Add(GameObject.Find("Nurse").GetComponent<Player>());
        Players.Add(GameObject.Find("Rescuers").GetComponent<Player>());


        _currGameState = GameState.STAGE_READY;
    }
    private void StageReady() {
        playCanvas.GetComponent<Canvas>().enabled = true;

        foreach (Player player in Players)
            player.StageStartActive();

        bStagePlaying = true;

        _currGameState = GameState.PLAYER_TURN;
    }
    private void PlayerTurn() {
        if (bTurnEndClicked) {
            // 캐릭터들의 턴 종료 행동 함수 호출
            for (int i = 0; i < Players.Count; i++)
                Players[i].TurnEndActive();
            _currGameState = GameState.SURVIVOR_TURN;
            bTurnEndClicked = false;
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
                _currGameState = GameState.SPREAD_FIRE;
            }
        }
    }
    private void SpreadFire() {
        TileMgr.Instance.SpreadFire();
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
        goalMgr.OnTurnEnd();

        _currGameState = GameState.PLAYER_TURN;
    }
    private void StageEnd() {

	}

    public void OnClickTurnEnd() {
        if (CurrGameState == GameState.PLAYER_TURN)
            bTurnEndClicked = true;
    }
    public void OnClickDisasterAlarm() {
        if (CurrGameState == GameState.DISASTER_ALARM)
            bDisasterAlarmClicked = true;
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

        switch (CurrentChar) {
        case 0:
            _charNameText.text = "01. 주인공";
            break;
        case 1:
            _charNameText.text = "02. 빅토르";
            break;
        case 2:
            _charNameText.text = "03. 레  오";
            break;
        case 3:
            _charNameText.text = "04. 시노에";
            break;
        }
    }

    public List<Player> GetAroundPlayers(Vector3Int pos, int range) {
        List<Player> players = new List<Player>();

        foreach (Player player in Players) {
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
