using System;
using UnityEngine;

[Serializable]
public class PlayerModel
{
    // TODO : 포켓몬에 따라 스탯 달라짐
    public string PlayerName;
    public float MoveSpeed;

    public PlayerModel(string playerName, float moveSpeed)
    {
        PlayerName = playerName;
        MoveSpeed = moveSpeed;
    }
}
