using System.Collections.Generic;
using UnityEngine;

public enum RoundPhase
{
    Selecting,
    Responding,
    Discussion,
    RoundEnd,
    ScoreGuessing,
    GameOver
}

public enum DiscussionMessageType
{
    AnnounceScore,
    Accuse,
    Defend
}

[System.Serializable]
public class DiscussionMessage
{
    public int speakerId;
    public DiscussionMessageType type;
    public int targetId;
    public int claimedScoreChange;
    public string displayText;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏设置")]
    public int totalPlayers = 8;
    public int villainCount = 2;
    public int maxRounds = 7;

    [Header("分数设置")]
    public int goodPlayerStartScore = 1;
    public int villainStartScore = -1;
    public int soloGuessBonus = 1;
    public int bestGuessBonus = 1;
    public int worstGuessPenalty = -1;

    [Header("规则开关")]
    public bool allowRepeatTarget = false;
    public bool allowSelfContact = false;
    public bool consecutiveNoContactLimit = true;

    [Header("AI调参")]
    public float rejectProbabilityMin = 0.05f;
    public float rejectProbabilityMax = 0.70f;
    public float initialSuspicion = 0.5f;
    public float villainAllySuspicion = 0f;
    public float myScoreLossSuspicionDelta = 0.2f;
    public float myScoreGainSuspicionDelta = -0.15f;
    public float accuseTargetSuspicionDelta = 0.08f;
    public float defendTargetSuspicionDelta = -0.06f;
    public float positiveScoreClaimSuspicionDelta = -0.05f;
    public float negativeScoreClaimSuspicionDelta = 0.05f;
    public float guessVarianceThreshold = 0.05f;
    public float villainScoreGuessSuspicionThreshold = 0.6f;

    [Header("玩家设置")]
    public int humanPlayerIndex = 0;

    [Header("测试模式")]
    public bool isTestMode = false;

    public static int testTotalGames = 0;
    public static int testGoodWins = 0;
    public static int testVillainWins = 0;
    public static int testDraws = 0;
    public static float testGoodScoreTotal = 0f;
    public static float testVillainScoreTotal = 0f;
    public static int testTotalContacts = 0;
    public static int testAcceptedContacts = 0;
    public static int testRejectedContacts = 0;
    public static int testSkippedTurns = 0;
    public static int testSubmittedGuesses = 0;
    public static int testSkippedGuesses = 0;
    public static int testTotalGuessCorrect = 0;

    public static void ResetTestStats()
    {
        testTotalGames = testGoodWins = testVillainWins = testDraws = 0;
        testGoodScoreTotal = testVillainScoreTotal = 0f;
        testTotalContacts = testAcceptedContacts = testRejectedContacts = testSkippedTurns = 0;
        testSubmittedGuesses = testSkippedGuesses = testTotalGuessCorrect = 0;
    }

    private List<Player> players = new List<Player>();
    private int currentRound = 0;
    private bool villainActedThisRound = false;

    private RoundPhase currentPhase;
    private int currentPlayerTurn;
    private int pendingContactor;
    private int pendingTarget;
    private bool[] lockedThisRound;
    private bool[] inContactThisRound;

    private List<DiscussionMessage> discussionMessages = new List<DiscussionMessage>();
    private int discussionSpeakerIndex = 0;
    private string pendingRoundEndMessage = "";

    private int guessingPlayerIndex = 0;
    private Dictionary<int, int[]> playerGuesses = new Dictionary<int, int[]>();
    private string lastGuessingResult = "";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeGame(bool testMode = false)
    {
        PortfolioConfig.ApplyTo(this);

        isTestMode = testMode;
        if (testMode)
            humanPlayerIndex = -1;

        players.Clear();
        currentRound = 0;

        for (int i = 0; i < totalPlayers; i++)
            players.Add(new Player(i));

        List<int> indices = new List<int>();
        for (int i = 0; i < totalPlayers; i++)
            indices.Add(i);

        for (int i = 0; i < villainCount; i++)
        {
            int r = Random.Range(0, indices.Count);
            int idx = indices[r];
            players[idx].isVillain = true;
            players[idx].score = villainStartScore;
            indices.RemoveAt(r);
        }

        foreach (Player p in players)
            if (!p.isVillain)
                p.score = goodPlayerStartScore;

        InitSuspicion();
        StartNewRound();
    }

