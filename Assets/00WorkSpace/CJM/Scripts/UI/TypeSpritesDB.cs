using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


//[CreateAssetMenu(menuName = "Create TypeSpritesDB(SO)")]
public class TypeSpritesDB : ScriptableObject
{
    [SerializeField] List<TypeSpriteData> spriteDatas = new List<TypeSpriteData>();
    public Dictionary<PokemonType, Sprite> dic = new Dictionary<PokemonType, Sprite>();

    private void OnEnable()
    {
        /*Sprite[] spriteList = Resources.LoadAll<Sprite>("Type Icon DB/TypeIconSprites");

        foreach (Sprite _sprite in spriteList)
        {
            spriteDatas.Add(new TypeSpriteData() { type = PokemonType.None, sprite = _sprite });
        }*/

        Init();
    }

    void Init()
    {
        // ����� �����͸� ��ųʸ��� ������Ʈ
        dic.Clear();
        for (int i = 0; i < spriteDatas.Count; i++)
        {
            if (!dic.ContainsKey(spriteDatas[i].type))
                dic.Add(spriteDatas[i].type, spriteDatas[i].sprite);
        }
    }

}

[System.Serializable]
public class TypeSpriteData
{
    public PokemonType type;
    public Sprite sprite;
}