using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public int playerId;
    public bool isVillain;
    public int score;
    public string playerName;

    // 连续不动限制：放弃 或 拒绝 都算"不动"
    public bool noContactLastRound;
    public bool noContactThisRound;

    // 已选择过的目标（整局游戏内不能重复选择同一人）
    public List<int> contactedTargets;

    // 对其他玩家的可疑度（0.0=完全信任，1.0=高度怀疑），初始 0.5
    public Dictionary<int, float> suspicion;

    // 本回合分数变化，用于生成讨论信息
    public int scoreChangedThisRound;

    public Player(int id)
    {
        playerId = id;
        playerName = "玩家 " + (id + 1);
        isVillain = false;
        score = 0;
        noContactLastRound = false;
        noContactThisRound = false;
        contactedTargets = new List<int>();
        suspicion = new Dictionary<int, float>();
        scoreChangedThisRound = 0;
    }

    public string GetDisplayInfo()
    {
        return $"{playerName} - 分数: {score}";
    }

    public string GetFullInfo()
    {
        string role = isVillain ? "负一（坏人）" : "一（好人）";
        return $"{playerName} - {role} - 分数: {score}";
    }
}
