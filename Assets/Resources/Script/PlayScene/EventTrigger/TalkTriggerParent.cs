using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkTriggerParent : MonoBehaviour
{
    public TalkTrigger[] triggers;
    [SerializeField]
    private int talkID;
    [SerializeField]
    private bool isDone = false;
    public void TriggerConditionCheck()
    {
        bool flag = true;
        for(int i=0; i<triggers.Length; i++)
        {
            if (!triggers[i].isDone)
            {
                flag = false;
                break;
            }
        }
        if(flag && !isDone)
        {
            StartCoroutine(TalkMgr.Instance.StartTalk(talkID));
            isDone = true;
        }
    }
}
