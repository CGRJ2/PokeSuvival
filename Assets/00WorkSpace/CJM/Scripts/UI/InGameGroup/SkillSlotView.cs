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
    public float targetCoolTime;
    public float startedTime;

    void Update()
    {
        // 쿨타임 존재 시
        if (targetCoolTime - Time.time > 0)
        {
            tmp_CoolTime.text = ((targetCoolTime - Time.time) % 1000f).ToString("F1");
            image_CoolTime.gameObject.SetActive(true);
            image_CoolTime.fillAmount = Mathf.Clamp01((targetCoolTime - Time.time) / (targetCoolTime - startedTime));
        }
        else
        {
            image_CoolTime.gameObject.SetActive(false);
        }
    }

    public void CoolTimeUpdate(float targetCoolTime)
    {
        this.targetCoolTime = targetCoolTime;
        this.startedTime = Time.time;
    }

    public void UpdateSlotView(PokemonSkill skill)
    {
        image_Blocked.gameObject.SetActive(false);
        targetCoolTime = 0;

        tmp_Name.text = skill.SkillName;

        // 속성 타입 sprite 업데이트
        TypeSpritesDB typeSpriteDB = Resources.Load<TypeSpritesDB>("Type Icon DB/PokemonTypeSpritesForSkillSlotDB");
        image_PokeType.sprite = typeSpriteDB.dic[skill.PokeType];

        // 공격 타입 sprite 업데이트
        AttackTypeSpritesDB atkTypeSpriteDB = Resources.Load<AttackTypeSpritesDB>("Type Icon DB/AttackTypeSpritesDB");
        image_AttackType.sprite = atkTypeSpriteDB.dic[skill.SkillType];
    }


    public void BlockSlotView()
    {
        tmp_Name.text = "";
        image_Blocked.gameObject.SetActive(true);
    }
}
