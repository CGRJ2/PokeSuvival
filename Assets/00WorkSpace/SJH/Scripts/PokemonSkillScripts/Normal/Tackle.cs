using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tackle : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		PlayerManager.Instance.StartCoroutine(MoveAttackRoutine(attacker, attackDir, attackerData, skill));
	}

	IEnumerator MoveAttackRoutine(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		float duration = 0.2f;
		float time = 0f;

		Vector3 spawnPos = attackDir * (attackerData.PokeData.PokeSize + 1f);
		GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, Quaternion.identity);

		float radius = skill.Range / 2;
		float size = attackerData.PokeData.PokeSize;
		float range = size > 1 ? skill.Range + size : skill.Range;

		Vector2 startPos = attacker.position;
		Vector2 targetPos = startPos + attackDir * range;
		List<Transform> hitTargets = new();

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;
			Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);

			go.transform.position = newPos + (attackDir * (attackerData.PokeData.PokeSize + 1f));
			attacker.position = newPos;

			var enemies = Physics2D.OverlapCircleAll(attacker.position, radius);
			foreach (var enemy in enemies)
			{
				if (attacker == enemy.transform || hitTargets.Contains(enemy.transform)) continue;

				Vector2 dir = (enemy.transform.position - attacker.position).normalized;
				if (Vector2.Dot(attackDir, dir) >= 0.6f)
				{
					var iD = enemy.GetComponent<IDamagable>();
					if (iD == null) continue;
					iD.TakeDamage(attackerData, skill);
					hitTargets.Add(enemy.transform);
				}
			}
			yield return null;
		}
		PhotonNetwork.Destroy(go);
		Debug.Log($"{skill.SkillName} 공격 완료!");
	}
}
