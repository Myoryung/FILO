using UnityEngine;
using UnityEngine.UI;
using System;

public enum Tool { FIREWALL, FIRE_EX, O2_CAN, STICKY_BOMB, FLARE };

class PlayerToolMgr {
    public static readonly GameObject FIREWALL_BTN      = Resources.Load<GameObject>("Prefabs/UI/ToolButtons/FirewallBtn");
    public static readonly GameObject FIRE_EX_BTN       = Resources.Load<GameObject>("Prefabs/UI/ToolButtons/FireExBtn");
    public static readonly GameObject O2_CAN_BTN        = Resources.Load<GameObject>("Prefabs/UI/ToolButtons/O2CanBtn");
    public static readonly GameObject STICKY_BOMB_BTN   = Resources.Load<GameObject>("Prefabs/UI/ToolButtons/StickyBombBtn");
    public static readonly GameObject FLARE_BTN         = Resources.Load<GameObject>("Prefabs/UI/ToolButtons/FlareBtn");

    private static readonly float START_Y = 123, INTERVER_Y = -73;

    public static void AddToolBtn(Transform UI_tools, Action<Tool> callback, Tool tool) {
        // 프리팹 선정
        GameObject prefab = null;
        switch (tool) {
        case Tool.FIREWALL:     prefab = FIREWALL_BTN;      break;
        case Tool.FIRE_EX:      prefab = FIRE_EX_BTN;       break;
        case Tool.O2_CAN:       prefab = O2_CAN_BTN;        break;
        case Tool.STICKY_BOMB:  prefab = STICKY_BOMB_BTN;   break;
        case Tool.FLARE:        prefab = FLARE_BTN;   break;
        }

        // 위치 계산
        Vector3 pos = UI_tools.position;
        pos.y += (START_Y + INTERVER_Y*UI_tools.childCount) / 100.0f;

        // 오브젝트 생성
        GameObject btnObj = GameObject.Instantiate(prefab, pos, Quaternion.identity, UI_tools);

        // 콜백 함수 설정
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => callback(tool));
    }

    public static string ToString(Tool tool) {
        switch (tool) {
        case Tool.FIREWALL:     return "방화벽";
        case Tool.FIRE_EX:      return "소화기";
        case Tool.FLARE:        return "조명탄";
        case Tool.O2_CAN:       return "산소캔";
        case Tool.STICKY_BOMB:  return "점착폭탄";
        }

        return null;
    }
}
