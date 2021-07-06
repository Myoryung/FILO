using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Nurse : Player {

    public const int OPERATOR_NUMBER = 3;
    private GameObject Heal_UI;
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
        Heal_UI = GameObject.Find("MiddleUI").transform.Find("Heal_UI").gameObject;
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
        if (isUsedUlt)
            return;
        Action oldact = playerAct;
        StartCoroutine(ShowCutScene());
        StartCoroutine(HealDrone(oldact));
    }

    protected override void RescueSuccess() {
        base.RescueSuccess();
        
        overcomeTraumaCount++;
        if (overcomeTraumaCount >= 3)
            isOverComeTrauma = true;
    }

    IEnumerator Heal() {
        UI_Actives.SetActive(false); // UI 숨기기

        InteractEffector effector = new InteractEffector(currentTilePos, Floor, InteractEffector.Type.Cross, 1);
        effector.Enable();

        while (true) { // 클릭 작용시까지 반복
            Vector3Int mousePos = GetMousePosOnTilemap();
            List<Player> targetPlayers = GameMgr.Instance.GetPlayersAt(mousePos, floor);
            bool isPossible = effector.IsInArea(mousePos) && targetPlayers.Count > 0;
            effector.Set(mousePos, isPossible);

            if (Input.GetMouseButtonDown(0)) {
                if (isPossible) {
                    foreach (Player targetPlayer in targetPlayers) {
                        targetPlayer.AddHP(30.0f);
                        targetPlayer.AddO2(20.0f);

                        // 산소 소비
                        AddO2(-GetSkillUseO2());
                        break;
                    }
                }
                break;
            }
            else if (IsMoving)
                break;

            yield return null;
        }

        effector.Disable();
    }

    IEnumerator HealDrone(Action act)
    {
        Heal_UI.SetActive(true);
        while(!isUsedUlt)
        {
            yield return null;
        }
        Heal_UI.SetActive(false);
        playerAct = act;
    }
}