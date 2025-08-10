using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Discharge : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		PlayerManager.Instance.StartCoroutine(ChargeRoutine(attacker, attackDir, attackerData, skill));
	}

	IEnumerator ChargeRoutine(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", attacker.transform.position, Quaternion.identity);

		var pc = attackerData.PC;
		if (pc != null) pc.Status.SetStun(1);
		else attacker.GetComponent<Enemy>()?.Status?.SetStun(1);

		yield return new WaitForSeconds(1f);

		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/DischargeEffect", attacker.transform.position, Quaternion.identity);
		AOE_Discharge aoe = go.GetComponent<AOE_Discharge>();
		if (aoe != null) aoe.Init(attacker, attackDir, attackerData, skill);
		else
		{
			if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(go);
		}
	}
}
