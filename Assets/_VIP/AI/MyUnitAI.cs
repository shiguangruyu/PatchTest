using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyUnitAI : MyAIBase
{
    //public GameObject projectilePrefab;
    public AssetReference projectilePrefab;//addrressables系統引用對象不使用gameobject
    public Transform firePos;

    public void OnDealDamage()
    {
        if(this.target==null)
        {
            return;
        }

        this.target.GetComponent<MyPlaceableView>().data.hitPoints -= this.GetComponent<MyPlaceableView>().data.damagePerAttack;
        if (this.target.GetComponent<MyPlaceableView>().data.hitPoints < 0)
        {
            this.target.GetComponent<MyPlaceableView>().data.hitPoints = 0;

            MyPlaceableMgr.instance.OnEnterDie(this.target);

            this.target = null;
        }
    }

    public async void OnFireProjectile()
    {
        //实例化法师火球
        GameObject go = await Addressables.InstantiateAsync(
            projectilePrefab,
            firePos.position,
            Quaternion.identity,
            MyProjectileMgr.instance.transform
            ).Task;//把投掷物放在手部位置，但是不是以手为父节点，不跟手移动
        //设置投掷物的释放者
        go.GetComponent<MyProjectile>().caster = this;
        go.GetComponent<MyProjectile>().target = this.target;
        //go.GetComponent<MyProjectile>().target = this;

        //投掷物的飞行被myplaceablemgr统一管理
        MyProjectileMgr.instance.mine.Add(go.GetComponent<MyProjectile>());
    }
}
