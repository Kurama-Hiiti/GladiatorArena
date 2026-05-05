using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MagicPool : MonoBehaviour 
{

    //発射した魔法のリスト
    private List<MageMagic> magicList = new List<MageMagic>();

    //発射したスキルRの魔法リスト
    private List<MageMagic> skillRList = new List<MageMagic>();

    //魔法の生成フラグ
    private bool magicSpawnd;

    //スキルR生成フラグ
    private bool skillRSpawnd;


    //プレイヤーの前方方向
    private Vector3 playerForward;

    //魔法発射関数
    public void MagicShoot(GameObject magic, Transform generatePos, Transform parentPos)
    {
        magicSpawnd = false;

        TakeMagicPool(magic, generatePos, parentPos);
    }


    //スキルR発射関数
    public void SkillRShoot(GameObject magic, Transform generatePos, Transform parentPos)
    {
        skillRSpawnd = false;

        SkillRTakeMagicPool(magic, generatePos, parentPos);
    }


    //すでに生成してある魔法を指定した位置に配置する関数
    private void TakeMagicPool(GameObject magic, Transform generatePos, Transform parentPos)
    {
        if (0 < magicList.Count)
        {
            for (int i = 0; i < magicList.Count; i++)
            {
                if (!magicList[i].gameObject.activeInHierarchy)//非表示の場合
                {
                    magicList[i].gameObject.SetActive(true);
                    magicList[i].gameObject.transform.position = generatePos.position;
                    magicList[i].MoveDirection(this.transform.forward);

                    magicSpawnd = true;
                    break;

                }
            }
        }


        if (!magicSpawnd)
        {
            MagicGenerate(magic, generatePos, parentPos);
        }
    }

    //すでに生成してある魔法を指定した位置に配置する関数
    private void SkillRTakeMagicPool(GameObject magic, Transform generatePos, Transform parentPos)
    {
        if (0 < skillRList.Count)
        {
            for (int i = 0; i < skillRList.Count; i++)
            {
                if (!skillRList[i].gameObject.activeInHierarchy)//非表示の場合
                {
                    skillRList[i].gameObject.SetActive(true);
                    skillRList[i].gameObject.transform.position = generatePos.position;
                    skillRList[i].MoveDirection(playerForward);

                    skillRSpawnd = true;
                    break;

                }
            }
        }


        if (!skillRSpawnd)
        {
            SkillRGenerate(magic, generatePos, parentPos);
        }
    }


    //魔法の生成
    private void MagicGenerate(GameObject magic, Transform generatePos, Transform parentPos)
    {
        //魔法生成
        GameObject newMagic = Instantiate(magic, generatePos);

        //魔法の位置の親設定
        newMagic.transform.parent = parentPos;

        //魔法の発射方向設定
        newMagic.GetComponent<MageMagic>().MoveDirection(this.transform.forward);

        //発射した魔法をリストへ格納
        magicList.Add(newMagic.GetComponent<MageMagic>());
    }

    //スキルRの生成
    private void SkillRGenerate(GameObject magic, Transform generatePos, Transform parentPos)
    {
        //魔法生成
        GameObject newMagic = Instantiate(magic, generatePos.position, Quaternion.identity);

        //魔法の位置の親設定
        newMagic.transform.parent = parentPos;

        //魔法の発射方向設定
        newMagic.GetComponent<MageMagic>().MoveDirection(playerForward);

        //発射した魔法をリストへ格納
        skillRList.Add(newMagic.GetComponent<MageMagic>());
    }


    public void PlayerFoword()
    {
        playerForward = transform.forward;
    }

}
