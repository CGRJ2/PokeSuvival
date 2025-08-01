using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Pokemon/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;// 스킬 이름
    public float damage;// 스킬 데미지
    public float speed;// 투사체 속도
    public float lifetime = 3f;// 투사체 생존 시간
    public GameObject projectilePrefab; // 투사체 프리팹
}