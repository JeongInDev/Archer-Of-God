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
        // 플레이어 사망 구독
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) playerHealth = p.GetComponent<Health>();
        if (playerHealth) playerHealth.OnDied += OnPlayerDied;

        // 적 전멸 구독
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
        // 현재 씬 리로드
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
