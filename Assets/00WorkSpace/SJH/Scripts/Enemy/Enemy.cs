using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviourPun, IDamagable, IPunInstantiateMagicCallback, IPunObservable
{
	public EnemyData EnemyData;
	public EnemyAI EnemyAI;
	[SerializeField] private Rigidbody2D _rigid;
	[SerializeField] private SpriteRenderer _sprite;
	[SerializeField] private Animator _anim;

	[SerializeField] private bool _flipX;
	public Vector2 MoveDir = Vector2.down;
	public bool IsInit = false;
	[SerializeField] private float _deleteTime;
	[SerializeField] private float _moveSpeed = 0;
	public LayerMask PlayerLayer;
	public float MoveSpeed
	{
		get
		{
			if (_moveSpeed == 0 && EnemyData != null) _moveSpeed = EnemyData.GetMoveSpeed();
			return _moveSpeed;
		}
	}

	public BattleDataTable BattleData => new BattleDataTable(EnemyData.PokeLevel, EnemyData.PokeData, EnemyData.AllStat, EnemyData.MaxHp, EnemyData.CurrentHp);

	void Update()
	{
		if (!IsInit || !PhotonNetwork.IsMasterClient || EnemyAI == null) return;

		EnemyAI.EnemyAction();
	}
	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		Debug.Log("몬스터 생성");
		EnemyInit();
	}

	public void EnemyInit()
	{
		gameObject.tag = "Enemy";

		_rigid = GetComponent<Rigidbody2D>();
		_sprite = GetComponent<SpriteRenderer>();
		_anim = GetComponent<Animator>();

		object[] data = photonView.InstantiationData;
		int pokeNumber = (int)data[0];
		int pokeLevel = (int)data[1];
		PokemonData pokeData = Define.GetPokeData(pokeNumber);
		EnemyData = new EnemyData(this, pokeData.PokeName, pokeData, pokeLevel);
		_anim.runtimeAnimatorController = EnemyData.PokeData.AnimController;
		_anim.SetFloat("X", MoveDir.x);
		_anim.SetFloat("Y", MoveDir.y);

		EnemyAI = new EnemyAI(this, EnemyData);

		IsInit = true;
		Debug.Log($"Lv.{pokeLevel} {pokeData.PokeName} 적 생성");
	}

	public bool TakeDamage(BattleDataTable attackerData, PokemonSkill skill)
	{
		if (EnemyData.CurrentHp <= 0 || EnemyData.IsDead) return false;

		BattleDataTable defenderData = ((IDamagable)this).BattleData;
		int damage = PokeUtils.CalculateDamage(attackerData, defenderData, skill);
		PlayerManager.Instance?.ShowDamageText(transform, damage, Color.white);
		photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.AllBuffered, damage);

		Debug.Log($"Lv.{attackerData.Level} {attackerData.PokeData.PokeName} 이/가 Lv.{defenderData.Level} {defenderData.PokeData.PokeName} 을/를 {skill.SkillName} 공격!");
		return true;
	}

	[PunRPC]
	public void RPC_TakeDamage(int value)
	{
		if (value > 0) SetIsHit();
		Debug.Log($"{value} 대미지 입음");
		EnemyData.SetCurrentHp(EnemyData.CurrentHp - value);
		PlayerManager.Instance.ShowDamageText(transform, value, Color.red);
	}

	[PunRPC]
	public void RPC_EnemyDead()
	{
		Debug.Log("몬스터 사망");
		SetIsDead(EnemyData.IsDead);
		// TODO : 경험치 드랍

		if (PhotonNetwork.IsMasterClient) StartCoroutine(EnemyDeleteRoutine());
	}
	IEnumerator EnemyDeleteRoutine()
	{
		yield return new WaitForSeconds(_deleteTime);
		PhotonNetwork.Destroy(gameObject);
		Debug.Log("몬스터 파괴");
	}

	public void Move(Vector2 dir)
	{
		if (MoveDir.x != 0) _flipX = dir.x > 0.1f;
		SetIsMoving(dir);
		_sprite.flipX = _flipX;
		Vector2 movePos = dir * MoveSpeed;
		_rigid.velocity = movePos;
		_anim.SetFloat("X", dir.x);
		_anim.SetFloat("Y", dir.y);
	}

	public void StopMove()
	{
		_rigid.velocity = Vector2.zero;
		SetIsMoving(Vector2.zero);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		// 수동 동기화
		if (stream.IsWriting)
		{
			stream.SendNext(_flipX);
		}
		else
		{
			SetFlip(_flipX = (bool)stream.ReceiveNext());
		}
	}

	public void SetFlip(bool flip) => _sprite.flipX = flip;
	public void SetIsMoving(Vector2 dir) => _anim.SetBool("IsMoving", dir != Vector2.zero);
	public void SetIsAttack() => _anim.SetTrigger("IsAttack");
	public void SetIsSpeAttack() => _anim.SetTrigger("IsSpeAttack");
	public void SetIsHit() => _anim.SetTrigger("IsHit");
	public void SetIsDead(bool isDead) => _anim.SetBool("IsDead", isDead);
}
