public class PSL_LRSFormat
{
    public string Id;

    public Actor Actor;
    public Verb Verb;
    public Result Result;
    public Context Context;

    public string TimeStamp;
    public string Version;
    public Object Object;

}

public class Actor
{
    public string ObjectType;
    public Account Account;
}

public class Team
{
    public Actor[] Member;
    public string ObjectType;
}

public class Account
{
    public string HomePage;
    public string Name;
}

public class Verb
{
    public string Id;
}

public class Result
{
    public Score Score;
}

public class Score
{
    public float Raw;
}

public class Context
{
    public Team Team;
    public ContextActivities ContextActivities;
}

public class ContextActivities
{
    public Object[] Parent;
}

public class Object
{
    public string Id;
    public string ObjectType;
}