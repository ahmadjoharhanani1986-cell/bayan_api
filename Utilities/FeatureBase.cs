using SHLAPI;
using SHLAPI.Database;

public class FeatureBase
{
    public int user_id { get; set; }
    public int lang_id { get; set; }
    public string token { get; set; }
    public NavigationTypes navigationType { get; set; }
}


public class FeatureHandlerBase
{
    protected IShamelDatabase _con;  protected IMasterDatabase _conMaster;
    public FeatureHandlerBase(IShamelDatabase con) => _con = con;
}