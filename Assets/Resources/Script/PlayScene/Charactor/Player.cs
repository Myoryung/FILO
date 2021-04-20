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
    private InteractiveObject aroundInteractiveObject;
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
    private bool isInSafetyArea = false, isInGas = false, isInStair = false;
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
    }

    // Update is called once per frame
    protected virtual void Update() {
        float currTime = Time.time;
        if (inFireCount > 0 && currTime - startTimeInFire >= 2.0f) {
            AddHP(-5);
            startTimeInFire = currTime;
        }
        if (inElectricCount > 0 && currTime - startTimeInElectric >= 2.0f) {
            AddHP(-5);
            startTimeInElectric = currTime;
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

        if (_playerAct == Action.Carry) // 업는 중이라면
        {
            _rescuingSurvivor.CarryCount--;
            if (_rescuingSurvivor.CarryCount <= 0) // 업는턴 값이 0보다 작으면
            {
                _rescuingSurvivor.TurnOffRender();
                _playerAct = Action.Rescue; // Rescue 상태로 변경
            }
        }

        // 애니메이션 종료
        if (_anim.GetBool("IsRunning"))
            _anim.SetBool("IsRunning", false);

        // UI 비활성화
        UI_Actives.SetActive(false);
        UI_ToolBtns.SetActive(false);
        if (aroundInteractiveObject != null)
            aroundInteractiveObject.SetActive_ConditionUI(false);
    }
    public void OnSetMain() {
        // 이동 제한 해제
        rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    public void OnUnsetMain() {
        // 이동 제한
        rbody.constraints = RigidbodyConstraints2D.FreezeAll;
        rbody.velocity = Vector3.zero;

        // 애니메이션 종료
        if (_anim.GetBool("IsRunning"))
            _anim.SetBool("IsRunning", false);

        // UI 비활성화
        UI_Actives.SetActive(false);
        UI_ToolBtns.SetActive(false);
        if (aroundInteractiveObject != null)
            aroundInteractiveObject.SetActive_ConditionUI(false);
    }

    public virtual void Move() {
        float hor = Input.GetAxisRaw("Horizontal"); // 가속도 없는 Raw값 사용
        float ver = Input.GetAxisRaw("Vertical");

        if (CurrentHP <= 0 || CurrentO2 <= 0 || _currentMental <= 0 ||
            Act == Action.Carry || Act == Action.MoveFloor ||
            (hor == 0.0f && ver == 0.0f)) {

            // 이동 종료
            rbody.velocity = Vector3.zero;

            // 애니메이션 종료
            if (_anim.GetBool("IsRunning"))
                _anim.SetBool("IsRunning", false);
            return;
        }

        // 캐릭터 이동
        Vector3 dir = new Vector3(hor, ver, 0.0f);
        dir /= dir.magnitude;
        rbody.velocity = dir * _movespeed;

        _currentTilePos = TileMgr.Instance.WorldToCell(transform.position);

        // 이동 방향에 따라 캐릭터 이미지 회전
        bool isRight = hor > 0;
        if (isRight) {
            if (_body.rotation.y != 180)
                _body.rotation = new Quaternion(0, 180.0f, 0, transform.rotation.w);
        }
        else {
            if (_body.rotation.y != 0)
                _body.rotation = Quaternion.identity;
        }
        
        // 애니메이션 시작
        if (!_anim.GetBool("IsRunning"))
            _anim.SetBool("IsRunning", true);

        // 산소 소비
        float o2UseRate = 1.0f;
        if (isInGas)
            o2UseRate *= 1.5f;
        if (Act == Action.Rescue)
            o2UseRate *= 1.5f;

        AddO2(-UseO2 * o2UseRate * Time.deltaTime);

        // 트리거 실행
        GameMgr.Instance.OnMovePlayer(currentTilePos);
    }

    public virtual void Activate() { // 행동 UI (구조, 도구사용)
        if (Input.GetMouseButtonUp(1)) { // 마우스 우클릭 시 UI 표시
            if (UI_Actives.activeSelf || UI_ToolBtns.activeSelf) { // 이미 켜져있었다면 UI 끄기
                UI_Actives.SetActive(false);
                UI_ToolBtns.SetActive(false);
                if (aroundInteractiveObject != null)
                    aroundInteractiveObject.SetActive_ConditionUI(false);
            }
            else {
                UI_Actives.SetActive(true);
            }
        }

        InteractiveObject interactiveObject = SearchAroundInteractiveObject(currentTilePos);
        if (interactiveObject != null && interactiveObject.IsAvailable())
            SetActive_InteractBtn(true);
        else
            SetActive_InteractBtn(false);

        if (UI_Actives.activeSelf) {
            if (aroundInteractiveObject != null)
                aroundInteractiveObject.SetActive_ConditionUI(false);

            if (interactiveObject != null && interactiveObject.IsActive())
                interactiveObject.SetActive_ConditionUI(true);
        }
        aroundInteractiveObject = interactiveObject;
    }
    protected InteractiveObject SearchAroundInteractiveObject(Vector3Int pos) {
        InteractiveObject activeObject = null;

        // 주변에 있는 상호작용 오브젝트 탐색
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int targetPos = pos + new Vector3Int(x, y, 0);
                InteractiveObject ino = TileMgr.Instance.GetInteractiveObject(targetPos);
                if (ino == null) continue;

                if (ino.IsActive() && activeObject == null)
                    activeObject = ino;
                if (ino.IsAvailable())
                    return ino;
            }
        }

        return activeObject;
    }
    private void SetActive_InteractBtn(bool value) {
        UI_Actives.transform.Find("InteractBtn").gameObject.SetActive(value);
    }

    public void OnClickBehaviorBtn(int number) {
        switch (number) {
        case 0: ActiveSkill(); break;
        case 1: StartCoroutine(Rescue()); break;
        case 2: OpenToolBtns(); break;
        case 3: OnClickInteractBtn(); break;
        }

        if (number != 2) {
            UI_Actives.SetActive(false);
            if (aroundInteractiveObject != null)
                aroundInteractiveObject.SetActive_ConditionUI(false);
        }
    }
    public virtual void ActiveSkill() { }
    public virtual void ActiveUltSkill() { }
    private void OnClickInteractBtn() {
        if (aroundInteractiveObject != null && aroundInteractiveObject.IsAvailable())
            aroundInteractiveObject.Activate();
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
        UI_ToolBtns.SetActive(!UI_ToolBtns.activeSelf); // 도구 UI 보이기
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
        if (aroundInteractiveObject != null)
            aroundInteractiveObject.SetActive_ConditionUI(false);
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
            if (inFireCount == 1)
                AddMental(-2);
            break;

        case "Ember":
            if (inEmberCount == 1)
                AddMental(-1); // 멘탈 감소
            break;

        case "Electric":
        case "Water(Electric)":
            startTimeInElectric = Time.time;
            if (inElectricCount == 1)
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
                ChangeFloor(true);
            }
            break;
        case "DownStair":
            if (!isInStair) {
                isInStair = true;
                ChangeFloor(false);
            }
            break;
        }
    }
	protected override void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);

        switch (collision.tag) {
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

    public void ChangeFloor(bool isUp) {
        StartCoroutine(CoroutineChangeFloor(isUp));
    }
    private IEnumerator CoroutineChangeFloor(bool isUp){
        Action oldAct = _playerAct;
        _playerAct = Action.MoveFloor;
        rbody.velocity = Vector2.zero;
        StartCoroutine(GameMgr.Instance.StartLoading());
        yield return new WaitUntil(() => GameMgr.Instance.CurrentLoadingState
                                        == GameMgr.LoadingState.Stay);
        
        int floorNumber = _currentTilePos.z + ((isUp) ? 1 : -1);
        _currentTilePos.z = floorNumber;
        transform.position = TileMgr.Instance.CellToWorld(currentTilePos);
        Camera.main.GetComponent<FollowCam>().SetPosition(transform.position);

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

    protected float GetSkillUseO2() {
        float o2UseRate = 1.0f;
        if (isInGas)
            o2UseRate *= 1.5f;
        if (Act == Action.Rescue)
            o2UseRate *= 1.5f;

        return skillUseO2 * o2UseRate;
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