using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class CollectibleGeneration : LevelLayout
{

    public GameObject CollectibleGameObject;

    public override void Setup<T>(int numCollectibles, T info)
    {
        base.Setup<T>(numCollectibles, info);

        if (info.GetType() != typeof(CurriculumChallenge))
        {
            return;
        }

        var curriculumInfo = (CurriculumChallenge)Convert.ChangeType(info, typeof(CurriculumChallenge));

        // Clear the current level
        ClearChildren();

        CreateLevel(curriculumInfo.RequiredOperations, "+");
        CreateLevel(curriculumInfo.ExtraOperations, "c");

        // Generate the collectibles based on the level array
        GenerateCollectibles(CollectibleGameObject);
        GenerateObstacles();

        GameObject.Find("LevelManager").GetComponent<LevelManager>().Target = curriculumInfo.Target.ToString();

    }

    public override void NewSetup<T>(int numCollectibles, T info)
    {
        base.Setup<T>(numCollectibles, info);

        if (info.GetType() != typeof(CurriculumChallenge))
        {
            return;
        }
        var curriculumInfo = (CurriculumChallenge) Convert.ChangeType(info, typeof(CurriculumChallenge));

        // Clear the current level
        ClearChildren();

        CreateLevel(curriculumInfo.RequiredOperations, "+");
        CreateLevel(curriculumInfo.ExtraOperations, "c");

        GameObject.Find("LevelManager").GetComponent<LevelManager>().Target = curriculumInfo.Target.ToString();
    }

    [Server]
    public void ResetColliders()
    {
        var colliders = ObstacleParent.GetComponentsInChildren<Collider>();
        foreach (var collider1 in colliders)
        {
            collider1.enabled = true;
        }
        RpcResetColliders();
    }

    [ClientRpc]
    private void RpcResetColliders()
    {
        var colliders = ObstacleParent.GetComponentsInChildren<Collider>();
        foreach (var collider1 in colliders)
        {
            collider1.enabled = true;
        }
    }

    [Server]
    public void CreateLevel(string[] challengeInfo, string validPosition)
    {
        var depth = GeneratedLevelLayout.GetLength(1);
        var startDepth = 0;

        for (var i = 0; i < challengeInfo.Length; i++)
        {
            // Decide the location
            var x = 0;
            var z = 0;
            do
            {
                z = startDepth;
                x = Random.Range(0, GeneratedLevelLayout.GetLength(0));

            } while (GeneratedLevelLayout[x, z] != validPosition);

            // Mark the position as used
            GeneratedLevelLayout[x, z] = challengeInfo[i];

            startDepth += (depth - z) / (challengeInfo.Length-i);
        }

    }

    

}
