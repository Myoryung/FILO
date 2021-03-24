using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public GameObject[] Players; // 타겟들의 배열
    [SerializeField]
    private Transform Target; // 현재 따라가는 타켓

    private void LateUpdate()
    {
        if (Target != null)
        {
            Vector3 pos = Target.position;
            this.transform.position = Vector3.Lerp(this.transform.position, pos, 3.0f * Time.deltaTime); // 카메라 이동
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -100.0f);
        }
    }

    public void ChangeChar(bool isRight) // 타겟 변경
    {
        if(isRight && GameMgr.Instance.CurrentChar < 3) // 오른쪽 클릭 시 마지막 번호가 아니면
        {
            GameMgr.Instance.CurrentChar++;
        }
        else if(!isRight && GameMgr.Instance.CurrentChar > 0) // 왼쪽 클릭 시 마지막 번호가 아니면
        {
            GameMgr.Instance.CurrentChar--;
        }
        Target = Players[GameMgr.Instance.CurrentChar].transform;
    }
}
