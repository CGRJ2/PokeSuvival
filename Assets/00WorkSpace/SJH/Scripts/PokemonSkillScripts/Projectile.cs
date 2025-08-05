using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviourPun
{
    [SerializeField] private Rigidbody2D _rigid;
    [SerializeField] private Transform _attacker;
	[SerializeField] private BattleDataTable _attackerData;
	[SerializeField] private PokemonSkill _skill;
	[SerializeField] private Vector2 _startPos;
    [SerializeField] private float _speed;
    [SerializeField] private float _endDistance;

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

        if (collision.TryGetComponent(out IDamagable target))
        {
            if (_attacker == collision.transform) return;

			if (target.TakeDamage(_attackerData, _skill)) PhotonNetwork.Destroy(gameObject);

            //var defenderData = target.BattleData;
            //if (defenderData.CurrentHp <= 0) return;

            //int damage = PokeUtils.CalculateDamage(_attackerData, defenderData, _skill);
            //target.TakeDamage(damage);
            //PlayerManager.Instance?.ShowDamageText(collision.transform, damage, Color.white);
        }
	}
}
