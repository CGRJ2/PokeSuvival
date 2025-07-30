using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Create TypeSpritesDB(SO)")]
public class AttackTypeSpritesDB : ScriptableObject
{
    [SerializeField] List<AttackTypeSpriteData> spriteDatas = new List<AttackTypeSpriteData>();
    public Dictionary<SkillType, Sprite> dic = new Dictionary<SkillType, Sprite>();

    private void OnEnable()
    {
        Init();
    }

    void Init()
    {
        // 저장된 데이터를 딕셔너리에 업데이트
        dic.Clear();
        for (int i = 0; i < spriteDatas.Count; i++)
        {
            if (!dic.ContainsKey(spriteDatas[i].type))
                dic.Add(spriteDatas[i].type, spriteDatas[i].sprite);
        }
    }

}

[System.Serializable]
public class AttackTypeSpriteData
{
    public SkillType type;
    public Sprite sprite;
}

