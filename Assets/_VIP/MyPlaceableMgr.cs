using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityRoyale;


public partial class MyPlaceable
{
    public Placeable.Faction faction = Placeable.Faction.None;

    /// <summary>
    /// 克隆一个myplaceable对象
    /// </summary>
    /// <returns></returns>
    public MyPlaceable Clone()
    {
        return this.MemberwiseClone() as MyPlaceable;
    }
}

/// <summary>
/// 游戏单位管理器
/// </summary>
public class MyPlaceableMgr : MonoBehaviour
{
    public static MyPlaceableMgr instance;

    public List<MyPlaceableView> mine = new List<MyPlaceableView>();//自己得小兵
    public List<MyPlaceableView> his = new List<MyPlaceableView>();//敌人的小兵

    

    public Transform trHisTower,trMyTower;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        his.Add(trHisTower.GetComponent<MyPlaceableView>());
        mine.Add(trMyTower.GetComponent<MyPlaceableView>());
    }

    private void Update()
    {
        UpdatePlaceable(mine);
        UpdatePlaceable(his);

        
    }

    
    private void UpdatePlaceable(List<MyPlaceableView> pViews)
    {
        for (int i = 0; i < pViews.Count; i++)
        {
            //获取的是游戏单位上面的脚本
            MyPlaceableView view = pViews[i];//游戏兵种上面的包含数据和变现相关的脚本
            MyPlaceable data = view.data;
            MyAIBase ai = view.GetComponent<MyAIBase>();//获取游戏对像上的ai脚本组件
            NavMeshAgent nav = ai.GetComponent<NavMeshAgent>();
            Animator anim = view.GetComponent<Animator>();

            switch (ai.state)
            {
                case AIState.Idle:
                    {
                        if (ai is MyBuildingAI)
                        {
                            break;
                        }

                        //找场景内最近的敌人去攻击
                        ai.target = FindNearestEnemy(ai.transform.position, data.faction);

                        if (ai.target != null)
                        {
                            print($"找到最近的角色{ai.target.gameObject.name}");

                            ai.state = AIState.Seek;

                            
                            nav.enabled = true;

                            anim.SetBool("IsMoving", true);
                        }
                        else
                        {
                            anim.SetBool("IsMoving", false);
                        }

                    }
                    break;
                case AIState.Seek:
                    {
                        if (ai.target == null)//可能出现先寻路过程中目标被别的单位打死了，而出现的空对象的现象
                        {
                            ai.state = AIState.Idle;
                            break;
                        }

                        //向敌人方向前进
                        nav.SetDestination(ai.target.transform.position);
                        //print($"目标的坐标:{trHisTower.position}");

                        //判断是否进入攻击范围
                        if (IsInAttackRange(view.transform.position, ai.target.transform.position, view.data.attackRange))
                        {
                            //若是，则停止移动，开始估计，转换为估计状态
                            nav.enabled = false;
                            ai.state = AIState.Attack;
                        }

                    }
                    break;
                case AIState.Attack:
                    {
                        if (ai.target == null)
                        {
                            ai.state = AIState.Idle;
                            break;
                        }

                        //print($"当前小兵的坐标位置：{ai.transform.position}");
                        if (IsInAttackRange(view.transform.position, ai.target.transform.position, view.data.attackRange))
                        {
                            //转换到空闲状态
                            ai.state = AIState.Idle;
                        }

                        //如果再攻击间隔内，则不攻击
                        if (Time.time < ai.lastBlowTime + data.attackRatio)
                        {
                            break;
                        }

                        //面向攻击目标
                        ai.transform.LookAt(ai.target.transform);

                        //禁止之前的行走动画
                        anim.SetBool("IsMoving", false);

                        //执行攻击动作
                        anim.SetTrigger("Attack");

                        //攻击伤害结算
                        //ai.target.GetComponent<MyPlaceableView>().data.hitPoints -= data.damagePerAttack;
                        if (ai.target.GetComponent<MyPlaceableView>().data.hitPoints <= 0)
                        {
                            OnEnterDie(ai.target);

                            ai.state = AIState.Idle;
                        }

                        //设置上一次的时间为当前时间
                        ai.lastBlowTime = Time.time;
                    }
                    break;
                case AIState.Die:
                    {
                        //anim.SetTrigger("IsDead");
                        if(ai is MyBuildingAI)
                        {
                            break;
                        }

                        var rds = ai.GetComponentsInChildren<Renderer>();
                        view.dieProgress += Time.deltaTime * (1 / view.dieDuration);
                        foreach (var rd in rds)
                        {
                            rd.material.SetFloat("_DissolveFactor", view.dieProgress);
                        }

                    }
                    break;
            }
        }
    }

    public void OnEnterDie(MyAIBase target)
    {
        print($"{target.gameObject.name} is dead!");

        //防止重复进入死亡状态
        if (target.state == AIState.Die)
        {
            return;
        }

        //停止移动
        target.GetComponent<NavMeshAgent>().enabled = false;

        //设置死亡状态
        target.GetComponent<MyAIBase>().state = AIState.Die;
        target.GetComponent<MyPlaceableView>().data.hitPoints = 0;

        if (target.GetComponent<Animator>() != null)
        {
            target.GetComponent<Animator>().SetBool("IsDead", true);
        }

        //死亡溶解
        var rds = target.GetComponentsInChildren<Renderer>();
        var view = target.GetComponent<MyPlaceableView>();
        var color = view.data.faction == Placeable.Faction.Player ? Color.red : Color.blue;
        view.dieProgress = 0;
        foreach(var rd in rds)
        {
            rd.material.SetColor("_EdgeColor", color * 8);
            rd.material.SetFloat("_EdgeWidth", 0.1f);
            rd.material.SetFloat("_DissolveFactor", view.dieProgress);
        }

        //设置对象destroy延时
        Destroy(target.gameObject, view.dieDuration);
    }

    private bool IsInAttackRange(Vector3 myPos, Vector3 enemyPos, float attackRange)
    {
        return Vector3.Distance(myPos, enemyPos) <= attackRange;
    }

    /// <summary>
    /// 此方法为玩家和敌人公用的方法，其作用是玩家找敌人，敌人找玩家，所以需要传入玩家和角色阵营
    /// </summary>
    /// <param name="myPos"></param>
    /// <param name="faction"></param>
    /// <returns></returns>
    private MyAIBase FindNearestEnemy(Vector3 myPos,Placeable.Faction faction)
    {
        List<MyPlaceableView> units = faction == Placeable.Faction.Player ? his : mine;//根据传入的阵营寻找到对面的游戏单位

        float x = float.MaxValue;
        MyAIBase nearest = null;
        foreach(MyPlaceableView unit in units)//这里只是找到了敌人列表中的敌人满足以下条件的对象来当作目标，并没有实现先找小兵然后再找塔这个操作
        {
            float d = Vector3.Distance(unit.transform.position, myPos);
            if (d < x&&unit.data.hitPoints>0)
            {
                x = d;
                nearest = unit.GetComponent<MyAIBase>();
            }
        }

        return nearest;
    }

}
