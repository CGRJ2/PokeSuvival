using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHitEffectNoDestroy : Projectile
{
	// Rigidbody2D _rigid;
	// Transform _attacker;
	// BattleDataTable _attackerData;
	// PokemonSkill _skill;
	// Vector2 _startPos;
	// float _speed;
	// float _endDistance;
	[SerializeField] private List<Transform> _hitTargets = new();

	public override void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		base.Init(attacker, attackDir, attackerData, skill);

		_hitTargets = new();
		_hitTargets.Add(attacker);
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (!photonView.IsMine) return;

		if (collision.TryGetComponent(out IDamagable target))
		{
			if (_hitTargets.Contains(collision.transform)) return;

			if (target.TakeDamage(_attackerData, _skill))
			{
				// 타겟 위치에 이펙트 생성
				// 파괴는 프리팹에 있는 EffectAutoDestroy 에서 재생이 끝나면 자동 파괴
				Debug.Log($"{_skill.name}Effect 생성!"); // ex EnerygyBallEffect
				var mono = target as MonoBehaviour;
				Vector3 spawnPos = mono.transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
				PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{_skill.name}Effect", spawnPos, Quaternion.identity);
				_hitTargets.Add(collision.transform);
			}
		}
	}
}
