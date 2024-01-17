using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class Grid
{
    int width;
    int height;
    float cellSize;
    Vector3 origin;
    Rigidbody[] planets;
    Rigidbody ball;
    Vector2[,] gridArray;

    public Grid(int width, int height, float cellSize, float colorDivisor, Vector3 origin, Rigidbody[] planets, Rigidbody ball, GameObject arrow, Transform parent, Gradient colorGrad)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;
        this.planets = planets;
        this.ball = ball;

        gridArray = new Vector2[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                GameObject arrowInsta = Object.Instantiate(arrow, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, Quaternion.identity);
                arrowInsta.transform.localScale = Vector3.one * cellSize;
                arrowInsta.transform.SetParent(parent, true);
                Vector3 force = ArtificialGravity.GravityForceAtPoint(GetWorldPosition(x, y), planets, ball.mass);
                if(force.magnitude != 0)
                {
                    arrow.GetComponent<SpriteRenderer>().color = colorGrad.Evaluate(1-(1 / (force.magnitude / colorDivisor)));
                    
                }
                else
                {
                    GameObject.Destroy(arrowInsta);
                }
                float zRot = Mathf.Atan2(force.normalized.y, force.normalized.x) * Mathf.Rad2Deg;
                arrowInsta.transform.rotation = Quaternion.Euler(0, 0, zRot);
            }
        }
    }
    Vector3 GetWorldPosition(int x, int y)
    {
        return origin + new Vector3(x, y) * cellSize;
    }
}
public static class ArtificialGravity
{
    public static float G = 1f;
    public static float GravitationalCutoff = 15;

    public static Vector3 AttractForce(Rigidbody rbToAttract, Rigidbody rbAttracting) //brackeys gravity tutorial
    {
        Vector3 direction = rbAttracting.position - rbToAttract.position; //direction vector
        float distance = direction.magnitude; //distance

        float forceMagnitude = ArtificialGravity.G * (rbAttracting.mass * rbToAttract.mass) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
        Vector3 force = direction.normalized * forceMagnitude; //storing force

        if( force.magnitude > GravitationalCutoff)
        {
            return force; //returning force
        }
        else
        {
            return force.normalized;
        }
    }
    public static Vector2 GravityForceAtPoint(Vector3 worldPos, Rigidbody[] planetRB, float objMass)
    {
        Vector2 totalForce = Vector2.zero;
        Vector2 totalForceNormalized= Vector2.zero;
        foreach (Rigidbody rbAttracting in planetRB)
        {
            Vector3 direction = rbAttracting.position - worldPos; //direction vector
            float distance = direction.magnitude; //distance

            float forceMagnitude = G * (rbAttracting.mass * objMass) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
            Vector3 force = (direction.normalized * forceMagnitude).magnitude > GravitationalCutoff ? direction.normalized * forceMagnitude : direction.normalized; //storing force
            totalForce += new Vector2(force.x, force.y);
            totalForceNormalized += new Vector2(force.x, force.y).normalized;
        }
        if (totalForce.magnitude > GravitationalCutoff)
        {
            return totalForce;
        }
        else
        {
            return totalForceNormalized;
        }

    }
}
