using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Tilemap용
using UnityEngine.UI; // UI 용

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


    public int GameTurn = 0; // 게임 턴
    public Player[] Comp_Players; // 사용할 캐릭터들의 Components
    private int _currentChar = 0; // 현재 사용중인 캐릭터의 번호

    public int CurrentChar {
        get { return _currentChar; }
        set { _currentChar = value; }
    } // CurrentChar Property
    private float _EmberTime = 0.0f; // 작은 불 생성 주기
    public float EmberTime {
        get { return _EmberTime; }
        set { _EmberTime = value; }
    } // 작은 불 생성 주기Property
    private int _usedEmberCount = 0; // 현재 사용중인 작은 불의 숫자
    public int UsedEmberCount {
        get { return _usedEmberCount; }
        set { _usedEmberCount = value; }
    } //현재 사용중인 작은 불의 숫자 Property
    public Transform Embers; // 작은 불의 부모 오브젝트의 Transform
    public GameObject Ember; // 작은 불 Prefab
    public Tilemap BackTile; // Background Tilemap
    public Tilemap Obstacle; // Obstacel Tilemap
    public Tilemap RescueTilemap; // RescueTarget Tilemap

    [SerializeField]
    private Image    DisasterAlaram = null;
    [SerializeField]
    private Text    DisasterAlaramText = null;

    [SerializeField]
    private Text _timerText = null; // 시계 UI
    private int _minute; // 게임 상의 시간

    private int _stage = 0;
    private DisasterMgr disasterMgr;

    public int stage {
        get { return _stage; }
    }

    public enum GameState {
        STAGE_SETUP, PLAYER_TURN, RESCUE_TARGET_TURN, SPREAD_FIRE, DISASTER_ALARM, DISASTER_TURN, TURN_END
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
        case GameState.STAGE_SETUP:         StageSetup();       break;
        case GameState.PLAYER_TURN:         PlayerTurn();       break;
        case GameState.RESCUE_TARGET_TURN:  RescueTargetTurn(); break;
        case GameState.SPREAD_FIRE:         SpreadFire();       break;
        case GameState.DISASTER_ALARM:      DisasterAlarm();    break;
        case GameState.DISASTER_TURN:       DisasterTurn();     break;
        case GameState.TURN_END:            TurnEnd();          break;
        }
    }

    private void StageSetup() {

        for (int i = 0; i < 100; i++) // 작은 불 생성
        {
            Instantiate(Ember, Vector3.zero, Quaternion.identity, Embers);
        }
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
        _minute = 1189; // 게임 상의 시간

        disasterMgr = new DisasterMgr(_stage);

        _currGameState = GameState.PLAYER_TURN;
    }
    private void PlayerTurn() {
        if (bTurnEndClicked) {
            // 캐릭터들의 턴 종료 행동 함수 호출
            for (int i = 0; i < Comp_Players.Length; i++)
                Comp_Players[i].TurnEndActive();
            _currGameState = GameState.RESCUE_TARGET_TURN;
            bTurnEndClicked = false;
        }
    }
    private void RescueTargetTurn() {
        if (!bRescueTargetActive) {
            foreach (var rt in TileMgr.Instance.RescueTargets.Values)
                rt.TurnEndActive();
            bRescueTargetActive = true;
        }
        else {
            bool moveDone = true;
            foreach (var rt in TileMgr.Instance.RescueTargets.Values) {
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
        // 큰 불 생성
        for (int i = 0, count = TileMgr.Instance.Fires.Count; i< count; i++)
            TileMgr.Instance.Fires[i].SpreadFire();

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
                bDisasterExist = false;
                _currGameState = GameState.TURN_END;
            }
        }
    }
    private void TurnEnd() {
        GameTurn++;
        _minute += 5;
        _timerText.text = (_minute / 60).ToString() + " : " + (_minute % 60).ToString(); // 시계 UI 갱신

        _currGameState = GameState.PLAYER_TURN;
    }


    public bool CheckEmberTick()
    {
        if (_EmberTime > 2.0f) // 2초가 지나면 true 반환
        {
            _EmberTime = 0.0f;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnClickTurnEnd() {
        if (CurrGameState == GameState.PLAYER_TURN)
            bTurnEndClicked = true;
    }
    public void OnClickDisasterAlarm() {
        if (CurrGameState == GameState.DISASTER_ALARM)
            bDisasterAlarmClicked = true;
	}
}
