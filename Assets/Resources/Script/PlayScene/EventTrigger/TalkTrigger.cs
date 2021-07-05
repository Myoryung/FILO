using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkTrigger : MonoBehaviour
{
    public bool isDone;
    public enum TriggerTarget { Captain, HammerMan, Rescuer, Nurse, Anybody }
    [SerializeField]
    private TriggerTarget target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            if (target == TriggerTarget.Anybody)
            {
                isDone = true;
                GetComponentInParent<TalkTriggerParent>().TriggerConditionCheck();
            }
            else if(collision.GetComponent<Player>().OperatorNumber == (int)target)
            {
                isDone = true;
                GetComponentInParent<TalkTriggerParent>().TriggerConditionCheck();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(isDone)
        {
            isDone = false;
        }
    }
}
