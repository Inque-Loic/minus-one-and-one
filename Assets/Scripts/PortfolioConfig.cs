using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class PortfolioConfig
{
    private const string ConfigFolder = "Docs/Portfolio/ConfigTables";
    private const string FieldColumn = "field";
    private const string ValueColumn = "recommended_demo_value";

    private static Dictionary<string, string> gameRules;
    private static Dictionary<string, string> aiTuning;

    public static void ApplyTo(GameManager manager)
    {
        EnsureLoaded();
        if (manager == null || gameRules == null) return;

        int totalPlayers = GetInt(gameRules, "totalPlayers", manager.totalPlayers);
        int villainCount = GetInt(gameRules, "villainCount", manager.villainCount);
        int maxRounds = GetInt(gameRules, "maxRounds", manager.maxRounds);

        if (totalPlayers < 3)
        {
            Debug.LogWarning($"[PortfolioConfig] totalPlayers={totalPlayers} 无效，继续使用 Inspector 默认值 {manager.totalPlayers}。");
            totalPlayers = manager.totalPlayers;
        }

        if (villainCount <= 0 || villainCount >= totalPlayers)
        {
            Debug.LogWarning($"[PortfolioConfig] villainCount={villainCount} 无效，继续使用 Inspector 默认值 {manager.villainCount}。");
            villainCount = manager.villainCount;
        }

        if (maxRounds <= 1)
        {
            Debug.LogWarning($"[PortfolioConfig] maxRounds={maxRounds} 无效，继续使用 Inspector 默认值 {manager.maxRounds}。");
            maxRounds = manager.maxRounds;
        }

        manager.totalPlayers = totalPlayers;
        manager.villainCount = villainCount;
        manager.maxRounds = maxRounds;
        manager.goodPlayerStartScore = GetInt(gameRules, "goodPlayerStartScore", manager.goodPlayerStartScore);
        manager.villainStartScore = GetInt(gameRules, "villainStartScore", manager.villainStartScore);
        manager.allowRepeatTarget = GetBool(gameRules, "allowRepeatTarget", manager.allowRepeatTarget);
        manager.allowSelfContact = GetBool(gameRules, "allowSelfContact", manager.allowSelfContact);
        manager.consecutiveNoContactLimit = GetBool(gameRules, "consecutiveNoContactLimit", manager.consecutiveNoContactLimit);
        manager.soloGuessBonus = GetInt(gameRules, "soloGuessBonus", manager.soloGuessBonus);
        manager.bestGuessBonus = GetInt(gameRules, "bestGuessBonus", manager.bestGuessBonus);
        manager.worstGuessPenalty = GetInt(gameRules, "worstGuessPenalty", manager.worstGuessPenalty);

        if (aiTuning == null) return;
        manager.rejectProbabilityMin = GetFloat(aiTuning, "rejectProbabilityMin", manager.rejectProbabilityMin);
        manager.rejectProbabilityMax = GetFloat(aiTuning, "rejectProbabilityMax", manager.rejectProbabilityMax);
        manager.initialSuspicion = GetFloat(aiTuning, "initialSuspicion", manager.initialSuspicion);
        manager.villainAllySuspicion = GetFloat(aiTuning, "villainAllySuspicion", manager.villainAllySuspicion);
        manager.myScoreLossSuspicionDelta = GetFloat(aiTuning, "myScoreLossDelta", manager.myScoreLossSuspicionDelta);
        manager.myScoreGainSuspicionDelta = GetFloat(aiTuning, "myScoreGainDelta", manager.myScoreGainSuspicionDelta);
        manager.accuseTargetSuspicionDelta = GetFloat(aiTuning, "accuseTargetSuspicionDelta", manager.accuseTargetSuspicionDelta);
        manager.defendTargetSuspicionDelta = GetFloat(aiTuning, "defendTargetSuspicionDelta", manager.defendTargetSuspicionDelta);
        manager.positiveScoreClaimSuspicionDelta = GetFloat(aiTuning, "positiveScoreClaimDelta", manager.positiveScoreClaimSuspicionDelta);
        manager.negativeScoreClaimSuspicionDelta = GetFloat(aiTuning, "negativeScoreClaimDelta", manager.negativeScoreClaimSuspicionDelta);
        manager.guessVarianceThreshold = GetFloat(aiTuning, "guessVarianceThreshold", manager.guessVarianceThreshold);
        manager.villainScoreGuessSuspicionThreshold = GetFloat(aiTuning, "villainScoreGuessSuspicionThreshold", manager.villainScoreGuessSuspicionThreshold);
    }

    public static void ApplyTo(UIManager manager)
    {
        EnsureLoaded();
        if (manager == null || aiTuning == null) return;

        manager.aiActionDelay = Mathf.Max(0.01f, GetFloat(aiTuning, "aiActionDelay", manager.aiActionDelay));
        manager.testAiActionDelay = Mathf.Max(0.001f, GetFloat(aiTuning, "testAiActionDelay", manager.testAiActionDelay));
        manager.randomSkipProbability = Mathf.Clamp01(GetFloat(aiTuning, "randomSkipProbability", manager.randomSkipProbability));
    }

    private static void EnsureLoaded()
    {
        if (gameRules != null && aiTuning != null) return;

        string root = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(root))
        {
            Debug.LogWarning("[PortfolioConfig] 无法定位项目根目录，使用 Inspector 默认配置。");
            gameRules = new Dictionary<string, string>();
            aiTuning = new Dictionary<string, string>();
            return;
        }

        gameRules = LoadTable(Path.Combine(root, ConfigFolder, "game_rules.csv"));
        aiTuning = LoadTable(Path.Combine(root, ConfigFolder, "ai_tuning.csv"));
    }

    private static Dictionary<string, string> LoadTable(string path)
    {
        var values = new Dictionary<string, string>();
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[PortfolioConfig] 未找到配置表：{path}，使用 Inspector 默认配置。");
            return values;
        }

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length == 0) return values;

        string[] headers = ParseCsvLine(lines[0]);
        int fieldIndex = IndexOf(headers, FieldColumn);
        int valueIndex = IndexOf(headers, ValueColumn);

        if (fieldIndex < 0 || valueIndex < 0)
        {
            Debug.LogWarning($"[PortfolioConfig] 配置表缺少 {FieldColumn} 或 {ValueColumn} 列：{path}");
            return values;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cells = ParseCsvLine(lines[i]);
            if (cells.Length <= Mathf.Max(fieldIndex, valueIndex)) continue;

            string field = cells[fieldIndex].Trim();
            if (field.Length == 0) continue;
            values[field] = cells[valueIndex].Trim();
        }

        return values;
    }

    private static string[] ParseCsvLine(string line)
    {
        var cells = new List<string>();
        var cell = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                cells.Add(cell.ToString());
                cell.Length = 0;
            }
            else
            {
                cell.Append(c);
            }
        }

        cells.Add(cell.ToString());
        return cells.ToArray();
    }

    private static int IndexOf(string[] values, string target)
    {
        for (int i = 0; i < values.Length; i++)
            if (values[i].Trim() == target)
                return i;
        return -1;
    }

    private static int GetInt(Dictionary<string, string> values, string key, int fallback)
    {
        if (values.TryGetValue(key, out string raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            return value;
        return fallback;
    }

    private static float GetFloat(Dictionary<string, string> values, string key, float fallback)
    {
        if (values.TryGetValue(key, out string raw) && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            return value;
        return fallback;
    }

    private static bool GetBool(Dictionary<string, string> values, string key, bool fallback)
    {
        if (!values.TryGetValue(key, out string raw)) return fallback;
        if (bool.TryParse(raw, out bool value)) return value;
        if (raw == "1") return true;
        if (raw == "0") return false;
        return fallback;
    }
}
