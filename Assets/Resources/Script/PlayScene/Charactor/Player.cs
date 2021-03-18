using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Player : Charactor
{
    // UI 및 타일 정보
    public Image MTGage; // 멘탈
    public GameObject UI_Actives; // 행동 버튼 UI
    public GameObject UI_ToolBtns; // 도구 버튼 UI
    public GameObject FireWall; // 방화벽 Prefab
    public TileBase FireWallTile; // 타일맵에 적용할 방화벽
    private Text _mentalText; // ID Card 멘탈 Text UI
    private Text _stateText; // ID Card 상태 Text UI

    // 플레이어 스테이터스
    //안녕안녕?
    protected enum _Act { Idle, Walk, Run, Rescue, Interact, Panic, Retire } // 캐릭터 행동 상태 종류
    protected _Act _playerAct; 
    private RescueTarget _rescueTarget; // 현재 구조중인 타겟
    [SerializeField]
    private int _playerNum = 0; // 캐릭터 번호
    [SerializeField]
    private float _movespeed = 0.0f; // 캐릭터 이동속도

    [SerializeField]
    private float _maxMental = 0; // 최대 멘탈
    private float _currentMental = 0; // 현재 멘탈
    private Vector3 _moveDir = Vector3.left; // 캐릭터가 바라보는 방향

    // 타일 충돌체크용 값
    private GridLayout _tileLayout; // 타일맵 값 변경용 변수 (Tilemap::Background)
    private Vector3Int _currentTilePos = Vector3Int.zero; // 현재 캐릭터의 타일맵 좌표

    // Local Component
    private Animator _anim; // 캐릭터 애니메이션
    [SerializeField]
    private Transform _body = null; // 캐릭터 이미지의 Transform
    
    public Vector3Int currentTilePos
    {
        get { return _currentTilePos; }
    }

    protected override void Start()
    {
        base.Start();
        _tileLayout = GameMgr.Instance.BackTile.GetComponent<GridLayout>();
        _mentalText = GameObject.FindWithTag("MentalText").GetComponent<Text>();
        _stateText = GameObject.FindWithTag("StateText").GetComponent<Text>();
        _anim = GetComponentInChildren<Animator>();
        _currentMental = _maxMental;
        //SetFOV();
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        if (GameMgr.Instance != null && GameMgr.Instance.CurrGameState == GameMgr.GameState.PLAYER_TURN) {
            Move();
            Activate();
        }
    }

    protected virtual void Move()
    {
        float hor = Input.GetAxisRaw("Horizontal"); // 가속도 없이 Raw값 사용
        float ver = Input.GetAxisRaw("Vertical");

        //구조 상태가 아니며, 현재 체력과 산소가 남아있는 현재 조종중인 캐릭터를 Translate로 이동시킨다.
        if (CurrentO2 > 0.0f && GameMgr.Instance.CurrentChar == _playerNum && CurrentHP > 0.0f && _playerAct != _Act.Rescue && _currentMental > 0)
        {
            this.transform.Translate(hor * Time.deltaTime * _movespeed, ver * Time.deltaTime * _movespeed, 0.0f);
            if (GameMgr.Instance.CheckEmberTick()) // 작은 불 재생성
            {
                for (int i = 0; i < GameMgr.Instance.UsedEmberCount; i++) // 기존에 있던 작은불들 Active false
                {
                    GameMgr.Instance.Embers.GetChild(i).gameObject.SetActive(false);
                }
                GameMgr.Instance.UsedEmberCount = 0;
                for (int i = 0; i < TileMgr.Instance.Fires.Count; i++) // 현재 남아있는 불들의 수만큼 작은불 재생성
                {
                    TileMgr.Instance.Fires[i].FindEmberArea();
                }
            }
            if (hor != 0.0f) // 좌, 우 이동중이라면
            {
                AddO2(-(UseO2 * Time.deltaTime));
                GameMgr.Instance.EmberTime += Time.deltaTime / 2; // 작은불 재생성 시간 증가
                _moveDir = new Vector3(hor, 0, 0); // 바라보는 방향 변경
            }
            if (ver != 0.0f) //상, 하 이동중이라면
            {
                AddO2(-(UseO2 * Time.deltaTime));
                GameMgr.Instance.EmberTime += Time.deltaTime / 2; // 작은불 재생성 시간 증가
            }
            //if (_currentTilePos != _tileLayout.WorldToCell(transform.position))
            //{
            //    SetFOV();
            //}
            if((hor != 0 || ver != 0) && _anim.GetBool("IsRunning") == false) // 이동 시작 시
            {
                _anim.SetBool("IsRunning", true); // 달리기 애니메이션 재생
            }
            if(_moveDir.x > 0) // 바라보는 방향이 우측이라면
            {
                _body.rotation = new Quaternion(0, 180.0f, 0, this.transform.rotation.w); // 우측으로 이미지 회전
            }
            else if(_moveDir.x < 0) // 좌측이라면
            {
                _body.rotation = Quaternion.identity; // y값 초기화
            }

            Vector3 worldPos = transform.position;
            worldPos.y -= 100;
            _currentTilePos = _tileLayout.WorldToCell(worldPos); // 현재 캐릭터의 타일맵 좌표 갱신
        }
        if (hor == 0 && ver == 0) // 이동 종료 시
        {
            _anim.SetBool("IsRunning", false); // 달리기 애니메이션 종료
        }
    }

    protected InteractiveObject SearchAroundInteractiveObject(Vector3Int pos) {
        // 주변에 있는 상호작용 오브젝트 탐색
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int targetPos = pos + new Vector3Int(x, y, 0);
                InteractiveObject ino = TileMgr.Instance.GetInteractiveObject(targetPos);
                if (ino != null && ino.IsAvailable())
                    return ino;
            }
        }

        return null;
    }
    protected void ActivateInteractBtn(InteractiveObject interactiveObject) {
        GameObject InteractiveBtn = UI_Actives.transform.Find("InteractBtn").gameObject;
        Button button = (Button)InteractiveBtn.GetComponent("Button");
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(interactiveObject.Activate);

        InteractiveBtn.SetActive(true);
    }
    protected void DeactivateInteractBtn() {
        GameObject InteractiveBtn = UI_Actives.transform.Find("InteractBtn").gameObject;
        InteractiveBtn.SetActive(false);
    }

    protected virtual void Activate() // 행동 들 (구조, 도구사용)
    {
        if (GameMgr.Instance.CurrentChar == _playerNum) {
            InteractiveObject interactiveObject = SearchAroundInteractiveObject(currentTilePos);
            if (interactiveObject != null && interactiveObject.IsAvailable())
                ActivateInteractBtn(interactiveObject);
            else
                DeactivateInteractBtn();
        }

        if (Input.GetMouseButtonUp(1) && GameMgr.Instance.CurrentChar == _playerNum) // 마우스 우클릭 시 현재 조작중인 캐릭터의 버튼 UI 표시
        {
            if (UI_Actives.activeSelf == true || UI_ToolBtns.activeSelf == true) // 이미 켜져있었다면 UI 끄기
            {
                UI_Actives.SetActive(false);
                UI_ToolBtns.SetActive(false);
                
            }
            else if (UI_Actives.activeSelf == false && UI_ToolBtns.activeSelf == false) // Active UI 보이기
            {
                UI_Actives.SetActive(true);
            }
        }
    }

    public virtual void TurnEndActive() // 캐릭터가 턴이 끝날 때 호출되는 함수
    {
        AddO2(10.0f);
        if(_playerAct == _Act.Rescue) // 구조중이라면
        {
            _rescueTarget.RescueCount--; // 구조중인 대상의 남은 구조턴 감소
            if(_rescueTarget.RescueCount <= 0) // 구조턴 값이 0보다 작으면
            {
                _rescueTarget.gameObject.SetActive(false); // 구조 대상 숨기기
                _playerAct = _Act.Idle; // Idle 상태로 변경
                ChangeStateText();
            }
        }
    }

    public virtual void ActiveSkill()
    {
        UI_Actives.SetActive(false);
    }

    public void Rescue() // 구조 버튼 누를 시 호출되는 함수
    {
        Debug.Log("구조버튼 클릭");
        int RescueLayer = 1 << LayerMask.NameToLayer("Rescue"); // 생존자의 Layer
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _moveDir, 128, RescueLayer); // 레이캐스트 쏘기
        if (hit)
        {
            if (hit.transform.CompareTag("RescueTarget")) // 레이캐스트 충돌 대상이 구조대상이라면
            {
                _rescueTarget = hit.transform.GetComponent<RescueTarget>(); // 생존자 값 저장
                _playerAct = _Act.Rescue; // 구조 상태로 변경
                ChangeStateText();
            }
        }
        UI_Actives.SetActive(false); // UI 숨기기
    }

    public void OpenToolBtns() // 도구 버튼 누를 시 호출되는 함수
    {
        UI_ToolBtns.SetActive(true); // 도구 UI 보이기
        Debug.Log("도구버튼 클릭");
    }

    public void UseTool(int toolnum) // 도구 UI의 버튼 누를 시 호출되는 함수
    {
        switch(toolnum)
        {
            case 1: // FireExtinguisher
                StopAllCoroutines();
                StartCoroutine(UseFireExtinguisher());
                break;
            case 2: // StickyBomb
                UseStickyBomb();
                break;
            case 3: // FireWall
                UseFireWall();
                break;
            case 4: // SmokeAbsorption
                break;
            case 5: // O2Can
                UseO2Can();
                break;
        }
        UI_Actives.SetActive(false);
        UI_ToolBtns.SetActive(false); // UI 숨기기
    }

    IEnumerator UseFireExtinguisher() // 소화기 사용 코드 
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        //좌표 값 변경으로 인해 수정해야 할 코드
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int nPos = GameMgr.Instance.BackTile.WorldToCell(mousePos); // 마우스의 타일맵 상 좌표
        if (_currentTilePos.x - nPos.x < 2 &&
            _currentTilePos.x - nPos.x > -2 &&
            _currentTilePos.y - nPos.y < 2 &&
            _currentTilePos.y - nPos.y > -2) // 누른 위치가 캐릭터 기준 2칸 이내라면
        {
            Vector3Int offset = Vector3Int.zero; // 소화기가 퍼져나갈 크기
            if (mousePos.x > transform.position.x) // 마우스가 캐릭터보다 오른쪽에 있다면
            {
                offset.x = 1;
            }
            else if (mousePos.x < transform.position.x) // 마우스가 캐릭터보다 왼쪽에 있다면
            {
                offset.x = -1;
            }
            if (mousePos.y > transform.position.y) // 마우스가 캐릭터보다 위에 있다면
            {
                offset.y = 1;
            }
            else if (mousePos.y < transform.position.y) // 마우스가 캐릭터보다 아래 있다면
            {
                offset.y = -1;
            }
            if (GameMgr.Instance.Obstacle.GetTile(nPos).name == "Fire") // 탐색한 타일이 큰 불 타일이라면
            {
                GameMgr.Instance.Obstacle.SetTile(nPos, null); // 불 제거
                                                               //TileMgr의 Fire 리스트 속 ID를 검색받아 타일제거를 해야합니다 웅연쿤 // 고맙다 과거의 나
                                                               //이 아래부턴 소화기가 퍼져나간 곳의 불타일을 제거하는 코드
                nPos.x += offset.x;
                if (GameMgr.Instance.Obstacle.GetTile(nPos).name == "Fire")
                {
                    GameMgr.Instance.Obstacle.SetTile(nPos, null);
                }
                nPos.x -= offset.x;
                nPos.y += offset.y;
                if (GameMgr.Instance.Obstacle.GetTile(nPos).name == "Fire")
                {
                    GameMgr.Instance.Obstacle.SetTile(nPos, null);
                }
                nPos.x += offset.x;
                if (GameMgr.Instance.Obstacle.GetTile(nPos).name == "Fire")
                {
                    GameMgr.Instance.Obstacle.SetTile(nPos, null);
                }
            }
        }
    }

    private void UseFireWall() // 방화벽 설치
    {
        //좌표 단위가 변화되면서 수정해야할 코드
        Vector3Int nPos = GameMgr.Instance.BackTile.WorldToCell(transform.position) + new Vector3Int((int)_moveDir.x, 0, 0);
        if (GameMgr.Instance.Obstacle.GetTile(nPos) == null) // 설치할 위치에 장애물이 없다면 방화벽 설치
        {
            GameMgr.Instance.Obstacle.SetTile(nPos, FireWallTile);
        }
    }

    private void UseStickyBomb() // 점착폭탄 설치
    {
        //좌표 단위가 변화되면서 수정해야할 코드
        int ObstacleLayer = 1 << LayerMask.NameToLayer("Obstacle"); // 장애물만 체크할 Layer
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _moveDir, 128, ObstacleLayer); // 레이캐스트 발사=
        if (hit)
        {
            Debug.Log("Ray hit");
            Debug.Log(hit.transform.name);
            if (hit.transform.CompareTag("Wall")) // 레이캐스트 충돌대상이 벽이라면
            {
                Vector3Int nPos = GameMgr.Instance.BackTile.WorldToCell(transform.position) + new Vector3Int((int)_moveDir.x, 0, 0);
                GameMgr.Instance.Obstacle.SetTile(nPos, null); // 벽 제거
            }
        }
    }

    private void UseO2Can() // 산소캔 사용
    {
        AddO2(45.0f);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other) { // 충돌체크
        switch (other.tag) {
        case "Fire": // 큰 불
            AddHP(-25.0f); // 체력 감소
            AddMental(-2); // 멘탈 감소
            break;

        case "Ember": // 작은 불
            AddHP(-10.0f); // 체력 감소
            AddMental(-1); // 멘탈 감소
            break;

        case "Electric":
        case "Water(Electric)":
            AddHP(-35.0f); // 체력 감소
            AddMental(-2); // 멘탈 감소
            break;

        case "Beacon":
            // TODO: 구조 종료
            break;
        }

        ChangeMentalText(); // 멘탈 UI 변경
        ChangeStateText(); // 상태 UI 변경
    }

    protected void RenderInteractArea(ref Vector3Int oPos)
    {
        Vector3 basePos = GameMgr.Instance.BackTile.CellToWorld(currentTilePos) + GameMgr.Instance.BackTile.cellSize/2.0f;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - basePos; // 마우스 로컬 좌표
        Vector3Int direction; // 캐릭터 기준 마우스 방향
        if (Mathf.Abs(mousePos.x) > Mathf.Abs(mousePos.y))
            direction = (mousePos.x > 0) ? Vector3Int.right : Vector3Int.left;
        else
            direction = (mousePos.y > 0) ? Vector3Int.up : Vector3Int.down;

        Vector3Int nPos = currentTilePos + direction; // 새 좌표 갱신
        if (nPos != oPos) // 기존의 렌더부분과 갱신된 부분이 다르면
        {
            GameMgr.Instance.BackTile.SetTileFlags(oPos, TileFlags.None);
            GameMgr.Instance.BackTile.SetColor(oPos, new Color(1, 1, 1, 1)); // 기존의 좌표 색 복구
            GameMgr.Instance.BackTile.SetTileFlags(nPos, TileFlags.None);
            GameMgr.Instance.BackTile.SetColor(nPos, new Color(0, 0, 1, 1)); // 새로운 좌표 색 변경
            oPos = nPos;
        }
    }

    public void ChangeMentalText()
    {
        switch(_currentMental)
        {
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

    public void ChangeStateText()
    {
        switch(_playerAct)
        {
            case _Act.Rescue:
                _stateText.text = "구조중";
                _stateText.color = new Color(1, 0.5f, 0);
                break;
            case _Act.Retire:
                _stateText.text = "행동불능";
                _stateText.color = new Color(0.35f, 0.35f, 0.35f);
                break;
            case _Act.Panic:
                _stateText.text = "패    닉";
                _stateText.color = new Color(0.8f, 0.35f, 0.35f);
                break;
            default:
                _stateText.text = "정    상";
                _stateText.color = new Color(1, 1, 1);
                break;
        }
    }

    public override void AddHP(float value)
    {
        base.AddHP(value);

        if (CurrentHP <= 0 )
            _playerAct = _Act.Retire;
    }

    public override void AddO2(float value)
    {
        base.AddO2(value);

        if (CurrentO2 <= 0)
            _playerAct = _Act.Retire;
    }

    private void AddMental(int value)
    {
        _currentMental += value;
        if(_currentMental <= 0)
        {
            _playerAct = _Act.Panic;
        }
        if(_currentMental > _maxMental)
        {
            _currentMental = _maxMental;
        }
    }

    //private void SetFOV()
    //{
    //    GridLayout gridLayout = GameMgr.Instance.FogTile.GetComponent<GridLayout>();
    //    Vector3Int nPos = gridLayout.WorldToCell(transform.position + (Vector3.left * 2000));
    //    int count = 5 / 2;
    //    for(int i= -count; i<=count; i++)
    //    {
    //        int checkArea = 0;
    //        for(int j=0; j< 5 - Mathf.Abs(i); j++)
    //        {
    //            if(checkArea >= Mathf.Abs(i))
    //            {
    //                GameMgr.Instance.FogTile.SetTileFlags(nPos + new Vector3Int(checkArea, i, 0), TileFlags.None);
    //                GameMgr.Instance.FogTile.SetColor(nPos + new Vector3Int(checkArea, i, 0), new Color(1, 1, 1, 0.0f));
    //            }
    //            checkArea++;
    //        }
    //    }
    //}
}