using System;

[Serializable]
public class CurriculumDescription
{
    public string Lesson;
    public string Description;
}

public class CurriculumDescriptions
{
    public CurriculumDescription[] LevelDescriptions;
}