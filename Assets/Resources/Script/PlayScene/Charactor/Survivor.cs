using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class Survivor : Charactor {
    public GameObject SmileMark;

    private enum State { Panic, Static }
    [SerializeField]
    private State state;

    [SerializeField]
    private bool isImportant = false;

    protected int _carryCount = 1;

    private const int _panicMoveCount = 2;
    private float _speed = 100.0f;
    private bool _moveDone = false;

    public int CarryCount {
        get { return _carryCount; }
        set { _carryCount = value; }
    }

    protected override void Start()
    {
        base.Start();
        GameMgr.Instance.AddSurvivor(TileMgr.Instance.WorldToCell(transform.position), this);
    }

    public void TurnEndActive() {
        if (GameMgr.Instance.GameTurn % 1 == 0 && state == State.Panic) {
            _moveDone = false;
            StartCoroutine(Move());
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

    IEnumerator Move() {
        yield return null;

        for (int i = 0; i < _panicMoveCount; i++) {
            Vector3Int pPos = TileMgr.Instance.WorldToCell(transform.position);
            Vector3Int nPos;

            while (true) {
                int randx = Random.Range(-1, 2);
                int randy = Random.Range(-1, 2);
                nPos = pPos + new Vector3Int(randx, randy, 0);

                if (GameMgr.Instance.GetSurvivorAt(nPos) != null)
                    continue;
                break;
            }

            GameMgr.Instance.OnMoveSurvivor(pPos, nPos);

            Vector3 arrivePos = TileMgr.Instance.CellToWorld(nPos);

            float delta = _speed * Time.deltaTime;
            while (Vector3.Distance(arrivePos, transform.position) > delta) {
                transform.position = Vector3.MoveTowards(transform.position, arrivePos, delta);
                yield return null;
            }

            transform.position = arrivePos;
        }
        _moveDone = true;
    }

    public override void AddHP(float value)
    {
        base.AddHP(value);

        float hpRate = CurrentHP / MaxHP;
        if (hpRate <= 0)
            GameMgr.Instance.OnDieSurvivor(this);
        else if (hpRate <= 0.5) {
            _carryCount = 2;
            state = State.Static;
        }
        else
            _carryCount = 1;
    }
    public bool IsMoveDone {
        get { return _moveDone; }
	}
    public bool IsImportant {
        get { return isImportant; }
	}

    public void TurnOffRender() {
        Transform sprite = gameObject.transform.Find("Sprite");
        Transform ui = gameObject.transform.Find("UI");
        sprite.GetComponent<SpriteRenderer>().enabled = false;
        ui.gameObject.SetActive(false);
    }
}
