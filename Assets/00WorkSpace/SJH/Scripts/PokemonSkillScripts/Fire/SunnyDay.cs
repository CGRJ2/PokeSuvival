using Photon.Pun;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class SunnyDay : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + (Vector2.up * 3);
		// 스킬 이펙트
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, Quaternion.identity);
		// 버프 이펙트
		GameObject skillGo = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/SunnyDayEffect", Vector3.zero, Quaternion.identity);
		skillGo.transform.SetParent(attacker.transform, false);

		// 플레이어
		if (attackerData.PC != null)
		{
			var pc = attackerData.PC;
			pc.Buff.SetBuff(skill);
			pc.StartCoroutine(BuffEffectDestroy(skillGo, skill));
		}
		// 몬스터
		else
		{
			var enemy = attacker.GetComponent<Enemy>();
			enemy.photonView.RPC(nameof(enemy.RPC_SetBuff), RpcTarget.AllBuffered);
			enemy.StartCoroutine(BuffEffectDestroy(skillGo, skill));
		}
	}

	IEnumerator BuffEffectDestroy(GameObject go, PokemonSkill skill)
	{
		yield return new WaitForSeconds(skill.StatusDuration);
		PhotonNetwork.Destroy(go);
	}
}
