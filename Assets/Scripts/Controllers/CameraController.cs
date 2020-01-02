using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraMovementType { followTarget, playerControlled, showStage }

public class CameraController : MonoBehaviour
{
    public static CameraController instance;    // Singleton, any class can access the instance through this var

    public CameraMovementType cameraMovementType;   // what rules the camera is using to calculate it's next pos
    public float[] heightBorders = new float[2];    // min & max y for player controlled camera
    public float[] widthBorders = new float[2];     // min & max z for player controlled camera

    public Transform target;    // target that the camera focueses on

    public float cameraSpeed;   // player controlled speed
    public float followSpeed;   // auto follow speed
    public float zoomSpeed;     // how fast the camera zooms (moves on the x axis)

    public float shakeDecaySpeed;
    public float shakeStrength;

    public bool zoomed;     // is the camera zoomed?
    float defaultZoom;  // zoom value to reset too
    public float targetZoom; // how much the camera zooms in on a target that it's following
    public float stageZoom;     // how much the camera zooms to view the whole stage

    public Vector2 centreStage; // where the camera centers to while viewing the whole stage

    public bool lockToggle;     // whether or not the player ca switch between camera modes

    private void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Debug.LogError("Multiple instances of " + this + " found");
        }
        instance = this;
        #endregion

        defaultZoom = transform.position.x; // sets the default zoom to the camera's starting x pos
    }

    void FixedUpdate()  // updates not every frame, but 0.02 seconds, allowing us to have accurate camera movement based off time, not frames
    {
        switch (cameraMovementType)     // basically, run the function based on what move type being used
        {
            default:
                cameraMovementType = CameraMovementType.followTarget;   // if the cameraMoveType bugged out, just set it to follow target
                break;

            case CameraMovementType.followTarget:
                FollowTarget();
                break;

            case CameraMovementType.playerControlled:
                PlayerControlled();
                break;

            case CameraMovementType.showStage:
                ShowStage();
                break;
        }

        Shake();
    }

    void Shake()
    {
        transform.position += new Vector3
        {
            x = 0,
            y = Mathf.Lerp(0, Random.Range(-shakeStrength, shakeStrength), Time.deltaTime),
            z = Mathf.Lerp(0, Random.Range(-shakeStrength, shakeStrength), Time.deltaTime),
        };

        shakeStrength -= shakeDecaySpeed;
        if (shakeStrength < 0)
        {
            shakeStrength = 0;
        }
    }

    public void SetShake(float setShake)
    {
        shakeStrength = setShake;
    }

    void ShowStage()
    {
        transform.position = new Vector3    // set the camera's x, y, & z to the following values
        {       // lerp (linear interpolation) creates nice smooth chages to move from value A to value B. Lerp between the current X and the full stage zoom, at a speed of the zoom speed
            x = Mathf.Lerp(transform.position.x, stageZoom, Time.deltaTime * zoomSpeed),
            y = Mathf.Lerp(transform.position.y, centreStage.y, Time.deltaTime * followSpeed),  // lerp between current Y and center stage Y, at followSpeed
            z = Mathf.Lerp(transform.position.z, centreStage.x, Time.deltaTime * followSpeed)   // lerp between current Z and center stage Z, at followSpeed
        };
    }

    void FollowTarget()
    {
        if (target != null) // if a target has been found, we can follow it
        {
            if (zoomed) // do one if the camera is zoomed, or do the other if it isn't
            {
                transform.position = new Vector3
                {
                    x = Mathf.Lerp(transform.position.x, targetZoom, Time.deltaTime * zoomSpeed),   // lerp between current X and target zoom, at zoomSpeed
                    y = Mathf.Lerp(transform.position.y, target.position.y, Time.deltaTime * followSpeed),  // lerp bewtween current Y and the target's Y, at followSpeed
                    z = Mathf.Lerp(transform.position.z, target.position.z, Time.deltaTime * followSpeed)   // lerp bewtween current Z and the target's Z, at followSpeed
                };
            }
            else
            {
                transform.position = new Vector3
                {
                    x = Mathf.Lerp(transform.position.x, defaultZoom, Time.deltaTime * zoomSpeed),  // lerp between current X and default zoom, at zoomSpeed
                    y = Mathf.Lerp(transform.position.y, target.position.y, Time.deltaTime * followSpeed),  // lerp bewtween current Y and the target's Y, at followSpeed
                    z = Mathf.Lerp(transform.position.z, target.position.z, Time.deltaTime * followSpeed)   // lerp bewtween current Z and the target's Z, at followSpeed
                };
            }
        }
    }

    void PlayerControlled()
    {
        Vector3 newPos = new Vector3
        {   // calculate a new pos based off of the user's inputs
            x = Mathf.Lerp(transform.position.x, defaultZoom, Time.deltaTime * zoomSpeed),   // lerp between current X and default zoom, at zoomSpeed
            y = transform.position.y + Input.GetAxis("Vertical") * cameraSpeed,     // add (or subtract) the user's input to it's current pos on the Y axis
            z = transform.position.z + Input.GetAxis("Horizontal") * cameraSpeed    // add (or subtract) the user's input to it's current pos on the Z axis
        };

        if (newPos.y < heightBorders[0])    // if the new pos Y less than the min Y, set the new pos Y to the min Y
        {
            newPos.y = heightBorders[0];
        }
        else if (newPos.y > heightBorders[1])    // if the new pos Y greater than the max Y, set the new pos Y to the max Y
        {
            newPos.y = heightBorders[1];
        }

        if (newPos.z < widthBorders[0])  // if the new pos Z less than the min Z, set the new pos Z to the min Z
        {
            newPos.z = widthBorders[0];
        }
        else if (newPos.z > widthBorders[1])    // if the new pos Z greater than the max Z, set the new pos Z to the max Z
        {
            newPos.z = widthBorders[1];
        }

        transform.position = newPos;    // set the camera's pos to the new pos calculated
    }

    public IEnumerator ShowLevel()  // IEnumerator's are functions that can be run as coroutines, enabling the ability to have code run parallel to the main code ( started with StartCoroutine(CoroutineName()); )
    {   // this IEnumerator shows the entire level at the start of a new level, then resumes focus on the cannon
        lockToggle = true;  // lock the user's ability to toggle bewtween camera move types
        cameraMovementType = CameraMovementType.showStage;  // set the camera move type to showStage
        yield return new WaitForSecondsRealtime(5); // wait for 5 seconds
        cameraMovementType = CameraMovementType.followTarget;    // set the camera move type to followTarget
        lockToggle = false; // unlock the user's ability to toggle bewtween camera move types
    }
}
