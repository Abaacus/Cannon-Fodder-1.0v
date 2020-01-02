using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    [HideInInspector]
    public static PrefabPlacer instance;
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

    public Boundary cannonBoundary; // spawn boundary information for the cannon and target (mostly used to visualize the boundaries) 
    public Boundary targetBoundary;

    private void Start()
    {
        cannonBoundary.objTransform = CannonController.instance.transform;  // set the boundaries object's to the cannon and target
        targetBoundary.objTransform = Target.instance.transform;
    }

    public void UpdateBoundaries()
    {
        cannonBoundary.UpdateBoundary();
        targetBoundary.UpdateBoundary();
    }

    public void UpdatePos()    // create a new pos for the cannon and target
    {
        cannonBoundary.GenerateNewPos();
        targetBoundary.GenerateNewPos();
    }
}
