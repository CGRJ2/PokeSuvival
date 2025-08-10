using Photon.Pun;
using UnityEngine;

public class AOE_Skill : MonoBehaviourPun
{
	[SerializeField] protected Rigidbody2D _rigid;
	[SerializeField] protected Transform _attacker;
	[SerializeField] protected BattleDataTable _attackerData;
	[SerializeField] protected PokemonSkill _skill;
	[SerializeField] protected float _size;

	public virtual void Init(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		_attacker = attacker;
		_attackerData = attackerData;
		_skill = skill;

		_size = attackerData.PokeData.PokeSize;
		transform.localScale = _size > 1 ? Vector3.one * _size : Vector3.one;
	}
}
