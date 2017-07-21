using System;

[Serializable]
public class CurriculumChallenge
{
    public int KeyStage;
    public int Lesson;
    public int Level;
    public double Target;

    public string[] RequiredOperations;
    public string[] ExtraOperations;

}

public class CurriculumChallenges
{
    public CurriculumChallenge[] MathsProblems;
}

