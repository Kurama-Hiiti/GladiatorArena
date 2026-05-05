using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MastarSoundManager<T> : MonoBehaviour where T : System.Enum
{

    //SEの配列
    [SerializeField]
    private AudioClip[] se;


    //BGMの配列
    [SerializeField]
    private AudioClip[] bgm; 

    //SE用
    public AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    //音を鳴らす関数
    public void PlaySE(T type)
    {
        int index = System.Convert.ToInt32(type);

        if (index >= 0 && index < se.Length)
        {
            audioSource.PlayOneShot(se[index]);
        }

    }


    //BGM用関数
    //音を鳴らす関数
    public void PlayBGM(T type)
    {
        int index = System.Convert.ToInt32(type);

        if (index >= 0 && index < bgm.Length)
        {
            if (audioSource.isPlaying && audioSource.clip == bgm[index])
            {
                //同じ音源の場合は何もしない
                return;
            }

            audioSource.Stop();
            audioSource.clip = bgm[index];
            audioSource.Play();
        }

    }
}
