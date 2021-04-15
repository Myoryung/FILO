using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabletFloor : MonoBehaviour {
    private static readonly Color NORMAL_COLOR = new Color32(255, 255, 255, 255);
    private static readonly Color HIGHLIGHT_COLOR = new Color32(216, 54, 21, 255);
    private static readonly Color DISABLE_COLOR = new Color32(77, 77, 77, 255);

    private Text floorText, numberText;

	private void Awake() {
        floorText = GetComponent<Text>();
        numberText = transform.Find("Number").GetComponent<Text>();

        SetNormal();
	}

    public void SetFloorNumber(int floor) {
        numberText.text = floor.ToString();

        GetComponent<Button>().onClick.RemoveAllListeners();
		GetComponent<Button>().onClick.AddListener(() => GameMgr.Instance.OnClickTabletFloor(floor));
	}

    public void SetNormal() {
        floorText.color = NORMAL_COLOR;
        numberText.color = NORMAL_COLOR;
    }
    public void SetHighlight() {
        floorText.color = HIGHLIGHT_COLOR;
		numberText.color = HIGHLIGHT_COLOR;
	}
    public void SetDisable() {
        floorText.color = DISABLE_COLOR;
        numberText.color = DISABLE_COLOR;
        GetComponent<Button>().enabled = false;
	}
}
