using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//それぞれのSoundManagerの親スクリプト
public class MastarSoundManager<T> : MonoBehaviour where T : System.Enum
{

    //SEの配列
    [SerializeField]
    private AudioClip[] se;


    //BGMの配列
    [SerializeField]
    private AudioClip[] bgm; 

    //SE用オーディオ
    public AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    //SE用関数
    public void PlaySE(T type)
    {
        //SEの名称(enum)をint（番号）へ変換
        int index = System.Convert.ToInt32(type);

        //指定したSEを番号で決定する
        if (index >= 0 && index < se.Length)
        {
            audioSource.PlayOneShot(se[index]);
        }

    }


    //BGM用関数
    public void PlayBGM(T type)
    {
        //BGMの名称(enum)をint（番号）へ変換
        int index = System.Convert.ToInt32(type);

        //指定したBGMを番号で決定する
        if (index >= 0 && index < bgm.Length)
        {
            if (audioSource.isPlaying && audioSource.clip == bgm[index])
            {
                //再生する音源が同じ場合は何もしない
                return;
            }

            //音源停止→新たにBGM設定→再生
            audioSource.Stop();
            audioSource.clip = bgm[index];
            audioSource.Play();
        }

    }
}
