using System;

[Serializable]
public class CurriculumChallenge
{
    public string Year;
    public string Lesson;
    public string Level;
    public double Target;

    public string[] RequiredOperations;
    public string[] ExtraOperations;

}

public class CurriculumChallenges
{
    public CurriculumChallenge[] MathsProblems;
}

