using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSLSkills : MonoBehaviour {

    public enum Verb
    {
        IntroducedSelf,
        WasAssertive,
        SaidNo,
        AcceptedNo,
        IdentifiedSocialCues,
        ConcernedForFeelingsProtocol,
        ConcernedForFeelingsResponse,
        DealtWithFeelings,
        Helped,
        SolvedAsGroup,
        Shared,
        Cooperated,
        TootTurns,
        SetGoal,
        MetGoal,
        StayedOnTask
    }

    public void Send(string playerId, Verb verb)
    {
        
    }

}
