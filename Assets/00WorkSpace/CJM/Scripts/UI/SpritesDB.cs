using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Create TypeSpritesDB(SO)")]
public class SpritesDB : ScriptableObject
{
    [SerializeField] List<SpriteDataKey> spriteDatas = new List<SpriteDataKey>();
    public Dictionary<string, Sprite> dic = new Dictionary<string, Sprite>();

    private void OnEnable()
    {
        /*Sprite[] spriteList = Resources.LoadAll<Sprite>("RegionIlustrations");

        foreach (Sprite _sprite in spriteList)
        {
            spriteDatas.Add(new SpriteDataKey() { key = _sprite.name, sprite = _sprite });
        }*/

        Init();
    }

    void Init()
    {
        // 저장된 데이터를 딕셔너리에 업데이트
        dic.Clear();
        for (int i = 0; i < spriteDatas.Count; i++)
        {
            if (!dic.ContainsKey(spriteDatas[i].key))
                dic.Add(spriteDatas[i].key, spriteDatas[i].sprite);
        }
    }
}

[Serializable]
public class SpriteDataKey
{
    public string key;
    public Sprite sprite;
}