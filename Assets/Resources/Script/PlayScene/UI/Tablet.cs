using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tablet {
	private Image tabletCodeTop, tabletCodeBottom;
	private const float TABLET_CODE_MOVE_SPEED = 0.005f;
	private Vector3 tabletCodeInitPos;

	private Image tabletRecord;
	private const float TABLET_RECORD_TURN_PERIOD = 1.0f;
	private float tabletRecordTurnTime = 0.0f;

	public Tablet() {
		GameObject tabletUI = GameObject.Find("UICanvas/SelectCanvas/Tablet/UI");

		tabletCodeTop = tabletUI.transform.Find("CodeTop").GetComponent<Image>();
		tabletCodeBottom = tabletUI.transform.Find("CodeBottom").GetComponent<Image>();
		tabletCodeInitPos = tabletCodeBottom.rectTransform.localPosition;

		tabletRecord = tabletUI.transform.Find("Record").GetComponent<Image>();
	}

	public void Update() {
        // 레코드 점멸
        float currTime = Time.time;
        if (currTime - tabletRecordTurnTime >= TABLET_RECORD_TURN_PERIOD) {
            tabletRecord.enabled = !tabletRecord.enabled;
            tabletRecordTurnTime = currTime;
        }

        // 코드 이동
        float codeHeight = tabletCodeBottom.rectTransform.rect.height;
        float moveAmount = codeHeight * TABLET_CODE_MOVE_SPEED;
        tabletCodeBottom.fillAmount -= TABLET_CODE_MOVE_SPEED;
        tabletCodeTop.fillAmount += TABLET_CODE_MOVE_SPEED;
        tabletCodeBottom.rectTransform.localPosition += new Vector3(0, moveAmount);
        tabletCodeTop.rectTransform.localPosition += new Vector3(0, moveAmount);

        if (tabletCodeBottom.fillAmount <= 0) {
            tabletCodeBottom.rectTransform.localPosition = tabletCodeInitPos;
            tabletCodeTop.rectTransform.localPosition = tabletCodeInitPos;
            tabletCodeBottom.fillAmount = 1;
            tabletCodeTop.fillAmount = 0;
        }
    }
}
