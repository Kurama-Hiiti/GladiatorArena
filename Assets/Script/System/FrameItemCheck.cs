using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameItemCheck : MonoBehaviour
{
    //フレーム内のアイテム
    [SerializeField]
    private List<Transform> item = new List<Transform>();

    public bool isSet;


    private void Update()
    {
        if (GameManager.instance.state == GameManager.GameState.CharactorSelect)
        {
            ItemSetCheck();
        }
    }

    private void ItemSetCheck()
    {
        item.Clear();

        if (0 < gameObject.transform.childCount)
        {
            item.Add(gameObject.transform.GetChild(0));
            isSet = true;
        }
        else
        {
            isSet = false;
        }
    }




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Item"))
        {
            if (item.Count == 0)
            {
                item.Add(collision.gameObject.transform);

            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {

            if (item.Count > 0 && item[0] != null && item[0].name == collision.gameObject.name)
            {
                item.Clear();
                isSet = false;


                if (GameManager.instance.state == GameManager.GameState.Shop)
                {
                    if (this.gameObject.name == "PotionPos1")
                    {
                        GameManager.instance.player.GetComponent<Player>().potion1 = null;
                    }
                    else if (this.gameObject.name == "PotionPos2")
                    {
                        GameManager.instance.player.GetComponent<Player>().potion2 = null;
                    }
                    else if (this.gameObject.name == "PotionPos3")
                    {
                        GameManager.instance.player.GetComponent<Player>().potion3 = null;
                    }
                }



            }

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isSet)
        {
            if (collision.gameObject.CompareTag("Item"))
            {
                if (item.Count == 0)
                {
                    item.Add(collision.gameObject.transform);
                    isSet = true;
                }
            }
        }
        
    }

}
