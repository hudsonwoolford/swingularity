using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;  
using UnityEngine.Rendering;

public class OctahedronSphereCreator : MonoBehaviour {

    private static Vector3[] directions = {
        Vector3.left,
        Vector3.back,
        Vector3.right,
        Vector3.forward
    };

    public static Mesh Create (TerrainVar terrainVar, ComputeShader calculateVertexHeight)
    {
        //bit shift i.e. 2^n to calculate resolution (i.e. number of rows based on subdivision)
        int resolution = 1 << terrainVar.generalSettings.subdivisions;

        /* Calculate in quadrants - one quadrant has 4 verts, then 9, then 25 based on subdivs -> (resolution + 1)^2
         * multiply by 4 for each quadrant of octahedron
         Subtract the 3(2r - 1) vert overlaps resulting in 4(r + 1)^2 - 3(2r - 1) formula for number of verts */

        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1) * 4 - (resolution * 2 - 1) * 3];

        int[] triangles = new int[(1 << (terrainVar.generalSettings.subdivisions * 2 + 3)) * 3];
        CreateOctahedron(vertices, triangles, resolution);

        Spherize(vertices);

        //Create a mesh to be used
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.name = "Planet";

        //Set all mesh parameters so unity can visualise them
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //Send data to computation script
        mesh = PlanetaryGenerator.ModifyGeometry(mesh, terrainVar, calculateVertexHeight);
        mesh.RecalculateNormals();
        return mesh;
    }

#region SphereGeneration
    private static void Spherize(Vector3[] vertices)
    {
        //Normalize all vertices - making them a magnitude of 1 i.e all equidistant and hence a circle
        for (int i = 0; i < vertices.Length;i++)
        {
            vertices[i] = vertices[i].normalized;
        }
    }

    private static void CreateOctahedron(Vector3[] vertices, int[] triangles, int resolution)
    {
        //init vertex iterator, first vertex index of previous row, triangle iterator
        int v = 0, vBottom = 0, t = 0;

        //create the bottom vertices
        for (int i = 0; i < 4; i++)
        {
            vertices[v++] = Vector3.down;
        }

        //Circling round each triangle row for all 4 quadrants of southern hemisphere
        for (int i = 1; i <= resolution; i++)
        {
            float progress = (float) i / resolution;
            Vector3 from, to;
            vertices[v++] = to = Vector3.Lerp(Vector3.down, Vector3.forward, progress);
            //For all quadrants
            for (int d = 0; d < 4; d++)
            {
                // move up triangle faces iteratively, drawing a line of all the verts along it
                from = to;
                to = Vector3.Lerp(Vector3.down, directions[d], progress);
                t = CreateLowerStrip(i, v, vBottom, t, triangles);
                v = CreateVertexLine(from, to, i, v, vertices);
                //first row (i.e. bottom point) is special case because verts are duplicated
                vBottom += i > 1 ? (i - 1) : 1;
            }
            //reset first vert of previous row to the beginning of the first quadrant's row
            vBottom = v - 1 - i * 4;
        }
        //top hemisphere quadrants
        for (int i = resolution - 1; i >= 1; i--)
        {
            float progress = (float)i / resolution;
            Vector3 from, to;
            vertices[v++] = to = Vector3.Lerp(Vector3.up, Vector3.forward, progress);
            //For all quadrants
            for (int d = 0; d < 4; d++)
            {
                from = to;
                to = Vector3.Lerp(Vector3.up, directions[d], progress);
                t = CreateUpperStrip(i, v, vBottom, t, triangles);
                v = CreateVertexLine(from, to, i, v, vertices);
                //top 4 verts are special, not bottom, so no need to worry about vBottom being out of bounds
                vBottom += i + 1;
            }
            vBottom = v - 1 - i * 4;
        }
        //final four triangles in for the top part 
        for (int i = 0; i < 4; i++)
        {
            triangles[t++] = vBottom;
            triangles[t++] = v;
            triangles[t++] = ++vBottom;
            vertices[v++] = Vector3.up;
        }
    }

    private static int CreateVertexLine(Vector3 from, Vector3 to, int steps, int v, Vector3[] vertices)
    {
        //Create a line of vertices between the two points ("from" and "to" ) which sit at some y coordinate on two 90 degree separated edges of a simple octahedron

        //Iterate for however long the row needs to be
        for (int i = 1; i <= steps; i++)
        {
            vertices[v++] = Vector3.Lerp(from, to, (float)i / steps);
        }
        return v;
    }

    private static int CreateLowerStrip(int steps, int vTop, int vBottom, int t, int[] triangles)
    {
        for (int i = 1; i < steps; i++)
        {
            //Shift along the two rows in question, drawing triangles between the points in question
            triangles[t++] = vBottom;
            triangles[t++] = vTop - 1;
            triangles[t++] = vTop;

            triangles[t++] = vBottom++;
            triangles[t++] = vTop++;
            triangles[t++] = vBottom;
        }

        triangles[t++] = vBottom;
        triangles[t++] = vTop - 1;
        triangles[t++] = vTop;
        return t;
    }

    private static int CreateUpperStrip(int steps, int vTop, int vBottom, int t, int[] triangles)
    {
        triangles[t++] = vBottom;
        triangles[t++] = vTop - 1;
        triangles[t++] = ++vBottom;
        for (int i = 1; i <= steps; i++)
        {
            //Shift along the two rows in question, drawing triangles between the points in question
            triangles[t++] = vTop - 1;
            triangles[t++] = vTop;
            triangles[t++] = vBottom;

            triangles[t++] = vBottom;
            triangles[t++] = vTop++;
            triangles[t++] = ++vBottom;
        }
        return t;
    }

    #endregion
}

