using Photon.Pun;
using UnityEngine;

public class SunnyDay : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + (Vector2.up * 3);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, Quaternion.identity);

		// 쾌청 이펙트 생성
		// 쾌청 버프 적용 코루틴
		// 유저
		if (attackerData.PC != null)
		{
			var pc = attackerData.PC;
			pc.Buff.SetBuff(skill);
		}
		// 몬스터
		else
		{
			var enemy = attacker.GetComponent<Enemy>();
			enemy.photonView.RPC(nameof(enemy.RPC_SetBuff), RpcTarget.AllBuffered);
		}
	}
}
