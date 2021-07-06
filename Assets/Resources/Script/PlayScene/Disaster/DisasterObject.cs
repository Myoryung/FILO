using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DisasterObject : MonoBehaviour {
    protected Sprite[] sprites;
    protected float frameInterval;

    private SpriteRenderer spriteRenderer;
    private float accumTime = 0;
    private int spriteIndex = 0;

    BoxCollider2D boxCollider;

    private bool bActive = false;
    private int floor;
    public int Floor {
        get { return floor; }
    }
    private Vector3Int _pos;
    public Vector3Int Pos {
        set { _pos = value; }
        get { return _pos; }
	}

    // Start is called before the first frame update
    protected virtual void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[0];

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
            boxCollider.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        if (bActive) return;

        accumTime += Time.deltaTime;
        spriteIndex = Mathf.FloorToInt(accumTime / frameInterval);

        if (spriteIndex >= sprites.Length) {
            Active();
            bActive = true;
        }
        else
            spriteRenderer.sprite = sprites[spriteIndex];
    }

    protected virtual void Active() {
        if (boxCollider != null) {
            boxCollider.enabled = true;
            boxCollider.isTrigger = true;
        }
    }

    public bool IsActive {
        get { return bActive; }
    }

    public void OnDisasterTalkTrigger(int ID)
    {
        StartCoroutine(TalkMgr.Instance.StartTalk(ID));
    }
}
