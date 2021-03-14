﻿using UnityEngine;
using System.Collections;

public class Nurse : Player
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void Move()
    {
        base.Move();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }

    public override void ActiveSkill()
    {
        base.ActiveSkill();
        if (CurrentO2 > 15) // 현재 산소가 10 이상있다면
        {
            StartCoroutine(Heal()); // 스킬 발동
        }
    }

    IEnumerator Heal()
    {
        Vector3Int oPos = Vector3Int.zero; // 갱신용 old Pos
        while (true) // 클릭 작용시까지 반복
        {
            RenderInteractArea(ref oPos);
            if (Input.GetMouseButtonDown(0))
            {
                GameMgr.Instance.BackTile.SetTileFlags(oPos, UnityEngine.Tilemaps.TileFlags.None);
                GameMgr.Instance.BackTile.SetColor(oPos, new Color(1, 1, 1, 1));
                foreach(Player player in GameMgr.Instance.Comp_Players)
                {
                    if(player.currentTilePos == oPos)
                    {
                        player.AddHP(30.0f);
                        player.AddO2(20.0f);
                        AddO2(-15);
                        break;
                    }
                }
                break;
            }
            else if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                GameMgr.Instance.BackTile.SetTileFlags(oPos, UnityEngine.Tilemaps.TileFlags.None);
                GameMgr.Instance.BackTile.SetColor(oPos, new Color(1, 1, 1, 1));
                break;
            }
            yield return null;
        }
    }
}