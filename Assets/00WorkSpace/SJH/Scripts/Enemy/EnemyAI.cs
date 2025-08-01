using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyAI
{
	private Enemy _enemy;
	private EnemyData _enemyData;

	[SerializeField] private AIState _prevState;
	public AIState CurrentState;

	// Idle
	private float _idleStayTime;

	// Move
	private float _nextMoveTime;
	private float _moveTime;
	private float _minRange = 0.9f;
	private Vector2[] _directions = new Vector2[]
	{
		Vector2.zero,
		Vector2.up,
		Vector2.down,
		Vector2.left,
		Vector2.right,
		(Vector2.up + Vector2.left).normalized,
		(Vector2.up + Vector2.right).normalized,
		(Vector2.down + Vector2.left).normalized,
		(Vector2.down + Vector2.right).normalized,
	};
	// Attack
	private float _searchRange = 10f;
	private float _globalCooldown = 2f;
	private float _gcdEndTime = 0;
	public GameObject TargetPlayer;
	private Collider2D[] _players;

	public EnemyAI(Enemy enemy, EnemyData enemyData)
	{
		_enemy = enemy;
		_enemyData = enemyData;

		// 상태 초기화
		_prevState = AIState.None;
		CurrentState = AIState.Idle;
		// OverlapCircleNonAlloc
		_players = new Collider2D[20];
	}

	public void EnemyAction()
	{
		if (_enemyData.IsDead && CurrentState != AIState.Die) ChangeState(AIState.Die);

		if (CurrentState != _prevState)
		{
			OnStateEnd(_prevState);
			//Debug.Log($"{_prevState} 상태 종료");
			OnStateStart(CurrentState);
			//Debug.Log($"{CurrentState} 상태 시작");
			_prevState = CurrentState;
		}
		OnStateUpdate(CurrentState);
		//Debug.Log($"{CurrentState} 상태 진행");
	}
	public void ChangeState(AIState nextState) => CurrentState = nextState;
	void OnStateStart(AIState state)
	{
		switch (state)
		{
			case AIState.Idle: IdleStartAction(); break;
			case AIState.Move: MoveStartAction(); break;
			case AIState.Attack: AttackStartAction(); break;
			case AIState.Die: DieStartAction(); break;
		}
	}
	void OnStateUpdate(AIState state)
	{
		switch (state)
		{
			case AIState.Idle: IdleUpdateAction(); break;
			case AIState.Move: MoveUpdateAction(); break;
			case AIState.Attack: AttackUpdateAction(); break;
			case AIState.Die: DieUpdateAction(); break;
		}
	}
	void OnStateEnd(AIState state)
	{
		switch (state)
		{
			case AIState.Idle: IdleEndAction(); break;
			case AIState.Move: MoveEndAction(); break;
			case AIState.Attack: AttackEndAction(); break;
			case AIState.Die: DieEndAction(); break;
		}
	}
	#region Idle Action
	void IdleStartAction()
	{
		_enemy.StopMove();

		// 랜덤 이동 방향 설정
		int dirIndex = UnityEngine.Random.Range(0, _directions.Length);
		Vector2 newDir = _directions[dirIndex];
		_enemy.MoveDir = newDir;

		// Idle 상태에서 대기
		_idleStayTime = UnityEngine.Random.Range(1f, 3f);
		_nextMoveTime = Time.time + _idleStayTime;
	}
	void IdleUpdateAction()
	{
		TargetPlayer = FindPlayer();
		// 플레이어가 있으면 이동
		if (TargetPlayer != null)
		{
			ChangeState(AIState.Move);
			return;
		}
		// 대기 시간이 지났으면 이동
		if (Time.time >= _nextMoveTime)
		{
			ChangeState(AIState.Move);
		}
		// 대기
	}
	void IdleEndAction() { }
	#endregion
	#region Move Action
	void MoveStartAction()
	{
		if (TargetPlayer == null) TargetPlayer = FindPlayer();

		if (TargetPlayer != null)
		{
			float dist = Vector2.Distance(_enemy.transform.position, TargetPlayer.transform.position);
			Vector2 playerDir = (TargetPlayer.transform.position - _enemy.transform.position).normalized;
			_enemy.MoveDir = playerDir;
			_enemy.SetDirAnim(playerDir);
			// 최소거리에 있으면 이동 없이 방향만 전환
			if (dist < _minRange)
			{
				_enemy.StopMove();
				ChangeState(AIState.Attack);
				return;
			}
		}

		// 플레이어가 없으면 랜덤이동 시간 지정
		if (TargetPlayer == null)
		{
			_moveTime = UnityEngine.Random.Range(1f, 3f);
			_nextMoveTime = Time.time + _moveTime;
		}
		
		_enemy.Move();
	}
	void MoveUpdateAction()
	{
		// 타겟이 있으면 공격 상태로 전환 (Start에서 이동 시작)
		if (TargetPlayer != null)
		{
			Vector2 toTarget = TargetPlayer.transform.position - _enemy.transform.position;

			if (toTarget.sqrMagnitude <= _minRange * _minRange)
			{
				_enemy.StopMove();
				ChangeState(AIState.Attack);
				return;
			}

			Vector2 playerDir = toTarget.normalized;
			_enemy.MoveDir = playerDir;
			_enemy.SetDirAnim(playerDir);
			_enemy.Move();
			ChangeState(AIState.Attack);
			return;
		}
		// 타겟이 없으면 랜덤이동 후 Idle로
		else
		{
			if (Time.time >= _nextMoveTime)
			{
				ChangeState(AIState.Idle);
				return;
			}
			_enemy.Move();
		}
	}
	void MoveEndAction()
	{
		if (TargetPlayer == null) ChangeState(AIState.Idle);
	}
	#endregion
	#region Attack Action
	void AttackStartAction()
	{
		// 타겟이 없으면 대기 상태로
		if (TargetPlayer == null)
		{
			ChangeState(AIState.Idle);
			return;
		}
	}
	void AttackUpdateAction()
	{
		// 글쿨이면 이동으로
		if (Time.time < _gcdEndTime)
		{
			ChangeState(AIState.Move);
			return;
		}
		float dist = Vector2.Distance(_enemy.transform.position, TargetPlayer.transform.position);

		// 타겟이 멀면 초기화
		if (dist > _searchRange * 1.5f)
		{
			TargetPlayer = null;
			ChangeState(AIState.Idle);
			return;
		}

		// 랜덤 스킬 선택
		if (CanUseSkill(out SkillSlot slot, out PokemonSkill skill))
		{
			// 사거리가 안되면 다시 이동
			if (dist > skill.Range)
			{
				ChangeState(AIState.Move);
				return;
			}

			// 애니메이션 변경을 위해 지정
			_enemy.SetDirAnim(_enemy.LastDir);
			_enemy.Attack(slot);
			_gcdEndTime = Time.time + _globalCooldown;
		}
		// 사용할 스킬이 없으면 다시 이동으로
		else
		{
			ChangeState(AIState.Move);
			return;
		}
	}
	void AttackEndAction()
	{
		//if (TargetPlayer == null) ChangeState(AIState.Idle);
	}
	#endregion
	#region Die Action
	void DieStartAction()
	{
		_enemy.StopMove();
		_enemy.SetIsDead(true);
	}
	void DieUpdateAction() { }
	void DieEndAction() { }
	#endregion

	GameObject FindPlayer()
	{
		int playerCount = Physics2D.OverlapCircleNonAlloc(_enemy.transform.position, _searchRange, _players, _enemy.PlayerLayer);

		if (playerCount == 0) return null;

		GameObject target = null;
		PlayerController targetPC = null;
		float nearDistance = float.MaxValue;
		for (int i = 0; i < playerCount; i++)
		{
			if (_players[i] == null) continue;

			var pc = _players[i].GetComponent<PlayerController>();
			if (pc == null || pc.Model.IsDead) continue;

			float distance = Vector2.Distance(_enemy.transform.position, _players[i].transform.position);
			if (distance < nearDistance)
			{
				nearDistance = distance;
				target = _players[i].gameObject;
			}
		}
		if (targetPC != null) Debug.Log($"AI 공격할 대상 찾음 : {targetPC.Model.PlayerName}");
		return target;
	}

	bool CanUseSkill(out SkillSlot slot, out PokemonSkill skill)
	{
		skill = null;
		slot = SkillSlot.Skill1;

		var canUseSkills = new List<SkillSlot>();
		foreach (SkillSlot skillSlot in _enemyData.SkillCooldownDic.Keys)
		{
			if (!_enemyData.IsSkillCooldown(skillSlot) && _enemyData.GetSkill((int)skillSlot) != null)
			{
				canUseSkills.Add(skillSlot);
			}
		}
		if (canUseSkills.Count == 0) return false;

		slot = canUseSkills[UnityEngine.Random.Range(0, canUseSkills.Count)];
		skill = _enemyData.GetSkill((int)slot);

		if (skill == null) return false;

		return true;
	}
}
