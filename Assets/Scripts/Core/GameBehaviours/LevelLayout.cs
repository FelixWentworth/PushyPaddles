using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class LevelLayout : NetworkBehaviour {

    public GameObject ObstacleParent;

    public GameObject _spawnArea;

    public GameObject[] RockGameObjects;

    protected string[,] GeneratedLevelLayout;

    protected string[,] EmptyLevelLayout;

    // Our boundaries, set using the GameObject positions in scene
    protected float MinX;
    protected float MinZ;
    protected float MaxX;
    protected float MaxZ;

    public virtual void Setup<T>(int num, T info)
    {
        _spawnArea = transform.Find("SpawnArea").gameObject;

        MinX = _spawnArea.transform.Find("BottomLeft").position.x;
        MinZ = _spawnArea.transform.Find("BottomLeft").position.z;
        MaxX = _spawnArea.transform.Find("TopRight").position.x;
        MaxZ = _spawnArea.transform.Find("TopRight").position.z;

        SetLevelLayout();
    }

    public virtual void NewSetup<T>(int num, T info)
    {
        GeneratePath();
    }

    private void SetLevelLayout()
    {
        // Get the width available
        // Each block is 1x1x1
        var blockWidth = 1;

        var width = MaxX - MinX;
        var height = MaxZ - MinZ;

        // Get the intervals
        var widthSpace = Mathf.RoundToInt(width / blockWidth);
        // Add 1 to width for block at 0 pos
        widthSpace += 1;

        var heightSpece = Mathf.RoundToInt(height / blockWidth);
        if (GeneratedLevelLayout == null)
        {
            GeneratedLevelLayout = new string[widthSpace, heightSpece];

            ResetLevelArray(widthSpace, heightSpece);
        }

        GeneratePath();
    }

    public void ResetLevelArray(int width, int height)
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                // Set char to clear
                GeneratedLevelLayout[i, j] = "c";
            }
        }
    }

    /// <summary>
    /// Generate a path that the players can follow from start to finish
    /// </summary>
    public void GeneratePath()
    {
        ResetLevelArray(GeneratedLevelLayout.GetLength(0), GeneratedLevelLayout.GetLength(1));

        // Set the start and end positions
        var width = GeneratedLevelLayout.GetLength(0);
        var midpoint = (width / 2);

        // Set midpoint of first row as start point
        GeneratedLevelLayout[midpoint, 0] = "+";
        GeneratedLevelLayout[midpoint - 1, 0] = "+";

        // Set midpoint of the last row as end point
        GeneratedLevelLayout[midpoint, GeneratedLevelLayout.GetLength(1) - 1] = "s";
        GeneratedLevelLayout[midpoint - 1, GeneratedLevelLayout.GetLength(1) - 1] = "s";


        var previousPosition = midpoint;

        var centerX = GeneratedLevelLayout.GetLength(0) / 2;
        var centerZ = GeneratedLevelLayout.GetLength(1) / 2;

        GeneratedLevelLayout[centerX + 1, centerZ] = "s";
        GeneratedLevelLayout[centerX - 2, centerZ] = "s";
        GeneratedLevelLayout[centerX + 1, centerZ - 1] = "s";
        GeneratedLevelLayout[centerX - 2, centerZ - 1] = "s";

        // Iterate through the array to create a path
        for (var j = 1; j < GeneratedLevelLayout.GetLength(1); j++)
        {
            var pathPosition = previousPosition + Random.Range(-1, 2);

            if (pathPosition < 0)
            {
                pathPosition = 0;
            }
            if (pathPosition >= width)
            {
                pathPosition = width - 1;
            }
            // allow for some space to move from the previous position to the new path position
            GeneratedLevelLayout[previousPosition, j] = "+";
            GeneratedLevelLayout[pathPosition, j] = "+";

            //// HACK allow game be completed solo
            //GeneratedLevelLayout[midpoint, j] = '+';
            //GeneratedLevelLayout[midpoint - 1, j] = '+';
            //// END HACK

            previousPosition = pathPosition;
        }


        // Block the path so players must work together s = sign location
        GeneratedLevelLayout[centerX, centerZ] = "s";
        GeneratedLevelLayout[centerX - 1, centerZ] = "s";

        // Make sure that their is still a path around the blocked area
        //bool right = GeneratedLevelLayout[centerX, centerZ - 2] == "s" ||
        //             GeneratedLevelLayout[centerX + 1, centerZ - 2] == "s";

        GeneratedLevelLayout[centerX, centerZ - 1] = "s";
        GeneratedLevelLayout[centerX - 1, centerZ - 1] = "s";
        GeneratedLevelLayout[centerX, centerZ + 1] = "s";
        GeneratedLevelLayout[centerX - 1, centerZ + 1] = "s";
        GeneratedLevelLayout[centerX - 2, centerZ + 1] = "s";
        GeneratedLevelLayout[centerX + 1, centerZ + 1] = "s";

        //// Left/Right
        //if (right)
        //{
        //    GeneratedLevelLayout[centerX + 1, centerZ] = "+";

        //}
        //else
        //{
        //    GeneratedLevelLayout[centerX - 2, centerZ] = "+";
        //}

        //// Up/Down
        //if (right)
        //{
        //    GeneratedLevelLayout[centerX, centerZ - 1] = "+";
        //}
        //else
        //{
        //    GeneratedLevelLayout[centerX - 1, centerZ - 1] = "+";
        //}

        //// Diagonals
        //if (right)
        //{
        //    GeneratedLevelLayout[centerX + 1, centerZ + 1] = "s";
        //    GeneratedLevelLayout[centerX + 1, centerZ - 1] = "s";
        //}
        //else
        //{
        //    GeneratedLevelLayout[centerX - 2, centerZ + 1] = "s";
        //    GeneratedLevelLayout[centerX - 2, centerZ - 1] = "s";
        //}




    }

    public string GetArrayString()
    {
        var arrayString = "";

        for (var i = 0; i < GeneratedLevelLayout.GetLength(1); i++)
        {
            var rowString = "{";
            for (var j = 0; j < GeneratedLevelLayout.GetLength(0); j++)
            {
                rowString += " " + GeneratedLevelLayout[j, i] + ",";
            }   

            rowString = rowString.Substring(0, rowString.Length - 1) + " }";
            arrayString += "\n" + rowString;
        }

        return arrayString;
    }

    

    public int GetAvailableSpaces()
    {
        var available = 0;
        foreach (var c in GeneratedLevelLayout)
        {
            if (c == "c")
            {
                // Clear position
                available++;
            }
        }

        return available;
    }

   

    [ServerAccess]
    public void ClearChildren()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        var children = ObstacleParent.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.GetComponent<NetworkIdentity>() == null)
                continue;

            if (child != ObstacleParent.transform)
            {
                NetworkServer.UnSpawn(child.gameObject);
                Destroy(child.gameObject);
            }
        }
    }

    [ServerAccess]
    public void GenerateObstacles()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        for (var i = 0; i < GeneratedLevelLayout.GetLength(0); i++)
        {
            for (var j = 0; j < GeneratedLevelLayout.GetLength(1); j++)
            {
                if (GeneratedLevelLayout[i, j] == "x")
                {
                    // Create an object
                    var location = new Vector3(MinX + i, -0.5f, MinZ + j);
                    CreateObstacle(location, ObstacleParent.transform);
                }
            }
        }
    }

    [ServerAccess]
    private void CreateObstacle(Vector3 position, Transform parent)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        var rockNumber = Random.Range(0, RockGameObjects.Length);
        var yRotation = Random.Range(0, 4) * 90f;

        var go = Instantiate(RockGameObjects[rockNumber], position, Quaternion.identity);

        go.transform.SetParent(parent, false);
        go.transform.eulerAngles = new Vector3(0f, yRotation, 0f);


        NetworkServer.Spawn(go);
    }

    [ServerAccess]
    public void GenerateCollectibles(GameObject collectible)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        for (var i = 0; i < GeneratedLevelLayout.GetLength(0); i++)
        {
            for (var j = 0; j < GeneratedLevelLayout.GetLength(1); j++)
            {
                // Check that the space has extra content that should be shown for the collectible
                if (GeneratedLevelLayout[i, j] != "c" && GeneratedLevelLayout[i,j] != "x" && GeneratedLevelLayout[i,j] != "+" && GeneratedLevelLayout[i, j] != "s")
                {
                    // Create an object
                    var location = new Vector3(MinX + i, -0.5f, MinZ + j);
                    CreateCollectible(collectible, location, ObstacleParent.transform, GeneratedLevelLayout[i, j]);
                }
            }
        }
    }

    [ServerAccess]
    private void CreateCollectible(GameObject gameObject, Vector3 position, Transform parent, string info)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        var go = Instantiate(gameObject, position, Quaternion.identity);

        go.transform.SetParent(parent, false);
        go.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        go.GetComponent<MathsCollectible>().Set(info);

        NetworkServer.Spawn(go);
    }


    public void GenerateNewLevel(int obstacles)
    {
        //StartCoroutine(ChangeBlocks(obstacles, 0.8f));
        ChangeBlocksImmediate(obstacles);
    }

    public void ChangeBlocksImmediate(int obstacles)
    {
        NewSetup(obstacles, "");
    }

    public IEnumerator ChangeBlocks(int obstacles, float time)
    {
        var initialPos = transform.position;
        var hiddenPos = new Vector3(transform.position.x, transform.position.y - 2f, transform.position.z);

        yield return Move(initialPos, hiddenPos, time);

        NewSetup(obstacles, "");

        yield return Move(hiddenPos, initialPos, time);


    }

    public IEnumerator Move(Vector3 from, Vector3 to, float time)
    {
        var elapsed = 0f;

        while (elapsed < time)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
    }
}
