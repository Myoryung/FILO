﻿using System.Collections;
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
        if (GameMgr.Instance.GameTurn % 1 == 0 && _RescueTargetState == _state.Panic)
            StartCoroutine(Move());
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

    IEnumerator Move()
    {
        int randx = 0;
        int randy = 0;
        while (true)
        {
            randx = Random.Range(-1, 2);
            randy = Random.Range(-1, 2);
            if (randx == 0 && randy == 0)
            {
                continue;
            }
            else
            {
                break;
            }
        }
        Vector3Int rPos = new Vector3Int(randx, randy, 0);
        Vector3Int nPos = GameMgr.Instance.BackTile.WorldToCell(transform.position) + rPos;
        Vector3 arrivePos = GameMgr.Instance.BackTile.CellToWorld(nPos) - (GameMgr.Instance.BackTile.cellSize / 2);
        while (_panicMoveCount > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, arrivePos, _speed * Time.deltaTime);
            yield return null;
            if (Vector3.Distance(arrivePos, transform.position) < 10f)
            {
                while (true)
                {
                    randx = Random.Range(-1, 2);
                    randy = Random.Range(-1, 2);
                    if (randx == 0 && randy == 0)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                transform.position = new Vector3(arrivePos.x, arrivePos.y);
                rPos = new Vector3Int(randx, randy, 0);
                nPos = GameMgr.Instance.BackTile.WorldToCell(transform.position) + rPos;
                arrivePos = GameMgr.Instance.BackTile.CellToWorld(nPos) - (GameMgr.Instance.BackTile.cellSize / 2);

                _panicMoveCount--;
            }
        }
        TileMgr.Instance.RescueTargets.Remove(GameMgr.Instance.BackTile.WorldToCell(transform.position));
        TileMgr.Instance.RescueTargets.Add(GameMgr.Instance.BackTile.WorldToCell(transform.position), this);
        _panicMoveCount = 2;
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
