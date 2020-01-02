using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WindController : MonoBehaviour
{
    public static WindController instance;
    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Debug.LogError("Multiple instances of " + this + " found");
        }
        instance = this;
        #endregion
    }

    [Range(0,1)]
    public float[] strengthRanges;  // thresholds for calculting the strength score
    public float windStrengthMax;   // max wind strength (min is automatically -windStrengthMax)
    public float windStrength = 1f; // current wind strength
    public int windDirection;  // wind direction (either 1 or -1, positive or negative)

    public void GenerateNewWind()   // generates a new random wind strength for -max to max
    {
        windStrength = Random.Range(-windStrengthMax, windStrengthMax);
        windDirection = (int)(windStrength / Mathf.Abs(windStrength));
        // divide the wind speed by the ABS of the wind speed, giving us either 1, or -1. we save this as the wind direction
    }

    public int WindStrengthScore()    // generates a wind score and direction for strengths
    {
        int score = 0;  // initialize scores

        foreach (int strengthThreshold in strengthRanges)
        {   // keep increasing the score until we hit a threshold that the wind strength doesn't reach
            if (Mathf.Abs(windStrength) > strengthThreshold)
            {
                score++;
            }
        }

        return score;  // returns the wind strength score
    }
}
