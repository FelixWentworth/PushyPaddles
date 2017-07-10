using System;

[Serializable]
public class CurriculumChallenge
{
    public int Level;
    public double Target;

    public string[] RequiredOperations;
    public string[] ExtraOperations;

}

public class CurriculumChallenges
{
    public CurriculumChallenge[] Items;
}

