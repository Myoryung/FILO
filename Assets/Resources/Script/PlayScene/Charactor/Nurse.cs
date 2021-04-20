using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Nurse : Player {

    public const int OPERATOR_NUMBER = 3;
    public override int OperatorNumber {
        get { return OPERATOR_NUMBER; }
    }

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

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }

    public override void ActiveSkill() {
        if (CurrentO2 >= GetSkillUseO2())
            StartCoroutine(Heal()); // 스킬 발동
    }
    public override void ActiveUltSkill() {
    }

    IEnumerator Heal() {
        Vector3Int oPos = currentTilePos; // 갱신용 old Pos
        UI_Actives.SetActive(false); // UI 숨기기

        while (true) { // 클릭 작용시까지 반복
            RenderInteractArea(ref oPos);
            if (Input.GetMouseButtonDown(0)) {
                List<Player> players = GameMgr.Instance.GetPlayersAt(oPos);
                foreach (Player player in players) {
                    player.AddHP(30.0f);
                    player.AddO2(20.0f);

                    // 산소 소비
                    AddO2(-GetSkillUseO2());
                    break;
                }
                break;
            }
            else if (IsMoving)
                break;

            yield return null;
        }

        TileMgr.Instance.RemoveEffect(oPos);
    }
}