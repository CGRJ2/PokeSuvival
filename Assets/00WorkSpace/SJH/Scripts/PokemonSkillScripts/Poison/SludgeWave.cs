using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SludgeWave : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", attacker.transform.position, Quaternion.identity);
		AOE_SludgeWave aoe = go.GetComponent<AOE_SludgeWave>();
		if (aoe != null) aoe.Init(attacker, attackDir, attackerData, skill);
		else
		{
			if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(go);
		}
	}
}
