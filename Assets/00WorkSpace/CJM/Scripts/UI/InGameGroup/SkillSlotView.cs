using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotView : MonoBehaviour
{
    [SerializeField] TMP_Text tmp_Name;
    [SerializeField] TMP_Text tmp_CoolTime;
    [SerializeField] Image image_PokeType;
    [SerializeField] Image image_AttackType;
    [SerializeField] Image image_CoolTime;
    [SerializeField] Image image_Blocked;

    public bool actived;

    public void UpdateSlotView(PokemonSkill skill)
    {
        image_Blocked.gameObject.SetActive(false);

        tmp_Name.text = skill.SkillName;

        // ��Ÿ�� ���� ��
        if (skill.Cooldown > 0)
        {
            tmp_CoolTime.text = skill.Cooldown.ToString();
            image_CoolTime.gameObject.SetActive(true);
            image_CoolTime.fillAmount = Mathf.Clamp01(skill.Cooldown);
        }
        else
        {
            image_CoolTime.gameObject.SetActive(false);
        }


        // �Ӽ� Ÿ�� sprite ������Ʈ
        TypeSpritesDB typeSpriteDB = Resources.Load<TypeSpritesDB>("Type Icon DB/PokemonTypeSpritesDB");
        image_PokeType.sprite = typeSpriteDB.dic[skill.PokeType];

        // ���� Ÿ�� sprite ������Ʈ
        AttackTypeSpritesDB atkTypeSpriteDB = Resources.Load<AttackTypeSpritesDB>("Type Icon DB/AttackTypeSpritesDB");
        image_AttackType.sprite = atkTypeSpriteDB.dic[skill.SkillType];


    }

    public void BlockSlotView()
    {
        image_Blocked.gameObject.SetActive(true);
    }
}
