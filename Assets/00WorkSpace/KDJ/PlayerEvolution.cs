using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerEvolution : MonoBehaviour
{
    [Header("���� �� ����ġ")]
    public int currentLevel = 1;// ���� �÷��̾��� ����
    public int maxLevel = 5;// �÷��̾ ������ �� �ִ� �ִ� ����
    public int currentExp = 0;// ���� ����ġ ��
    public int[] expToNextLevel;// �� �������� ���� ������ ���� ���� �ʿ��� ����ġ

    [Header("��ȭ ����")]
    public GameObject[] evolutionForms;// �� ������ �����ϴ� ���� Prefab
    private GameObject currentForm;// ���� �÷��̾� ������ �ν��Ͻ�

    [Header("�ɷ�ġ")]
    public float baseSpeed = 5f;// �⺻ �̵� �ӵ�
    public float baseDamage = 10f;// �⺻ ���ݷ�
    public float growthFactor = 1.3f;// ������ �� �ɷ�ġ ��� ����
    public float damageGrowth = 2f;        // ������ �� ������ ������
    public float sizeGrowth = 0.2f;        // ������ �� ũ�� ������

    private void Start()
    {
        // ���� ���� �� ù ��° ������ �����ؼ� �÷��̾ ����
        if (evolutionForms.Length > 0)
        {
            currentForm = Instantiate(
                evolutionForms[0],// ù ��° ���� Prefab
                transform.position,// �÷��̾� ��ġ
                Quaternion.identity,// �⺻ ȸ�� ��
                transform// �÷��̾� ������Ʈ�� �ڽ����� ����
            );
        }
    }

 
    /// ����ġ�� �߰��ϴ� �Լ�.
    /// ����ġ�� �߰��ϰ�, ������ ������ �����Ǹ� Levelu\p()�� ȣ��.
    public void AddExperience(int amount)
    {
        currentExp += amount; // ����ġ ����

        // ���� ������ �ִ� �������� �۰�,
        // ���� ������ �ʿ��� ����ġ �̻��� ȹ�������� ������ ����
        if (currentLevel < maxLevel && currentExp >= expToNextLevel[currentLevel - 1])
        {
            LevelUp();
        }
    }

    /// ������ ó�� �Լ�.
    /// - ���� ����
    /// - ���� ����ġ �ʱ�ȭ
    /// - ���� ��ü
    /// - �ɷ�ġ ��ȭ
    private void LevelUp()
    {
        currentLevel++;// ���� ����
        currentExp = 0;// ����ġ �ʱ�ȭ

        // ���� ���� ����
        if (currentForm != null) Destroy(currentForm);

        // ���� ������ �´� �� ���� ����
        currentForm = Instantiate(
            evolutionForms[currentLevel - 1],  // ���� ������ �����ϴ� ���� Prefab
            transform.position,
            Quaternion.identity,
            transform
        );

        // �ɷ�ġ ��ȭ
        baseSpeed *= growthFactor;
        baseDamage *= growthFactor;

        Debug.Log($"������! ���� ����: {currentLevel}");

        currentLevel++;
        baseDamage += damageGrowth; // ������ ����
        transform.localScale += Vector3.one * sizeGrowth; // �÷��̾� ���� Ŀ��
        Debug.Log($"������! ���� ����: {currentLevel}, ������: {baseDamage}");
    }
}