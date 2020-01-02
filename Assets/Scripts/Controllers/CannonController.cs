using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    public static CannonController instance;    //  singleton
    CameraController cameraController;
    AudioManager audioManager;

    public Transform barrel;    // rotation and position of the cannon barrel
    public Transform bulletSpawnpoint;  // position and rotation of where bullets spawn in

    public float[] maxMinAngle = new float[2];  // min and max firing angles
    public float barrelAngle;   // current firing angle
    float aimSensitvity;    // current sensitivity to user input for the firing angle (defaults to 1)
    [Range(0.001f, 1)]
    public float shootAimSensitvity;    // sensitivity to user input for the firing angle while shooting the ball 
    public float[] maxMinPower = new float[2];  // min and max firing power levels
    public float power; //  current firing power level
    public float powerChargeSpeed;  // speed at which the cannon's power increases
    public float shotShakeStrength;

    public TrajectoryArc[] trajectoryArcs;  // class used to calculate trajectorys
    public int trajectoryArcIndex; // which trajectory arc being used
    public GameObject[] bulletPrefabs;  // list of possible bullet prefabs
    public GameObject bullet;   // current bullet gameObject
    int bulletType; // index of the level's bullet type

    public bool aimLocked;      // is aiming enabled?
    public bool shootLocked;    // is shooting enabled
    public bool cannonShot;     // has the cannon been fired?
    bool bulletSpawned;     // has a bullet been spawned

    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Debug.LogError("Multiple instances of " + this + " found");
        }
        instance = this;
        #endregion

        aimSensitvity = 1;  // default aim sensitivity to 1
    }

    void Start()
    {
        audioManager = AudioManager.instance;
        cameraController = CameraController.instance;
        StartCoroutine(InitializeTrajectoryArc()); // initialize the cannon (it really just starts up the trajectory arc)
    }

    void Update()
    {
        AimBarrel();    // check for change in firing angle
        Shoot();    // check to see if the player started shooting

        if (!bulletSpawned)
        {   // if the cannon has no bullet, instatiate a new one
            CreateNewBullet();
        }

        if (Input.GetAxis("Vertical") != 0 || Input.GetButton("Shoot"))
        {   // if the player is aiming or shooting, update the trajectory arc
            UpdateTrajectoryArc();
        }

    }

    void AimBarrel()
    {
        if (!aimLocked)
        {   // if aiming is enabled... 
            barrelAngle += Input.GetAxis("Vertical") * aimSensitvity;   // update the barrel angle based off of player input * the current sensitivity (usually 1)
            if (barrelAngle < maxMinAngle[0])
            {   // if the new angle is less than the min angle, set it to min
                barrelAngle = maxMinAngle[0];   
            }
            if (barrelAngle > maxMinAngle[1])
            {   // if the new angle is greater than the max angle, set it to max
                barrelAngle = maxMinAngle[1];
            }

            if (barrel != null)
            {   // if the barrel exists, update it's rotation based off of our barrel angle
                barrel.eulerAngles = Vector3.right * (360 - barrelAngle);
            }
        }

        if (bullet != null)
        {   // if the bullet exists...
            if (!cannonShot)    
            {   // and the cannon isn't shot...
                bullet.transform.position = bulletSpawnpoint.position;  // move the bullet to the bullet spawnpoint (the front of the cannon)
            }   // *NOTE* the bullet spawnpoint will move with the cannon as it is rotated because it is parented to the cannon barrel
        }
    }

   void UpdateTrajectoryArc()
    {
        if (trajectoryArcIndex < 0 || trajectoryArcIndex >= trajectoryArcs.Length)
        {
            trajectoryArcIndex = 0;
        }

        for (int i = 0; i < trajectoryArcs.Length; i++)
        {
            if (i != trajectoryArcIndex)
            {
                trajectoryArcs[i].EnableDashes(false);
            }
            else
            {
                trajectoryArcs[i].EnableDashes(true);
                trajectoryArcs[i].DrawPath();
            }
        }
    }

    void Shoot()
    {
        if (!cannonShot)
        {
            if (!shootLocked)
            {   // if shooting isn't locked & "Shoot" is being held down
                if (Input.GetButtonDown("Shoot"))
                {   // start the ShootCannon coroutine
                    StartCoroutine(ShootCannon());
                }
            }
        }
    }
    public void ResetCannon()
    {   // this is the default state the cannon returns to
        aimSensitvity = 1;  
        power = maxMinPower[0]; // set the power to it's min possible value
        aimLocked = false;
        shootLocked = false;
        cannonShot = false;
        bulletSpawned = false;

        UpdateTrajectoryArc();  // after these adjustments, update the trajectory arc
    }

    public void RandomizeBulletType()
    {   // this picks a random number between (0 - number of bullet types). this number will be reassinged each level, and is used when generating bullet types 
        bulletType = Random.Range(0, bulletPrefabs.Length);
    }

    void LaunchBall()
    {   // finds the bullet monobehaviour attached to the bullet var and run LaunchBall
        bullet.GetComponent<Bullet>().LaunchBall();
        audioManager.PlaySound("CannonShot");
        cameraController.SetShake(shotShakeStrength);
        aimLocked = true;
        shootLocked = true; // lock aiming, shooting, and set cannonShot to true
        cannonShot = true;
    }

    public void CreateNewBullet()
    {
        if (bullet != null)
        {   // if a bullet already exists, destroy it
            Destroy(bullet);
        }
        // instantiate a bullet from the bullet prefabs at the bullet spawnpoint. the bullet it picks is the random number assigned earlier
        bullet = Instantiate(bulletPrefabs[bulletType], bulletSpawnpoint.position, Quaternion.identity);
        bulletSpawned = true;
    }

    IEnumerator InitializeTrajectoryArc()  // used to update the Trajectory Arc before the player has touched anything
    {
        yield return new WaitForEndOfFrame();  // waits until the frame after it was started
        barrelAngle = (maxMinAngle[0] + maxMinAngle[1]) / 2;
        barrel.eulerAngles = Vector3.right * (360 - barrelAngle);
        UpdateTrajectoryArc();  // updates the trajectory arc
    }

    IEnumerator ShootCannon()   // used to charge and then shoot the cannon
    {   
        aimSensitvity = shootAimSensitvity; // set the aiming sensistivity to the shooting sensitivity (allows for tiny adjustments instead of locking down your angle)
        while (Input.GetButton("Shoot"))
        {   // while shoot is being held down...
            power += powerChargeSpeed;  // increase power by the power charging rate
            if (power > maxMinPower[1])
            {   // if the power is greater than the max power, set it to the max power
                power = maxMinPower[1];
            }
            yield return new WaitForFixedUpdate();  // wait for fixedUpdate (runs consistently every 0.02 seconds, regardless of framerate)
        }

        LaunchBall();   // after the shoot button is released, launch the ball
    }
}