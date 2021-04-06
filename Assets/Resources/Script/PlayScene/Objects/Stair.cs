using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair : MonoBehaviour
{
    public bool isUpper;
    private Vector3Int _position;
    public Vector3Int Position { get { return _position; } }
    private Vector3Int _portalPosition;
    public Vector3Int PortalPosition { get { return _portalPosition; } }
    private int _flag = 0;
    public int Flag { get { return _flag; } }

    private void Start()
    {
        if (isUpper) _flag = 1;
        else _flag = -1;
        //transform.Translate(0, 0, 2.5f + (-Flag * 0.5f));
        //TileMgr.Instance.SetStair((int)transform.position.z, isUpper, this);
        //_position = TileMgr.Instance.WorldToCell(transform.position);
    }
}
