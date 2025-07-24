using UnityEngine;

public class PlayerView : MonoBehaviour
{
	[SerializeField] private Rigidbody2D _rigid;
	[SerializeField] private SpriteRenderer _sprite;
	[SerializeField] private Animator _anim;
	private Vector2 _lastDir = Vector2.down;

	void Awake()
	{
		_rigid = GetComponent<Rigidbody2D>();
		_sprite = GetComponent<SpriteRenderer>();
		_anim = GetComponent<Animator>();
	}

	public void PlayerMove(Vector2 dir, float moveSpeed)
	{
		Vector2 movePos = dir.normalized * moveSpeed;
		_rigid.velocity = movePos;

		if (dir != Vector2.zero) _lastDir = dir;
		if (dir.x != 0) _sprite.flipX = dir.x > 0.1f;

		_anim.SetFloat("X", _lastDir.x);
		_anim.SetFloat("Y", _lastDir.y);
	}

	public void SetAnimator(RuntimeAnimatorController anim) => _anim.runtimeAnimatorController = anim;
	public void SetFlip(bool flip) => _sprite.flipX = flip;
}
