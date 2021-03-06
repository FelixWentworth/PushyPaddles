﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class CollectibleGeneration : LevelLayout
{

    public GameObject CollectibleGameObject;
    public GameObject Sign;
	[SyncVar] public bool SignActive;

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
		Debug.Log("Generating Level: " + curriculumInfo.RequiredOperations[0]);
        CreateLevel(curriculumInfo.RequiredOperations, "+");
        CreateLevel(curriculumInfo.ExtraOperations, "c");

        // Generate the collectibles based on the level array
        GenerateCollectibles(CollectibleGameObject);
        GenerateObstacles();

	    SignActive = Convert.ToInt16(curriculumInfo.Level) > 3;

	    Sign.SetActive(SignActive);
	    if (SP_Manager.Instance.IsSinglePlayer())
	    {
		    ClientSetSignActive(SignActive);
	    }
		else
	    {
		    Debug.Log("Setting sign " + SignActive);

		    RpcSetSign(SignActive);
	    }

		Debug.Log("Setting Target");
		GameObject.Find("LevelManager").GetComponent<LevelManager>().Target = curriculumInfo.Target.ToString();
    }

    [ServerAccess]
    public void Reset(int numCollectibles, CurriculumChallenge curriculumInfo)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        // Clear the current level
        ClearChildren();

        CreateLevel(curriculumInfo.RequiredOperations, "+");
        CreateLevel(curriculumInfo.ExtraOperations, "c");

        // Generate the collectibles based on the level array
        GenerateCollectibles(CollectibleGameObject);
        GenerateObstacles();
		var signActive = Convert.ToInt16(curriculumInfo.Level) > 3;

	    Sign.SetActive(signActive);
	    if (SP_Manager.Instance.IsSinglePlayer())
	    {
		    ClientSetSignActive(signActive);
	    }
	    else
	    {
		    RpcSetSign(signActive);
	    }
		GameObject.Find("LevelManager").GetComponent<LevelManager>().Target = curriculumInfo.Target.ToString();
    }

    public override void NewSetup<T>(int numCollectibles, T info)
    {
        base.NewSetup<T>(numCollectibles, info);
        if (info.GetType() != typeof(CurriculumChallenge))
        {
            return;
        }
        var curriculumInfo = (CurriculumChallenge) Convert.ChangeType(info, typeof(CurriculumChallenge));

        // Clear the current level
        ClearChildren();

        CreateLevel(curriculumInfo.RequiredOperations, "+");
        CreateLevel(curriculumInfo.ExtraOperations, "c");

        GenerateCollectibles(CollectibleGameObject);
        GenerateObstacles();

	    var signActive = Convert.ToInt16(curriculumInfo.Level) > 3;

		Sign.SetActive(signActive);
		if (SP_Manager.Instance.IsSinglePlayer())
	    {
		    ClientSetSignActive(signActive);
	    }
	    else
	    {
		    RpcSetSign(signActive);
	    }
		GameObject.Find("LevelManager").GetComponent<LevelManager>().Target = curriculumInfo.Target.ToString();
    }

    [ServerAccess]
    public void ResetColliders()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        //var colliders = ObstacleParent.GetComponentsInChildren<Collider>();
        //foreach (var collider1 in colliders)
        //{
        //    collider1.enabled = true;
        //    var mathsCollectible = collider1.GetComponent<MathsCollectible>();
        //    if (mathsCollectible != null)
        //    {
        //        mathsCollectible.RpcReset();
        //    }
        //}
        //RpcResetColliders();

        GameObject.Find("GameManager").GetComponent<GameManager>().ResetRound();

    }

	[ClientRpc]
	private void RpcSetSign(bool active)
	{
		ClientSetSignActive(active);
	}

	[ClientAccess]
	private void ClientSetSignActive(bool active)
	{
		var method = MethodBase.GetCurrentMethod();
		var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
		if (!attr.HasAccess)
		{
			return;
		}
		SignActive = active;

		Sign.SetActive(active);
	}

	[ClientRpc]
    private void RpcResetColliders()
    {
        ClientResetColliders();
    }

    [ClientAccess]
    private void ClientResetColliders()
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ClientAccess)method.GetCustomAttributes(typeof(ClientAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }

        var colliders = ObstacleParent.GetComponentsInChildren<Collider>();
        foreach (var collider1 in colliders)
        {
            collider1.enabled = true;
        }
    }

    [ServerAccess]
    public void CreateLevel(string[] challengeInfo, string validPosition)
    {
        var method = MethodBase.GetCurrentMethod();
        var attr = (ServerAccess)method.GetCustomAttributes(typeof(ServerAccess), true)[0];
        if (!attr.HasAccess)
        {
            return;
        }
        var depth = GeneratedLevelLayout.GetLength(1);
        var startDepth = Random.Range(0, 2);
        // Make sure to remove the empty values in the array
        challengeInfo = challengeInfo.Where(x => !string.IsNullOrEmpty(x)).ToArray();

        for (var i = 0; i < challengeInfo.Length; i++)
        {
            // Decide the location
            var x = 0;
            var z = startDepth;
            
            // check if start depth has valid positions
            while (!CheckDepthValid(z, validPosition))
            {
                // if no valid position, move forward the start depth
                z += 1; 
            }    

            do
            {
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

    private bool CheckDepthValid(int depth, string validPosition)
    {
        for (var i = 0; i < GeneratedLevelLayout.GetLength(0); i++)
        {
            if (GeneratedLevelLayout[i, depth] == validPosition)
            {
                return true;
            }
        }
        return false;
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
