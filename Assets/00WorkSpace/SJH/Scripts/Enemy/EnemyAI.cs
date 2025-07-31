using System;
using UnityEngine;

[Serializable]
public class EnemyAI
{
	private Enemy _enemy;
	private EnemyData _enemyData;

	[SerializeField] private AIState _prevState;
	public AIState CurrentState;

	// Move
	private float _nextMoveTime;
	private float _moveTime;
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
	private float _skillRange;
	private float _searchRange = 10f;
	private float _attackDelay = 2f;
	private bool _isAttackDelay = false;
	private GameObject _targetPlayer;
	private Collider2D[] _players;

	public EnemyAI(Enemy enemy, EnemyData enemyData)
	{
		_enemy = enemy;
		_enemyData = enemyData;

		_prevState = AIState.None;
		CurrentState = AIState.Idle;

		foreach (var skill in _enemyData.PokeData.Skills)
		{
			_skillRange += skill.Range;
		}
		_skillRange /= _enemyData.PokeData.Skills.Length;

		_players = new Collider2D[20];
	}

	public void EnemyAction()
	{
		if (_enemyData.IsDead && CurrentState != AIState.Die) ChangeState(AIState.Die);

		if (CurrentState != _prevState)
		{
			OnStateEnd(_prevState);
			OnStateStart(CurrentState);
			_prevState = CurrentState;
		}
		OnStateUpdate(CurrentState);
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
	// Idle
	void IdleStartAction()
	{
		// TODO : 적탐색?
		_nextMoveTime = Time.time + UnityEngine.Random.Range(1f, 3f);
		_enemy.StopMove();
	}
	void IdleUpdateAction()
	{
		_targetPlayer = FindPlayer();

		// 적 발견이 이동
		if (_targetPlayer != null)
		{
			ChangeState(AIState.Move);
			return;
		}
		// 랜덤 이동
		if (Time.time >= _nextMoveTime)
		{
			int dirIndex = UnityEngine.Random.Range(0, _directions.Length);
			Vector2 newDir = _directions[dirIndex];
			_enemy.MoveDir = newDir;
			_moveTime = UnityEngine.Random.Range(1f, 3f);
			_nextMoveTime = Time.time + _moveTime;

			if (newDir != Vector2.zero)
			{
				ChangeState(AIState.Move);
				return;
			}
		}
	}
	void IdleEndAction() { }
	// Move
	void MoveStartAction()
	{
		_enemy.Move(_enemy.MoveDir);
	}
	void MoveUpdateAction()
	{
		_targetPlayer = FindPlayer();

		if (_targetPlayer != null)
		{
			Vector2 playerDir = (_targetPlayer.transform.position - _enemy.transform.position).normalized;
			float distance = Vector2.Distance(_enemy.transform.position, _targetPlayer.transform.position);
			if (distance < _skillRange)
			{
				ChangeState(AIState.Attack);
				return;
			}

			_enemy.MoveDir = playerDir;
			_enemy.Move(_enemy.MoveDir);
		}
		else
		{
			if (Time.time > _nextMoveTime)
			{
				ChangeState(AIState.Idle);
				return;
			}
		}
	}
	void MoveEndAction()
	{
		_enemy.StopMove();
	}
	// Attack
	void AttackStartAction()
	{
		_enemy.StopMove();
	}
	void AttackUpdateAction()
	{
		if (_targetPlayer == null)
		{
			ChangeState(AIState.Idle);
			return;
		}

		if (_targetPlayer == null) return;

		PokemonSkill canUseSkill = null;
		SkillSlot skillSlot = SkillSlot.Skill1;

		foreach (SkillSlot slot in _enemyData.SkillCooldownDic.Keys)
		{
			if (!_enemyData.IsSkillCooldown(slot))
			{
				int skillIndex = (int)slot;
				if (skillIndex < _enemyData.PokeData.Skills.Length)
				{
					canUseSkill = _enemyData.PokeData.Skills[skillIndex];
					skillSlot = slot;
					break;
				}
			}
		}

		if (canUseSkill != null)
		{
			float distanceToPlayer = Vector2.Distance(_enemy.transform.position, _targetPlayer.transform.position);

			if (distanceToPlayer > canUseSkill.Range)
			{
				ChangeState(AIState.Move);
				return;
			}

			if (canUseSkill.SkillAnimType == SkillAnimType.Attack) _enemy.SetIsAttack();
			else _enemy.SetIsSpeAttack();

			var damagable = _targetPlayer.GetComponent<IDamagable>();
			if (damagable != null)
			{
				damagable.TakeDamage(_enemy.BattleData, canUseSkill);
			}

			_enemyData.SetSkillCooldown(skillSlot, canUseSkill.Cooldown);

			Debug.Log($"{_enemyData.PokeName}이/가 {_targetPlayer.name}을/를 {canUseSkill.SkillName}으로 공격!");
		}
		else
		{
			ChangeState(AIState.Idle);
		}
	}
	void AttackEndAction()
	{
		_enemy.StopMove();
		ChangeState(AIState.Idle);
	}
	// Die
	void DieStartAction()
	{
		_enemy.StopMove();
		_enemy.SetIsDead(true);
	}
	void DieUpdateAction(){}
	void DieEndAction() { }
	//

	GameObject FindPlayer()
	{
		int playerCount = Physics2D.OverlapCircleNonAlloc(_enemy.transform.position, _searchRange, _players, _enemy.PlayerLayer);

		if (playerCount == 0) return null;

		GameObject target = null;
		float nearDistance = float.MaxValue;
		for (int i = 0; i < playerCount; i++)
		{
			if (_players[i] != null)
			{
				float distance = Vector2.Distance(_enemy.transform.position, _players[i].transform.position);
				if (distance < nearDistance)
				{
					nearDistance = distance;
					target = _players[i].gameObject;
				}
			}
		}
		return target;
	}
	private Vector2 GetCloseDirection(Vector2 targetDir)
	{
		Vector2 closestDir = _directions[1]; // Vector2.up
		float closestDot = -2f;

		for (int i = 1; i < _directions.Length; i++) // Vector2.zero 제외
		{
			float dot = Vector2.Dot(targetDir.normalized, _directions[i]);
			if (dot > closestDot)
			{
				closestDot = dot;
				closestDir = _directions[i];
			}
		}

		return closestDir;
	}
}
