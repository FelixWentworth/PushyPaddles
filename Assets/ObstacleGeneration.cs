using UnityEngine;
using UnityEngine.Networking;

public class ObstacleGeneration : NetworkBehaviour
{
    public GameObject[] RockGameObjects;

    public GameObject ObstacleParent;

    private GameObject _spawnArea;

    private char[,] _levelLayout;

    // Our boundaries, set using the GameObject positions in scene
    private float _minX;
    private float _minZ;
    private float _maxX;
    private float _maxZ;

    void FixedUpdate()
    {
        if (isServer && ObstacleParent.transform.childCount == 0)
        {
            Setup();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetLevelLayout();
            CreateLevel(15);
        }
    }
    public void Setup()
    {
        _spawnArea = transform.FindChild("SpawnArea").gameObject;

        _minX = _spawnArea.transform.FindChild("BottomLeft").position.x;
        _minZ = _spawnArea.transform.FindChild("BottomLeft").position.z;
        _maxX = _spawnArea.transform.FindChild("TopRight").position.x;
        _maxZ = _spawnArea.transform.FindChild("TopRight").position.z;

        SetLevelLayout();
        CreateLevel(15);
    }

    private void SetLevelLayout()
    {
        // Get the width available
        // Each block is 1x1x1
        var blockWidth = 1;

        var width = _maxX - _minX;
        var height = _maxZ - _minZ;

        // Get the intervals
        var widthSpace = Mathf.RoundToInt(width/blockWidth);
        // Add 1 to width for block at 0 pos
        widthSpace += 1;

        var heightSpece = Mathf.RoundToInt(height/blockWidth);

        _levelLayout = new char[widthSpace,heightSpece];

        for (var i = 0; i < widthSpace; i++)
        {
            for (var j = 0; j < heightSpece; j++)
            {
                // Set char to clear
                _levelLayout[i, j] = 'c';
            }
        }
        GeneratePath();
    }

    /// <summary>
    /// Generate a path that the players can follow from start to finish
    /// </summary>
    private void GeneratePath()
    {
        // Set the start and end positions
        var width = _levelLayout.GetLength(0);
        var midpoint = (width / 2);

        // Set midpoint of first row as start point
        _levelLayout[midpoint, 0] = '+';
        _levelLayout[midpoint-1, 0] = '+';

        // Set midpoint of the last row as end point
        _levelLayout[midpoint, _levelLayout.GetLength(1)-1] = '+';
        _levelLayout[midpoint - 1, _levelLayout.GetLength(1)-1] = '+';

        var previousPosition = midpoint;

        
        // Iterate through the array to create a path
        for (var j = 1; j < _levelLayout.GetLength(1); j++)
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
            _levelLayout[previousPosition, j] = '+';
            _levelLayout[pathPosition, j] = '+';

            //// HACK allow game be completed solo
            //_levelLayout[midpoint, j] = '+';
            //_levelLayout[midpoint - 1, j] = '+';
            //// END HACK

            previousPosition = pathPosition;
        }
    }

    private string GetArrayString()
    {
        var arrayString = "";

        for (var i = 0; i < _levelLayout.GetLength(1); i++)
        {
            var rowString = "{";
            for (var j = 0; j < _levelLayout.GetLength(0); j++)
            {
                rowString += " " + _levelLayout[j, i] + ",";
            }

            rowString = rowString.Substring(0, rowString.Length - 1) + " }";
            arrayString += "\n" + rowString;
        }

        return arrayString;
    }

    public void CreateLevel(int numObstacles)
    {
        // Clear the current level
        ClearChildren();

        for (var i = 0; i < numObstacles; i++)
        {
            // Decide the location
            var x = 0;
            var z = 0;
            do
            {
                x = Random.Range(0, _levelLayout.GetLength(0));
                z = Random.Range(0, _levelLayout.GetLength(1));
            } while (_levelLayout[x, z] != 'c');

            // Mark the position as used
            _levelLayout[x, z] = 'x';

            var xPos = _minX + x;
            var zPos = _minZ + z;

            var location = new Vector3(xPos, -0.5f, zPos);
            // Create the object
            CreateObstacle(location, ObstacleParent.transform);
        }
    }

    [Server]
    private void CreateObstacle(Vector3 position, Transform parent)
    {
        var rockNumber = Random.Range(0, RockGameObjects.Length);
        var yRotation = Random.Range(0, 4) * 90f;

        var go = Instantiate(RockGameObjects[rockNumber], position, Quaternion.identity);
        
        go.transform.SetParent(parent, false);
        go.transform.eulerAngles = new Vector3(0f, yRotation, 0f);

        NetworkServer.Spawn(go);
    }

    private void ClearChildren()
    {
        var children = ObstacleParent.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child != ObstacleParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
