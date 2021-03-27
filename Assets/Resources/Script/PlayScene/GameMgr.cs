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

    private Dictionary<Vector3Int, RescueTarget> RescueTargets = new Dictionary<Vector3Int, RescueTarget>();

    public int CurrentChar {
        get { return _currentChar; }
        set { _currentChar = value; }
    } // CurrentChar Property
   

    [SerializeField]
    private Image    DisasterAlaram = null;
    [SerializeField]
    private Text    DisasterAlaramText = null;

    private int _stage = 0;
    private DisasterMgr disasterMgr;
    private GoalMgr goalMgr;

    public int stage {
        get { return _stage; }
    }

    public enum GameState {
        STAGE_SETUP, SELECT_CHAR, STAGE_READY, PLAYER_TURN, RESCUE_TARGET_TURN, SPREAD_FIRE, DISASTER_ALARM, DISASTER_TURN, TURN_END
    }
    private GameState _currGameState = GameState.STAGE_SETUP;
    public GameState CurrGameState {
        get { return _currGameState; }
    }
    private bool bTurnEndClicked = false;
    private bool bRescueTargetActive = false;
    private bool bDisasterAlarmPopup = false, bDisasterAlarmClicked = false;
    private DisasterObject disasterObject = null;
    private bool bDisasterExist = false;

    private GameObject SelectCanvas, PlayCanvas;

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
        case GameState.RESCUE_TARGET_TURN: RescueTargetTurn(); break;
        case GameState.SPREAD_FIRE: SpreadFire(); break;
        case GameState.DISASTER_ALARM: DisasterAlarm(); break;
        case GameState.DISASTER_TURN: DisasterTurn(); break;
        case GameState.TURN_END: TurnEnd(); break;
        }

        if (CurrentChar < Players.Count) {
            Player player = Players[CurrentChar];
            ChangeMentalText(player);
            ChangeStateText(player);
            ChangeNameText();
        }

        if (TileMgr.Instance.IsChangedFire())
            goalMgr.CheckFireInArea();
    }

    private void StageSetup() {
        // Load Canvas Object
        GameObject canvas = GameObject.Find("UICanvas");
        SelectCanvas = canvas.transform.Find("SelectCanvas").gameObject;
        PlayCanvas = canvas.transform.Find("PlayCanvas").gameObject;

        SelectCanvas.SetActive(true);
        PlayCanvas.GetComponent<Canvas>().enabled = false;

        // Load ID Card Text
        _mentalText = GameObject.FindWithTag("MentalText").GetComponent<Text>();
        _stateText = GameObject.FindWithTag("StateText").GetComponent<Text>();
        _charNameText = GameObject.FindWithTag("NameText").GetComponent<Text>();

        //for(int i=BackTile.cellBounds.xMin; i<BackTile.cellBounds.xMax; i++)
        //{
        //    for(int j=BackTile.cellBounds.yMin; j<BackTile.cellBounds.yMax; j++)
        //    {
        //        Vector3Int nPos = new Vector3Int(i, j, 0);
        //        if (BackTile.GetTile(nPos) != null)
        //        {
        //            FogTile.SetTile(nPos, BlackFog);
        //        }
        //    }
        //}

        
        // Load XML
        XmlDocument doc = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("Stage/Stage" + stage);
        doc.LoadXml(textAsset.text);

        XmlNode disastersNode = doc.SelectSingleNode("Stage/Disasters");
        XmlNode goalsNode = doc.SelectSingleNode("Stage/Goals");

        disasterMgr = new DisasterMgr(disastersNode);
        goalMgr = new GoalMgr(goalsNode);

        StartCoroutine(disasterMgr.UpdateWillActiveDisasterArea()); // 다음 턴 재난 지역 타일맵에 동기화
        goalMgr.SetSurvivorNum(RescueTargets.Values.Count);

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
        PlayCanvas.GetComponent<Canvas>().enabled = true;

        foreach (Player player in Players)
            player.StageStartActive();

        _currGameState = GameState.PLAYER_TURN;
    }
    private void PlayerTurn() {
        if (bTurnEndClicked) {
            // 캐릭터들의 턴 종료 행동 함수 호출
            for (int i = 0; i < Players.Count; i++)
                Players[i].TurnEndActive();
            _currGameState = GameState.RESCUE_TARGET_TURN;
            bTurnEndClicked = false;
        }
    }
    private void RescueTargetTurn() {
        if (!bRescueTargetActive) {
            foreach (var rt in RescueTargets.Values)
                rt.TurnEndActive();
            bRescueTargetActive = true;
        }
        else {
            bool moveDone = true;
            foreach (var rt in RescueTargets.Values) {
                if (!rt.IsMoveDone)
                    moveDone = false;
            }

            if (moveDone) {
                bRescueTargetActive = false;
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
                DisasterAlaramText.text = disaster.type.ToString();
                DisasterAlaram.gameObject.SetActive(true);
                bDisasterAlarmPopup = true;
            }
            else
                _currGameState = GameState.TURN_END;
        }
        else {
            if (bDisasterAlarmClicked) {
                DisasterAlaram.gameObject.SetActive(false);
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
        goalMgr.TurnEnd();

        _currGameState = GameState.PLAYER_TURN;
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

    public void AddRescueTarget(Vector3Int pos, RescueTarget rt) {
        RescueTargets.Add(pos, rt);
	}
    public void RemoveRescueTarget(Vector3Int pos) {
        RescueTargets.Remove(pos);
    }
    public RescueTarget GetRescueTargetAt(Vector3Int pos) {
        if (RescueTargets.ContainsKey(pos))
            return RescueTargets[pos];
        return null;
    }
    public void MoveRescueTarget(Vector3Int oldPos, Vector3Int newPos) {
        if (RescueTargets.ContainsKey(oldPos)) {
            RescueTarget rt = RescueTargets[oldPos];
            RescueTargets.Remove(oldPos);
            RescueTargets.Add(newPos, rt);
        }
    }

    public void OnMovePlayer(Vector3Int playerTilePos) {
        TileMgr.Instance.MoveEmbers();
        goalMgr.CheckArriveAt(playerTilePos);
    }
    public void Rescue() {
        goalMgr.Rescue();
	}
}
