using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProjectileMgr : MonoBehaviour
{
    public static MyProjectileMgr instance;

    public List<MyProjectile> mine = new List<MyProjectile>();//我的投掷物列表
    public List<MyProjectile> his = new List<MyProjectile>();//他的投掷物列表

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        //子弹更新
        UpdateProjectiles(mine);
        UpdateProjectiles(his);
    }

    private void UpdateProjectiles(List<MyProjectile> projList)
    {
        List<MyProjectile> destroyProjList = new List<MyProjectile>();

        for (int i = 0; i < projList.Count; i++)
        {
            var proj = projList[i];

            MyUnitAI casterAI = proj.caster as MyUnitAI;
            MyAIBase targetAI = proj.target;

            proj.progress += Time.deltaTime * proj.speed;

            if (proj.target == null)
            {
                Destroy(proj.gameObject);
                destroyProjList.Add(proj);
                continue;
            }

            proj.transform.position = Vector3.Lerp(proj.caster.transform.position + Vector3.up, proj.target.transform.position + Vector3.up, proj.progress);

            if (proj.progress >= 1.0f)
            {
                casterAI.OnDealDamage();

                if (targetAI.GetComponent<MyPlaceableView>().data.hitPoints <= 0)
                {
                    MyPlaceableMgr.instance.OnEnterDie(targetAI);
                    
                }

                Destroy(proj.gameObject);
                destroyProjList.Add(proj);
            }
        }

        foreach (var des in destroyProjList)
        {
            projList.Remove(des);
        }

    }
}
