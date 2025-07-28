using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerEvolution : MonoBehaviour
{
    [Header("레벨 및 경험치")]
    public int currentLevel = 1;// 현재 플레이어의 레벨
    public int maxLevel = 5;// 플레이어가 도달할 수 있는 최대 레벨
    public int currentExp = 0;// 현재 경험치 값
    public int[] expToNextLevel;// 각 레벨에서 다음 레벨로 가기 위해 필요한 경험치

    [Header("진화 외형")]
    public GameObject[] evolutionForms;// 각 레벨에 대응하는 외형 Prefab
    private GameObject currentForm;// 현재 플레이어 외형의 인스턴스

    [Header("능력치")]
    public float baseSpeed = 5f;// 기본 이동 속도
    public float baseDamage = 10f;// 기본 공격력
    public float growthFactor = 1.3f;// 레벨업 시 능력치 상승 비율
    public float damageGrowth = 2f;        // 레벨업 시 데미지 증가량
    public float sizeGrowth = 0.2f;        // 레벨업 시 크기 증가량

    private void Start()
    {
        // 게임 시작 시 첫 번째 외형을 생성해서 플레이어에 부착
        if (evolutionForms.Length > 0)
        {
            currentForm = Instantiate(
                evolutionForms[0],// 첫 번째 외형 Prefab
                transform.position,// 플레이어 위치
                Quaternion.identity,// 기본 회전 값
                transform// 플레이어 오브젝트의 자식으로 설정
            );
        }
    }

 
    /// 경험치를 추가하는 함수.
    /// 경험치를 추가하고, 레벨업 조건이 충족되면 Levelu\p()을 호출.
    public void AddExperience(int amount)
    {
        currentExp += amount; // 경험치 누적

        // 현재 레벨이 최대 레벨보다 작고,
        // 다음 레벨에 필요한 경험치 이상을 획득했으면 레벨업 실행
        if (currentLevel < maxLevel && currentExp >= expToNextLevel[currentLevel - 1])
        {
            LevelUp();
        }
    }

    /// 레벨업 처리 함수.
    /// - 레벨 증가
    /// - 현재 경험치 초기화
    /// - 외형 교체
    /// - 능력치 강화
    private void LevelUp()
    {
        currentLevel++;// 레벨 증가
        currentExp = 0;// 경험치 초기화

        // 기존 외형 제거
        if (currentForm != null) Destroy(currentForm);

        // 현재 레벨에 맞는 새 외형 생성
        currentForm = Instantiate(
            evolutionForms[currentLevel - 1],  // 현재 레벨에 대응하는 외형 Prefab
            transform.position,
            Quaternion.identity,
            transform
        );

        // 능력치 강화
        baseSpeed *= growthFactor;
        baseDamage *= growthFactor;

        Debug.Log($"레벨업! 현재 레벨: {currentLevel}");

        currentLevel++;
        baseDamage += damageGrowth; // 데미지 증가
        transform.localScale += Vector3.one * sizeGrowth; // 플레이어 몸집 커짐
        Debug.Log($"레벨업! 현재 레벨: {currentLevel}, 데미지: {baseDamage}");
    }
}