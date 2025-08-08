using Photon.Pun;
using System.Collections;
using UnityEngine;

public class BubbleBeam : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + attackDir;
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, Quaternion.identity);
		ProjectileHitEffect projectile = go.GetComponent<ProjectileHitEffect>();
		if (projectile != null) projectile.Init(attacker, attackDir, attackerData, skill);
		else
		{
			if (PhotonNetwork.IsMasterClient) attackerData.PC.StartCoroutine(DestroyPrefab(go));
		}
	}

	IEnumerator DestroyPrefab(GameObject go)
	{
		yield return new WaitForSeconds(0.1f);
		PhotonNetwork.Destroy(go);
	}
}
