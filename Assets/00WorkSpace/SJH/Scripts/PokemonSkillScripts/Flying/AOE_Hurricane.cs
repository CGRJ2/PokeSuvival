using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOE_Hurricane : AOE_Skill
{
	[SerializeField] private Collider2D[] _enemies;
	[SerializeField] private List<Transform> _hitTargets;

	[SerializeField] private BoxCollider2D _coll;
	[SerializeField] private float _speed = 10;
	[SerializeField] private Vector2 _startPos;
	[SerializeField] private float _endDistance;

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

		_startPos = transform.position;
		float size = attackerData.PokeData.PokeSize;
		_endDistance = size > 1 ? _skill.Range + size : _skill.Range;
		_rigid.velocity = attackDir * _speed;
	}
	void Update()
	{
		if (!photonView.IsMine) return;

		float distance = Vector2.Distance(_startPos, transform.position);
		if (distance >= _endDistance)
		{
			Debug.Log($"최대 사거리[{_endDistance}] 이동완료 오브젝트 파괴");
			PhotonNetwork.Destroy(gameObject);
		}
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
}
