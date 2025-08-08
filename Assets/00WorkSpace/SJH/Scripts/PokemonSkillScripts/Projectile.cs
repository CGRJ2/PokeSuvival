using Photon.Pun;
using UnityEngine;

public class Projectile : MonoBehaviourPun
{
	[SerializeField] protected Rigidbody2D _rigid;
	[SerializeField] protected Transform _attacker;
	[SerializeField] protected BattleDataTable _attackerData;
	[SerializeField] protected PokemonSkill _skill;
	[SerializeField] protected Vector2 _startPos;
	[SerializeField] protected float _speed;
	[SerializeField] protected float _endDistance;

	public void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		_attacker = attacker;
		_attackerData = attackerData;
		_skill = skill;
		_startPos = transform.position;

		_rigid.velocity = attackDir * _speed;
		float size = attackerData.PokeData.PokeSize;
		_endDistance = size > 1 ? _skill.Range + size : _skill.Range;
	}

	void Update()
	{
		if (!photonView.IsMine) return;

		float distance = Vector2.Distance(_startPos, transform.position);
		if (distance >= _endDistance) PhotonNetwork.Destroy(gameObject);
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (!photonView.IsMine) return;
		if (_attacker == collision.transform) return;
		if (collision.TryGetComponent(out IDamagable target))
		{
			if (target.TakeDamage(_attackerData, _skill)) PhotonNetwork.Destroy(gameObject);
		}
	}
}