    private void InitSuspicion()
    {
        foreach (Player p in players)
        {
            p.suspicion.Clear();
            foreach (Player other in players)
            {
                if (other.playerId == p.playerId) continue;
                // 坏人开局知道所有人身份：同伴怀疑度0（不会误伤），好人保持0.5
                if (p.isVillain && other.isVillain)
                    p.suspicion[other.playerId] = Mathf.Clamp01(villainAllySuspicion);
                else
                    p.suspicion[other.playerId] = Mathf.Clamp01(initialSuspicion);
            }
        }
    }

    public void StartNewRound()
    {
        foreach (Player p in players)
        {
            p.noContactLastRound = p.noContactThisRound;
            p.noContactThisRound = false;
            p.scoreChangedThisRound = 0;
        }

        lockedThisRound = new bool[totalPlayers];
        inContactThisRound = new bool[totalPlayers];
        villainActedThisRound = false;
        currentPhase = RoundPhase.Selecting;
        currentPlayerTurn = FindNextAvailable(0);
    }

    public bool InitiateContact(int initiator, int target)
    {
        if (currentPhase != RoundPhase.Selecting) return false;
        if (initiator != currentPlayerTurn) return false;
        if (lockedThisRound[initiator]) return false;
        if (inContactThisRound[target]) return false;
        if (!allowSelfContact && initiator == target) return false;
        if (!allowRepeatTarget && players[initiator].contactedTargets.Contains(target)) return false;

        players[initiator].contactedTargets.Add(target);
        players[target].contactedTargets.Add(initiator);

        lockedThisRound[initiator] = true;
        lockedThisRound[target] = true;
        inContactThisRound[initiator] = true;
        inContactThisRound[target] = true;

        pendingContactor = initiator;
        pendingTarget = target;
        currentPhase = RoundPhase.Responding;
        return true;
    }

    public string SkipTurn(int playerIndex)
    {
        players[playerIndex].noContactThisRound = true;
        lockedThisRound[playerIndex] = true;
        if (isTestMode) testSkippedTurns++;

        int next = FindNextAvailable(0);
        string result = $"{players[playerIndex].playerName} 放弃了本回合接触。";

        if (next == -1)
        {
            int endingRound = currentRound + 1;
            string endMsg = villainActedThisRound ? "本回合有坏人出动过！" : "本回合没有坏人出动。";
            result += endMsg;
            currentRound++;

            if (currentRound >= maxRounds)
            {
                StartScoreGuessingPhase();
            }
            else
            {
                pendingRoundEndMessage = result;
                StartDiscussionPhase();
            }
        }
        else
        {
            currentPlayerTurn = next;
            currentPhase = RoundPhase.Selecting;
        }

        return result;
    }

    public string RespondToContact(bool accepted)
    {
        string result;

        if (accepted)
        {
            if (isTestMode)
            {
                testTotalContacts++;
                testAcceptedContacts++;
            }

            Player a = players[pendingContactor];
            Player b = players[pendingTarget];

            int deltaA = 0, deltaB = 0;

            if (!a.isVillain && !b.isVillain)
            {
                deltaA = 1; deltaB = 1;
                a.score += 1;
                b.score += 1;
            }
            else if (a.isVillain && b.isVillain)
            {
                villainActedThisRound = true;
            }
            else
            {
                villainActedThisRound = true;
                Player good = a.isVillain ? b : a;
                Player villain = a.isVillain ? a : b;
                good.score -= 1;
                villain.score += 1;

                if (a.isVillain) { deltaA = 1; deltaB = -1; }
                else             { deltaA = -1; deltaB = 1; }
            }

            a.scoreChangedThisRound += deltaA;
            b.scoreChangedThisRound += deltaB;

            UpdateSuspicionFromContact(pendingContactor, pendingTarget, deltaA);
            UpdateSuspicionFromContact(pendingTarget, pendingContactor, deltaB);

            players[pendingTarget].noContactThisRound = false;
            result = $"{players[pendingContactor].playerName} 与 {players[pendingTarget].playerName} 完成接触！";
        }
        else
        {
            if (isTestMode)
            {
                testTotalContacts++;
                testRejectedContacts++;
            }

            players[pendingTarget].noContactThisRound = true;
            result = $"{players[pendingTarget].playerName} 拒绝了接触。";
        }

        int next = FindNextAvailable(0);

        if (next == -1)
        {
            int endingRound = currentRound + 1;
            string endMsg = villainActedThisRound ? "本回合有坏人出动过！" : "本回合没有坏人出动。";
            result += endMsg;
            currentRound++;

            if (currentRound >= maxRounds)
            {
                StartScoreGuessingPhase();
            }
            else
            {
                pendingRoundEndMessage = result;
                StartDiscussionPhase();
            }
        }
        else
        {
            currentPlayerTurn = next;
            currentPhase = RoundPhase.Selecting;
        }

        return result;
    }

