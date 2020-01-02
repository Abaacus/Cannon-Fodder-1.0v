using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryArc : MonoBehaviour      // Used to create the arc that predicts where the cannon ball will fly *** DOESN'T account for the bullet's powerFactor or windFactor ***
{
    public static CannonController cannonController;    // takes values from the cannonController, specifically the angle and power
    public Transform origin;    // start point of the arc {moves the arc pass through point, (0,0) to (h,k)}
    Vector3 vertex;   // vertex of the graph 

    public const float g = 9.81f;  // gravity
    public float power;    // power level that the cannon is being shot at
    public float launchRadians;    // angle that the cannon is being shot at (in radians)
    public float powerFactor;   // how much the trajectoryArc modifies the power by
    public float arcDegreeBuffer;   // how many degrees

    public bool debugCalculation;   // should we debug what X's = what Y's?
    public bool debugLine;  // should we debug where the lines are being drawn?


    public GameObject trajectoryDashObject; // base gameObject the dashes imitate
    GameObject[] dashes;    // array of dash gameObjects
    public float dashWidth; // thickness of the dash
    public float dashLength;    // length of the trajectoryArc's dashes
    public float dashGaps;   // space between the trajectoryArc's dashes
    public int numberDashes;    // max number of dashes generated
    public bool vertexMasking; // stop rendering dashes after the index?
    
    private void Start()
    {
        cannonController = CannonController.instance;
        InitializeDashArray();  // on start, make initialize the dashes
    }

    public void DrawPath()
    {
        if (dashes.Length != numberDashes)
        {   // if the dash array isn't the desired number of dashes, create a new array
            InitializeDashArray();
        }   

        launchRadians = (cannonController.barrelAngle + arcDegreeBuffer) * Mathf.Deg2Rad;   // convert the cannon's angle (with the arc degree buffer) into radians
        power = cannonController.power / powerFactor;   // set the calculating velocity to an altered version of the cannon's power

        vertex = CalculateVertex() + origin.position;  // calculates the vertex of the parabola (the max height of the ball)

        float x = 0f;   // initialize the X of the graph at 0
        for (int i = 0; i < numberDashes; i++)
        {   // for each dash, calculate the dash endpoint using the current x and the desired dash length
            Vector3[] lineEndpoints = CalculateEndPoints(x, dashLength);
            // convert that information into dashes, and modifies the dash at the current index
            CreateDashLine(lineEndpoints, i);

            if (lineEndpoints[1].y + origin.position.y < 0 || x + origin.position.z > vertex.z)
            {   // if the line is below the 0 axis, or the x is past the vertex, don't render this part of the line
                dashes[i].SetActive(!vertexMasking);
            }
            else
            {   // otherwise, make/keep the dash visible
                dashes[i].SetActive(true);
            }
            // increase the x by the dash length and the dsah gaps
            x += dashGaps + dashLength;
        }
    }

    Vector3[] CalculateEndPoints(float x, float dashLength)   // takes in an x value, finds the equation of the tangent for that x, then uses that linear equation to calculate another point on that line
    {
        float slope = CalculatePointSlope(x);  // calculates the height and the slope of the end point
        float yOffset = CalculateHeight(x);
        return new Vector3[]
        {   new Vector3(0, slope * x + yOffset, x), // use this y = mx + b (linear equation) to find two points tangent to the graph, used as the end points for the dash
            new Vector3(0, slope * (x - dashLength) + yOffset,  x - dashLength)
        };
    }

    public virtual float CalculateHeight(float x)   // Calculates the y at a particular spot on the function
    {   //     *** NOTE: the method has the 'virtual' keyword, allowing new classes that derive from TrajectoryArc to override this method and use a different function { turn f(x) to g(x) } ***
        float height = x * Mathf.Tan(launchRadians);
        height -= g * Mathf.Pow(x, 2) / (Mathf.Pow(power, 2) * Mathf.Pow(Mathf.Cos(launchRadians), 2) * 2);
        if (debugCalculation)
        {   // debug the results of our calculation
            Debug.Log("Height of X: " + x + " with intial power " + power + ", launchDegree of " + launchRadians * Mathf.Rad2Deg + ", gravity of " + Physics.gravity.y + ": " + height);
        }
        return height;
    }

    public virtual float CalculatePointSlope(float x)   // Calculates the derivative at a particular spot on the function (it's rise/run, or the directional velocity of the bullet)
    {   // marked virtual for the same reason as CalculateHeight
        float slope = Mathf.Tan(launchRadians);
        slope -= g * x / (Mathf.Pow(Mathf.Cos(launchRadians), 2) * power);
        if (debugCalculation)
        {
            Debug.Log("Slope of X " + x + " with intial power " + power + ", launchDegree of " + launchRadians * Mathf.Rad2Deg + ", gravity of " + Physics.gravity.y + ": " + slope);
        }
        return slope;
    }

    public virtual Vector3 CalculateVertex()
    {
        float length = power * Mathf.Tan(launchRadians) * Mathf.Pow(Mathf.Cos(launchRadians), 2) / g;
        float height = CalculateHeight(length); // finds the midpoint, then calculates the height for that midpoint, returning the 2 values as the vertex

        return new Vector3(0, height, length);
    }

    void CreateDashLine(Vector3[] lineEndpoints, int index) // takes in two end points, and the index of the dash it's modifying, and converts it into real space
    {
        Vector3 spawnPoint = ((lineEndpoints[0] + lineEndpoints[1]) / 2) + origin.position; // finds the middle of these two endpoints, and moves it accordingly to the graph's (0,0) spot
        float lineLength = Vector3.Distance(lineEndpoints[0], lineEndpoints[1]);    // finds the length the dash needs to be to cross through these two points
        Vector3 rotationEulers = Vector3.right * Mathf.Asin((lineEndpoints[1].y - lineEndpoints[0].y) / lineLength);    // finds the angle of rotation needed to hit the two endpoints
        rotationEulers *= Mathf.Rad2Deg;    // converts the angle into degrees

        dashes[index].transform.position = spawnPoint;
        dashes[index].transform.rotation = Quaternion.Euler(rotationEulers);    // applies the transformations to the dash
        dashes[index].transform.localScale = new Vector3(dashWidth, dashWidth, lineLength);

        Debug.DrawLine(lineEndpoints[0] + origin.position, lineEndpoints[1] + origin.position, Color.white, 0.1f);  // debug draws the line

        if (debugLine)
        {
            Debug.Log("Raw line: " + lineEndpoints[0] + " to " + lineEndpoints[1]); // if we want to debug the line, print some info about it
            Debug.Log("Drew line from " + (lineEndpoints[0] + origin.position) + " to " + (lineEndpoints[1] + origin.position));
        }
    }

    void InitializeDashArray()
    {
        if (dashes != null)
        {   // if the dash array exists, we destroy all the dashes in it to avoid having duplicates
            foreach (GameObject dash in dashes)
            {
                Destroy(dash);
            }
        }

        dashes = new GameObject[numberDashes];  // creates a new dash array to hold the dashes we want

        for (int i = 0; i < numberDashes; i++)  // can't use foreach because all the entries are null!
        {
            dashes[i] = Instantiate(trajectoryDashObject, transform);   // creates theses dashes
            dashes[i].transform.localScale = new Vector3(dashWidth, dashWidth, 1);
        }
    }

    public void EnableDashes(bool enable)   // this allows us to hide unused trajectory arcs to save proccessing power
    {
        if (dashes != null)
        {   // if the dashes exist...
            for (int i = 0; i < numberDashes; i++)
            {   // enable or disable them all, depending on what was inputted
                dashes[i].SetActive(enable);
            }
        }
    }
}