using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokeBuffHandler
{
	[SerializeField] private MonoBehaviour _routineClass; // PlayerController, Enemy
	[field: SerializeField] public PokeBaseData BaseData { get; private set; }

	[field: SerializeField] public List<string> CurrentBuffs { get; private set; } = new();

	public PokeBuffHandler(MonoBehaviour routineClass, PokeBaseData baseData)
	{
		_routineClass = routineClass;
		BaseData = baseData;
		CurrentBuffs = new();
	}

	// 로컬용
	public void SetBuff(PokemonSkill skill)
	{
		CurrentBuffs.Add(skill.SkillName);
	}

	// 동기화용
	public void SetBuff(string skillName)
	{
		var skill = Define.GetPokeSkillData(skillName);
		if (skill == null) return;

		CurrentBuffs.Add(skillName);
	}

	public void RemoveBuff(PokemonSkill skill) => CurrentBuffs.Remove(skill.SkillName);
	public void RemoveBuff(string skillName) => CurrentBuffs.Remove(skillName);
	public void BuffAllClear() => CurrentBuffs = new();
}