    // ─── 讨论阶段 ─────────────────────────────────

    public void StartDiscussionPhase()
    {
        discussionMessages.Clear();
        discussionSpeakerIndex = 0;
        currentPhase = RoundPhase.Discussion;
    }

    public void SubmitDiscussionMessage(DiscussionMessage msg)
    {
        discussionMessages.Add(msg);
        foreach (Player p in players)
            if (p.playerId != msg.speakerId)
                UpdateSuspicionFromMessage(p.playerId, msg);
        discussionSpeakerIndex++;
    }

    public bool IsDiscussionComplete() => discussionSpeakerIndex >= totalPlayers;

    public int GetDiscussionSpeaker() => discussionSpeakerIndex < totalPlayers ? discussionSpeakerIndex : -1;

    public string FinishDiscussion()
    {
        currentPhase = RoundPhase.RoundEnd;
        return pendingRoundEndMessage;
    }

    public DiscussionMessage AIGenerateMessage(int speakerId)
    {
        Player self = players[speakerId];
        var msg = new DiscussionMessage { speakerId = speakerId, targetId = -1 };

        float roll = Random.value;
        float accuseThreshold = self.isVillain ? 0.5f : 0.4f;
        float announceThreshold = self.isVillain ? 0.8f : 0.8f;

        if (roll < accuseThreshold)
        {
            msg.type = DiscussionMessageType.Accuse;
            msg.targetId = self.isVillain ? PickByLowestSuspicion(speakerId) : PickByHighestSuspicion(speakerId);
        }
        else if (roll < announceThreshold)
        {
            msg.type = DiscussionMessageType.AnnounceScore;
            int realChange = self.scoreChangedThisRound;
            if (self.isVillain && Random.value < 0.6f)
                msg.claimedScoreChange = realChange + Random.Range(-1, 2);
            else
                msg.claimedScoreChange = realChange;
        }
        else
        {
            msg.type = DiscussionMessageType.Defend;
            msg.targetId = self.isVillain ? PickByHighestSuspicion(speakerId) : PickByLowestSuspicion(speakerId);
        }

        msg.displayText = BuildDiscussionText(speakerId, msg);
        return msg;
    }

    private string BuildDiscussionText(int speakerId, DiscussionMessage msg)
    {
        string name = players[speakerId].playerName;
        switch (msg.type)
        {
            case DiscussionMessageType.AnnounceScore:
                string sign = msg.claimedScoreChange >= 0 ? "+" : "";
                return $"[{name}] 本回合分数变化：{sign}{msg.claimedScoreChange}";
            case DiscussionMessageType.Accuse:
                return $"[{name}] 指控 {players[msg.targetId].playerName} 是坏人！";
            case DiscussionMessageType.Defend:
                return $"[{name}] 认为 {players[msg.targetId].playerName} 是好人。";
            default:
                return $"[{name}] ...";
        }
    }

    public int AIPickTarget(int speakerId, List<int> available)
    {
        float totalWeight = 0f;
        float[] weights = new float[available.Count];
        for (int i = 0; i < available.Count; i++)
        {
            float s = players[speakerId].suspicion.ContainsKey(available[i])
                ? players[speakerId].suspicion[available[i]] : 0.5f;
            weights[i] = (1f - s) + 0.1f;
            totalWeight += weights[i];
        }
        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        for (int i = 0; i < available.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return available[i];
        }
        return available[available.Count - 1];
    }

    public bool AIDecideAccept(int responderId, int contactorId)
    {
        float s = players[responderId].suspicion.ContainsKey(contactorId)
            ? players[responderId].suspicion[contactorId] : 0.5f;
        float rejectProb = Mathf.Lerp(rejectProbabilityMin, rejectProbabilityMax, s);
        return Random.value > rejectProb;
    }

    private void UpdateSuspicionFromContact(int observerId, int partnerId, int myScoreChange)
    {
        if (!players[observerId].suspicion.ContainsKey(partnerId)) return;
        float current = players[observerId].suspicion[partnerId];
        float delta = myScoreChange < 0 ? myScoreLossSuspicionDelta : myScoreChange > 0 ? myScoreGainSuspicionDelta : 0f;
        players[observerId].suspicion[partnerId] = Mathf.Clamp01(current + delta);
    }

