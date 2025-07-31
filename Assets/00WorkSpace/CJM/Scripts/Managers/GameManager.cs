using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] BackendManager backendManager;
    [SerializeField] UIManager uIManager;
    [SerializeField] NetworkManager networkManager;

    private void Awake() => Init();

    private void Init()
    {
        backendManager.Init();
        uIManager.Init();
        networkManager.Init();
    }
}
