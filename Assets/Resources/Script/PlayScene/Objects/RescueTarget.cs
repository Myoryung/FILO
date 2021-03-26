using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class RescueTarget : Charactor {
    public GameObject SmileMark;

    private enum _state { Panic, Static }
    private _state _RescueTargetState;

    protected int _carryCount = 1;

    private int _panicMoveCount = 2;
    private float _speed = 100.0f;
    private bool _moveDone = false;

    public int CarryCount {
        get { return _carryCount; }
        set { _carryCount = value; }
    }

    protected override void Start()
    {
        base.Start();

        GameMgr.Instance.AddRescueTarget(TileMgr.Instance.WorldToCell(transform.position), this);
    }

    public void TurnEndActive() {
        if (GameMgr.Instance.GameTurn % 1 == 0 && _RescueTargetState == _state.Panic) {
            _moveDone = false;
            StartCoroutine(Move());
        }
        else
            _moveDone = true;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Fire") || other.CompareTag("Ember"))
            AddHP(-25.0f);
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

                if (GameMgr.Instance.GetRescueTargetAt(nPos) != null)
                    continue;
                break;
            }

            GameMgr.Instance.MoveRescueTarget(pPos, nPos);

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
            Destroy(gameObject);
        else if (hpRate <= 0.5) {
            _carryCount = 2;
            _RescueTargetState = _state.Static;
        }
        else
            _carryCount = 1;
    }
    public bool IsMoveDone {
        get { return _moveDone; }
	}

    public void TurnOffRender() {
        Transform rt = gameObject.transform.Find("RescueTarget");
        Transform canvas = gameObject.transform.Find("Canvas");
        rt.GetComponent<SpriteRenderer>().enabled = false;
        canvas.gameObject.SetActive(false);

    }
}
