﻿using System;
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
            var mathsCollectible = collider1.GetComponent<MathsCollectible>();
            if (mathsCollectible != null)
            {
                mathsCollectible.RpcReset();
            }
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
        var startDepth = Random.Range(0, 2);

        // Make sure to remove the empty values in the array
        challengeInfo = challengeInfo.Where(x => !string.IsNullOrEmpty(x)).ToArray();

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
            challengeInfo[i] = CheckIfOperandRequired("+", challengeInfo[i]);

            // Mark the position as used
            GeneratedLevelLayout[x, z] = challengeInfo[i];

            // Avoid division by 0 TODO handle in for loop properly
            if (i + 1 < challengeInfo.Length)
            {
                var max = ((depth - z) / (challengeInfo.Length - (i + 1)));
                startDepth += Random.Range(1, max);
            }
        }

    }

    private string CheckIfOperandRequired(string operand, string text)
    {
        var operationsToCheck = new char[] { '+', '-', '/', 'x'};
        if (HasOperation(text, operationsToCheck))
        {
            return text;
        }
        else
        {
            return operand + text;
        }
    }

    private bool HasOperation(string text, char[] operations)
    {
        foreach (var operation in operations)
        {
            if (text.Contains(operation))
            {
                return true;
            }
        }
        return false;
    }

    

}
