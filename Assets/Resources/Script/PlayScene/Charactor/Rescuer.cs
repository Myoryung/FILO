using UnityEngine;

public class Rescuer : Player {

    public const int OPERATOR_NUMBER = 2;
    public override int OperatorNumber {
        get { return OPERATOR_NUMBER; }
    }

    [SerializeField]
    private GameObject robotDot;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        cutSceneIlust = Resources.Load<Sprite>("Sprite/OperatorSelect_UI/Operator/Operator2");
        ultName = "도와줘 멍멍아";
    }
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void ActiveSkill() {
        if (CurrentO2 < GetSkillUseO2())
            return;

        int range = 5;
        int offset = -(range / 2);
        Vector3Int nPos = TileMgr.Instance.WorldToCell(transform.position);
        for(int i = offset; i < range + offset; i++) {
            for(int j = offset; j < range + offset; j++) {
                Vector3Int targetPos = nPos + new Vector3Int(i, j, 0);
                Survivor survivor = GameMgr.Instance.GetSurvivorAt(targetPos);
                if (survivor != null)
                    survivor.ActiveSmileMark();
            }
        }

        AddO2(-GetSkillUseO2());
    }

    public override void ActiveUltSkill()
    {
        base.ActiveUltSkill();
        //if(TileMgr ~ 오른쪽 타일 체크)
        Action oldact = _playerAct;
        StartCoroutine(ShowCutScene());
        _playerAct = oldact;
        isUsedUlt = true;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }

    private void SpawnRobotDog()
    {
        robotDot.transform.position = TileMgr.Instance.CellToWorld(_currentTilePos + Vector3Int.right);
        robotDot.SetActive(true);
    }
}