﻿using Photon.Pun;
using UnityEngine;

public class RazorLeaf : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		Vector2 spawnPos = (Vector2)attacker.position + attackDir;
		Quaternion rot = Quaternion.FromToRotation(Vector2.up, attackDir.normalized);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, rot);
		Projectile projectile = go.GetComponent<Projectile>();
		if (projectile != null) projectile.Init(attacker, attackDir, attackerData, skill);
		else PhotonNetwork.Destroy(go);
	}
}
