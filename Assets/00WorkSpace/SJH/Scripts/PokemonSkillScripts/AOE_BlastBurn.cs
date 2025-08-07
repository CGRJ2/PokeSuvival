using System.Collections.Generic;
using UnityEngine;

public class AOE_BlastBurn : AOE_Skill
{
	[SerializeField] private Collider2D[] _enemies;
	[SerializeField] private List<Transform> _hitTargets;

	public override void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		base.Init(attacker, attackDir, attackerData, skill);

		_enemies = new Collider2D[30];
		_hitTargets = new();
		_hitTargets.Add(transform);
		_hitTargets.Add(_attacker);

		var pc = attackerData.PC;
		if (pc == null) attacker.GetComponent<PlayerController>();
		pc.Status.SetFreeze(1);
	}

	// 애니메이션 이벤트 함수로 연결
	public void Attack1()
	{
		if (!photonView.IsMine) return;

		int count = Physics2D.OverlapCircleNonAlloc(transform.position, 1.5f, _enemies);
		//Debug.Log($"블라스트번 1타 : {count}");
		if (count <= 0) return;
		Attack(count);
	}

	public void Attack2()
	{
		if (!photonView.IsMine) return;

		int count = Physics2D.OverlapCircleNonAlloc(transform.position, 2f, _enemies);
		//Debug.Log($"블라스트번 2타 : {count}");
		if (count <= 0) return;
		Attack(count);
	}

	public void Attack3()
	{
		if (!photonView.IsMine) return;

		int count = Physics2D.OverlapCircleNonAlloc(transform.position, 2.5f, _enemies);
		//Debug.Log($"블라스트번 3타 : {count}");
		if (count <= 0) return;
		Attack(count);
	}

	void Attack(int count)
	{
		if (!photonView.IsMine) return;

		//Debug.Log($"{count} 마리 공격");
		for (int i = 0; i < count; i++)
		{
			var enemy = _enemies[i];
			if (_hitTargets.Contains(enemy.transform)) continue;

			var iD = enemy.GetComponent<IDamagable>();
			if (iD == null) continue;

			iD.TakeDamage(_attackerData, _skill);
			_hitTargets.Add(enemy.transform);
		}
	}
}
