using Photon.Pun;
using UnityEngine;

public class HydorPump : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + attackDir;
		Quaternion rot = Quaternion.FromToRotation(Vector2.up, attackDir.normalized);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, rot);
		AOE_HydorPump aoe = go.GetComponent<AOE_HydorPump>();
		if (aoe != null) aoe.Init(attacker, attackDir, attackerData, skill);
		else
		{
			if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(go);
		}
	}
}
