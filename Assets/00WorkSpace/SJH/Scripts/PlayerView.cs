using UnityEngine;

public class PlayerView : MonoBehaviour
{
	[SerializeField] private Rigidbody2D _rigid;
	[SerializeField] private SpriteRenderer _sprite;
	[SerializeField] private Animator _anim;

	void Awake()
	{
		_rigid = GetComponent<Rigidbody2D>();
		_sprite = GetComponent<SpriteRenderer>();
		_anim = GetComponent<Animator>();
	}

	public void PlayerMove(Vector2 dir, Vector2 lastDir, float moveSpeed)
	{
		SetIsMoving(dir);
		Vector2 movePos = dir * moveSpeed;
		_rigid.velocity = movePos;

		if (dir.x != 0) _sprite.flipX = lastDir.x > 0.1f;

		_anim.SetFloat("X", lastDir.x);
		_anim.SetFloat("Y", lastDir.y);
	}

	public void SetAnimator(RuntimeAnimatorController anim) => _anim.runtimeAnimatorController = anim;
	public void SetFlip(bool flip) => _sprite.flipX = flip;
	public void SetIsMoving(Vector2 dir) => _anim.SetBool("IsMoving", dir != Vector2.zero);
	public void SetIsAttack() => _anim.SetTrigger("IsAttack");
	public void SetIsHit() => _anim.SetTrigger("IsHit");
	public void SetIsDead(bool isDead) => _anim.SetBool("IsDead", isDead);
}
