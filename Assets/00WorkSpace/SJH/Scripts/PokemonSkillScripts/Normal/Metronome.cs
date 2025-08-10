using Photon.Pun;
using UnityEngine;

public class Metronome : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		var pc = attackerData.PC;
		if (pc != null) pc.Status.SetStun(1f);
		else attacker.GetComponent<Enemy>()?.Status?.SetStun(1f);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", attacker.transform.position, Quaternion.identity);
	}
}
