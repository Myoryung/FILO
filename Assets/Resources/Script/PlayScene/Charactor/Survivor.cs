using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Survivor : Charactor {
    public GameObject SmileMark;

    private enum Type { Panic, Static }
    [SerializeField]
    private Type type;

    public enum State { Idle, Carried, Rescued }
    private State state = State.Idle;
    public State CurrState {
        get { return state; }
    }

    [SerializeField]
    private bool isImportant = false;

    protected const int IDLE_CARRY_COUNT = 0, INJURY_CARRY_COUNT = 2;
    protected int _carryCount = 0;

    private const int _panicMoveCount = 2;
    private float _speed = 100.0f;
    private bool _moveDone = false;
    public GameObject body = null;

    protected Animator _anim;

    public int CarryCount {
        get { return _carryCount; }
        set { _carryCount = value; }
    }

    protected override void Start()
    {
        base.Start();
        _anim = GetComponentInChildren<Animator>();
        if (type == Type.Static) _anim.SetTrigger("Static");
        floor = transform.parent.parent.GetComponent<Floor>().floor;
        _currentTilePos = TileMgr.Instance.WorldToCell(transform.position, floor);
        GameMgr.Instance.AddSurvivor(this);
    }

    public override void TurnEndActive() {
        if (GameMgr.Instance.GameTurn % 1 == 0 && type == Type.Panic && state == State.Idle) {
            _moveDone = false;
            StartCoroutine(MoveTile());
        }
        else
            _moveDone = true;
    }

    public void ActiveSmileMark()
    {
        StartCoroutine(DisableSmileMarkCounter());
    }

    IEnumerator DisableSmileMarkCounter()
    {
        SmileMark.SetActive(true);
        int oldGameTurn = GameMgr.Instance.GameTurn;
        yield return new WaitWhile(() => GameMgr.Instance.GameTurn - oldGameTurn < 2);
        SmileMark.SetActive(false);
    }

    IEnumerator MoveTile() {
        yield return null;
        _anim.SetBool("IsRunning", true);
        for (int i = 0; i < _panicMoveCount; i++) {
            Vector3Int pPos = TileMgr.Instance.WorldToCell(transform.position, floor);
            Vector3Int nPos;

            while (true) {
                int randx = Random.Range(-1, 2);
                int randy = Random.Range(-1, 2);
                nPos = pPos + new Vector3Int(randx, randy, 0);

                if (GameMgr.Instance.GetSurvivorAt(nPos, floor) != null)
                    continue;
                break;
            }

            Vector3 arrivePos = TileMgr.Instance.CellToWorld(nPos, floor);

            float delta = _speed * Time.deltaTime;
            while (Vector3.Distance(arrivePos, transform.position) > delta) {
                transform.position = Vector3.MoveTowards(transform.position, arrivePos, delta);
                yield return null;
            }

            transform.position = arrivePos;
            _currentTilePos = nPos;

            // Sorting Order 수정
            int order = TileMgr.GetOrder(transform.position, _currentTilePos);
            GetComponent<SortingGroup>().sortingOrder = order;
        }
        _anim.SetBool("IsRunning", false);
        _moveDone = true;
    }

    public override void AddHP(float value)
    {
        base.AddHP(value);

        float hpRate = CurrentHP / MaxHP;
        if (hpRate <= 0)
            GameMgr.Instance.OnDieSurvivor(this);
        else if (hpRate <= 0.5)
        {
            type = Type.Static;
            _anim.SetTrigger("Static");
        }
    }
    public bool IsMoveDone {
        get { return _moveDone; }
	}
    public bool IsImportant {
        get { return isImportant; }
	}

    public void OnStartCarried() {
        _carryCount = (CurrentHP / MaxHP > 0.5) ? IDLE_CARRY_COUNT : INJURY_CARRY_COUNT;
        state = State.Carried;
    }
    public void OnStopCarried() {
        state = State.Idle;
    }
    public void OnStartRescued() {
        body.SetActive(false);
        transform.Find("UI").gameObject.SetActive(false);
        state = State.Rescued;
    }
    public void OnStopRescued(Vector3Int pos, int floor) {
        List<Vector3Int> candidates = new List<Vector3Int>();
        for (int y = -1; y <= 1; y++) {
            for (int x = -1; x <= 1; x++) {
                if (x == 0 && y == 0) continue;
                Vector3Int tempPos = pos + new Vector3Int(x, y, 0);
                if (!TileMgr.Instance.ExistObject(tempPos, floor))
                    candidates.Add(tempPos);
            }
        }

        int index = Random.Range(0, candidates.Count);
        Vector3Int targetPos = candidates[index];

        this.floor = floor;
        _currentTilePos = targetPos;
        transform.position = TileMgr.Instance.CellToWorld(_currentTilePos, floor);

        body.SetActive(true);
        transform.Find("UI").gameObject.SetActive(true);
        state = State.Idle;
    }
}
