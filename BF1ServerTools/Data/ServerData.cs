namespace BF1ServerTools.Data;

public class ServerData
{
    public string Name { get; set; }
    public long GameId { get; set; }
    public float Time { get; set; }

    public string GameMode { get; set; }
    public string MapName { get; set; }

    public int MaxScore { get; set; }

    public string Team1Name { get; set; }
    public string Team2Name { get; set; }

    public int Team1Score { get; set; }
    public int Team2Score { get; set; }

    public int Team1Kill { get; set; }
    public int Team2Kill { get; set; }

    public int Team1Flag { get; set; }
    public int Team2Flag { get; set; }

    public string Team1Img { get; set; }
    public string Team2Img { get; set; }

    public int Team1MaxPlayerCount { get; set; }
    public int Team1PlayerCount { get; set; }
    public int Team1Rank150PlayerCount { get; set; }
    public int Team1AllKillCount { get; set; }
    public int Team1AllDeadCount { get; set; }

    public int Team2MaxPlayerCount { get; set; }
    public int Team2PlayerCount { get; set; }
    public int Team2Rank150PlayerCount { get; set; }
    public int Team2AllKillCount { get; set; }
    public int Team2AllDeadCount { get; set; }
}
