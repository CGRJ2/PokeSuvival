using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    private float damage;// 탄환이 가하는 피해량
    private float speed;// 탄환의 이동 속도
    private float lifetime;// 탄환의 수명
    private string targetTag = "Enemy";// 탄환이 맞춰야 할 대상의 태그
    /// 탄환 초기화 함수.
    /// 발사 시 피해량, 속도, 수명을 설정하고,
    /// 지정된 시간이 지나면 탄환을 자동으로 제거한다.
    public void Init(float dmg, float spd, float life)
    {
        damage = dmg;// 피해량 설정
        speed = spd;// 속도 설정
        lifetime = life;// 수명 설정
        Destroy(gameObject, lifetime); // 수명이 끝나면 게임 오브젝트 자동 파괴
    }
    /// 매 프레임마다 호출되어 탄환을 오른쪽 방향Vector2.right으로 이동
    /// Time.deltaTime을 곱해 프레임 레이트와 상관없이 일정한 속도로 이동
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }
    /// 다른 Collider2D와 충돌했을 때 호출되는 이벤트 함수.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 대상이 목표 태그(Enemy)와 일치하면
        if (collision.CompareTag(targetTag))
        {
            // EnemyHealth 스크립트를 가져와서 체력 감소 처리
            var enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage); // 적에게 피해를 준다.
            }
            Destroy(gameObject);// 탄환 파괴
        }
        // 그렇지 않고, 충돌한 오브젝트가 Ground 레이어라면
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 탄환 파괴
            Destroy(gameObject);
        }
    }
}
