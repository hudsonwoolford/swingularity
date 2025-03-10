using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class AchievementsManager : MonoBehaviour
{
    private enum Achievement
    { 
        ACH_HOLE_IN_ONE
    }
    private void UnlockAchievement(Achievement achievement)
    {
        return;
        try
        {
            SteamUserStats.SetAchievement(achievement.ToString());
        }
        catch {
            Debug.LogError($"Setting achievement ID:{achievement.ToString()} failed");
        }
    }
    #region Delegates
    //on ball score
    private void OnScore(string score, int scoreRelToPar, int totalShots, Color scoreColor)
    {
        if(totalShots == 1)
        {
            UnlockAchievement(Achievement.ACH_HOLE_IN_ONE);
        }
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
