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

        CreateLevel(curriculumInfo.RequiredOperations, '+');
        CreateLevel(curriculumInfo.ExtraOperations, 'c');

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

        CreateLevel(curriculumInfo.RequiredOperations, '+');
        CreateLevel(curriculumInfo.ExtraOperations, 'c');

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
    }

    [Server]
    public void CreateLevel(string[] challengeInfo, char validPosition)
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
            GeneratedLevelLayout[x, z] = 'x';

            var xPos = MinX + x;
            var zPos = MinZ + z;

            var location = new Vector3(xPos, -0.5f, zPos);
            // Create the object
            CreateCollectible(location, ObstacleParent.transform, challengeInfo[i]);

            startDepth += (depth - z) / (challengeInfo.Length-i);
        }
    }

    [Server]
    private void CreateCollectible(Vector3 position, Transform parent, string info)
    {
        var go = Instantiate(CollectibleGameObject, position, Quaternion.identity);

        go.transform.SetParent(parent, false);
        go.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        go.GetComponent<MathsCollectible>().Set(info);

        NetworkServer.Spawn(go);
    }

}
