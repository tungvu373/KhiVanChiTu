using System;

[Serializable]
public class DaoAnLevelState
{
    public string id;
    public int level;
}

[Serializable]
public class DaoAnSaveState
{
    public DaoAnLevelState[] levels = Array.Empty<DaoAnLevelState>();
}
