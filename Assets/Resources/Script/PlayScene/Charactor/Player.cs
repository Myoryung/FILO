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

    // 플레이어 스테이터스
    public enum Action { Idle, Walk, Run, Carry, Rescue, Interact, Panic, Retire, MoveFloor } // 캐릭터 행동 상태 종류
    protected Action _playerAct; 
    private Survivor _rescuingSurvivor; // 현재 구조중인 생존자
    [SerializeField]
    private int _playerNum = 0; // 캐릭터 번호
    [SerializeField]
    private float _movespeed = 0.0f; // 캐릭터 이동속도

    [SerializeField]
    private float _maxMental = 0; // 최대 멘탈
    private float _currentMental = 0; // 현재 멘탈

    [SerializeField]
    private float skillUseO2; // 스킬 발동 시 사용되는 산소량
    private const float O2_ADDED_PER_TURN = 10.0f; // 턴 종료시마다 회복되는 산소량

    // 타일 충돌체크용 값
    private Vector3Int _currentTilePos = Vector3Int.zero; // 현재 캐릭터의 타일맵 좌표
    private bool isInSafetyArea = false, isInFire = false, isInElectric = false, isInGas = false, isInStair = false;
    private float startTimeInFire = 0, startTimeInElectric = 0;

	// Local Component
	private Animator _anim; // 캐릭터 애니메이션
    [SerializeField]
    private Transform _body = null; // 캐릭터 이미지의 Transform
    private Rigidbody2D rbody = null;
    
    public Vector3Int currentTilePos
    {
        get { return _currentTilePos; }
    }

    protected override void Start()
    {
        base.Start();

        _anim = GetComponentInChildren<Animator>();
        _currentMental = _maxMental;
        rbody = GetComponent<Rigidbody2D>();
        //SetFOV();

        _currentTilePos = TileMgr.Instance.WorldToCell(transform.position);
        Debug.Log(gameObject.name + ": " + transform.position + " -> " + _currentTilePos);
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        if (GameMgr.Instance != null && GameMgr.Instance.CurrGameState == GameMgr.GameState.PLAYER_TURN) {
            Move();
            Activate();
        }


        float currTime = Time.time;
        if (isInFire && currTime - startTimeInFire >= 2.0f) {
            AddHP(-5);
            startTimeInFire = currTime;
        }
        if (isInElectric && currTime - startTimeInElectric >= 2.0f) {
            AddHP(-5);
            startTimeInElectric = currTime;
        }
    }

    protected virtual void Move()
    {
        float hor = Input.GetAxisRaw("Horizontal"); // 가속도 없이 Raw값 사용
        float ver = Input.GetAxisRaw("Vertical");

        bool isMoved = false;

        //구조 상태가 아니며, 현재 체력과 산소가 남아있는 현재 조종중인 캐릭터를 Translate로 이동시킨다.
        if (CurrentO2 > 0.0f && GameMgr.Instance.CurrentChar == _playerNum && Act != Action.Carry && _playerAct != Action.MoveFloor && CurrentHP > 0.0f && _currentMental > 0)
        {
            //transform.Translate(hor * Time.deltaTime * _movespeed, ver * Time.deltaTime * _movespeed, 0.0f);
            if (hor != 0.0f || ver != 0.0f) {
                Vector3 dir = new Vector3(hor, ver, 0.0f);
                dir /= dir.magnitude;
                rbody.velocity = dir * _movespeed;

                isMoved = true;
            }
            else
                rbody.velocity = Vector3.zero;


            //if ((hor != 0 || ver != 0) && _anim.GetBool("IsRunning") == false) // 이동 시작 시
            //{
            //    //_anim.SetBool("IsRunning", true); // 달리기 애니메이션 재생
            //}
            //if (hor > 0) // 바라보는 방향이 우측이라면
            //{
            //    _body.rotation = new Quaternion(0, 180.0f, 0, this.transform.rotation.w); // 우측으로 이미지 회전
            //}
            //else if (hor < 0) // 좌측이라면
            //{
            //    _body.rotation = Quaternion.identity; // y값 초기화
            //}

            _currentTilePos = TileMgr.Instance.WorldToCell(transform.position); // 현재 캐릭터의 타일맵 좌표 갱신
            if (hor != 0 || ver != 0)
                GameMgr.Instance.OnMovePlayer(currentTilePos);
        }

        if (isMoved) {
            float o2UseRate = 1.0f;
            if (isInGas)
                o2UseRate *= 1.5f;
            if (Act == Action.Rescue)
                o2UseRate *= 1.5f;

            AddO2(-UseO2 * o2UseRate * Time.deltaTime);
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
        if (GameMgr.Instance.CurrentChar != _playerNum) return;

        InteractiveObject interactiveObject = SearchAroundInteractiveObject(currentTilePos);
        if (interactiveObject != null && interactiveObject.IsAvailable())
            ActivateInteractBtn(interactiveObject);
        else
            DeactivateInteractBtn();

        if (Input.GetMouseButtonUp(1)) { // 마우스 우클릭 시 UI 표시
            if (UI_Actives.activeSelf || UI_ToolBtns.activeSelf) { // 이미 켜져있었다면 UI 끄기
                UI_Actives.SetActive(false);
                UI_ToolBtns.SetActive(false);
            }
            else
                UI_Actives.SetActive(true);
        }
    }

    public virtual void StageStartActive() {
	}
    public virtual void TurnEndActive() // 캐릭터가 턴이 끝날 때 호출되는 함수
    {
        if (_playerAct != Action.Panic) { // 패닉 상태는 산소가 회복되지 않는다
            if (IsInSafetyArea)
                AddO2(O2_ADDED_PER_TURN * 2.0f);
            else
                AddO2(O2_ADDED_PER_TURN);
        }

        if(_playerAct == Action.Carry) // 업는 중이라면
        {
            _rescuingSurvivor.CarryCount--;
            if (_rescuingSurvivor.CarryCount <= 0) // 업는턴 값이 0보다 작으면
            {
                _rescuingSurvivor.TurnOffRender();
				_playerAct = Action.Rescue; // Rescue 상태로 변경
            }
		}
    }

    public virtual void ActiveSkill() {
        UI_Actives.SetActive(false);
    }
    protected float GetSkillUseO2() {
        float o2UseRate = 1.0f;
        if (isInGas)
            o2UseRate *= 1.5f;
        if (Act == Action.Rescue)
            o2UseRate *= 1.5f;

        return skillUseO2 * o2UseRate;
    }

    public virtual void ActiveUltSkill() // 궁국기 virtual 함수
    {
        UI_Actives.SetActive(false);
    }

    public virtual void StartRescue() // Rescue가 IEnumrator가 되며 버튼용 함수 추가
    {
        UI_Actives.SetActive(false);
        StartCoroutine(Rescue());
    }

    IEnumerator Rescue() // 구조 버튼 누를 시 호출되는 함수
    {
        Vector3Int nPos = currentTilePos;
        while (true) {
            RenderInteractArea(ref nPos); // 구조 영역선택
            if (Input.GetMouseButtonDown(0)) {
                Survivor survivor = GameMgr.Instance.GetSurvivorAt(nPos);
                if (survivor != null) {
                    GameMgr.Instance.OnCarrySurvivor(nPos);
                    _rescuingSurvivor = survivor;
                    _playerAct = Action.Carry; // 업는 상태로 변경
                }
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving) // 움직일 시 취소
                break;

            yield return null;
        }

        TileMgr.Instance.RemoveEffect(nPos); // 구조 영역 선택한거 원상복구
    }

    public void OpenToolBtns() // 도구 버튼 누를 시 호출되는 함수
    {
        UI_ToolBtns.SetActive(true); // 도구 UI 보이기
    }

    public void UseTool(int toolnum) // 도구 UI의 버튼 누를 시 호출되는 함수
    {
        switch(toolnum)
        {
        case 1: // FireExtinguisher
            StartCoroutine(UseFireExtinguisher());
            break;
        case 2: // StickyBomb
            StartCoroutine(UseStickyBomb());
            break;
        case 3: // FireWall
            StartCoroutine(UseFireWall());
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
        //좌표 값 변경으로 인해 수정해야 할 코드
        while (true) {
            if (Input.GetMouseButton(0)) {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = transform.position.z;

                Vector3 localPos = mousePos - transform.position; // 마우스 캐릭터 기준 로컬좌표
                Vector3Int moustIntPos = TileMgr.Instance.WorldToCell(mousePos); // 타일맵에서 마우스 좌표
                if (_currentTilePos.x - moustIntPos.x <= 2 &&
                    _currentTilePos.x - moustIntPos.x >= -2 &&
                    _currentTilePos.y - moustIntPos.y <= 2 &&
                    _currentTilePos.y - moustIntPos.y >= -2) // 누른 위치가 캐릭터 기준 2칸 이내라면
                {
                    Vector3Int offset = Vector3Int.zero; // 소화기가 퍼져나갈 크기
                    offset.x = (localPos.x > 0) ? 1 : -1; // y축으로 퍼질 방향
                    offset.y = (localPos.y > 0) ? 1 : -1; // x축으로 퍼질 방향
                    Vector2 SpreadRange = new Vector2(2, 2); // 불 제거 범위
                    for (int i = 0; Mathf.Abs(i) < SpreadRange.x; i+= offset.x) {
                        for (int j = 0; Mathf.Abs(j) < SpreadRange.y; j+= offset.y) {
                            Vector3Int fPos = moustIntPos + new Vector3Int(i, j, 0); // 탐색할 타일 좌표
                            TileMgr.Instance.RemoveFire(fPos);
                        }
                    }
                }
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving) // 움직이면 취소
                break;
            yield return null;
        }
    }
    IEnumerator UseFireWall() // 방화벽 설치
    {
        Vector3Int nPos = currentTilePos;
        while (true) {
            RenderInteractArea(ref nPos);
            if (Input.GetMouseButtonDown(0)) {
                TileMgr.Instance.CreateFireWall(nPos);
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving)
                break;
            yield return null;
        }

        TileMgr.Instance.RemoveEffect(nPos);
    }
    IEnumerator UseStickyBomb() // 점착폭탄 설치
    {
        Vector3Int nPos = currentTilePos;

        while (true) {
            RenderInteractArea(ref nPos);
            if (Input.GetMouseButtonDown(0)) {
                TileMgr.Instance.RemoveTempWall(nPos);
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving)
                break;
            yield return null;
        }

        TileMgr.Instance.RemoveEffect(nPos);
    }

    private void UseO2Can() // 산소캔 사용
    {
        AddO2(45.0f);
    }

    protected override void OnTriggerEnter2D(Collider2D other) {
        base.OnTriggerEnter2D(other);

        switch (other.tag) {
        case "Fire":
            startTimeInFire = Time.time;
            isInFire = true;
            AddMental(-2);
            break;

        case "Ember":
            AddMental(-1); // 멘탈 감소
            break;

        case "Electric":
        case "Water(Electric)":
            startTimeInElectric = Time.time;
            isInElectric = true;
            AddMental(-2); // 멘탈 감소
            break;

        case "Gas":
            isInGas = true;
            break;

        case "Beacon":
            isInSafetyArea = true;
            GameMgr.Instance.OnEnterSafetyArea();

            // 구조 종료
            if (_playerAct == Action.Rescue) {
                GameMgr.Instance.OnRescueSurvivor(_rescuingSurvivor);
                _rescuingSurvivor = null;
                _playerAct = Action.Idle;
            }
            break;

        case "UpStair":
            if (!isInStair) {
                isInStair = true;
                StartCoroutine(ChangeFloor(true));
            }
            break;
        case "DownStair":
            if (!isInStair) {
                isInStair = true;
                StartCoroutine(ChangeFloor(false));
            }
            break;
        }
    }

	protected void OnTriggerExit2D(Collider2D collision) {
		switch (collision.tag) {
        case "Fire":
            isInFire = false;
            break;

        case "Electric":
        case "Water(Electric)":
            isInElectric = false;
            break;

        case "Gas":
            isInGas = false;
            break;

        case "Beacon":
            isInSafetyArea = false;
            GameMgr.Instance.OnExitSafetyArea();
            break;

        case "UpStair":
        case "DownStair":
            isInStair = false;
            break;
        }
	}

    IEnumerator ChangeFloor(bool isUp){
        Action oldAct = _playerAct;
        _playerAct = Action.MoveFloor;
        rbody.velocity = Vector2.zero;
        StartCoroutine(GameMgr.Instance.StartLoading());
        yield return new WaitUntil(() => GameMgr.Instance.CurrentLoadingState
                                        == GameMgr.LoadingState.Stay);
        
        int floorNumber = _currentTilePos.z + ((isUp) ? 1 : -1);
        _currentTilePos.z = floorNumber;
        transform.position = TileMgr.Instance.CellToWorld(currentTilePos);

        TileMgr.Instance.SwitchFloorTilemap(floorNumber);
        GameMgr.Instance.CurrentLoadingState = GameMgr.LoadingState.End;
        _playerAct = oldAct;
    }

	protected void RenderInteractArea(ref Vector3Int oPos) {
        Vector3Int direction = GetMouseDirectiontoTilemap();

        Vector3Int nPos = currentTilePos + direction; // 새 좌표 갱신
        if (nPos != oPos) { // 기존의 렌더부분과 갱신된 부분이 다르면
            TileMgr.Instance.RemoveEffect(oPos);            // 기존의 좌표 색 복구
            TileMgr.Instance.SetEffect(nPos, Color.blue);   // 새로운 좌표 색 변경
            oPos = nPos;
        }
    } // 

    public override void AddHP(float value) {
        base.AddHP(value);

        if (CurrentHP <= 0)
        {
            _playerAct = Action.Retire;
            rbody.velocity = Vector2.zero;
        }
    }

    public override void AddO2(float value) {
        base.AddO2(value);

        if (CurrentO2 <= 0)
            _playerAct = Action.Retire;
    }

    private void AddMental(int value) {
        _currentMental += value;
        if(_currentMental <= 0)
        {
            _playerAct = Action.Panic;
            rbody.velocity = Vector2.zero;
        }
        if(_currentMental > _maxMental)
        {
            _currentMental = _maxMental;
        }
    }

    public Action Act {
        get { return _playerAct; }
	}
    public float Mental {
        get { return _currentMental; }
	}
    public bool IsInSafetyArea {
        get { return isInSafetyArea; }
	}

    protected bool IsMoving { // 현재 움직이는 상태인가 체크하는 함수
        get { return (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0); }
    }

    private Vector3Int GetMouseDirectiontoTilemap() // 현재 캐릭터 기준으로 마우스가 어느 위치에 있는지 반환하는 함수
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position; // 마우스 로컬 좌표
        Vector3Int direction;
        if (Mathf.Abs(mousePos.x) > Mathf.Abs(mousePos.y))
            direction = (mousePos.x > 0) ? Vector3Int.right : Vector3Int.left;
        else
            direction = (mousePos.y > 0) ? Vector3Int.up : Vector3Int.down;
        return direction;
    }
}