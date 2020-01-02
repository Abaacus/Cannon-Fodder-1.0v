 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
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

    CannonController cannonController;  // singleton instances
    WindController windController;

    public Slider powerSlider;  // UI slider, allows for easy programmable value bars

    public TextMeshProUGUI scoreGUI;    // scoreGUI element
    string score;   // score string

    public Sprite[] windStrengths;  // array of wind graphics
    public Image windStrength;  // image GUI 

    void Start()
    {
        cannonController = CannonController.instance;   // create the pathways to the instances
        windController = WindController.instance;
    }

    void Update()
    {
        UpdatePowerSlider();    // constantly updates the power slider
    }

    public void UpdateWindGraphic()
    {
        int strengthScore = windController.WindStrengthScore();   // generates a strength score, a number from 0 - 2, and a direction
        windStrength.sprite = windStrengths[strengthScore];  // uses the first index for to pick the wind graphic

        if (windController.windDirection < 0)
        {   // if the direction is negative, make the x scale negative, flipping the picture
            windStrength.rectTransform.localScale = new Vector2(-Mathf.Abs(windStrength.rectTransform.localScale.x), windStrength.rectTransform.localScale.y);
        }
        else
        {   // other wise, keep it at a normal scale
            windStrength.rectTransform.localScale = new Vector2(Mathf.Abs(windStrength.rectTransform.localScale.x), windStrength.rectTransform.localScale.y);
        }
    }

    void UpdatePowerSlider()
    {   // turn the power into a percemt of 0% - 100%
        powerSlider.value = (cannonController.power - cannonController.maxMinPower[0]) / (cannonController.maxMinPower[1] - cannonController.maxMinPower[0]);
    }

    public void UpdateScore(int value)
    {
        score = "Score:  " + value; // updates the score string
        scoreGUI.text = score;  // set the GUI to the score string
    }
}
