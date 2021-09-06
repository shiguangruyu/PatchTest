using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 卡牌管理器2 : MonoBehaviour
{
    public Transform[] cards; // 活动牌

    public GameObject[] cardPrefabs; // 卡牌预制体（弓箭手/战士/法师/。。。）

    public Transform canvas; // 创建出来的卡牌必须放在Canvas下，否则显示不出来

    public Transform startPos, endPos; // 发牌动画的起始位置和终止位置

    private Transform previewCard; // 预览卡牌 

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(创建卡牌到预览区(0.5f));
        StartCoroutine(预览区到出牌区(0, 1f));

        StartCoroutine(创建卡牌到预览区(1.5f));
        StartCoroutine(预览区到出牌区(1, 2f));

        StartCoroutine(创建卡牌到预览区(2.5f));
        StartCoroutine(预览区到出牌区(2, 3f));

        StartCoroutine(创建卡牌到预览区(3.5f));
    }

    IEnumerator 创建卡牌到预览区(float 延迟值)
    {
        yield return new WaitForSeconds(延迟值);

        GameObject cardPrefab = cardPrefabs[Random.Range(0, cardPrefabs.Length)];
        previewCard = Instantiate(cardPrefab).transform;
        previewCard.SetParent(canvas, false); // 位于父节点下的（0，0，0）偏移处
        previewCard.localScale = Vector3.one * 0.7f;
        previewCard.position = startPos.position;
        previewCard.DOMove(endPos.position, .5f);
    }

    IEnumerator 预览区到出牌区(int i, float 延迟值)
    {
        yield return new WaitForSeconds(延迟值);

        previewCard.localScale = Vector3.one;
        previewCard.DOMove(cards[i].position, .5f);
    }
}
