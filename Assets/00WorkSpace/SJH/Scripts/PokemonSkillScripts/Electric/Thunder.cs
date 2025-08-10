using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Thunder : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + attackDir;
		Quaternion rot = Quaternion.FromToRotation(Vector2.right, attackDir.normalized);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, rot);
		ProjectileHitEffectNoDestroy projectile = go.GetComponent<ProjectileHitEffectNoDestroy>();
		if (projectile != null) projectile.Init(attacker, attackDir, attackerData, skill);
		else
		{
			PlayerManager.Instance?.StartCoroutine(DestroyPrefab(go));
		}
	}

	IEnumerator DestroyPrefab(GameObject go)
	{
		yield return new WaitForSeconds(0.1f);
		PhotonNetwork.Destroy(go);
	}
}
