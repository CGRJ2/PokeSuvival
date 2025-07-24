using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_PokemonInfo : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Number;
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_Hp;
    [SerializeField] TMP_Text tmp_Atk;
    [SerializeField] TMP_Text tmp_Def;
    [SerializeField] TMP_Text tmp_SpAtk;
    [SerializeField] TMP_Text tmp_SpDef;
    [SerializeField] TMP_Text tmp_Speed;
    [SerializeField] Image[] images_Type;


    public void Init()
    {

    }

    public void UpdateView(PokemonData selectedPokemonData)
    {
        tmp_Number.text = $"{selectedPokemonData.PokeNumber}";
        tmp_Name.text = selectedPokemonData.PokeName;
        tmp_Hp.text = $"{selectedPokemonData.BaseStat.Hp}";
        tmp_Atk.text = $"{selectedPokemonData.BaseStat.Attak}";
        tmp_Def.text = $"{selectedPokemonData.BaseStat.Defense}";
        tmp_SpAtk.text = $"{selectedPokemonData.BaseStat.SpecialAttack}";
        tmp_SpDef.text = $"{selectedPokemonData.BaseStat.SpecialDefense}";
        tmp_Speed.text = $"{selectedPokemonData.BaseStat.Speed}";

        // 타입이 1개일 때와 2개일 때 예외처리
        TypeSpritesDB typeSpriteDB = Resources.Load<TypeSpritesDB>("Type Icon DB/PokemonTypeSpritesDB");
        if (selectedPokemonData.Types.Length > 1)
        {
            images_Type[0].gameObject.SetActive(true);
            images_Type[1].gameObject.SetActive(true);
            images_Type[0].sprite = typeSpriteDB.dic[selectedPokemonData.Types[0]];
            images_Type[1].sprite = typeSpriteDB.dic[selectedPokemonData.Types[1]];
        }
        else
        {
            images_Type[0].gameObject.SetActive(true);
            images_Type[1].gameObject.SetActive(false);
            images_Type[0].sprite = typeSpriteDB.dic[selectedPokemonData.Types[0]];
        }
    }
}
