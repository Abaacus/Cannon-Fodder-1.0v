using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    GameManager gameManager;

    public override void OnInspectorGUI()
    {

        if (gameManager.gameStarted)
        {
            if (GUILayout.Button("Bump Score"))
            {
                gameManager.IncreaseScore();
            }

            if (GUILayout.Button("Reset Score"))
            {
                gameManager.DecreaseScore();
            }

            if (GUILayout.Button("Generate New Level"))
            {
                if (!gameManager.levelGenerating)
                {

                    gameManager.StartCoroutine(gameManager.GenerateNewLevel());
                }
            }
        }
    }

    private void OnEnable()
    {
        gameManager = (GameManager)target;
    }
}
