using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class Grid
{
    int width;
    int height;
    float cellSize;
    Vector3 origin;
    CelestialMass[] planets;
    Rigidbody ball;
    Vector2[,] gridArray;

    public Grid(int width, int height, float cellSize, int subDivisions, float maxMult, float divisor, Vector3 origin, CelestialMass[] planets, Rigidbody ball, LineRenderer xlr, LineRenderer ylr)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;
        this.planets = planets;
        this.ball = ball;


        gridArray = new Vector2[width, height];
        if(subDivisions == 0 ) { return; }
        xlr.positionCount = 0;
        xlr.positionCount = subDivisions*(width * height);
        ylr.positionCount = 0;
        ylr.positionCount = subDivisions*(width * height);
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1) * subDivisions; y++)
            {
                float yValue = (float)y / subDivisions;
                Vector3 pos = GetWorldPosition(x, yValue) + new Vector3(cellSize, cellSize) * 0.5f;
                Vector3 displacement = ArtificialGravity.CalculateDisplacement(GetWorldPosition(x, yValue), planets, cellSize * maxMult, divisor);

                if (x % 2 == 0 || x == 0) {
                    int h = height * subDivisions;
                    xlr.SetPosition((x) * (h) + y, pos + displacement); 
                }
                else { 
                    int h = height*subDivisions;
                    xlr.SetPosition((x) * (h) + ((h - 1) - y), pos + displacement); 
                }
            }
        }
        for (int y = 0; y < gridArray.GetLength(1); y++)
        {
            for (int x = 0; x < gridArray.GetLength(0) * subDivisions; x++)
            {
                float xValue = (float)x / subDivisions;
                Vector3 pos = GetWorldPosition(xValue, y) + new Vector3(cellSize, cellSize) * 0.5f;
                Vector3 displacement = ArtificialGravity.CalculateDisplacement(GetWorldPosition(xValue, y), planets, cellSize * maxMult, divisor);

                if (y % 2 == 0 || y == 0)
                {
                    int w = width * subDivisions;
                    ylr.SetPosition((y) * (w) + x, pos + displacement);
                }
                else
                {
                    int w = width * subDivisions;
                    ylr.SetPosition((y) * (w) + ((w - 1) - x), pos + displacement);
                }
            }
        }
    }
    Vector3 GetWorldPosition(float x, float y)
    {
        return new Vector3(origin.x-width/2, origin.y-height/2) * cellSize + new Vector3(x, y) * cellSize;
    }
}
public static class ArtificialGravity
{
    public static float G = 1f;

    public static Vector3 AttractForce(float bigMass, float smallMass, Transform big, Transform small) //brackeys gravity tutorial
    {
        Vector3 direction = big.position - small.position; //direction vector
        float distance = direction.magnitude; //distance
        if(distance == 0){
            distance = 0.00001f;
        }

        float forceMagnitude = ArtificialGravity.G * (smallMass * bigMass) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
        Vector3 force = direction.normalized * forceMagnitude; //storing force

            return force; //returning force
    }
    public static Vector2 GravityForceAtPoint(Vector3 worldPos, CelestialMass[] attractors, float objMass)
    {
        Vector2 totalForce = Vector2.zero;
        Vector2 totalForceNormalized= Vector2.zero;
        foreach (CelestialMass rbAttracting in attractors)
        {
            Vector3 direction = rbAttracting.transform.position - worldPos; //direction vector
            float distance = direction.magnitude; //distance

            float forceMagnitude = G * (rbAttracting.mass * objMass) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
            Vector3 force = direction.normalized * forceMagnitude ; //storing force
            totalForce += new Vector2(force.x, force.y);
            totalForceNormalized += new Vector2(force.x, force.y).normalized;
        }
            return totalForce;

    }
    public static Vector2 CalculateDisplacement(Vector2 vertexPosition, CelestialMass[] attractors, float max, float divisor)
    {
        float deltaZ = 0;
        foreach (CelestialMass attractor in attractors)
        {
            Vector2 direction = new Vector2(attractor.transform.position.x, attractor.transform.position.y) - vertexPosition; //direction vector
            float distance = direction.magnitude; //distance

            deltaZ = G * (attractor.mass * 1) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
        }
        divisor = divisor == 0 ? 1 : divisor;
        deltaZ = deltaZ == 0 ? 1 : deltaZ;
        float deltaX = vertexPosition.x / deltaZ / divisor;
        float deltaY = vertexPosition.y / deltaZ / divisor;

        return Vector2.ClampMagnitude(new Vector2(deltaX, deltaY), max*5);
    }
}
