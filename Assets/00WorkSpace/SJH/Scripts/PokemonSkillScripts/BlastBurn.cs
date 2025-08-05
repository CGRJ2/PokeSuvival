using Photon.Pun;
using UnityEngine;

public class BlastBurn : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", attacker.transform.position, Quaternion.identity);
		AOESkill aoe = go.GetComponent<AOESkill>();
		if (aoe != null) aoe.Init(attacker, attackDir, attackerData, skill);
		else
		{
			if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(go);
		}
	}
}
