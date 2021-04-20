using UnityEngine;

public class Captain : Player {

    public const int OPERATOR_NUMBER = 0;
    public override int OperatorNumber {
        get { return OPERATOR_NUMBER; }
    }

    Vector3Int SkillRange = new Vector3Int(7, 7, 0); // 스킬 범위
    Vector3Int OldPos;

	protected override void Start() {
		base.Start();
        OldPos = currentTilePos;
    }

	protected override void Update()
    {
        base.Update();
        ActiveSkill();
    }

    public override void StageStartActive() {
        TurnOnWarning();
    }

    public override void ActiveSkill() {
        if (OldPos != currentTilePos) {
            TurnOffWarning();
            TurnOnWarning();

            OldPos = currentTilePos;
        }
    }
    public override void ActiveUltSkill() {
    }

    private void TurnOnWarning() {
        for (int i = -(SkillRange.x/2); i < (Mathf.Ceil(SkillRange.x)/2); i++) {
            for (int j = -(SkillRange.y/2); j<(Mathf.Ceil(SkillRange.y)/2); j++) {
                Vector3Int SearchPos = currentTilePos + new Vector3Int(i, j, 0);
                TileMgr.Instance.TurnWarning(SearchPos, true);
            }
        }
    }
    private void TurnOffWarning() {
        Vector3Int nPos = OldPos - currentTilePos;
        nPos.z = currentTilePos.z;
        
        int clearArea;
        if (nPos.x != 0) {
            clearArea = (int)(Mathf.Ceil(SkillRange.x) / 2);
            for (int i = -clearArea; i <= clearArea; i++) {
                Vector3Int SearchPos = OldPos + new Vector3Int(clearArea * nPos.x, i, 0);
                TileMgr.Instance.TurnWarning(SearchPos, false);
            }
        }
        if (nPos.y != 0) {
            clearArea = (int)(Mathf.Ceil(SkillRange.y) / 2);
            for (int i = -clearArea; i <= clearArea; i++) {
                Vector3Int SearchPos = OldPos + new Vector3Int(i, clearArea * nPos.y, 0);
                TileMgr.Instance.TurnWarning(SearchPos, false);
            }
        }
    }
}