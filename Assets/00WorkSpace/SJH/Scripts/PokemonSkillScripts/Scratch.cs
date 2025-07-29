using Photon.Pun;
using UnityEngine;

public class Scratch : IAttack
{
	public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
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
				if (iD == null) continue;
				iD.TakeDamage(attackerData, skill);
				//var pv = enemy.GetComponent<PhotonView>();
				//if (iD == null || pv == null) continue;
				//var defenderData = iD.BattleData;
				//if (defenderData.CurrentHp <= 0) continue;
				//int damage = PokeUtils.CalculateDamage(attackerData, defenderData, skill);
				////pv.RPC("RPC_TakeDamage", pv.Owner, damage);
				//iD.TakeDamage(damage);
				//PlayerManager.Instance?.ShowDamageText(pv.gameObject.transform, damage, Color.white);
				//Debug.Log($"Lv.{attackerData.Level} {attackerData.PokeData.PokeName} 이/가 Lv.{defenderData.Level} {defenderData.PokeData.PokeName} 을/를 {skill.SkillName} 공격!");
			}
		}
		Debug.Log($"{skill.SkillName} 공격!");
	}
}
