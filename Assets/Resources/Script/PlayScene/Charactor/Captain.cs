using UnityEngine;
using System.Collections;

public class Captain : Player {

    public const int OPERATOR_NUMBER = 0;
    public override int OperatorNumber {
        get { return OPERATOR_NUMBER; }
    }

    Vector3Int SkillRange = new Vector3Int(7, 7, 0); // 스킬 범위
    Vector3Int OldPos;

    protected override void Awake()
    {
        base.Awake();
        cutSceneIlust = Resources.Load<Sprite>("Sprite/PlayScene/UI/CutScene/Ultimate_leader");
        ultName = "광범위 소화지원";
    }

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

    public override void ActiveUltSkill()
    {
        base.ActiveUltSkill();
        Action oldact = playerAct;
        StartCoroutine(ShowCutScene());
        StartCoroutine(MassExtinguish(oldact));
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
    IEnumerator MassExtinguish(Action act)
    {
        Vector3Int ultSkillRange = new Vector3Int(3, 3, 0);
        for (int i = -ultSkillRange.x; i <= ultSkillRange.x; i++)
        {
            for (int j = -ultSkillRange.y; j <= ultSkillRange.y; j++)
            {
                TileMgr.Instance.SetEffect(_currentTilePos + new Vector3Int(i, j, 0), new Color(1, 1, 0, 0.3f));
            }
        }
        while (true)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            Vector3Int moustIntPos = TileMgr.Instance.WorldToCell(mousePos); // 타일맵에서 마우스 좌표
            // 마우스 위에 사용 시 적용범위 타일 표시
            if (Input.GetMouseButton(0))
            {

                Vector3 localPos = mousePos - transform.position; // 마우스 캐릭터 기준 로컬좌표
                if (_currentTilePos.x - moustIntPos.x <= ultSkillRange.x &&
                    _currentTilePos.x - moustIntPos.x >= -ultSkillRange.x &&
                    _currentTilePos.y - moustIntPos.y <= ultSkillRange.y &&
                    _currentTilePos.y - moustIntPos.y >= -ultSkillRange.y) // 누른 위치가 캐릭터 기준 2칸 이내라면
                {
                    Vector2Int SpreadRange = new Vector2Int(1, 1); // 불 제거 범위
                    for (int i = -SpreadRange.x; i <= SpreadRange.x; i++)
                    {
                        for (int j = -SpreadRange.y; j < SpreadRange.y; j++)
                        {
                            Vector3Int fPos = moustIntPos + new Vector3Int(i, j, 0); // 탐색할 타일 좌표
                            TileMgr.Instance.RemoveFire(fPos);
                        }
                    }
                }
                break;
            }
            yield return null;
        }
        playerAct = act;
        for (int i = -ultSkillRange.x; i <= ultSkillRange.x; i++)
        {
            for (int j = -ultSkillRange.y; j <= ultSkillRange.y; j++)
            {
                TileMgr.Instance.SetEffect(_currentTilePos + new Vector3Int(i, j, 0), new Color(1, 1, 1, 0));
            }
        }
    }
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        switch (other.tag)//대장 피격소리
        {
            case "Fire":
                SoundManager.instance.PlayLeaderHurt();
                break;
            case "Ember":
                SoundManager.instance.PlayLeaderHurt();
                break;
            case "Electric":
            case "Water(Electric)":
                SoundManager.instance.PlayLeaderHurt();
                break;
        }
    }
}