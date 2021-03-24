using System.Collections;
using UnityEngine;

public class HammerMan : Player
{
    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
    }

    protected override void Move() {
        base.Move();
    }

    public override void ActiveSkill() {
        base.ActiveSkill();
        if (CurrentO2 > 10) // 현재 산소가 10 이상있다면
            StartCoroutine(RescueHammer()); // 스킬 발동
    }

    IEnumerator RescueHammer() {
        Vector3Int oPos = Vector3Int.zero; // 갱신용 old Pos
        UI_Actives.SetActive(false); // UI 숨기기

        while (true) { // 클릭 작용시까지 반복
            RenderInteractArea(ref oPos);
            if (Input.GetMouseButtonDown(0))
            {
                TileMgr.Instance.RemoveEffect(oPos);
                if (TileMgr.Instance.ExistObject(oPos)) { // 클릭 좌표에 장애물이 있다면 제거
                    TileMgr.Instance.RemoveObject(oPos);
                    AddO2(-10);
                    if (GameMgr.Instance.GetRescueTargetAt(oPos - TileMgr.Instance.WorldToCell(transform.position)))
                        _playerAct = Action.Panic; // 턴제한 추가 필요
                }
                break;
            }
            else if (IsMoving) {
                break;
            }

            yield return null;
        }

        TileMgr.Instance.RemoveEffect(oPos);
    }

    protected override void OnTriggerEnter2D(Collider2D other) {
        base.OnTriggerEnter2D(other);
    }
}