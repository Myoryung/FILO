using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class RescueTarget : Charactor {
    public GameObject SmileMark;

    private enum _state { Panic, Static }
    private _state _RescueTargetState;

    protected int _rescueCount = 0;

    private int _panicMoveCount = 2;
    private float _speed = 100.0f;
    private bool _moveDone = false;

    public int RescueCount
    {
        get { return _rescueCount; }
        set { _rescueCount = value; }
    }

    protected override void Start()
    {
        base.Start();

        switch(_RescueTargetState)
        {
            case _state.Panic:
                _rescueCount = 1;
                break;
            case _state.Static:
                _rescueCount = 2;
                break;
        }
    }

    public void TurnEndActive() {
        if (GameMgr.Instance.GameTurn % 1 == 0 && _RescueTargetState == _state.Panic) {
            _moveDone = false;
            StartCoroutine(Move());
        }
        else
            _moveDone = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fire"))
        {
            AddHP(-25.0f);
            if(CurrentHP / MaxHP < 0.5f && _RescueTargetState == _state.Panic)
            {
                _rescueCount++;
                _RescueTargetState = _state.Static;
            }
        }
        else if (other.CompareTag("Ember"))
        {
            AddHP(-25.0f);
            if (CurrentHP / MaxHP < 0.5f && _RescueTargetState == _state.Panic)
            {
                _rescueCount++;
                _RescueTargetState = _state.Static;
            }
        }
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
            Vector3Int pPos = GameMgr.Instance.BackTile.WorldToCell(transform.position);
            Vector3Int nPos;

            while (true) {
                int randx = Random.Range(-1, 2);
                int randy = Random.Range(-1, 2);
                nPos = pPos + new Vector3Int(randx, randy, 0);

                if (TileMgr.Instance.RescueTargets.ContainsKey(nPos))
                    continue;

                break;
            }

            TileMgr.Instance.RescueTargets.Remove(pPos);
            TileMgr.Instance.RescueTargets.Add(nPos, this);

            Vector3 arrivePos = GameMgr.Instance.BackTile.CellToWorld(nPos) + (GameMgr.Instance.BackTile.cellSize / 2);

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

        if (CurrentHP <= 0)
            Destroy(gameObject);
    }
    public bool IsMoveDone {
        get { return _moveDone; }
	}
}
