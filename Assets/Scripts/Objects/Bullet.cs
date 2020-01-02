using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]   // forces a rigidbody on the gameObject that has this monoBehaviour
public class Bullet : MonoBehaviour
{
    CannonController cannonController;  // reference to the cannon
    AudioManager audioManager; // reference to the audioManager
    [HideInInspector]
    public Rigidbody rb;    // quick access to the rigidbody

    public float powerFactor;   // how much the projectile is affected by the cannon's powers
    public float windFactor;    // how much the projectile is affected by the wind

    static readonly float decaySpeed = 0.2f;    // how slow the bullet is moving before the game destroys it
    static readonly float decayTime = 4f;   // how long it waits while the bullet is at or below the decaySpeed before destroying it
    static readonly float lifeTime = 5f;    // the max time the bullet can be shot for
    float windSpeed;    // the force the bullet adds to it's speed
    bool bulletShot;

    float lifeTimeDelta = 0;    // how long the bullet has been launched for
    float decayTimeDelta = 0;   // how long the bullet has been decaying due to slowness


    private void Start()
    {   // on creation, create pathway to the cannon, audioManager, and rigidbody
        cannonController = CannonController.instance;
        audioManager = AudioManager.instance;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;  //stop the bullet from being affected by Physics
    }

    private void FixedUpdate()
    {
        if (bulletShot)
        {   // if the bullet has been shot, push it backwards based off of the windSpeed
            rb.AddForce(Vector3.forward * windSpeed);
            Decay();    // check to see if the bullet should be destroyed
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        audioManager.PlaySound("Thud");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Target"))
        {   // if the bullet has hit the target, run a function
            HitTarget();
        }
    }
    public void LaunchBall()
    {
        if (!bulletShot)
        {   // if the bullet hasn't been shot...
            windSpeed = WindController.instance.windStrength / windFactor;  // get the most recent windSpeed
            Vector3 direction = new Vector3(0, Mathf.Sin(cannonController.barrelAngle * Mathf.Deg2Rad), Mathf.Cos(cannonController.barrelAngle * Mathf.Deg2Rad));   // convert the angle to a directional vector
            Debug.DrawLine(transform.position, transform.position + direction * (cannonController.power / powerFactor), Color.red, 3);    // debug the cannon's direction
            rb.isKinematic = false;
            rb.AddForce(direction * (cannonController.power / powerFactor), ForceMode.Impulse); // add the force to the ball
            rb.AddTorque(new Vector3(cannonController.power, 1f, 1f));  // adds a nice spin
            bulletShot = true;  // declare that the bullet has been shot
        }
    }

    void Decay()
    {
        if (!rb.isKinematic)
        {   // if the bullet is moving
            if (rb.velocity.magnitude < decaySpeed)
            {   // if it's speed is less than the decay threshold, start the decay Timer
                decayTimeDelta += Time.deltaTime;
                if (decayTimeDelta > decayTime)
                {   // if the decay timer has reached it's end, reset the shot
                    GameManager.instance.ResetShot();
                }
            }
            else
            {   // if the bullet has is still or has become above the decay threshold, set the decay timer to 0
                decayTimeDelta = 0;
            }

            lifeTimeDelta += Time.deltaTime;
            if (lifeTimeDelta > lifeTime)   // update the lifeTimer constantly, and reset the shot when it reaches 0
            {
                GameManager.instance.ResetShot();
            }
        }
    }

    void HitTarget()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero; // freeze the bullet
        rb.angularVelocity = Vector3.zero;

        GameManager.instance.OnTargetHit(); // run the target hit code for the main game
    }
}
