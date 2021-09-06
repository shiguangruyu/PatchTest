using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlaceableView : MonoBehaviour
{
    public MyPlaceable data;//游戏单位数据

    public float dieDuration = 10f;//死亡溶解总时间，按秒
    public float dieProgress = 0f;//当前溶解进度

    private void OnDestroy()
    {
        MyPlaceableMgr.instance.mine.Remove(this);
        MyPlaceableMgr.instance.his.Remove(this);
    }
}
