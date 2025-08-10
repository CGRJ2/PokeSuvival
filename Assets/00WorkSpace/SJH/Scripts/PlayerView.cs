using UnityEngine;

public class PlayerView : MonoBehaviour
{
	[SerializeField] private Rigidbody2D _rigid;
	[SerializeField] private SpriteRenderer _sprite;
	[SerializeField] private Animator _anim;
	[SerializeField] private CircleCollider2D _coll;

	void Awake()
	{
		_rigid = GetComponent<Rigidbody2D>();
		_sprite = GetComponent<SpriteRenderer>();
		_anim = GetComponent<Animator>();
		_coll = GetComponent<CircleCollider2D>();
	}

	public void PlayerMove(Vector2 dir, Vector2 lastDir, float speedValue)
	{
		Vector2 movePos = dir * speedValue;
		SetIsMoving(movePos.sqrMagnitude);
		_rigid.velocity = movePos;

		if (dir.x != 0) _sprite.flipX = lastDir.x > 0.1f;

		_anim.SetFloat("X", lastDir.x);
		_anim.SetFloat("Y", lastDir.y);
	}

	public void SetAnimator(RuntimeAnimatorController anim) => _anim.runtimeAnimatorController = anim;
	public void SetFlip(bool flip) => _sprite.flipX = flip;
	public void SetIsMoving(float speed) => _anim.SetBool("IsMoving", speed > 0);
	public void SetIsAttack() => _anim.SetTrigger("IsAttack");
	public void SetIsSpeAttack() => _anim.SetTrigger("IsSpeAttack");
	public void SetIsHit() => _anim.SetTrigger("IsHit");
	public void SetIsDead(bool isDead) => _anim.SetBool("IsDead", isDead);
	public void SetOrderInLayer(bool isMine)
	{
		if (isMine) _sprite.sortingOrder = 11;
		else _sprite.sortingOrder = 10;
	}
	public void SetColliderSize(float size) => _coll.radius = size;
	public void SetStop()
	{
		SetIsMoving(0);
		_rigid.velocity = Vector3.zero;
	}
}
