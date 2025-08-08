using Photon.Pun;
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

	private WaitForSeconds _sec = new(1);

	public PokeStatusHandler(MonoBehaviour routineClass, PokeBaseData baseData, StatusType status = StatusType.None)
	{
		_routineClass = routineClass;
		BaseData = baseData;
		CurrentStatus = new();
		CurrentStatus.Add(status);
	}

	// 공격자 로컬에서 피격자 상태 적용
	public bool SetStatus(PokemonSkill skill)
	{
		if (skill.StatusEffect == StatusType.None) return false;

		if (!CanApply(skill.StatusEffect))
		{
			Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {skill.StatusEffect} 면역!");
			return false;
		}

		// TODO : 확률 계산 0 ~ 1f
		var rate = skill.StatusRate;
		UnityEngine.Random.InitState(BaseData.PokeData.PokeNumber + BaseData.PokeLevel);
		var ran = UnityEngine.Random.value;
		if (ran > rate)
		{
			Debug.Log($"{skill.SkillName} 의 상태이상은 실패!");
			return false;
		}

		CurrentStatus.Add(skill.StatusEffect);
		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {skill.StatusEffect} 상태!");

		_routineClass.StartCoroutine(StatusRoutine(skill.StatusEffect, skill.StatusDuration));
		return true;
	}

	// 다른 클라이언트에서 상태 동기화, 로컬에서 성공 유무를 정해서 아래의 함수를 실행하기 때문에 항상 적용으로 변경
	public void SetStatus(string skillName, bool isBuffUpdate)
	{
		var skill = Define.GetPokeSkillData(skillName);
		if (skill == null)
		{
			Debug.Log($"{skillName} 은/는 없는 스킬입니다.");
			return;
		}

		if (!CanApply(skill.StatusEffect))
		{
			Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {skill.StatusEffect} 면역!");
			return;
		}

		CurrentStatus.Add(skill.StatusEffect);
		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {skill.StatusEffect} 상태!");

		_routineClass.StartCoroutine(StatusRoutine(skill.StatusEffect, skill.StatusDuration));

		if (isBuffUpdate && _routineClass is PlayerController pc) pc.OnBuffUpdate?.Invoke(skill.StatusSprite, skill.StatusDuration);
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

	public void RemoveStatus(StatusType status) => CurrentStatus.Remove(status);

	public bool IsBurn() => CurrentStatus.Contains(StatusType.Burn);
	public bool IsPoison() => CurrentStatus.Contains(StatusType.Poison);
	public bool IsFreeze() => CurrentStatus.Contains(StatusType.Freeze);
	public bool IsConfusion() => CurrentStatus.Contains(StatusType.Confusion);
	public bool IsBinding() => CurrentStatus.Contains(StatusType.Binding);
	public bool IsParalysis() => CurrentStatus.Contains(StatusType.Paralysis);
	public bool IsSleep() => CurrentStatus.Contains(StatusType.Sleep);
	public bool IsStun() => CurrentStatus.Contains(StatusType.Stun);

	IEnumerator StatusRoutine(StatusType status, float duration)
	{
		yield return new WaitForSeconds(duration);

		CurrentStatus.Remove(status);
		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {status} 상태 해제");
	}

	public void StatusAllClear()
	{
		_routineClass.StopAllCoroutines();
		CurrentStatus.Clear();
		CurrentStatus = new List<StatusType>() { StatusType.None };
		Debug.Log("모든 상태 해제");
	}

	// 상태이상 적용
	public void SetBurnDamage(float duration) => _routineClass?.StartCoroutine(BurnRoutine(duration));
	IEnumerator BurnRoutine(float duration)
	{
		float dur = duration;
		int hitCount = 0;

		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		while (dur > 0f)
		{
			yield return _sec;
			dur -= 1f;
			hitCount++;

			int burnDamage = Mathf.Max(1, BaseData.MaxHp / 16);
			BaseData.SetCurrentHp(BaseData.CurrentHp - burnDamage);
			if (pc != null && hitCount % 2 == 0)
			{
				// 힛 트리거도 RPC로 동기화해야함
				pc.RPC.ActionRPC(nameof(pc.RPC.RPC_SetHit), RpcTarget.All);
				PlayerManager.Instance.ShowDamageText(_routineClass.transform, burnDamage, Color.red);
			}
			else if (enemy != null && hitCount % 2 == 0)
			{
				enemy.photonView.RPC(nameof(enemy.RPC_TakeDamage), RpcTarget.All, burnDamage);
			}
		}
	}

	public void SetPoisonDamage(float duration) => _routineClass?.StartCoroutine(PoisonRoutine(duration));
	IEnumerator PoisonRoutine(float duration)
	{
		float dur = duration;
		int hitCount = 0;

		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		while (dur > 0f)
		{
			yield return _sec;
			dur -= 1f;
			hitCount++;

			int poisonDamage = Mathf.Max(1, BaseData.MaxHp / 8);
			BaseData.SetCurrentHp(BaseData.CurrentHp - poisonDamage);
			if (pc != null && hitCount % 2 == 0)
			{
				// 힛 트리거도 RPC로 동기화해야함
				pc.RPC.ActionRPC(nameof(pc.RPC.RPC_SetHit), RpcTarget.All);
				PlayerManager.Instance.ShowDamageText(_routineClass.transform, poisonDamage, Color.red);
			}
			else if (enemy != null && hitCount % 2 == 0)
			{
				enemy.photonView.RPC(nameof(enemy.RPC_TakeDamage), RpcTarget.All, poisonDamage);
			}
		}
	}

	public void SetBinding(float duration) => _routineClass?.StartCoroutine(BindingRoutine(duration));

	IEnumerator BindingRoutine(float duration)
	{
		CurrentStatus.Add(StatusType.Binding);
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;
		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
		yield return new WaitForSeconds(duration);
		CurrentStatus.Remove(StatusType.Binding);
	}

	public void SetFreeze(float duration) => _routineClass?.StartCoroutine(FreezeRoutine(duration));

	IEnumerator FreezeRoutine(float duration)
	{
		CurrentStatus.Add(StatusType.Freeze);
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;
		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
		yield return new WaitForSeconds(duration);
		CurrentStatus.Remove(StatusType.Freeze);
	}

	public void SetParalysis(float duration) => _routineClass?.StartCoroutine(ParalysisRoutine(duration));

	IEnumerator ParalysisRoutine(float duration)
	{
		CurrentStatus.Add(StatusType.Paralysis);
		yield return new WaitForSeconds(duration);
		CurrentStatus.Remove(StatusType.Paralysis);
	}

	public void SetConfusion(float duration) => _routineClass?.StartCoroutine(ConfusionRoutine(duration));
	IEnumerator ConfusionRoutine(float duration)
	{
		CurrentStatus.Add(StatusType.Confusion);
		yield return new WaitForSeconds(duration);
		CurrentStatus.Remove(StatusType.Confusion);
	}

	public void SetStun(float duration) => _routineClass?.StartCoroutine(StunRoutine(duration));
	IEnumerator StunRoutine(float duration)
	{
		CurrentStatus.Add(StatusType.Stun);
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;
		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
		yield return new WaitForSeconds(duration);
		CurrentStatus.Remove(StatusType.Stun);
	}
	public void SetSleep(float duration) => _routineClass?.StartCoroutine(SleepRoutine(duration));
	IEnumerator SleepRoutine(float duration)
	{
		CurrentStatus.Add(StatusType.Sleep);
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;
		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
		yield return new WaitForSeconds(duration);
		CurrentStatus.Remove(StatusType.Sleep);
	}
}
