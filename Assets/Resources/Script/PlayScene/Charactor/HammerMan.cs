using System.Collections;
using UnityEngine;

public class HammerMan : Player
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

    public override void ActiveSkill()
    {
        base.ActiveSkill();
        if (CurrentO2 > 10) // 현재 산소가 10 이상있다면
        {
            StartCoroutine(RescueHammer()); // 스킬 발동
        }
    }

    IEnumerator RescueHammer()
    {
        Vector3Int oPos = Vector3Int.zero; // 갱신용 old Pos
        while (true) // 클릭 작용시까지 반복
        {
            RenderInteractArea(ref oPos);
            if (Input.GetMouseButtonDown(0))
            {
                GameMgr.Instance.BackTile.SetTileFlags(oPos, UnityEngine.Tilemaps.TileFlags.None);
                GameMgr.Instance.BackTile.SetColor(oPos, new Color(1, 1, 1, 1));
                if (GameMgr.Instance.Obstacle.GetTile(oPos) != null) // 클릭 좌표에 장애물이 있다면 제거
                {
                    GameMgr.Instance.Obstacle.SetTile(oPos, null);
                    AddO2(-10);
                    if (GameMgr.Instance.RescueTilemap.GetTile(oPos - GameMgr.Instance.RescueTilemap.WorldToCell(transform.position)) != null)
                    {
                        _playerAct = Action.Panic; // 턴제한 추가 필요
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

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }
}