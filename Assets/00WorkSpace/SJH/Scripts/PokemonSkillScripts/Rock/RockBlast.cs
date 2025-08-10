using Photon.Pun;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class RockBlast : IAttack
{
    Transform attacker;
    Vector2 attackDir;
    BattleDataTable attackerData;
    PokemonSkill skill;
    public void Attack(Transform attacker, Vector2 attackDir, BattleDataTable attackerData, PokemonSkill skill)
    {
        this.attacker = attacker;
        this.attackDir = attackDir;
        this.attackerData = attackerData;
        this.skill = skill;

        int r = Random.Range(2, 5);

        if (attackerData.PC != null)
            attackerData.PC?.StartCoroutine(MultiShot(r));
        else
            attacker.GetComponent<Enemy>()?.StartCoroutine(MultiShot(r));
    }

    IEnumerator MultiShot(int r)
    {
        yield return null;
        for (int i = 0; i < r; i++)
        {
            Shot();
            yield return new WaitForSeconds(0.12f); // 0.12초 간격으로 발사
        }
    }

    private void Shot()
    {
        Vector2 spawnPos = (Vector2)attacker.position + attackDir;
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, attackDir.normalized);
        GameObject go = PhotonNetwork.Instantiate($"PokemonSkillPrefabs/{skill.EffectPrefab.name}", spawnPos, rot);
        ProjectileHitEffect projectile = go.GetComponent<ProjectileHitEffect>();
        if (projectile != null) projectile.Init(attacker, attackDir, attackerData, skill);
        else
        {
            if (PhotonNetwork.IsMasterClient) attackerData.PC.StartCoroutine(DestroyPrefab(go));
        }
    }


    IEnumerator DestroyPrefab(GameObject go)
    {
        yield return new WaitForSeconds(0.1f);
        PhotonNetwork.Destroy(go);
    }
}
