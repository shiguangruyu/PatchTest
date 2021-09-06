using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityRoyale;
using UnityEngine.Profiling;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class MyCardView : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    public MyCard data;//卡牌的数据

    public int index;//卡牌再出牌区的序号

    private Transform previewHolder;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        previewHolder = GameObject.Find("PreviewHolder").GetComponent<Transform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //把点击的卡牌放到所有卡牌所在的节点的最后一个，使其在绘制时叠加再其他卡牌上面，因为unityui的绘制时从最后一个子节点往第一个子节点绘制的
        transform.SetAsLastSibling();//设置为最后一个子节点

        //将地方区域渲染为禁止放置区域
        //显示禁止区域
        MyCardMgr.instance.forbiddenAreaRenderer.enabled = true;
    }


    private bool isDragging = false;//是否变成了兵
    public async void OnDrag(PointerEventData eventData)
    {
        //卡牌的位置随着鼠标的拖拽移动
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, eventData.position, null, out Vector3 posWorld);
        transform.position = posWorld;

        //从鼠标位置发射一条射线
        Ray ray = mainCamera.ScreenPointToRay(eventData.position);


        //判断射线碰撞到场景的位置
        bool hitGround = Physics.Raycast(ray,out RaycastHit hit , float.PositiveInfinity, 1 << LayerMask.NameToLayer("PlayingField"));

        //如果碰撞到场景物体
        if (hitGround)
        {
            previewHolder.position = hit.point;
            if (isDragging == false)//如果卡牌之前没有被拖拽出来（没有变兵）
            {
                print("命中地面，卡牌变兵");

                isDragging = true;

                //隐藏改卡牌
                GetComponent<CanvasGroup>().alpha = 0;

                //这里暂时不能使用await。因为才createPlaceable还没有完成之前，if这个代码可能已经被重入了无数次了就会创建多个角色所以在此之前就把isdragging设置为true，就能防止这个情况
                //从卡牌数据数组中找出该卡牌数据，创建预览卡牌
                await CreatePlaceable(data,hit.point,previewHolder.transform,Placeable.Faction.Player);

                
            }
            else//卡牌已经被拖拽出来变成了小兵
            {
                print("命中地面，卡牌已经变兵");
            }
        }
        else//鼠标没有命中地面（将卡牌放回出牌位置）
        {
            if (isDragging)//如果卡牌曾经激活（曾经放到场景中了）
            {
                print("鼠标没有中低迷那（正常出牌位置）");
                //标记卡牌为未激活（未显示预览小兵）
                isDragging = false;

                //显示卡牌
                GetComponent<CanvasGroup>().alpha = 1f;

                //销毁预览小兵
                foreach(Transform trUnit in previewHolder)
                {
                    Destroy(trUnit.gameObject);
                }

                
            }
        }

    }

    /// <summary>
    /// 根据兵种数据创建一个兵种到场地中
    /// </summary>
    /// <param name="cardData"></param>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    /// <param name="faction"></param>
    public static async Task<List<MyPlaceableView>> CreatePlaceable(MyCard cardData,Vector3 pos,Transform parent, Placeable.Faction faction)
    {
        List<MyPlaceableView> viewList = new List<MyPlaceableView>();
        
        for (int i = 0; i < cardData.placeablesIndices.Length; i++)
        {
            //取出小兵数据
            int unitId = cardData.placeablesIndices[i];
            MyPlaceable p = null;//可放置的小兵的数据

            for (int j = 0; j < MyPlaceableModel.instance.list.Count; j++)
            {
                if (MyPlaceableModel.instance.list[j].id == unitId)
                {
                    p = MyPlaceableModel.instance.list[j];
                    break;
                }
            }

            //取出小兵之间的相对偏移
            Vector3 offset = cardData.relativeOffsets[i];

            //生成改卡牌对应的小兵数组，并且将其设置为预览用的卡牌（将其放置到一个统一的节点下（previewHolder））
            //Profiler.BeginSample("Create unit by Resources");
            //GameObject unitPrefab = Resources.Load<GameObject>(faction == Placeable.Faction.Player ? p.associatedPrefab : p.alternatePrefab);

            ////GameObject unit = Instantiate(unitPrefab, previewHolder, false);//设置实例化的对象的父节点，并且不把设置相对于父节点的世界坐标
            ////unit.transform.localPosition = offset;//设置实例化单位相对于父节点的偏移量

            ////parent.position = pos;
            //GameObject unit = Instantiate(unitPrefab, parent, false);//设置实例化的对象的父节点，并且不把设置相对于父节点的世界坐标
            string prefabName = faction == Placeable.Faction.Player ? p.associatedPrefab : p.alternatePrefab;
            var unit = await Addressables.InstantiateAsync(prefabName,parent,false).Task;

            //Profiler.EndSample();

            unit.transform.localPosition = offset;//设置实例化单位相对于父节点的偏移量
            unit.transform.position = pos + offset;

            MyPlaceable p2 = p.Clone();
            p2.faction = faction;
            MyPlaceableView view = unit.GetComponent<MyPlaceableView>();
            view.data = p2;

            viewList.Add(view);
        }
        //Debug.Log("创建兵种失败");
        return viewList;
    }


    public async void OnPointerUp(PointerEventData eventData)
    {
        

        //从鼠标位置发射一条射线
        Ray ray = mainCamera.ScreenPointToRay(eventData.position);

        //判断该射线碰撞到场景中的什么位置
        bool hitGround = Physics.Raycast(ray, float.PositiveInfinity, 1 << LayerMask.NameToLayer("PlayingField"));

        if (hitGround)
        {
            OnCardUsed();

            //销毁打出去的卡牌
            Destroy(this.gameObject);

            //从预览区取出一张卡牌放到出牌区,这里这样调用是因为前面已经把该对象销毁了，就不会再执行后面得方法了
            //这里只是使用了异步等待，没有new一个task对象，所以他的方法可以使用void类型
            await MyCardMgr.instance.PreviewCardToCard(index, 0.5f);

            //生成一张卡牌放到预览区
            await MyCardMgr.instance.CreatePreviewCard(0.5f);
        }
        else
        {
            //卡牌放回出牌区
            transform.DOMove(MyCardMgr.instance.cards[index].position, 0.2f);
        }

        // 隐藏禁止区域
        MyCardMgr.instance.forbiddenAreaRenderer.enabled = false;
    }

    private void OnCardUsed()
    {
        //游戏单位放到游戏单位管理器中（MyPlaceableView）
        for(int i=previewHolder.childCount-1;i>=0;i--)
        {
            Transform trUnit = previewHolder.GetChild(i);
            trUnit.SetParent(MyPlaceableMgr.instance.transform, true);

            MyPlaceableMgr.instance.mine.Add(trUnit.GetComponent<MyPlaceableView>());
        }
    }
}
