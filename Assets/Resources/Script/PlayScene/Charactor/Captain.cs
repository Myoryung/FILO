using UnityEngine;

public class Captain : Player {
    Vector3Int SkillRange = new Vector3Int(7, 7, 0); // 스킬 범위
    Vector3Int OldPos;

	protected override void Update()
    {
        base.Update();
        ActiveSkill();
    }

    public override void StageStartActive() {
        TurnOnWarning();
    }

    public override void ActiveSkill() {
        base.ActiveSkill();

        if (OldPos != currentTilePos) {
            TurnOffWarning();
            TurnOnWarning();

            OldPos = currentTilePos;
        }
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
        int ClearArea = 0;
        if (nPos.x != 0) {
            ClearArea = (int)(Mathf.Ceil(SkillRange.x) / 2);
            for (int i = -ClearArea; i <= ClearArea; i++) {
                Vector3Int SearchPos = OldPos + new Vector3Int(ClearArea * nPos.x, i, 0);
                TileMgr.Instance.TurnWarning(SearchPos, false);
            }
        }
        if (nPos.y != 0) {
            ClearArea = (int)(Mathf.Ceil(SkillRange.y) / 2);
            for (int i = -ClearArea; i <= ClearArea; i++) {
                Vector3Int SearchPos = OldPos + new Vector3Int(i, ClearArea * nPos.y, 0);
                TileMgr.Instance.TurnWarning(SearchPos, false);
            }
        }
    }
}