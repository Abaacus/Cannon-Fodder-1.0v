using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public static CloudController instance;
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

    public List<Cloud> clouds = new List<Cloud>();  // create a list of cloud objects (essentially holds the mass value for each cloud)
    public Mesh[] cloudMeshs;   // array of possible cloud shapes
    public Material cloudMaterial;  // material assigned to clouds

    WindController windController;  // accesses wind speeds to move clouds at multiple speeds
    public float windSpeedReductionFactor;  // value that the windSpeed is reduced by

    public float cloudLimit;    // z value at which clouds dissapear
    public Vector3 cloudRotation;   // staring rotation in euler angles

    public float averageCloudMass;  // base cloud mass  (used in determining how fast cloud moves, heavy clouds are less affected by wind, lighter clouds are more affected)
    public float cloudMassBuffer;   // range of possible cloud mass adjustments, or how much the mass could increase or decrease by to create uniqueness { base + Random.Range(-buffer, buffer) }

    public Vector3 averageCloudSize;    // base cloud scale (in XYZ)
    public Vector3 cloudSizeBuffer;     // range of cloud scale adjustments (each axis is affected independently)

    public Vector3 cloudSpawnpoint;     // base spawn pos for clouds
    public Vector3 cloudPointBuffer;    // range of possible spawn pos adjustments (axes affected independently)

    public int initClouds;  // how many clouds start
    [Range(0.01f, 10)]
    public float spamCloudMassMod; // how heavy the starting spam clouds weigh (should be lower than default to push them on screen)

    float spawnTimer;   // how much longer the script wait before making another cloud (constantly changing)
    public float averageSpawnTime;   // base time to wait before creating a new cloud
    public float spawnTimeBuffer;   // range of time adjustments

    private void Start()
    {
        windController = WindController.instance;   // access the windController instance and save the path
        spawnTimer = averageSpawnTime + spawnTimeBuffer; // set the spawntime to it's maximum possible wait
    }

    public void FixedUpdate()   // fixed update is used to move the clouds smoothly
    {
        if (spawnTimer <= 0)
        {   // if the spawnTimer has reached (or gone past) 0, create a new cloud
            GenerateNewCloud();
        }
        // update the spawnTimer by subtracting the time that has passed since the last frame (about 0.02s) multiplied by cloud speeds (faster the clouds, the more clouds we need, so they need to spawn in at a rate base off of the cloud speeds)
        spawnTimer -= Time.fixedDeltaTime * Mathf.Abs(windController.windStrength) * windSpeedReductionFactor;
        UpdateCloudPos();   // update the cloud positions
    }

    void UpdateCloudPos()
    {
        foreach (Cloud cloud in clouds)
        {   // loops through every cloud in clouds, current cloud in loop is accessible as < cloud >
            if (cloud != null)
            {   // if the cloud still exists (it might have been destoryed while we were looping through other clouds)
                cloud.transform.position += Vector3.forward * windController.windStrength * windSpeedReductionFactor / cloud.mass;  // move the cloud along it's z-axis by the cloud speed divided by the cloud's weight
                if (Mathf.Abs(cloud.transform.position.z) > cloudLimit)
                {   // if cloud is too far left or right (greater than the cloud limit)
                    clouds.Remove(cloud);   // remove the script from the list
                    Destroy(cloud.gameObject);  // destroy the cloud game object
                    UpdateCloudPos();   // run a new loop (foreach loops throw an error when an entry in them is deleted)
                    break;  // cancel the rest of this loop
                }
            }
        }
    }

    void GenerateNewCloud()
    {
        spawnTimer = averageSpawnTime + Random.Range(-spawnTimeBuffer, spawnTimeBuffer);    // reset the spawnTimer

        clouds.Add(CreateCloud());  // create a new cloud and add it to the cloud list
    }

    Cloud CreateCloud(float massMod = 1, string name = "cloud") // returns a cloud based off of the cloudController's settings. the parameters are used to adjust the spammed starter clouds' speed and name
    {
        GameObject cloudObj = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));   // create a game object named "name", and given the meshRenderer and meshFilter components
        cloudObj.transform.parent = transform;  // cloud parent is set to the cloudController (helps with sorting)
        cloudObj.transform.Rotate(cloudRotation);   // rotates the cloud to a proper rotation
        cloudObj.transform.position = new Vector3
        {   // set the cloud spawnpoint to the base XYZ, and adjust it slightly by a random number from -buffer to buffer
            x = cloudSpawnpoint.x + Random.Range(-cloudPointBuffer.x, cloudPointBuffer.x),
            y = cloudSpawnpoint.y + Random.Range(-cloudPointBuffer.y, cloudPointBuffer.y),
            z = (cloudSpawnpoint.z * windController.windDirection) + Random.Range(-cloudPointBuffer.z, cloudPointBuffer.z)
        };   // *NOTE* the z pos is multiplied by the wind direction so it spawns the side of the level where it will blow on stage
        cloudObj.transform.localScale = new Vector3
        {   // set the cloud scale to the base XYZ, then adjust it slightly by a random number from -buffer to buffer
            x = averageCloudSize.x + Random.Range(-cloudSizeBuffer.x, cloudSizeBuffer.x),
            y = averageCloudSize.y + Random.Range(-cloudSizeBuffer.y, cloudSizeBuffer.y),
            z = averageCloudSize.z + Random.Range(-cloudSizeBuffer.z, cloudSizeBuffer.z)
        };

        cloudObj.GetComponent<MeshRenderer>().sharedMaterial = cloudMaterial;   // set the meshRenderer's material to the cloudMaterial
        cloudObj.GetComponent<MeshFilter>().sharedMesh = cloudMeshs[Random.Range(0, cloudMeshs.Length)];    // set the cloud's mesh to a random mesh from the cloudMesh array

        Cloud cloudScr = cloudObj.AddComponent<Cloud>();    // add a cloud monobehaviour to the cloud object
        cloudScr.mass = massMod * (averageCloudMass + Random.Range(-cloudMassBuffer, cloudMassBuffer));    // set the cloud's mass to base + (-buffer to buffer) mulitplied by the mass mod

        return cloudScr;    // return the cloud monobehaviour to add to the list
    }

    public IEnumerator CloudSpammer()
    {
        for (int i = 0; i < initClouds; i++)
        {   // loop thorugh the number of clouds we want to start with
            clouds.Add(CreateCloud(spamCloudMassMod, "spammedCloud")); // create a cloud with the spamCloudMass modifier, named "spammedCloud"
            yield return new WaitForSecondsRealtime(2 * spamCloudMassMod); // wait for 2 * spawmCloudMass modifier before making a new spammed cloud
        }

        yield return new WaitForSecondsRealtime(spawnTimeBuffer);   // wait for the spawnTimeBuffer amount (kind of a hradcoded number, no really sure why it works)

        foreach (Cloud cloud in clouds)
        {   // for every cloud in the cloud list
            if (cloud != null)
            {   // if the cloud isn't destroyed
                cloud.mass = averageCloudMass + Random.Range(-cloudMassBuffer, cloudMassBuffer);    // reset the cloud mass to a normal mass
            }
        }
    }
}
