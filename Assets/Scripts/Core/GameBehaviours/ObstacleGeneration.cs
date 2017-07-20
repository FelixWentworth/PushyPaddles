using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObstacleGeneration : LevelLayout
{

    public override void Setup<T>(int numObstacles, T info)
    {
        base.Setup(numObstacles, info);
        CreateLevel(numObstacles);
    }

    public override void NewSetup<T>(int numObstacles, T info)
    {
        base.Setup(numObstacles, info);
        CreateLevel(numObstacles);
    }

    [Server]
    public void CreateLevel(int numObstacles)
    {
        // Clear the current level
        ClearChildren();

        var availablePlaces = GetAvailableSpaces();
        if (numObstacles > availablePlaces)
        {
            numObstacles = availablePlaces;
        }

        for (var i = 0; i < numObstacles; i++)
        {

            // Decide the location
            var x = 0;
            var z = 0;
            do
            {
                x = Random.Range(0, GeneratedLevelLayout.GetLength(0));
                z = Random.Range(0, GeneratedLevelLayout.GetLength(1));

            } while (GeneratedLevelLayout[x, z] != "c");

            // Mark the position as used
            GeneratedLevelLayout[x, z] = "x";
        }
        GenerateObstacles();
    }
}
