using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : IAttack
{

	public MeleeAttack()
	{

	}

	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		switch (skill.SkillName)
		{
			case "할퀴기": FrontAttack(attacker, attackDir, attackerData, skill); break;
			case "몸통박치기": MoveAttack(attacker, attackDir, attackerData, skill); break;
		}
	}

	void FrontAttack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		var enemies = Physics2D.OverlapCircleAll((Vector2)attacker.position, skill.Range);
		if (enemies.Length <= 0) return;

		foreach (var enemy in enemies)
		{
			if (attacker == enemy.transform) continue;

			Vector2 dir = (enemy.transform.position - attacker.position).normalized;
			if (Vector2.Dot(attackDir, dir) >= 0.7f) // 45
			{
				var iD = enemy.GetComponent<IDamagable>();
				var pv = enemy.GetComponent<PhotonView>();
				if (iD == null || pv == null) continue;
				int damage = PokeUtils.CalculateDamage(attackerData, iD.BattleData, skill);
				//pv.RPC("RPC_TakeDamage", pv.Owner, damage);
				iD.TakeDamage(damage);
				PlayerManager.Instance?.ShowDamageText(pv.gameObject.transform, damage, Color.white);
				Debug.Log($"Lv.{attackerData.Level} {attackerData.PokeData.PokeName} 이/가 Lv.{iD.BattleData.Level} {iD.BattleData.PokeData.PokeName} 을/를 {skill.SkillName} 공격!");
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}

	void MoveAttack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		PlayerManager.Instance.StartCoroutine(MoveAttackRoutine(attacker, attackDir, attackerData, skill));
	}

	IEnumerator MoveAttackRoutine(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
	{
		float duration = 0.2f;
		float time = 0f;
		float radius = skill.Range / 2;

		Vector2 startPos = attacker.position;
		Vector2 targetPos = startPos + attackDir * skill.Range;
		List<Transform> hitTargets = new();

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;
			attacker.position = Vector2.Lerp(startPos, targetPos, t);

			var enemies = Physics2D.OverlapCircleAll(attacker.position, radius);
			foreach (var enemy in enemies)
			{
				if (attacker == enemy.transform || hitTargets.Contains(enemy.transform)) continue;

				Vector2 dir = (enemy.transform.position - attacker.position).normalized;
				if (Vector2.Dot(attackDir, dir) >= 0.7f)
				{
					var iD = enemy.GetComponent<IDamagable>();
					var pv = enemy.GetComponent<PhotonView>();
					if (iD == null || pv == null) continue;
					int damage = PokeUtils.CalculateDamage(attackerData, iD.BattleData, skill);
					//pv.RPC("RPC_TakeDamage", pv.Owner, damage);
					iD.TakeDamage(damage);
					PlayerManager.Instance?.ShowDamageText(pv.gameObject.transform, damage, Color.white);
					hitTargets.Add(enemy.transform);

					Debug.Log($"Lv.{attackerData.Level} {attackerData.PokeData.PokeName} 이/가 Lv.{iD.BattleData.Level} {iD.BattleData.PokeData.PokeName} 을/를 {skill.SkillName} 공격!");
				}
			}

			yield return null;
		}
		Debug.Log($"{skill.SkillName} 공격 완료!");
	}
}
