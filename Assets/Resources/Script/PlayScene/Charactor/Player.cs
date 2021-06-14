using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class Player : Charactor
{
    // UI 및 타일 정보
    public Image MTGage; // 멘탈
    public GameObject UI_ToolBtns; // 도구 버튼 UI
    private GameObject CutScene; // 궁극기 컷씬
    protected Sprite cutSceneIlust = null;
    protected string ultName = null;

    // 플레이어 스테이터스
    public enum Action { Idle, Walk, Run, Carry, Rescue, ActiveUlt, Interact, Panic, Retire, MoveFloor } // 캐릭터 행동 상태 종류
    private Action _playerAct;

    private Survivor _rescuingSurvivor; // 현재 구조중인 생존자
    private InteractiveObject aroundInteractiveObject;

    [SerializeField]
    private float _maxMental = 0; // 최대 멘탈
    private float _currentMental = 0; // 현재 멘탈

    [SerializeField]
    private float skillUseO2; // 스킬 발동 시 사용되는 산소량
    private const float O2_ADDED_PER_TURN = 10.0f; // 턴 종료시마다 회복되는 산소량
    protected bool isUsedUlt = false;
    protected bool isOverComeTrauma = false;
    protected int overcomeTraumaCount = 0;

    // 타일 충돌체크용 값
    private bool isInSafetyArea = false, isInGas = false, isInStair = false;
    private float startTimeInFire = 0, startTimeInElectric = 0;

	// Local Component
	protected Animator _anim; // 캐릭터 애니메이션
    
    public virtual int OperatorNumber {
        get { return -1; }
    }

    protected virtual void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();

        _anim = GetComponentInChildren<Animator>();
        _currentMental = _maxMental;
        //SetFOV();

        CutScene = GameObject.Find("MiddleUI").transform.Find("CutScene").gameObject;
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

    public override void StageStartActive() {
        _currentTilePos = TileMgr.Instance.WorldToCell(transform.position, floor);
    }
    public override void TurnEndActive() // 캐릭터가 턴이 끝날 때 호출되는 함수
    {
        if (playerAct != Action.Panic) { // 패닉 상태는 산소가 회복되지 않는다
            if (IsInSafetyArea)
                AddO2(O2_ADDED_PER_TURN * 2.0f);
            else
                AddO2(O2_ADDED_PER_TURN);
        }

        if (playerAct == Action.Carry) // 업는 중이라면
        {
            _rescuingSurvivor.CarryCount--;
            if (_rescuingSurvivor.CarryCount <= 0) // 업는턴 값이 0보다 작으면
            {
                _rescuingSurvivor.OnStartRescued();
                playerAct = Action.Rescue; // Rescue 상태로 변경
            }
        }

        // 애니메이션 종료
        if (_anim != null)
        {
            if (_anim.GetBool("IsRunning"))
                _anim.SetBool("IsRunning", false);
        }

        // UI 비활성화
        InactiveUIBtns();
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
        if (_anim != null)
        {
            if (_anim.GetBool("IsRunning"))
                _anim.SetBool("IsRunning", false);
        }

        // UI 비활성화
        InactiveUIBtns();
    }

    public override void Move() {
        base.Move();
        float hor = Input.GetAxisRaw("Horizontal"); // 가속도 없는 Raw값 사용
        float ver = Input.GetAxisRaw("Vertical");

        if (CurrentHP <= 0 || CurrentO2 <= 0 || _currentMental <= 0 ||
            playerAct == Action.Carry || playerAct == Action.MoveFloor ||
            playerAct == Action.ActiveUlt || (hor == 0.0f && ver == 0.0f)) {

            // 이동 종료
            rbody.velocity = Vector3.zero;
            

            // 애니메이션 종료
            if (_anim != null)
            {
                if (_anim.GetBool("IsRunning"))
                {
                    _anim.SetBool("IsRunning", false);
                    SoundManager_Walk.instance.StopSound();//걷는 소리 정지
                }
            }
            return;
        }

        // 캐릭터 이동
        Vector3 dir = new Vector3(hor, ver, 0.0f);
        dir /= dir.magnitude;
        rbody.velocity = dir * _movespeed;

        _currentTilePos = TileMgr.Instance.WorldToCell(transform.position, floor);

        // Sorting Order 수정
        int order = TileMgr.GetOrder(transform.position, _currentTilePos);
        GetComponent<SortingGroup>().sortingOrder = order;

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
        if (_anim != null)
        {
            if (!_anim.GetBool("IsRunning"))
            {
                _anim.SetBool("IsRunning", true);
                SoundManager_Walk.instance.PlayWalkSound();//걷는소리 재생
            }
        }

        // 산소 소비
        float o2UseRate = 1.0f;
        if (isInGas)
            o2UseRate *= 1.5f;
        if (playerAct == Action.Rescue)
            o2UseRate *= 1.5f;

        AddO2(-UseO2 * o2UseRate * Time.deltaTime);

        // 트리거 실행
        GameMgr.Instance.OnMovePlayer(currentTilePos, floor);
    }

    public override void Activate() { // 행동 UI (구조, 도구사용)
        if (Input.GetMouseButtonUp(1)) { // 마우스 우클릭 시 UI 표시
            if (UI_Actives.activeSelf) { // 이미 켜져있었다면 UI 끄기
                InactiveUIBtns();
            }
            else {
                UI_Actives.SetActive(true);
            }
        }
        UI_Actives.transform.Find("RescueBtn").GetComponent<Button>().interactable = (_rescuingSurvivor == null);

        if (OperatorNumber != RobotDog.OPERATOR_NUMBER)
        {
            InteractiveObject interactiveObject = SearchAroundInteractiveObject(currentTilePos);
            if (interactiveObject != null && interactiveObject.IsAvailable())
                SetActive_InteractBtn(true);
            else
                SetActive_InteractBtn(false);


            if (UI_Actives.activeSelf)
            {
                if (aroundInteractiveObject != null)
                    aroundInteractiveObject.SetActive_ConditionUI(false);

                if (interactiveObject != null && interactiveObject.IsActive())
                    interactiveObject.SetActive_ConditionUI(true);
            }
            aroundInteractiveObject = interactiveObject;
        }
    }
    protected InteractiveObject SearchAroundInteractiveObject(Vector3Int pos) {
        InteractiveObject activeObject = null;

        // 주변에 있는 상호작용 오브젝트 탐색
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                Vector3Int targetPos = pos + new Vector3Int(x, y, 0);
                InteractiveObject ino = TileMgr.Instance.GetInteractiveObject(targetPos, floor);
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
        case 4: ActiveUltSkill(); break;
        }

        if (number != 2) {
            UI_Actives.SetActive(false);
            if (aroundInteractiveObject != null)
                aroundInteractiveObject.SetActive_ConditionUI(false);
        }
    }
    public virtual void ActiveSkill() { }
    public virtual void ActiveUltSkill() {
    }
    private void OnClickInteractBtn() {
        if (aroundInteractiveObject != null && aroundInteractiveObject.IsAvailable())
            aroundInteractiveObject.Activate();
    }
    protected IEnumerator ShowCutScene()
    {
        playerAct = Action.ActiveUlt;
        CutScene.transform.Find("Ilustration").GetComponent<Image>().sprite = cutSceneIlust;
        CutScene.transform.GetChild(0).Find("UltText").GetComponent<Text>().text = ultName;
        CutScene.SetActive(true);
        CutScene.GetComponent<Animator>().SetBool("IsActive", true);
        yield return new WaitForSeconds(3.0f);
        CutScene.GetComponent<Animator>().SetBool("IsActive", false);
        CutScene.SetActive(false);
    }
    protected virtual IEnumerator Rescue() // 구조 버튼 누를 시 호출되는 함수
    {
        InteractEffector effector = new InteractEffector(currentTilePos, Floor, InteractEffector.Type.Cross, 1);
        effector.Enable();

        while (true) {
            Vector3Int mousePos = GetMousePosOnTilemap();
            Survivor survivor = GameMgr.Instance.GetSurvivorAt(mousePos, floor);
            bool isPossible = effector.IsInArea(mousePos) && survivor != null;

            effector.Set(mousePos, isPossible);

            if (Input.GetMouseButtonDown(0)) {
                if (isPossible) {
                    _rescuingSurvivor = survivor;
                    _rescuingSurvivor.OnStartCarried();
                    GameMgr.Instance.OnCarrySurvivor(survivor);
                    playerAct = Action.Carry; // 업는 상태로 변경
                    _anim.SetBool("IsRescue", true);

                    if (_rescuingSurvivor.CarryCount <= 0) {
                        _rescuingSurvivor.OnStartRescued();
                        playerAct = Action.Rescue; // Rescue 상태로 변경
                        _anim.SetBool("IsRescue", false);
                    }
                }
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving) // 움직일 시 취소
                break;

            yield return null;
        }

        effector.Disable();
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
        _anim.SetBool("IsFireExt", true);
        InteractEffector effector = new InteractEffector(currentTilePos, Floor, InteractEffector.Type.Diamond, 2);
        effector.Enable();

        //좌표 값 변경으로 인해 수정해야 할 코드
        while (true) {
            Vector3Int mousePos = GetMousePosOnTilemap();
            bool isPossible = effector.IsInArea(mousePos) && TileMgr.Instance.ExistFire(mousePos, floor);
            effector.Set(mousePos, isPossible);

            if (Input.GetMouseButton(0)) {
                if (isPossible) {
                    TileMgr.Instance.RemoveFire(mousePos, floor);
                    GameMgr.Instance.OnUseTool();
                }
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving) // 움직이면 취소
                break;
            yield return null;
        }

        effector.Disable();
        _anim.SetBool("IsFireExt", false);
    }
    IEnumerator UseFireWall() // 방화벽 설치
    {
        InteractEffector effector = new InteractEffector(currentTilePos, Floor, InteractEffector.Type.Cross, 1);
        effector.Enable();

        while (true) {
            Vector3Int mousePos = GetMousePosOnTilemap();
            bool isPossible = effector.IsInArea(mousePos) && !TileMgr.Instance.ExistObject(mousePos, floor)
                && GameMgr.Instance.GetPlayersAt(mousePos, Floor).Count == 0 && GameMgr.Instance.GetSurvivorAt(mousePos, Floor) == null;
            effector.Set(mousePos, isPossible);

            if (Input.GetMouseButtonDown(0)) {
                if (isPossible) {
                    TileMgr.Instance.CreateFireWall(mousePos, floor);
                    GameMgr.Instance.OnUseTool();
                }
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving)
                break;
            yield return null;
        }

        effector.Disable();
    }
    IEnumerator UseStickyBomb() // 점착폭탄 설치
    {
        InteractEffector effector = new InteractEffector(currentTilePos, Floor, InteractEffector.Type.Cross, 1);
        effector.Enable();

        while (true) {
            Vector3Int mousePos = GetMousePosOnTilemap();
            bool isPossible = effector.IsInArea(mousePos) && TileMgr.Instance.ExistTempWall(mousePos, Floor);
            effector.Set(mousePos, isPossible);

            if (Input.GetMouseButtonDown(0)) {
                if (isPossible) {
                    TileMgr.Instance.RemoveTempWall(mousePos, floor);
                    _anim.SetTrigger("Throw");
                    GameMgr.Instance.OnUseTool();
                }
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving)
                break;
            yield return null;
        }

        effector.Disable();
    }

    private void UseO2Can() // 산소캔 사용
    {
        AddO2(45.0f);
        _anim.SetTrigger("UseAirDrink");
        GameMgr.Instance.OnUseTool();
        SoundManager.instance.PlayAirCanUse();
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
            if (playerAct == Action.Rescue) {
                GameMgr.Instance.OnRescueSurvivor(_rescuingSurvivor);
                _rescuingSurvivor = null;
                playerAct = Action.Idle;
                if(OperatorNumber == Nurse.OPERATOR_NUMBER){
                    overcomeTraumaCount++;
                    if (overcomeTraumaCount >= 3)
                    {
                        isOverComeTrauma = true;
                    }
                }
            }
            break;

        case "UpStair":
                SoundManager.instance.PlayStairChange();
                if (!isInStair) {
                isInStair = true;
                ChangeFloor(true);
            }
            break;
        case "DownStair":
                SoundManager.instance.PlayStairChange();
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


    protected class InteractEffector {
        public enum Type { Cross, Diamond };
        private static readonly Color COLOR_BG = new Color(0, 0, 1, 0.2f);
        private static readonly Color COLOR_POSSIBLE = new Color(0, 1, 0, 0.4f);
        private static readonly Color COLOR_IMPOSSIBLE = new Color(1, 0, 0, 0.4f);

        public Vector3Int pos;
        public int floor;
        public Type type;
        public int size;

        private Vector3Int prevMousePos;
        private bool existPrevMousePos;

        public InteractEffector(Vector3Int pos, int floor, Type type, int size) {
            this.pos = pos;
            this.floor = floor;
            this.type = type;
            this.size = size;
        }


        public void Enable() {
            switch (type) {
            case Type.Cross:    Enable_Cross();     break;
            case Type.Diamond:  Enable_Diamond();   break;
            }

            existPrevMousePos = false;
        }
        public void Disable() {
            switch (type) {
            case Type.Cross:    Disable_Cross();    break;
            case Type.Diamond:  Disable_Diamond();  break;
            }
        }

        public void Set(Vector3Int mousePos, bool isPossible) {
            if (existPrevMousePos) {
                if (prevMousePos == mousePos)
                    return;

                TileMgr.Instance.SetEffect(prevMousePos, floor, COLOR_BG);
                existPrevMousePos = false;
            }

            if (IsInArea(mousePos)) {
                TileMgr.Instance.SetEffect(mousePos, floor, isPossible ? COLOR_POSSIBLE : COLOR_IMPOSSIBLE);
                prevMousePos = mousePos;
                existPrevMousePos = true;
            }
        }

        private void Enable_Cross() {
            Vector3Int xPos = pos, yPos = pos;
            xPos.x -= size;
            yPos.y -= size;

            for (int i = -size; i <= size; i++) {
                TileMgr.Instance.SetEffect(xPos, floor, COLOR_BG);
                TileMgr.Instance.SetEffect(yPos, floor, COLOR_BG);

                xPos.x++;
                yPos.y++;
            }
        }
        private void Enable_Diamond() {
            Vector3Int tPos = pos;
            tPos.y -= size;

            for (int i = -size, len = 0; i <= size; i++) {
                tPos.x = pos.x - len;

                for (int j = -len; j <= len; j++) {
                    TileMgr.Instance.SetEffect(tPos, floor, COLOR_BG);

                    tPos.x++;
                }

                tPos.y++;

                if (i < 0)
                    len++;
                else
                    len--;
            }
        }
        private void Disable_Cross() {
            Vector3Int xPos = pos, yPos = pos;
            xPos.x -= size;
            yPos.y -= size;

            for (int i = -size; i <= size; i++) {
                TileMgr.Instance.RemoveEffect(xPos, floor);
                TileMgr.Instance.RemoveEffect(yPos, floor);

                xPos.x++;
                yPos.y++;
            }
        }
        private void Disable_Diamond() {
            Vector3Int tPos = pos;
            tPos.y -= size;

            for (int i = -size, len = 0; i <= size; i++) {
                tPos.x = pos.x - len;

                for (int j = -len; j <= len; j++) {
                    TileMgr.Instance.RemoveEffect(tPos, floor);

                    tPos.x++;
                }

                tPos.y++;

                if (i < 0)
                    len++;
                else
                    len--;
            }
        }

        public bool IsInArea(Vector3Int tPos) {
            Vector3Int diff = tPos - pos;

            switch (type) {
            case Type.Cross:
                return (diff.x == 0 && Mathf.Abs(diff.y) <= size)
                    || (diff.y == 0 && Mathf.Abs(diff.x) <= size);
            case Type.Diamond:
                return Mathf.Abs(diff.x) + Mathf.Abs(diff.y) <= size;
            }

            return false;
        }
    }

    protected bool IsMoving
    { // 현재 움직이는 상태인가 체크하는 함수
        get { return (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0); }
    }

    protected Vector3Int GetMousePosOnTilemap() {
        return TileMgr.Instance.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition), Floor);
    }

    public void ChangeFloor(bool isUp) {
        StartCoroutine(CoroutineChangeFloor(isUp));
    }
    private IEnumerator CoroutineChangeFloor(bool isUp){
        Action oldAct = playerAct;
        playerAct = Action.MoveFloor;
        rbody.velocity = Vector2.zero;
        StartCoroutine(GameMgr.Instance.StartLoading());
        yield return new WaitUntil(() => GameMgr.Instance.CurrentLoadingState
                                        == GameMgr.LoadingState.Stay);
        
        floor += (isUp) ? 1 : -1;
        transform.position = TileMgr.Instance.CellToWorld(currentTilePos, floor);
        Camera.main.GetComponent<FollowCam>().SetPosition(transform.position);

        TileMgr.Instance.SwitchFloorTilemap(floor);
        GameMgr.Instance.CurrentLoadingState = GameMgr.LoadingState.End;
        playerAct = oldAct;
    }

    private void InactiveUIBtns()
    {
        if (UI_Actives != null)
            UI_Actives.SetActive(false);
        if (UI_ToolBtns != null)
            UI_ToolBtns.SetActive(false);
        if (aroundInteractiveObject != null)
            aroundInteractiveObject.SetActive_ConditionUI(false);
    }

    public override void AddHP(float value) {
        base.AddHP(value);
        if(value < 0)
            _anim.SetTrigger("Hit");

        if (CurrentHP <= 0 && OperatorNumber != RobotDog.OPERATOR_NUMBER)
        {
            if (playerAct != Action.Retire)
            {
                _anim.SetBool("PassOut", true);
            }
            playerAct = Action.Retire;
            rbody.velocity = Vector2.zero;
            if(OperatorNumber != Nurse.OPERATOR_NUMBER)
            {
                GameMgr.Instance.InvokeNurseTrauma();
            }
        }
    }
    public override void AddO2(float value) {
        base.AddO2(value);

        if (CurrentO2 <= 0 && OperatorNumber != RobotDog.OPERATOR_NUMBER)
        {
            playerAct = Action.Retire;
            if (OperatorNumber == Rescuer.OPERATOR_NUMBER)
            {
                overcomeTraumaCount++;
                if (overcomeTraumaCount >= 2)
                    isOverComeTrauma = true;
            }
        }
    }
    private void AddMental(int value) {
        _currentMental += value;
        if(_currentMental <= 0 && OperatorNumber != RobotDog.OPERATOR_NUMBER)
        {
            playerAct = Action.Panic;
            rbody.velocity = Vector2.zero;
        }
        if(_currentMental > _maxMental)
        {
            _currentMental = _maxMental;
        }
    }

    public Action CurrAct {
        get { return playerAct; }
    }
    protected Action playerAct {
        get { return _playerAct; }
        set {
            switch (value) {
            case Action.Retire:
            case Action.Panic:
                if (playerAct == Action.Carry) {
                    GameMgr.Instance.OnStopCarrySurvivor(_rescuingSurvivor);
                    _rescuingSurvivor = null;
                }
                else if (playerAct == Action.Rescue) {
                    GameMgr.Instance.OnStopRescueSurvivor(this, _rescuingSurvivor);
                    _rescuingSurvivor = null;
                }
                break;
            }
            _playerAct = value;
        }
    }

    protected float GetSkillUseO2() {
        float o2UseRate = 1.0f;
        if (isInGas)
            o2UseRate *= 1.5f;
        if (playerAct == Action.Rescue)
            o2UseRate *= 1.5f;

        return skillUseO2 * o2UseRate;
    }

    public float Mental {
        get { return _currentMental; }
	}
    public bool IsInSafetyArea {
        get { return isInSafetyArea; }
	}
    public bool IsOverComeTrauma
    {
        get { return isOverComeTrauma; }
    }

    public bool EqualsRescuingSurvivor(Survivor survivor) {
        return _rescuingSurvivor == survivor;
    }
    public void OnDieRescuingSurvivor() {
        _rescuingSurvivor = null;
        playerAct = Action.Idle;
    }

}