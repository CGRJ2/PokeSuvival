using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class AOESkill : MonoBehaviourPun
{
	[SerializeField] protected Rigidbody2D _rigid;
	[SerializeField] protected Transform _attacker;
	[SerializeField] protected BattleDataTable _attackerData;
	[SerializeField] protected PokemonSkill _skill;
	[SerializeField] protected Vector2 _startPos;
	[SerializeField] protected float _speed;
	[SerializeField] protected float _size;
	[SerializeField] protected float _radius;
	[SerializeField] protected Collider2D[] _enemies;
	[SerializeField] protected List<Transform> _hitTargets;

	public void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		_attacker = attacker;
		_attackerData = attackerData;
		_skill = skill;
		_startPos = transform.position;

		_size = attackerData.PokeData.PokeSize;
		transform.localScale = _size > 1 ? Vector3.one * _size : Vector3.one;

		_enemies = new Collider2D[30];
		_hitTargets = new();
		_hitTargets.Add(transform);
		_hitTargets.Add(_attacker);
	}

	// 애니메이션 이벤트 함수로 연결
	public void Attack1()
	{
		if (!photonView.IsMine) return;

		int count = Physics2D.OverlapCircleNonAlloc(transform.position, 1.5f, _enemies);
		Debug.Log($"블라스트번 1타 : {count}");
		if (count <= 0) return;
		Attack(count);
	}

	public void Attack2()
	{
		if (!photonView.IsMine) return;

		int count = Physics2D.OverlapCircleNonAlloc(transform.position, 2f, _enemies);
		Debug.Log($"블라스트번 2타 : {count}");
		if (count <= 0) return;
		Attack(count);
	}

	public void Attack3()
	{
		if (!photonView.IsMine) return;

		int count = Physics2D.OverlapCircleNonAlloc(transform.position, 2.5f, _enemies);
		Debug.Log($"블라스트번 3타 : {count}");
		if (count <= 0) return;
		Attack(count);
	}

	void Attack(int count)
	{
		if (!photonView.IsMine) return;

		Debug.Log($"{count} 마리 공격");
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
