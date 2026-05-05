using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageMagic : MonoBehaviour
{
    //魔法の弾にアタッチするスクリプト

    private Rigidbody rb;

    //移動スピード,弾が消えるまでの時間
    [SerializeField]
    private float moveSpeed = 10f, deactiveTimer = 3f, skillDeactiveTimer = 5f;


    private TrailRenderer trail;

    private void Awake()
    {
        if (!this.CompareTag("MageSkillF"))
        {
            rb = GetComponent<Rigidbody>();
        }

        if (this.CompareTag("Magic"))
        {
            trail = GetComponentInChildren<TrailRenderer>();
        }

    }


    //表示された時に実行される関数
    private void OnEnable()
    {
        if (this.CompareTag("MageSkillF"))
        {
            Invoke("DestroySkill", skillDeactiveTimer);
        }
        else
        {
            Invoke("HiddenMagic", deactiveTimer);
        }
        
    }

    //非表示された時に実行される関数
    private void OnDisable()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
        
 
    }

    //魔法の発射関数
    public void MoveDirection(Vector3 direction)
    {
       rb.velocity = direction * moveSpeed;
    }


    //魔法を非表示にする関数
    private void HiddenMagic()
    {
        if (gameObject.activeSelf)
        {
            if (trail != null)
            {
                trail.Clear();
            }
            
            this.gameObject.SetActive(false);
        }
        
    }

    private void DestroySkill()
    {
        Destroy(this.gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        //敵にあたると非表示
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            if (this.CompareTag("MageSkillR") || this.CompareTag("MageSkillF"))
            {
                return;
            }
            else
            {
                trail.Clear();
                this.gameObject.SetActive(false);
                CancelInvoke("HiddenMagic");
            }


        }
    }

}
