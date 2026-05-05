using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObject/AnimationSpeedData")]
public class AnimationSpeedData : ScriptableObject
{
    [System.Serializable]
    public struct AnimationSpeedEntry
    {
        public string animationName; // アニメーション名（ステート名 or クリップ名）
        public float baseSpeed;      // 基礎速度
    }

    [SerializeField]
    private List<AnimationSpeedEntry> animationSpeeds = new List<AnimationSpeedEntry>();

    //Dictionaryのキャッシュ
    private Dictionary<string, float> speedDictionary;

    // 名前から速度を取得（見つからなければ1.0f）
    public float GetBaseSpeed(string animationName)
    {
        // 初回アクセス時に辞書化
        if (speedDictionary == null)
        {
            speedDictionary = new Dictionary<string, float>();
            foreach (AnimationSpeedEntry entry in animationSpeeds)
            {
                if (!speedDictionary.ContainsKey(entry.animationName))
                    speedDictionary.Add(entry.animationName, entry.baseSpeed);
            }
        }

        return speedDictionary.TryGetValue(animationName, out float speed) ? speed : 1.0f;
    }

}
