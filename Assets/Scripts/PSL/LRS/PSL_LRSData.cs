using Newtonsoft.Json;
using UnityEngine;

public class PSL_LRSData
{
    private string _atosStoreAPI = "https://sta-psl.atosresearch.eu/store/api/getLRS/";
    private string _matchId = "";

    private string _atosStoreURL
    {
        get { return _atosStoreAPI + _matchId; }
    }

    private string _api = "http://{0}/data/xAPI/";
    private string _postExtension = "";

    private string _verbUrl = "http://prosociallearn.eu/plsxapi/verbs/";
    private string _gameId = "";

    public PSL_LRSFormat Data;

    public PSL_LRSData GetData(string playerId, string gameSituationId, string verb, float value)
    {
        var data = new PSL_LRSFormat();

        data.Actor.ObjectType = "Agent";
        data.Actor.Account.Name = playerId;
        data.Verb.Id = _verbUrl + verb;

        data.Result.Score.Raw = value;

        data.Context.ContextActivities.Parent = new Object[1];
        data.Context.ContextActivities.Parent[0].Id = _gameId;
        data.Context.ContextActivities.Parent[0].ObjectType = "Activity";

        data.Object.Id = gameSituationId;
        data.Object.ObjectType = "Activity";

        var dataObject = new PSL_LRSData()
        {
            Data = data
        };

        return dataObject;
    }

    public void SendData(PSL_LRSData data)
    {
        var requestApi = _api + _postExtension;
        var body = JsonConvert.SerializeObject(data.Data);
        
        var www = new WWW(requestApi);
        
        // TODO send data
    }

}
