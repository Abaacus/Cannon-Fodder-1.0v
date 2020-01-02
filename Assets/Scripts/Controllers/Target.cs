using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public static Target instance;  // SINGLETON (I love these things)
    public Transform target;    // the gameObject that the player is trying to hit
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

    public float[] minMaxAngles = new float[2]; // min and max angles it can generate
    public bool hit;    // has the target been hit?

    public void GenerateNewAngle()  // generates (and applies) an angle in the defined range
    {
        target.eulerAngles = new Vector3(Random.Range(minMaxAngles[0], minMaxAngles[1]), 0, 0); 
    }
}