    private void UpdateSuspicionFromMessage(int observerId, DiscussionMessage msg)
    {
        var sus = players[observerId].suspicion;
        switch (msg.type)
        {
            case DiscussionMessageType.Accuse:
                if (sus.ContainsKey(msg.targetId))
                    sus[msg.targetId] = Mathf.Clamp01(sus[msg.targetId] + accuseTargetSuspicionDelta);
                break;
            case DiscussionMessageType.Defend:
                if (sus.ContainsKey(msg.targetId))
                    sus[msg.targetId] = Mathf.Clamp01(sus[msg.targetId] + defendTargetSuspicionDelta);
                break;
            case DiscussionMessageType.AnnounceScore:
                if (sus.ContainsKey(msg.speakerId))
                {
                    float d = msg.claimedScoreChange > 0 ? positiveScoreClaimSuspicionDelta : msg.claimedScoreChange < 0 ? negativeScoreClaimSuspicionDelta : 0f;
                    sus[msg.speakerId] = Mathf.Clamp01(sus[msg.speakerId] + d);
                }
                break;
        }
    }

    private int PickByHighestSuspicion(int speakerId)
    {
        float max = -1f; int best = -1;
        foreach (var kv in players[speakerId].suspicion)
            if (kv.Value > max) { max = kv.Value; best = kv.Key; }
        return best == -1 ? GetRandomOther(speakerId) : best;
    }

    private int PickByLowestSuspicion(int speakerId)
    {
        float min = 2f; int best = -1;
        foreach (var kv in players[speakerId].suspicion)
            if (kv.Value < min) { min = kv.Value; best = kv.Key; }
        return best == -1 ? GetRandomOther(speakerId) : best;
    }

    private int GetRandomOther(int speakerId)
    {
        var others = players.FindAll(p => p.playerId != speakerId);
        return others[Random.Range(0, others.Count)].playerId;
    }

    // ─── 分数猜测阶段 ─────────────────────────────

    public void StartScoreGuessingPhase()
    {
        guessingPlayerIndex = 0;
        playerGuesses.Clear();
        lastGuessingResult = "";
        currentPhase = RoundPhase.ScoreGuessing;
    }

    public int GetGuessingPlayerIndex() => guessingPlayerIndex;
    public bool IsGuessingComplete() => guessingPlayerIndex >= totalPlayers;
    public string GetGuessingResultText() => lastGuessingResult;

    public void SkipGuessing()
    {
        if (isTestMode && !IsGuessingComplete())
            testSkippedGuesses++;

        guessingPlayerIndex++;
        if (IsGuessingComplete()) FinalizeGuesses();
    }

    public void SubmitGuess(int guesserIndex, int[] guesses)
    {
        playerGuesses[guesserIndex] = guesses;
        if (isTestMode) testSubmittedGuesses++;
        guessingPlayerIndex++;
        if (IsGuessingComplete()) FinalizeGuesses();
    }

    public void FinalizeGuesses()
    {
        if (playerGuesses.Count == 0)
        {
            lastGuessingResult = "无人参与猜测。";
            RecordTestResult();
            currentPhase = RoundPhase.GameOver;
            return;
        }

        var correctCounts = new Dictionary<int, int>();
        foreach (var kv in playerGuesses)
        {
            int correct = 0;
            for (int i = 0; i < players.Count; i++)
                if (kv.Value[i] == players[i].score) correct++;
            correctCounts[kv.Key] = correct;
            if (isTestMode) testTotalGuessCorrect += correct;
        }

        if (playerGuesses.Count == 1)
        {
            int solo = new List<int>(playerGuesses.Keys)[0];
            players[solo].score += soloGuessBonus;
            lastGuessingResult = $"{players[solo].playerName} 独自猜测，直接 {FormatScoreDelta(soloGuessBonus)} 分。";
            RecordTestResult();
            currentPhase = RoundPhase.GameOver;
            return;
        }

        int maxCorrect = 0, minCorrect = int.MaxValue;
        foreach (var v in correctCounts.Values)
        {
            if (v > maxCorrect) maxCorrect = v;
            if (v < minCorrect) minCorrect = v;
        }

        string result = "猜测结算：\n";
        foreach (var kv in correctCounts)
        {
            if (kv.Value == maxCorrect)
            {
                players[kv.Key].score += bestGuessBonus;
                result += $"{players[kv.Key].playerName} 猜对 {kv.Value} 个 → {FormatScoreDelta(bestGuessBonus)}\n";
            }
            else if (kv.Value == minCorrect)
            {
                players[kv.Key].score += worstGuessPenalty;
                result += $"{players[kv.Key].playerName} 猜对 {kv.Value} 个 → {FormatScoreDelta(worstGuessPenalty)}\n";
            }
            else
            {
                result += $"{players[kv.Key].playerName} 猜对 {kv.Value} 个 → 0\n";
            }
        }

        lastGuessingResult = result;
        RecordTestResult();
        currentPhase = RoundPhase.GameOver;
    }

