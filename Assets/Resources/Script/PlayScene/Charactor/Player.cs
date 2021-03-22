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

    // 플레이어 스테이터스
    //안녕안녕?
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
            _currentTilePos = TileMgr.Instance.WorldToCell(worldPos); // 현재 캐릭터의 타일맵 좌표 갱신
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
                _playerAct = Action.Rescue; // 구조 상태로 변경
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

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int nPos = TileMgr.Instance.WorldToCell(mousePos); // 마우스의 타일맵 상 좌표

        // 누른 위치가 캐릭터 기준 2칸 밖이면 종료
        if (Mathf.Abs(_currentTilePos.x - nPos.x) > 2 || Mathf.Abs(_currentTilePos.y - nPos.y) > 2)
            yield break;

        int startX = (transform.position.x < mousePos.x) ? nPos.x : nPos.x-1;
        int startY = (transform.position.y < mousePos.y) ? nPos.y : nPos.y-1;
        int endX = startX + 1;
        int endY = startY + 1;

        // 불 존재 확인
        bool existFire = false;
        for (int y = startY; y <= endY; y++) {
            for (int x = startX; x <= endX; x++) {
                Vector3Int tPos = new Vector3Int(x, y, 0);
                if (TileMgr.Instance.ExistFire(tPos)) {
                    existFire = true;
                    break;
                }
            }

            if (existFire) break;
        }

        if (!existFire)
            yield break;

        // 불 삭제
        for (int y = startY; y <= endY; y++) {
            for (int x = startX; x <= endX; x++) {
                Vector3Int tPos = new Vector3Int(x, y, 0);
                TileMgr.Instance.RemoveFire(tPos);
            }
        }
    }

    private void UseFireWall() // 방화벽 설치
    {
        Vector3Int nPos = TileMgr.Instance.WorldToCell(transform.position) + new Vector3Int((int)_moveDir.x, 0, 0);
        TileMgr.Instance.CreateFireWall(nPos);
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
                Vector3Int nPos = TileMgr.Instance.WorldToCell(transform.position) + new Vector3Int((int)_moveDir.x, 0, 0);
                TileMgr.Instance.RemoveWall(nPos);
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
    }

    protected void RenderInteractArea(ref Vector3Int oPos) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position; // 마우스 로컬 좌표
        Vector3Int direction; // 캐릭터 기준 마우스 방향
        if (Mathf.Abs(mousePos.x) > Mathf.Abs(mousePos.y))
            direction = (mousePos.x > 0) ? Vector3Int.right : Vector3Int.left;
        else
            direction = (mousePos.y > 0) ? Vector3Int.up : Vector3Int.down;

        Vector3Int nPos = currentTilePos + direction; // 새 좌표 갱신
        if (nPos != oPos) { // 기존의 렌더부분과 갱신된 부분이 다르면
            TileMgr.Instance.SetTileColor(oPos, Color.white);   // 기존의 좌표 색 복구
            TileMgr.Instance.SetTileColor(nPos, Color.blue);    // 새로운 좌표 색 변경
            oPos = nPos;
        }
    }

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