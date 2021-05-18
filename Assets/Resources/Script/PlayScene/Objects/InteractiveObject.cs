using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractiveObject : MonoBehaviour
{
    protected Vector3Int tilePos;
    protected int floor;
    private bool IsBeenUsed = false;

    private GameObject conditionCanvas;
    protected string conditionText;
    private bool isConditionInit = false;

	protected virtual void Start() {
        conditionCanvas = transform.Find("Canvas").gameObject;
        conditionCanvas.SetActive(false);
        if (conditionText != null) {
            conditionText = "조건\n" + conditionText;

            Image image = conditionCanvas.transform.Find("Image").GetComponent<Image>();
            Text text = image.transform.Find("Text").GetComponent<Text>();

            text.text = conditionText;
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
            image.rectTransform.sizeDelta = new Vector2(text.preferredWidth + 20, text.preferredHeight + 10);

            isConditionInit = true;
        }

        floor = transform.parent.parent.GetComponent<Floor>().floor;
        tilePos = TileMgr.Instance.WorldToCell(transform.position, floor);
    }

    public virtual bool IsActive() {
        return !IsBeenUsed;
    }
    public virtual bool IsAvailable() {
        return IsActive();
    }

    public void SetActive_ConditionUI(bool value) {
        if (isConditionInit)
            conditionCanvas.SetActive(value);
    }

    public virtual void Activate() {
        if (!IsAvailable()) return;
        IsBeenUsed = true;
    }
}
