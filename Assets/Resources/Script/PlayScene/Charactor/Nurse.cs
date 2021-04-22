using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Nurse : Player {

    public const int OPERATOR_NUMBER = 3;
    public override int OperatorNumber {
        get { return OPERATOR_NUMBER; }
    }

    protected override void Awake()
    {
        base.Awake();
        cutSceneIlust = Resources.Load<Sprite>("Sprite/OperatorSelect_UI/Operator/Operator3");
        ultName = "회복 드론";
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

    public override void ActiveUltSkill()
    {
        base.ActiveUltSkill();
        Action oldact = _playerAct;
        StartCoroutine(ShowCutScene());
        StartCoroutine(HealDrone(oldact));
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

    IEnumerator HealDrone(Action act)
    {
        while(true)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = transform.position.z;

                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector3.forward, 100.0f);
                if(hit)
                {
                    if(hit.transform.CompareTag("Player"))
                    {
                        Player target = hit.transform.GetComponent<Player>();
                        target.AddHP(target.MaxHP / 2);
                        target.AddO2(target.MaxO2 / 2);
                        //카메라 타겟 연출 추가 필요
                        //카메라 복귀 연출 추가 필요
                        break;
                    }
                }
            }
            yield return null;
        }
        _playerAct = act;
    }
}