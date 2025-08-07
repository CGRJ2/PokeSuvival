using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokeStatusHandler
{
	[SerializeField] private MonoBehaviour _routineClass; // PlayerController, Enemy
	[field: SerializeField] public PokeBaseData BaseData { get; private set; }

	[field: SerializeField] public List<StatusType> CurrentStatus { get; private set; } = new();

	public PokeStatusHandler(MonoBehaviour routineClass, PokeBaseData baseData, StatusType status = StatusType.None)
	{
		_routineClass = routineClass;
		BaseData = baseData;
		CurrentStatus = new();
		CurrentStatus.Add(status);
	}

	// 공격자 로컬에서 피격자 상태 적용
	public void SetStatus(PokemonSkill skill)
	{
		if (skill.StatusEffect == StatusType.None) return;

		if (!CanApply(skill.StatusEffect))
		{
			Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {skill.StatusEffect} 면역!");
			return;
		}

		CurrentStatus.Add(skill.StatusEffect);
		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {skill.StatusEffect} 상태!");

		_routineClass.StartCoroutine(StatusRoutine(skill.StatusEffect, skill.StatusDuration));
	}

	// 다른 클라이언트에서 상태 동기화
	public void SetStatus(string skillName, StatusType status, float duration)
	{
		if (!CanApply(status))
		{
			Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {status} 면역!");
			return;
		}

		CurrentStatus.Add(status);
		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {status} 상태!");

		_routineClass.StartCoroutine(StatusRoutine(status, duration));

		if (_routineClass is PlayerController pc)
		{
			// TODO : static 딕셔너리에서 값 받아오기
			//pc.OnBuffUpdate?.Invoke()
		}
	}

	public bool CanApply(StatusType status)
	{
		if (BaseData?.PokeData == null) return false;

		foreach (var type in BaseData.PokeData.PokeTypes)
		{
			switch (status)
			{
				case StatusType.Burn: if (type == PokemonType.Fire) return false; break;
				case StatusType.Poison: if (type == PokemonType.Poison) return false; break;
				case StatusType.Paralysis: if (type == PokemonType.Electric) return false; break;
				case StatusType.Freeze: if (type == PokemonType.Ice) return false; break;
			}
		}
		return true;
	}

	IEnumerator StatusRoutine(StatusType status, float duration)
	{
		yield return new WaitForSeconds(duration);

		CurrentStatus.Remove(status);
		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {status} 상태 해제");
	}

	public void StatusAllClear()
	{
		_routineClass.StopAllCoroutines();
		Debug.Log("모든 상태 해제");
	}
}
