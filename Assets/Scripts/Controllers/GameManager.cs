using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
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

    AudioManager audioManager;
    CameraController cameraController;
    CannonController cannonController;
    CloudController cloudController;
    InputManager inputManager;
    PrefabPlacer prefabPlacer;      // sets up pathways to access pretty much every manager in the scene
    Target target;
    TerrainGenerator terrainGenerator;
    UIManager UIManager;
    WindController windController;

    [HideInInspector]
    public bool gameStarted = false;    // whether or not the game has started

    int score;  // player's current score
    public bool levelGenerating;  // used to ensure that two levels aren't made at the same time

    void Start()
    {
        audioManager = AudioManager.instance;
        cameraController = CameraController.instance;
        cannonController = CannonController.instance;
        cloudController = CloudController.instance;
        inputManager = InputManager.instance;
        prefabPlacer = PrefabPlacer.instance;       // set up all pathways
        target = Target.instance;
        terrainGenerator = TerrainGenerator.instance;
        UIManager = UIManager.instance;
        windController = WindController.instance;

        cloudController.StartCoroutine(cloudController.CloudSpammer()); // start the cloudSpammer coroutine
        StartGame();
    }
    
    void StartGame()
    {
        StartCoroutine(IStartGame());    // start the startgame coroutine
    }

    IEnumerator IStartGame()
    {
        yield return new WaitForSeconds(1);   // wait until the start function frame is finished...
        audioManager.PlaySound("GameMusic");
        DecreaseScore();    // reset the score
        StartCoroutine(GenerateNewLevel());
        gameStarted = true;
    }

    public void OnTargetHit()   // this function runs when the player hits the target
    {
        inputManager.cameraType = true;   // lock the camera
        cameraController.zoomed = true; // zoom the camera
        cameraController.cameraMovementType = CameraMovementType.followTarget;
        audioManager.PlaySound("Point");
        IncreaseScore();
        StartCoroutine(WaitForNextGame());  // start waiting for the next game
    }

    public void ResetShot() // this function resets the cannon, is run when the player misses the shot
    {
        DecreaseScore();
        cameraController.zoomed = false;
        cannonController.ResetCannon();
        cannonController.CreateNewBullet(); // creates a new bullet of the same type
    }

    public IEnumerator GenerateNewLevel()
    {
        levelGenerating = true;
        cameraController.zoomed = false;
        terrainGenerator.GenerateNewTerrain();
        windController.GenerateNewWind();       // generates a level layout
        Debug.Log("World created");

        yield return new WaitForFixedUpdate();   // waits for game to finish generating the layout

        cannonController.ResetCannon();
        cannonController.RandomizeBulletType(); // updates the target and cannon
        target.GenerateNewAngle();
        prefabPlacer.UpdatePos();
        Debug.Log("Objects updated");

        yield return new WaitForFixedUpdate();   // waits for the the cannon and target to be updated

        UIManager.UpdateWindGraphic();
        cannonController.CreateNewBullet(); // finishing touches, such as initializing the bullet, updating wind
        Debug.Log("GUI initiated");

        cameraController.StartCoroutine(cameraController.ShowLevel());  // starts the level preview
        cannonController.ResetCannon();
        Debug.Log("Level Loaded");
        levelGenerating = false;
    }

    IEnumerator WaitForNextGame()   // shows the player there victory, then generates a new level
    {
        yield return new WaitForSecondsRealtime(3);

        StartCoroutine(GenerateNewLevel());
    }

    public void IncreaseScore() // increases score by 1
    {
        score++;
        UIManager.UpdateScore(score);   // updates the score graphic
    }

    public void DecreaseScore() // decreases score by 1
    {
        score--;
        if (score < 0) { score = 0; }   // if the score is less than 0, set it to 0 (no negative scores)
        UIManager.UpdateScore(score);   // updates the score graphic
    }
}
