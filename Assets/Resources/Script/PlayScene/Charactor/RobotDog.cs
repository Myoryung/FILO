using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDog : Charactor
{
    public enum DogAction { Idle, Walk, Carry, Rescue }
    private DogAction _dogAct;
    private Survivor _rescuingSurvivor; // 현재 구조중인 생존자

    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        FullO2();
        _dogAct = DogAction.Idle;
    }

    public virtual void StartRescue() // Rescue가 IEnumrator가 되며 버튼용 함수 추가
    {
        UI_Actives.SetActive(false);
        StartCoroutine(Rescue());
    }

    public override void Move()
    {
        base.Move();
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        if (CurrentO2 <= 0 || _dogAct == DogAction.Carry ||
            (hor == 0.0f && ver == 0.0f))
        {
            // 이동 종료
            rbody.velocity = Vector3.zero;
            
            return;
        }

        Vector3 dir = new Vector3(hor, ver, 0.0f);
        dir /= dir.magnitude;
        rbody.velocity = dir * _movespeed;

        _currentTilePos = TileMgr.Instance.WorldToCell(transform.position);

        bool isRight = hor > 0;
        if (isRight)
        {
            if (_body.rotation.y != 180)
                _body.rotation = new Quaternion(0, 180.0f, 0, transform.rotation.w);
        }
        else
        {
            if (_body.rotation.y != 0)
                _body.rotation = Quaternion.identity;
        }
        AddO2(-UseO2 * Time.deltaTime);
        // 트리거 실행
        GameMgr.Instance.OnMovePlayer(currentTilePos);
    }

    public override void Activate()
    {
        base.Activate();
        if (Input.GetMouseButtonUp(1))
        {
            if (UI_Actives.activeSelf) UI_Actives.SetActive(false);
            else UI_Actives.SetActive(true);
        }
    }

    IEnumerator Rescue() // 구조 버튼 누를 시 호출되는 함수
    {
        Vector3Int nPos = currentTilePos;
        while (true)
        {
            RenderInteractArea(ref nPos); // 구조 영역선택
            if (Input.GetMouseButtonDown(0))
            {
                Survivor survivor = GameMgr.Instance.GetSurvivorAt(nPos);
                if (survivor != null)
                {
                    GameMgr.Instance.OnCarrySurvivor(nPos);
                    _rescuingSurvivor = survivor;
                    _dogAct = DogAction.Carry; // 업는 상태로 변경
                }
                break;
            }
            else if (Input.GetMouseButtonDown(1) || IsMoving) // 움직일 시 취소
                break;

            yield return null;
        }

        TileMgr.Instance.RemoveEffect(nPos); // 구조 영역 선택한거 원상복구
    }
}
