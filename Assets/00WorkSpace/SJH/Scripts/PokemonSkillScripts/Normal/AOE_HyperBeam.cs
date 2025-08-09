using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOE_HyperBeam : AOE_Skill
{
	[SerializeField] private Collider2D[] _enemies;
	[SerializeField] private List<Transform> _hitTargets;

	[SerializeField] private BoxCollider2D _coll;

	public override void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		base.Init(attacker, attackDir, attackerData, skill);

		_enemies = new Collider2D[30];
		_hitTargets = new();
		_hitTargets.Add(transform);
		_hitTargets.Add(_attacker);

		var pc = attackerData.PC;
		if (pc != null) pc.Status.SetStun(2.5f);
		else attacker.GetComponent<Enemy>()?.Status?.SetStun(2.5f);

		_coll.enabled = false;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (!photonView.IsMine) return;
		// 중복대상 제외
		if (_hitTargets.Contains(collision.transform)) return;

		if (collision.TryGetComponent(out IDamagable target))
		{
			// 대미지 적용 후 리스트 추가
			if (target.TakeDamage(_attackerData, _skill)) _hitTargets.Add(collision.transform);
		}
	}

	public void OnCollider() => _coll.enabled = true;
	public void OffCollider() => _coll.enabled = false;
}
