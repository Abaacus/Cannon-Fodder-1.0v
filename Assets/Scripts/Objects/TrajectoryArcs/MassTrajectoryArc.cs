using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassTrajectoryArc : TrajectoryArc
{
    Bullet currentBullet;

    void Update() 
    {
        if (cannonController.bullet != null)
        {
            currentBullet = cannonController.bullet.GetComponent<Bullet>();
        }
    }

    public override float CalculateHeight(float x)
    {
        float height = x * Mathf.Tan(launchRadians);
        height -= g * Mathf.Pow(x, 2) / (Mathf.Pow(power / currentBullet.powerFactor, 2) * Mathf.Pow(Mathf.Cos(launchRadians), 2) * 2) * currentBullet.windFactor;
        if (debugCalculation)
        {
            Debug.Log("Height of X: " + x + " with intial power " + power + ", launchDegree of " + launchRadians * Mathf.Rad2Deg + ", gravity of " + Physics.gravity.y + ": " + height);
        }
        return height;
    }

    public override float CalculatePointSlope(float x)
    {
        float slope = Mathf.Tan(launchRadians);
        slope -= g * x / (Mathf.Pow(Mathf.Cos(launchRadians), 2) * power / currentBullet.powerFactor) * currentBullet.windFactor;
        if (debugCalculation)
        {
            Debug.Log("Slope of X " + x + " with intial power " + power + ", launchDegree of " + launchRadians * Mathf.Rad2Deg + ", gravity of " + Physics.gravity.y + ": " + slope);
        }
        return slope;
    }

    public override Vector3 CalculateVertex()
    {
        float length = power / currentBullet.powerFactor * Mathf.Tan(launchRadians) * Mathf.Pow(Mathf.Cos(launchRadians), 2) / g / currentBullet.windFactor;
        float height = CalculateHeight(length); // finds the midpoint, then calculates the height for that midpoint, returning the 2 values as the vertex

        return new Vector3(0, height, length);
    }
}