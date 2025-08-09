using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronHead : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		PlayerManager.Instance.StartCoroutine(MoveAttackRoutine(attacker, attackDir, attackerData, skill));
	}

	IEnumerator MoveAttackRoutine(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		float duration = 1f;
		float time = 0f;

		Vector3 spawnPos = attackDir * (attackerData.PokeData.PokeSize + 1f);
		//Quaternion rot = Quaternion.FromToRotation(Vector2.up, attackDir.normalized);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, Quaternion.identity);
		go.transform.SetParent(attacker.transform, false);

		float radius = attackerData.PokeData.PokeSize * 2;
		float size = attackerData.PokeData.PokeSize;
		float range = size > 1 ? skill.Range + size : skill.Range;

		Vector2 startPos = attacker.position;
		Vector2 targetPos = startPos + attackDir * range;
		List<Transform> hitTargets = new();
		hitTargets.Add(attacker);

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;
			attacker.position = Vector2.Lerp(startPos, targetPos, t);

			var enemies = Physics2D.OverlapCircleAll(attacker.position, radius);
			foreach (var enemy in enemies)
			{
				if (hitTargets.Contains(enemy.transform)) continue;

				Vector2 dir = (enemy.transform.position - attacker.position).normalized;
				if (Vector2.Dot(attackDir, dir) >= 0.1f)
				{
					var iD = enemy.GetComponent<IDamagable>();
					if (iD == null) continue;
					iD.TakeDamage(attackerData, skill);
					hitTargets.Add(enemy.transform);
					PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.name}Effect", enemy.transform.position, Quaternion.identity);
				}
			}

			yield return null;
		}
		PhotonNetwork.Destroy(go);
		Debug.Log($"{skill.SkillName} 공격 완료!");
	}
}
