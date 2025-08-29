using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] GameObject root;
    [SerializeField] Button restartButton;
    
    bool shown;
    Health playerHealth;

    void Awake()
    {
        if (root) root.SetActive(false);
        if (restartButton) restartButton.onClick.AddListener(Restart);
    }

    void OnEnable()
    {
        // �÷��̾� ��� ����
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) playerHealth = p.GetComponent<Health>();
        if (playerHealth) playerHealth.OnDied += OnPlayerDied;

        // �� ���� ����
        EnemyRegistry.OnAllEnemiesDead += OnAllEnemiesDead;
    }

    void OnDisable()
    {
        if (playerHealth) playerHealth.OnDied -= OnPlayerDied;
        EnemyRegistry.OnAllEnemiesDead -= OnAllEnemiesDead;
    }

    void OnPlayerDied(Health _)
    {
        Show();
    }

    void OnAllEnemiesDead()
    {
        Show();
    }

    void Show()
    {
        if (shown) return;
        shown = true;
        if (root) root.SetActive(true);
    }

    void Restart()
    {
        // ���� �� ���ε�
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
