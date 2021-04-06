using UnityEngine;

public class Rescuer : Player
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

    public override void ActiveSkill() {
        base.ActiveSkill();
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

    protected override void Move()
    {
        base.Move();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }
}