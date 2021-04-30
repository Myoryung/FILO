using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDog : Player
{
    public const int OPERATOR_NUMBER = 4;
    public override int OperatorNumber
    {
        get { return OPERATOR_NUMBER; }
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }
    private void OnEnable()
    {
        FullO2();
    }
}
