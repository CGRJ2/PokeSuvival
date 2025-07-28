using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
	[SerializeField] private TMP_Text _text;
	[SerializeField] private float _speed;
	[SerializeField] private float _duration;
	[SerializeField] private float _offset;
	public void InitFloatingDamage(string dmgText)
	{
		_text.text = dmgText;
		transform.position += Vector3.up * _offset;
		StartCoroutine(DamageTextRoutine());
	}

	public void InitFloatingDamage(string dmgText, Color color)
	{
		_text.text = dmgText;
		_text.color = color;
		transform.position += Vector3.up * _offset;

		Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
		transform.position += (Vector3)randomOffset;

		StartCoroutine(DamageTextRoutine());
	}

	IEnumerator DamageTextRoutine()
	{
		float timer = 0f;

		while (timer < _duration)
		{
			transform.position += Vector3.up * _speed * Time.deltaTime;
			float t = timer / _duration;
			_text.alpha = 1f - t;

			timer += Time.deltaTime;
			yield return null;
		}

		Destroy(gameObject);
	}
}
