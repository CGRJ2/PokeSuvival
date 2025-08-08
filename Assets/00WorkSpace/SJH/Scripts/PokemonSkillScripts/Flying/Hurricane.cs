using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Hurricane : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + attackDir;
		//Quaternion rot = Quaternion.FromToRotation(Vector2.up, attackDir.normalized);

		// 플레이어 1초 스턴 및 이펙트용 스프라이트 재생
		// 폭풍 스프라이트 발사

		// 이펙트용
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, Quaternion.identity);
		PlayerManager.Instance.StartCoroutine(ShootRoutine(go, spawnPos, attacker, attackDir, attackerData, skill));
	}

	IEnumerator ShootRoutine(GameObject effect, Vector2 spawnPos, Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		var pc = attackerData.PC;
		if (pc != null) pc.Status.SetStun(1);
		else attacker.GetComponent<Enemy>()?.Status?.SetStun(1);

		yield return new WaitForSeconds(1f);

		PhotonNetwork.Destroy(effect);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/HurricaneEffect", spawnPos, Quaternion.identity);
		Projectile aoe = go.GetComponent<Projectile>();
		if (aoe != null) aoe.Init(attacker, attackDir, attackerData, skill);
		else
		{
			if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(go);
		}
	}
}
