using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
using static StarCollectible;

public class AchievementsManager : MonoBehaviour
{
    public enum Achievement
    { 
        ACH_HOLE_IN_ONE, //I
        ACH_MULTIVERSE, //I
        ACH_COLLECTOR, //
        ACH_STYLISH, //I
        ACH_HIDDEN, //I
        ACH_PERFECT, //I
        ACH_PERFECTION, //I
        ACH_CHALLENGER, //I
        ACH_HAND_OF_GOD,//I
        ACH_FAILURE, //I
        ACH_GRAND_GOLFER //STT_TOTAL_SHOTS | I
    }
    [ContextMenu("Reset all stats")]
    private void ResetAllStats()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }
        SteamUserStats.ResetAllStats(true);
    }
    #region Public Methods
    public void UnlockAchievementP(Achievement achievement)
    {
        UnlockAchievement(achievement);
    }
    public void IncrementIntStatP(string stat, int increment)
    {
        IncrementIntStat(stat, increment);
    }
    #endregion
    #region STEAM API Access
    private void UnlockAchievement(Achievement achievement)
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        bool achieved;
        SteamUserStats.GetAchievement(achievement.ToString(), out achieved);
        Debug.Log(achieved);
        if (!achieved)
        {
            SteamUserStats.SetAchievement(achievement.ToString());
            SteamUserStats.StoreStats();
            SteamAPI.RunCallbacks();
        }   
    }
    private void IncrementIntStat(string stat, int increment)
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        int totalShots = 0;
        SteamUserStats.GetStat(stat, out totalShots);
        totalShots += increment;
        SteamUserStats.SetStat(stat, totalShots);
        SteamUserStats.StoreStats();
    }
    private void IncrementIntStat(string stat, int increment, Achievement achievement, int thresholdInclusive)
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        int totalShots;
        SteamUserStats.GetStat(stat, out totalShots);
        totalShots += increment;
        SteamUserStats.SetStat(stat, totalShots);
        SteamUserStats.StoreStats();

        if(totalShots >= thresholdInclusive)
        {
            UnlockAchievement(achievement);
        }
    }
    #endregion
    #region Delegates
    private void OnCollectArtifact()
    {
        UnlockAchievement(Achievement.ACH_HIDDEN);
    }
    private void OnShoot()
    {
        IncrementIntStat("STT_TOTAL_SHOTS", 1, Achievement.ACH_GRAND_GOLFER, 1000);
    }
    private void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor)
    {
        //HOLE IN ONE
        if(totalShots == 1)
        {
            UnlockAchievement(Achievement.ACH_HOLE_IN_ONE);
        }

        //MULTIVERSE
        if(GameManager.LevelManager.isCampaignFinalLevel)
        {
            UnlockAchievement(Achievement.ACH_MULTIVERSE);
        }
    }
    private void OnDeath(int deathIndex)
    {
        //FAILURE
        if(deathIndex == 3)
        {
            UnlockAchievement(Achievement.ACH_FAILURE);
        }
    }
    private void OnSceneLoad()
    {

    }
    #endregion

    private void OnEnable()
    {
        StarCollectible.onStarCollected += OnCollectArtifact;
        ShootBall.death += OnDeath;
        ShootBall.onShoot += OnShoot;
        LevelScoring.onScore += OnScore;
    }
    private void OnDisable()
    {
        StarCollectible.onStarCollected -= OnCollectArtifact;
        ShootBall.death -= OnDeath;
        ShootBall.onShoot -= OnShoot;
        LevelScoring.onScore -= OnScore;
    }
}