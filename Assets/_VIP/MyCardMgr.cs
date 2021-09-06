using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

public class MyCardMgr : MonoBehaviour
{
    public static MyCardMgr instance;

    public Transform[] cards;//活动牌

    public Transform canvas;//闯进啊出来得卡牌必须反正该canvas下面，否则无法显示

    public Transform startPos, endPos;//发牌动画的起使位置和终点位置

    private Transform previewCard;//预览卡牌

    public MeshRenderer forbiddenAreaRenderer;//禁止区域的渲染器

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    async void Start()
    {
        //StartCoroutine(CreatePreviewCard(0.5f));
        //StartCoroutine(PreviewCardToCard(0, 1f));

        //StartCoroutine(CreatePreviewCard(1.5f));
        //StartCoroutine(PreviewCardToCard(1, 2f));

        //StartCoroutine(CreatePreviewCard(2.5f));
        //StartCoroutine(PreviewCardToCard(2, 3f));

        //StartCoroutine(CreatePreviewCard(3.5f));

        await CreatePreviewCard(0.5f);
        await PreviewCardToCard(0, 0.5f);

        await CreatePreviewCard(0.5f);
        await PreviewCardToCard(1, 0.5f);

        await CreatePreviewCard(0.5f);
        await PreviewCardToCard(2, 0.5f);

        await CreatePreviewCard(0.5f);

    }

    public async Task CreatePreviewCard (float delayTime)
    {
        //yield return new WaitForSeconds(delayTime);
        await new WaitForSeconds(delayTime);//这里会返回一个task对象，所以方法的返回值必须是task类型

        int iCard = Random.Range(0, MyCardModel.instance.list.Count);//随机取得要生成的卡牌的序号
        MyCard card = MyCardModel.instance.list[iCard];//从卡牌数据中生成相应的卡牌数据模型

        //GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);//根据卡牌的数据模型从资源中加载卡牌到内存中
        //previewCard = Instantiate(cardPrefab).transform;//从内存中实例化卡牌

        //由于是异步实例化，所以我们不能直接通过InstantiateAsync的返回值获取到创建的卡牌对象
        //我们需要等待一部实例化完毕，同时又不能阻塞unity程序的执行（会造成卡顿）
        //所以我们需要使用c#的异步等待语法
        //在addressables系统中，instantiateAsync相当于是resources.load+instantiate
        //await异步等待必须写在支持异步的方法里面，必须在声明该方法的时候加上async关键字
        //用了异步就可以不再使用协程了，前提是我们必须要引入支持协程的功能（比如waitforsecond等等功能）的一个库
        GameObject cardPrefab= await Addressables.InstantiateAsync(card.cardPrefab).Task;
        previewCard = cardPrefab.transform;

        previewCard.SetParent(canvas, false);//设置再canvas下面的位置
        previewCard.localScale = Vector3.one * 0.7f;
        previewCard.position = startPos.position;
        previewCard.DOMove(endPos.position,0.5f);

        previewCard.GetComponent<MyCardView>().data = card;
    }

    public async Task PreviewCardToCard(int i,float delayTime)
    {
        //yield return new WaitForSeconds(delayTime);
        await new WaitForSeconds(delayTime);

        previewCard.DOScale(Vector3.one*0.7f, 0.5f);
        previewCard.DOMove(cards[i].transform.position, 0.5f);

        previewCard.GetComponent<MyCardView>().index = i;
    }
}
