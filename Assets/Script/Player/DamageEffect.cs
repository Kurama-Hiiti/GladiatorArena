using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static GameManager;

public class DamageEffect : MonoBehaviour
{
    public static DamageEffect instance { get; private set; }

    public PostProcessVolume volume; // インスペクターでPost-process Volumeをアタッチ
    private Vignette vignette;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }



    void Start()
    {
        // 初期設定：VolumeからVignetteを取り出しておく
        if (volume != null && volume.profile.TryGetSettings(out vignette))
        {
            vignette.intensity.value = 0f;
        }
    }

    public void PlayDamageEffect()
    {
        StopAllCoroutines(); // 重複動作を防ぐ
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        // 淵を赤くする
        vignette.intensity.value = 0.45f;

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 徐々に値を下げる
            vignette.intensity.value = Mathf.Lerp(0.45f, 0f, elapsed / duration);
            yield return null;
        }

        vignette.intensity.value = 0f;
    }
}
