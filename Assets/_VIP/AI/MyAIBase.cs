using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AIState
{
    Idle,
    Seek,
    Attack,
    Die,
}

public class MyAIBase : MonoBehaviour
{
    public AIState state = AIState.Idle;

    public MyAIBase target = null;

    public float lastBlowTime = 0;//上次估计时间

    public virtual void OnIdle() { }

    public virtual void OnSeeking() { }

    public virtual void OnAttack() { }

    public virtual void OnDie() { }
}
