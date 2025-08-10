using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

		// 확률 판정
		var ran = UnityEngine.Random.value;
		if (ran > skill.StatusRate)
		{
			Debug.Log($"{skill.SkillName} 의 상태이상 실패! / {ran} > {skill.StatusRate}");
			return false;
		}

		// 최종 적용 상태 결정
		var statusEffect = skill.StatusEffect;
		if (skill.SkillName == "트라이어택")
		{
			StatusType[] status = { StatusType.Paralysis, StatusType.Burn, StatusType.Freeze };
			statusEffect = status[UnityEngine.Random.Range(0, status.Length)];
		}

		// 면역 체크
		if (!CanApply(statusEffect))
		{
			Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {statusEffect} 면역!");
			return false;
		}

		// 상태이상 추가
		CurrentStatus.Add(statusEffect);
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.SC.OnStatus(skill.StatusEffect);
		if (enemy != null) enemy.SC.OnStatus(skill.StatusEffect);
		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {statusEffect} 상태! / {ran} <= {skill.StatusRate}");

		// 상태이상 종료 코루틴
		_routineClass.StartCoroutine(StatusRoutine(statusEffect, skill.StatusDuration));
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

		// 상태이상 추가
		CurrentStatus.Add(skill.StatusEffect);
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.SC.OnStatus(skill.StatusEffect);
		if (enemy != null) enemy.SC.OnStatus(skill.StatusEffect);

		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {skill.StatusEffect} 상태!");
		// 상태이상 종료 코루틴
		_routineClass.StartCoroutine(StatusRoutine(skill.StatusEffect, skill.StatusDuration));

		if (isBuffUpdate && pc != null)
		{
			Debug.Log($"{skill.SkillName} 상태이상 적용 및 UI 업데이트");
			pc.OnBuffUpdate?.Invoke(skill.StatusSprite, skill.StatusDuration);
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
	#region 상태이상 제거, 상태이상 체크용
	public void RemoveStatus(StatusType status) => CurrentStatus.Remove(status);
	public bool IsBurn() => CurrentStatus.Contains(StatusType.Burn);
	public bool IsPoison() => CurrentStatus.Contains(StatusType.Poison);
	public bool IsFreeze() => CurrentStatus.Contains(StatusType.Freeze);
	public bool IsConfusion() => CurrentStatus.Contains(StatusType.Confusion);
	public bool IsBinding() => CurrentStatus.Contains(StatusType.Binding);
	public bool IsParalysis() => CurrentStatus.Contains(StatusType.Paralysis);
	public bool IsSleep() => CurrentStatus.Contains(StatusType.Sleep);
	public bool IsStun() => CurrentStatus.Contains(StatusType.Stun);
	#endregion

	IEnumerator StatusRoutine(StatusType status, float duration)
	{
		yield return new WaitForSeconds(duration);

		CurrentStatus.Remove(status);

		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.SC.OffStatus(status);
		if (enemy != null) enemy.SC.OffStatus(status);

		Debug.Log($"{BaseData.PokeData.PokeName} 은/는 {status} 상태 해제");
	}

	public void StatusAllClear()
	{
		_routineClass.StopAllCoroutines();
		CurrentStatus.Clear();
		CurrentStatus = new List<StatusType>() { StatusType.None };

		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.SC.StatusEffectClear();
		if (enemy != null) enemy.SC.StatusEffectClear();

		Debug.Log("모든 상태 해제");
	}

	// 각 상태이상의 특수 효과를 적용 코루틴
	// ex
	// 화상, 독 : 도트딜
	// 마비 : 이동속도 반감
	// 동상, 수면 : 이동 불가, 회전 불가
	// 바인드 : 이동 불가, 회전 가능
	// 혼란 : 이동 반전
	public void SetBurnDamage(float duration) => _routineClass?.StartCoroutine(BurnRoutine(duration));
	IEnumerator BurnRoutine(float duration)
	{
		if (!CurrentStatus.Contains(StatusType.Burn))
		{
			Debug.Log("화상 상태가 아닙니다.");
			yield break;
		}

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
		if (!CurrentStatus.Contains(StatusType.Poison))
		{
			Debug.Log("독 상태가 아닙니다.");
			yield break;
		}

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
		if (!CurrentStatus.Contains(StatusType.Binding))
		{
			Debug.Log("속박 상태가 아닙니다.");
			yield break;
		}

		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
		yield return null;
	}

	public void SetFreeze(float duration) => _routineClass?.StartCoroutine(FreezeRoutine(duration));

	IEnumerator FreezeRoutine(float duration)
	{
		if (!CurrentStatus.Contains(StatusType.Freeze))
		{
			Debug.Log("동상 상태가 아닙니다.");
			yield break;
		}

		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
		yield return null;
	}

	public void SetParalysis(float duration) => _routineClass?.StartCoroutine(ParalysisRoutine(duration));

	IEnumerator ParalysisRoutine(float duration)
	{
		if (!CurrentStatus.Contains(StatusType.Paralysis))
		{
			Debug.Log("마비 상태가 아닙니다.");
			yield break;
		}
	}

	public void SetConfusion(float duration) => _routineClass?.StartCoroutine(ConfusionRoutine(duration));
	IEnumerator ConfusionRoutine(float duration)
	{
		if (!CurrentStatus.Contains(StatusType.Confusion))
		{
			Debug.Log("혼란 상태가 아닙니다.");
			yield break;
		}
	}

	public void SetStun(float duration) => _routineClass?.StartCoroutine(StunRoutine(duration));
	IEnumerator StunRoutine(float duration)
	{
		if (!CurrentStatus.Contains(StatusType.Stun))
		{
			Debug.Log("기절 상태가 아닙니다.");
			yield break;
		}
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
	}
	public void SetSleep(float duration) => _routineClass?.StartCoroutine(SleepRoutine(duration));
	IEnumerator SleepRoutine(float duration)
	{
		if (!CurrentStatus.Contains(StatusType.Sleep))
		{
			Debug.Log("기절 상태가 아닙니다.");
			yield break;
		}
		PlayerController pc = _routineClass as PlayerController;
		Enemy enemy = _routineClass as Enemy;

		if (pc != null) pc.View.SetStop();
		if (enemy != null) enemy.StopMove();
	}
}
