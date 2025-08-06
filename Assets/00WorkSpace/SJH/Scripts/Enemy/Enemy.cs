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
	public Vector2 LastDir = Vector2.down;
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

	[SerializeField] BattleDataTable _battleData;
	public BattleDataTable BattleData
	{
		get
		{
			//if (!_battleData.IsVaild()) _battleData = new BattleDataTable(EnemyData.PokeLevel, EnemyData.PokeData, EnemyData.AllStat, EnemyData.MaxHp, EnemyData.CurrentHp, true);
			//return _battleData;

			// 매번 갱신으로 변경
			_battleData = new BattleDataTable(EnemyData.PokeLevel, EnemyData.PokeData, EnemyData.AllStat, EnemyData.MaxHp, EnemyData.CurrentHp, true);
			return _battleData;
		}
	}

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
		EnemyData = new EnemyData(this, pokeData, pokeLevel);
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
	public void RPC_EnemyDead(int deadExp)
	{
		Debug.Log("몬스터 사망");
		SetIsDead(EnemyData.IsDead);

		if (PhotonNetwork.IsMasterClient)
		{
			StartCoroutine(EnemyDeleteRoutine());
			PhotonNetwork.InstantiateRoomObject("ExpOrb", transform.position, Quaternion.identity, 0, new object[] { deadExp });
		}
	}
	IEnumerator EnemyDeleteRoutine()
	{
		yield return new WaitForSeconds(_deleteTime);
		PhotonNetwork.Destroy(gameObject);
		Debug.Log("몬스터 파괴");
	}

	public void Move()
	{
		if (MoveDir == Vector2.zero)
		{
			StopMove();
			return;
		}

		if (MoveDir.x != 0) _flipX = MoveDir.x > 0.1f;
		_sprite.flipX = _flipX;
		Vector2 movePos = MoveDir * MoveSpeed;
		SetIsMoving(movePos.sqrMagnitude);
		_rigid.velocity = movePos;

		SetDirAnim(MoveDir);

		LastDir = MoveDir;
	}

	public void StopMove()
	{
		SetIsMoving(0);
		_rigid.velocity = Vector2.zero;

		if (MoveDir != Vector2.zero)
		{
			LastDir = MoveDir;
			SetDirAnim(LastDir);
		}
	}

	public void Attack(SkillSlot slot)
	{
		var target = EnemyAI.TargetPlayer;
		var targetPC = EnemyAI.TargetPC;
		if (target == null && targetPC == null) return;

		PokemonSkill skill = EnemyData.GetSkill((int)slot);
		if(skill == null) return;

		IAttack attack = new SkillStrategyAttack(skill.SkillName);
		if (attack == null) return;

		if (skill.SkillAnimType == SkillAnimType.SpeAttack) SetIsSpeAttack();
		else SetIsAttack();

		EnemyData.SetSkillCooldown(slot, skill.Cooldown);

		attack.Attack(transform, LastDir, BattleData, skill);
		Debug.Log($"몬스터 {skill.SkillName} 사용!");
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
	public void SetIsMoving(float speed) => _anim.SetBool("IsMoving", speed > 0);
	public void SetIsAttack() => _anim.SetTrigger("IsAttack");
	public void SetIsSpeAttack() => _anim.SetTrigger("IsSpeAttack");
	public void SetIsHit() => _anim.SetTrigger("IsHit");
	public void SetIsDead(bool isDead) => _anim.SetBool("IsDead", isDead);
	public void SetDirAnim(Vector2 dir)
	{
		_anim.SetFloat("X", dir.x);
		_anim.SetFloat("Y", dir.y);
	}
}
