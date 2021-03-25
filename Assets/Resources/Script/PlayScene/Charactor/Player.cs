﻿using System.Collections;
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
    public enum Action { Idle, Walk, Run, Rescue, Interact, Panic, Retire } // 캐릭터 행동 상태 종류
    protected Action _playerAct; 
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
        if (CurrentO2 > 0.0f && GameMgr.Instance.CurrentChar == _playerNum && CurrentHP > 0.0f && _playerAct != Action.Rescue && _currentMental > 0)
        {
            transform.Translate(hor * Time.deltaTime * _movespeed, ver * Time.deltaTime * _movespeed, 0.0f);

            // 플레이어 이동 시 작은 불 이동
            if (hor != 0 || ver != 0)
                TileMgr.Instance.MoveEmbers();

            if (hor != 0.0f) // 좌, 우 이동중이라면
            {
                AddO2(-(UseO2 * Time.deltaTime));
                _moveDir = new Vector3(hor, 0, 0); // 바라보는 방향 변경
            }
            if (ver != 0.0f) //상, 하 이동중이라면
                AddO2(-(UseO2 * Time.deltaTime));

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

            _currentTilePos = TileMgr.Instance.WorldToCell(transform.position); // 현재 캐릭터의 타일맵 좌표 갱신
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
        if (_playerAct != Action.Panic) // 패닉 상태는 산소가 회복되지 않는다
            AddO2(10.0f);

        if(_playerAct == Action.Rescue) // 구조중이라면
        {
            _rescueTarget.RescueCount--; // 구조중인 대상의 남은 구조턴 감소
            if(_rescueTarget.RescueCount <= 0) // 구조턴 값이 0보다 작으면
            {
                _rescueTarget.gameObject.SetActive(false); // 구조 대상 숨기기
                _playerAct = Action.Idle; // Idle 상태로 변경
            }
        }
    }

    public virtual void ActiveSkill()
    {
        UI_Actives.SetActive(false);
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
        UI_Actives.SetActive(false); // UI 숨기기

        Vector3Int nPos = Vector3Int.zero;
        while (true) {
            RenderInteractArea(ref nPos); // 구조 영역선택
            if (Input.GetMouseButtonDown(0)) {
                int RescueLayer = 1 << LayerMask.NameToLayer("Rescue"); // 생존자의 Layer
                RaycastHit2D hit = Physics2D.Raycast(TileMgr.Instance.CellToWorld(_currentTilePos),
                    (Vector3)GetMouseDirectiontoTilemap(), 128, RescueLayer); // 레이캐스트 쏘기
                if (hit) {
                    if (hit.transform.CompareTag("RescueTarget")) // 레이캐스트 충돌 대상이 구조대상이라면
                    {
                        _rescueTarget = hit.transform.GetComponent<RescueTarget>(); // 생존자 값 저장
                        _playerAct = Action.Rescue; // 구조 상태로 변경
                    }
                }
                break;
            }
            else if (IsMoving) // 움직일 시 취소
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
                Vector3 localPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position; // 마우스 캐릭터 기준 로컬좌표
                Vector3Int moustIntPos = TileMgr.Instance.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)); // 타일맵에서 마우스 좌표
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
            else if (IsMoving) // 움직이면 취소
                break;
            yield return null;
        }
    }
    IEnumerator UseFireWall() // 방화벽 설치
    {
        Vector3Int nPos = Vector3Int.zero;
        while (true) {
            RenderInteractArea(ref nPos);
            if (Input.GetMouseButtonDown(0)) {
                TileMgr.Instance.CreateFireWall(nPos);
                break;
            }
            else if (IsMoving)
                break;
            yield return null;
        }

        TileMgr.Instance.RemoveEffect(nPos);
    }
    IEnumerator UseStickyBomb() // 점착폭탄 설치
    {
        Vector3Int nPos = Vector3Int.zero;
        while (true) {
            RenderInteractArea(ref nPos);
            if (Input.GetMouseButtonDown(0)) {
                TileMgr.Instance.RemoveWall(nPos);
                break;
            }
            else if (IsMoving)
                break;
            yield return null;
        }

        TileMgr.Instance.RemoveEffect(nPos);
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

        if (CurrentHP <= 0 )
            _playerAct = Action.Retire;
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