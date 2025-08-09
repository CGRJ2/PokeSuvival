using Photon.Pun;
using UnityEngine;

public class Metronome : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", Vector3.zero, Quaternion.identity);
		go.transform.SetParent(attacker, false);
	}
}
