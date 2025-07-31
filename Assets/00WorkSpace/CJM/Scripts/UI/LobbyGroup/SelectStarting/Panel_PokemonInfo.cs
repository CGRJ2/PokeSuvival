using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_PokemonInfo : MonoBehaviour
{
    [SerializeField] Image image_StandingSprite;
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
        tmp_Number.text = selectedPokemonData.PokeNumber.ToString("D4");
        tmp_Name.text = selectedPokemonData.PokeName;
        tmp_Hp.text = $"{selectedPokemonData.BaseStat.Hp}";
        tmp_Atk.text = $"{selectedPokemonData.BaseStat.Attak}";
        tmp_Def.text = $"{selectedPokemonData.BaseStat.Defense}";
        tmp_SpAtk.text = $"{selectedPokemonData.BaseStat.SpecialAttack}";
        tmp_SpDef.text = $"{selectedPokemonData.BaseStat.SpecialDefense}";
        tmp_Speed.text = $"{selectedPokemonData.BaseStat.Speed}";

        image_StandingSprite.sprite = selectedPokemonData.PokemonInfoSprite;

        // 타입이 1개일 때와 2개일 때 예외처리
        TypeSpritesDB typeSpriteDB = Resources.Load<TypeSpritesDB>("Type Icon DB/PokemonTypeSpritesDB");

        // 타입 sprite 업데이트
        images_Type[0].gameObject.SetActive(true);
        images_Type[0].sprite = typeSpriteDB.dic[selectedPokemonData.PokeTypes[0]];

        // 보조타입 sprite 업데이트
        if (selectedPokemonData.PokeTypes[1] == PokemonType.None) // 보조 타입이 없다면
            images_Type[1].gameObject.SetActive(false);
        else
        {
            images_Type[1].gameObject.SetActive(true);
            images_Type[1].sprite = typeSpriteDB.dic[selectedPokemonData.PokeTypes[1]];
        }
    }
}
