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

	public void Move(Vector2 dir, float moveSpeed)
	{
		Vector2 movePos = dir.normalized * moveSpeed;
		_rigid.velocity = movePos;

		_sprite.flipX = dir.x < 0.1f;
	}

	public void SetAnimator(RuntimeAnimatorController anim) => _anim.runtimeAnimatorController = anim;
}
