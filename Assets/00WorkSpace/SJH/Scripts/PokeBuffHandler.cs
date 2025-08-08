using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokeBuffHandler
{
	[SerializeField] private MonoBehaviour _routineClass; // PlayerController, Enemy
	[field: SerializeField] public PokeBaseData BaseData { get; private set; }

	[field: SerializeField] public List<string> CurrentBuffs { get; private set; } = new();

	public Action<string> OnSyncToBuff;

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
		if (_routineClass != null)
		{
			if (_routineClass is PlayerController pc) pc.OnBuffUpdate?.Invoke(skill.StatusSprite, skill.StatusDuration);
		}
		OnSyncToBuff?.Invoke(skill.SkillName);
		_routineClass?.StartCoroutine(BuffRoutine(skill, skill.StatusDuration));
	}

	// 동기화용
	public void SetBuff(string skillName)
	{
		var skill = Define.GetPokeSkillData(skillName);
		if (skill == null) return;

		CurrentBuffs.Add(skillName);
		Debug.Log($"{skillName} 버프 동기화 완료");
	}

	public void RemoveBuff(PokemonSkill skill) => CurrentBuffs.Remove(skill.SkillName);
	public void RemoveBuff(string skillName) => CurrentBuffs.Remove(skillName);
	public void BuffAllClear() => CurrentBuffs = new();

	IEnumerator BuffRoutine(PokemonSkill skill, float duration)
	{
		yield return new WaitForSeconds(duration);
		RemoveBuff(skill);
	}
}