    public bool AIShouldGuess(int guesserIndex)
    {
        var sus = players[guesserIndex].suspicion;
        if (sus.Count == 0) return false;
        float mean = 0f;
        foreach (var v in sus.Values) mean += v;
        mean /= sus.Count;
        float variance = 0f;
        foreach (var v in sus.Values) variance += (v - mean) * (v - mean);
        variance /= sus.Count;
        return variance > guessVarianceThreshold;
    }

    public int[] AIGuessScores(int guesserIndex)
    {
        int[] guesses = new int[totalPlayers];
        Player self = players[guesserIndex];
        for (int i = 0; i < totalPlayers; i++)
        {
            if (i == guesserIndex) { guesses[i] = players[i].score; continue; }
            float sus = self.suspicion.ContainsKey(i) ? self.suspicion[i] : 0.5f;
            int baseScore = sus > villainScoreGuessSuspicionThreshold ? villainStartScore : goodPlayerStartScore;
            guesses[i] = baseScore + Random.Range(-1, 2);
        }
        return guesses;
    }

    private void RecordTestResult()
    {
        if (!isTestMode) return;

        float goodAvg = 0f, villainAvg = 0f;
        int goodCount = 0, villainCount2 = 0;
        foreach (Player p in players)
        {
            if (p.isVillain) { villainAvg += p.score; villainCount2++; }
            else { goodAvg += p.score; goodCount++; }
        }
        if (goodCount > 0) goodAvg /= goodCount;
        if (villainCount2 > 0) villainAvg /= villainCount2;

        testTotalGames++;
        testGoodScoreTotal += goodAvg;
        testVillainScoreTotal += villainAvg;
        if (goodAvg > villainAvg) testGoodWins++;
        else if (villainAvg > goodAvg) testVillainWins++;
        else testDraws++;
    }

    public List<int> GetAvailableTargets(int playerIndex)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < totalPlayers; i++)
        {
            if ((allowSelfContact || i != playerIndex)
                && !inContactThisRound[i]
                && (allowRepeatTarget || !players[playerIndex].contactedTargets.Contains(i)))
                available.Add(i);
        }
        return available;
    }

    public bool CanPlayerSkip(int playerIndex) => !consecutiveNoContactLimit || !players[playerIndex].noContactLastRound;
    public bool CanPlayerReject(int playerIndex) => !consecutiveNoContactLimit || !players[playerIndex].noContactLastRound;
    public bool IsHumanPlayer(int index) => index == humanPlayerIndex;

    private int FindNextAvailable(int startFrom)
    {
        for (int i = startFrom; i < totalPlayers; i++)
            if (!lockedThisRound[i]) return i;
        return -1;
    }

    public RoundPhase GetPhase() => currentPhase;
    public int GetCurrentPlayerTurn() => currentPlayerTurn;
    public int GetPendingContactor() => pendingContactor;
    public int GetPendingTarget() => pendingTarget;
    public bool IsPlayerLocked(int index) => lockedThisRound != null && lockedThisRound[index];
    public bool IsPlayerInContact(int index) => inContactThisRound != null && inContactThisRound[index];
    public List<Player> GetPlayers() => players;
    public int GetCurrentRound() => currentRound;
    public bool IsGameOver() => currentRound >= maxRounds;
    public List<DiscussionMessage> GetDiscussionMessages() => discussionMessages;

    public void GetCampAverages(out float goodAverage, out float villainAverage)
    {
        goodAverage = 0f;
        villainAverage = 0f;
        int goodCount = 0;
        int villainCount = 0;

        foreach (Player p in players)
        {
            if (p.isVillain)
            {
                villainAverage += p.score;
                villainCount++;
            }
            else
            {
                goodAverage += p.score;
                goodCount++;
            }
        }

        if (goodCount > 0) goodAverage /= goodCount;
        if (villainCount > 0) villainAverage /= villainCount;
    }

    public string GetCampResultText()
    {
        GetCampAverages(out float goodAverage, out float villainAverage);
        if (Mathf.Approximately(goodAverage, villainAverage))
            return $"阵营平局  一阵营平均分 {goodAverage:F1} / 负一平均分 {villainAverage:F1}";

        string winner = goodAverage > villainAverage ? "一阵营胜利" : "负一阵营胜利";
        return $"{winner}  一阵营平均分 {goodAverage:F1} / 负一平均分 {villainAverage:F1}";
    }

    private string FormatScoreDelta(int delta)
    {
        if (delta > 0) return $"+{delta}";
        return delta.ToString();
    }

}
