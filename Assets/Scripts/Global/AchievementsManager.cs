using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

public class AchievementsManager : MonoBehaviour
{
    private enum Achievement
    { 
        ACH_HOLE_IN_ONE,
        ACH_MULTIVERSE,
        ACH_COLLECTOR,
        ACH_STYLISH,
        ACH_HIDDEN,
        ACH_PERFECT,
        ACH_PERFECTION,
        ACH_CHALLENGER,
        ACH_HAND_OF_GOD,
        ACH_FAILURE,
        ACH_GRAND_GOLFER
    }
    [ContextMenu("Reset all stats")]
    private void ResetAllStats()
    {
        SteamUserStats.ResetAllStats(true);
    }
    private void UnlockAchievement(Achievement achievement)
    {
        if(!SteamManager.Initialized)
        {
            return;
        }
        string ach = achievement.ToString();

        //if(!SteamUserStats.GetAchievement(ach, out _))
        //{
            SteamUserStats.SetAchievement(achievement.ToString());
            SteamUserStats.StoreStats();

        print("set");
        //}
    }
    #region Delegates
    private void OnShoot()
    {
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
    private void OnSceneLoad()
    {

    }
    #endregion

    private void OnEnable()
    {
        LevelScoring.onScore += OnScore;
    }
    private void OnDisable()
    {
        LevelScoring.onScore -= OnScore;
    }
}