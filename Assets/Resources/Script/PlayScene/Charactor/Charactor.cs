using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Charactor : MonoBehaviour {
    [SerializeField]
    protected Image HPGage; // 체력
    [SerializeField]
    protected Image O2Gage; // 산소

    public GameObject UI_Actives; // 행동 버튼 UI

    [SerializeField]
    private float _maxo2 = 0.0f; // 캐릭터 최대 산소량
    private float _currento2 = 0.0f; // 캐릭터 현재 산소량
    [SerializeField]
    private float _useo2 = 0.0f; // 1초에 사용되는 산소량
    [SerializeField]
    private float _maxHp = 0.0f; // 최대 체력
    private float _currentHp = 0.0f; // 현재 체력
    [SerializeField]
    protected float _movespeed = 0.0f; // 캐릭터 이동속도
    [SerializeField]
    protected Transform _body = null; // 캐릭터 이미지의 Transform
    protected Rigidbody2D rbody = null;

    protected int inFireCount = 0, inEmberCount = 0, inElectricCount = 0;

    protected int floor;
    public int Floor {
        set { floor = value; }
        get { return floor; }
    }
    protected Vector3Int _currentTilePos = Vector3Int.zero; // 현재 캐릭터의 타일맵 좌표
    public Vector3Int currentTilePos
    {
        get { return _currentTilePos; }
    }

    protected virtual void Start()
    {
        _currento2 = _maxo2;
        _currentHp = _maxHp;
        rbody = GetComponent<Rigidbody2D>();
    }

    public virtual void Move() { }

    public virtual void Activate()
    {

    }

    public virtual void StageStartActive() { }

    public virtual void TurnEndActive() { }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        switch (collision.tag) {
        case "Fire":
            if (inFireCount++ == 0)
                AddHP(-25.0f);
            break;

        case "Ember":
            if (inEmberCount++ == 0)
                AddHP(-10.0f);
            break;

        case "Electric":
        case "Water(Electric)":
            if (inElectricCount++ == 0)
                AddHP(-35.0f);
            break;

        case "Disaster_FallingRock":
            AddHP(-40);
            break;
        case "Disaster_Smoke":
            AddO2(-30);
            break;
        }
    }
    protected virtual void OnTriggerExit2D(Collider2D collision) {
        switch (collision.tag) {
        case "Fire":
            inFireCount--;
            break;

        case "Ember":
            inEmberCount--;
            break;

        case "Electric":
        case "Water(Electric)":
            inElectricCount--;
            break;
        }
    }

    public virtual void AddHP(float value) {
        _currentHp += value;

        if (CurrentHP < 0)
            _currentHp = 0;
        else if (CurrentHP > MaxHP)
            FullHp();

        if (HPGage != null)
            HPGage.fillAmount = CurrentHP / MaxHP; // 체력 UI 변화
    }

    public virtual void AddO2(float value) {
        _currento2 += value;

        if (CurrentO2 < 0)
            _currento2 = 0;
        else if (CurrentO2 > MaxO2)
            FullO2();

        if (O2Gage != null)
            O2Gage.fillAmount = CurrentO2 / MaxO2; // 산소 UI 변화
    }

    protected virtual void FullO2() { _currento2 = MaxO2; }
    protected virtual void FullHp() { _currentHp = MaxHP; }

    public float MaxO2 {
        get { return _maxo2; }
    }
    public float CurrentO2 {
        get { return _currento2; }
    }
    public float UseO2 {
        get { return _useo2; }
    }
    public float MaxHP {
        get { return _maxHp; }
    }
    public float CurrentHP {
        get { return _currentHp; }
    }
}
