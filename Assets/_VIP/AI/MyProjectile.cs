using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProjectile : MonoBehaviour
{
    public MyAIBase caster;//投掷物的拥有者
    public MyAIBase target;//投掷物释放目标

    public float speed = 3;//子弹飞行速度

    public float progress;//飞行进度
}
