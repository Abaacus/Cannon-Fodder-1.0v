using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
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

    CameraController cameraController;  // set up pathports
    CannonController cannonController;
    public bool cameraType;   // bool is used to differtiate between camera types
    public float followSpeedFactor; // how fast the camera moves

    void Start()
    {
        cameraType = true;    // lock the camera
        cannonController = CannonController.instance;
        cameraController = CameraController.instance;   // init pathways
    }

    void Update()
    {
        if (Input.GetButtonDown("ToggleCameraType"))
        {   // if we are unclocking
            if (!cameraController.lockToggle)
            {   // if the ability to toggle isn't disabled
                Debug.Log("Camera lock toggled");
                cameraType = !cameraType;   // invert the current camera type
                UpdateCameraType();
            }
        }

        UpdateLocks();
        SetCameraTarget();
    }

    void UpdateLocks()
    {
        if (cameraController.lockToggle)
        {   // if the camera is in mode 1, lock the aim and shooting
            cannonController.aimLocked = true;
            cannonController.shootLocked = true;
        }
        else
        {   // if the camera is in mode 1, lock the aim and shooting
            cannonController.aimLocked = false;
            cannonController.shootLocked = false;
        }
    }

    void UpdateCameraType()
    {
        if (cameraType)
        {    // update the cameraController's camera type based off of being option 1 or option 2
            cameraController.cameraMovementType = CameraMovementType.followTarget;
        }
        else
        {
            cameraController.cameraMovementType = CameraMovementType.showStage;
        }
    }

    void SetCameraTarget()
    {
        if (cannonController.cannonShot)
        {   // if the cannon has been shot, update the camera speed to be able to follow the bullet
            cameraController.followSpeed = cannonController.power/followSpeedFactor;
            if (cannonController.bullet != null)
            {   // if the bullet exists, set the target to the bullet
                cameraController.target = cannonController.bullet.transform;
            }
            else
            {
                cannonController.cannonShot = false;
                cameraController.followSpeed = 1;   // otherwise, set the camera to default, focusing on the cannon
                cameraController.target = cannonController.transform;
            }
        }
        else
        {
            cameraController.followSpeed = 1;   // if the cannon hasn't been shot, use the camera's default focusing
            cameraController.target = cannonController.transform;
        }
    }
}
