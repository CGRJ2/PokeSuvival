using Photon.Pun;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AOE_SandTomb : AOE_Skill
{
	[SerializeField] private Collider2D[] _enemies;
	[SerializeField] private List<Transform> _hitTargets;
	[SerializeField] private int _tickCount;
	[SerializeField] private int _currentCount;

	public override void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		base.Init(attacker, attackDir, attackerData, skill);

		_enemies = new Collider2D[30];
		_hitTargets = new();
		_hitTargets.Add(transform);
		_hitTargets.Add(_attacker);

		var pc = attackerData.PC;
		if (pc != null) pc.Status.SetStun(1);
		else attacker.GetComponent<Enemy>()?.Status?.SetStun(1);

		Random.InitState(attacker.GetInstanceID());
		_tickCount = Random.Range(4, 6);
		_currentCount = 0;
	}

	// 애니메이션 이벤트 함수로 연결
	public void Attack1()
	{
		if (!photonView.IsMine) return;

		int count = Physics2D.OverlapCircleNonAlloc(transform.position, _skill.Range, _enemies);
		//Debug.Log($"블라스트번 1타 : {count}");
		if (count <= 0) return;
		Attack(count);
	}
	void Attack(int count)
	{
		if (!photonView.IsMine) return;

		_currentCount++;

		//Debug.Log($"{count} 마리 공격");
		for (int i = 0; i < count; i++)
		{
			var enemy = _enemies[i];
			if (_hitTargets.Contains(enemy.transform)) continue;

			var iD = enemy.GetComponent<IDamagable>();
			if (iD == null) continue;

			if (iD.TakeDamage(_attackerData, _skill)) PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{_skill.name}Effect", enemy.transform.position, Quaternion.identity);
		}

		if (_tickCount <= _currentCount) PhotonNetwork.Destroy(gameObject);
	}
}
