using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityRoyale;

public class CPU : MonoBehaviour
{
    public float interval = 5f;

    public Transform[] range = new Transform[2];
    // Start is called before the first frame update
    async void Start()
    {
        await CardOut();
    }

    async Task CardOut()
    {
        while (true)
        {
            var cardList = MyCardModel.instance.list;
            var cardData = cardList[Random.Range(0, cardList.Count)];
            var  viewList = await MyCardView.CreatePlaceable(cardData, 
                new Vector3(Random.Range(range[0].position.x,range[1].position.x), 0, Random.Range(range[0].position.z, range[1].position.z)), MyPlaceableMgr.instance.transform, 
                Placeable.Faction.Opponent);

            foreach(var view in viewList)
            {
                MyPlaceableMgr.instance.his.Add(view);
            }


            //yield return new WaitForSeconds(interval);
            await new WaitForSeconds(interval);
        }

    }
}
