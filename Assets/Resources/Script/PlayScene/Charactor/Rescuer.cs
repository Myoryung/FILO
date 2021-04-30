using UnityEngine;

public class Rescuer : Player {

    public const int OPERATOR_NUMBER = 2;
    public override int OperatorNumber {
        get { return OPERATOR_NUMBER; }
    }

    [SerializeField]
    private GameObject robotDogPrefab = null;
    private GameObject robotDog = null;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        cutSceneIlust = Resources.Load<Sprite>("Sprite/OperatorSelect_UI/Operator/Operator2");
        robotDog = Instantiate<GameObject>(robotDogPrefab, transform.position, Quaternion.identity);
        //강아지 여러번 생성되는거 방지해야함
        ultName = "도와줘 멍멍아";
    }
    protected override void Start()
    {
        base.Start();
        //강아지 할당 robotDog
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
        if (TileMgr.Instance.ExistObject(_currentTilePos + Vector3Int.right))
        {
            return;
        }
        Action oldact = playerAct;
        StartCoroutine(ShowCutScene());
        SpawnRobotDog();
        playerAct = oldact;
        isUsedUlt = true;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }

    private void SpawnRobotDog()
    {
        robotDog.transform.position = TileMgr.Instance.CellToWorld(_currentTilePos + Vector3Int.right);
        GameMgr.Instance.InsertRobotDogInPlayerList(robotDog.GetComponent<RobotDog>(), this);
        robotDog.SetActive(true);
    }
}