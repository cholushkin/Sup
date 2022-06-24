using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public class SimpleScoresDB : MonoBehaviour
{
    public class EventRequestSent
    {
        public State ForState;
    }

    public class EventResponseReceived
    {
        public Response Response;
    }

    public class ScoreTable
    {
        public class TableRow
        {
            public TableRow()
            {
                Name = "";
                Score = 0;
            }
            public string Name;
            public int Score;
        }

        public TableRow[] Rows;
    }

    public enum State
    {
        UploadScore,
        DownloadScore,
        Idle
    }

    private const int RecordsNumber = 10;
    private bool _useUniqueNameHack = true;

    public class Response
    {
        public Response(State state, UnityWebRequest.Result result, string text)
        {
            RequestResult = result;
            State = state;
            Text = text;
        }

        public override string ToString()
        {
            return $"{RequestResult} {Text}";
        }

        public UnityWebRequest.Result RequestResult;
        public State State;
        public string Text;
    }

    const string privateCode = "zpa15aXTrUiEgViQKxEezQ72AeLWfqkk-sT7fAY9YHmA";
    const string publicCode = "62b36ed28f40bb37dc2aff01";
    const string webURL = "http://dreamlo.com/lb/";
    private Response _lastResponse;

    private State _curState;

    void Awake()
    {
        _curState = State.Idle;
    }

    public void UploadScore(string playerName, int score)
    {
        Assert.IsTrue(_curState == State.Idle);
        var uniqueString =
            $"{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}_{DateTime.Now.Millisecond}";
        if (_curState != State.Idle)
            return;
        _curState = State.UploadScore;
        var url = _useUniqueNameHack ? $"{webURL}{privateCode}/add/{UnityWebRequest.EscapeURL(CleanString(playerName))}_{uniqueString}/{score}" :
            $"{webURL}{privateCode}/add/{UnityWebRequest.EscapeURL(CleanString(playerName))}/{score}";
        StartCoroutine(DoGetRequest(url));
    }

    public void DownloadScores()
    {
        Assert.IsTrue(_curState == State.Idle);
        if (_curState != State.Idle)
            return;
        _curState = State.DownloadScore;
        var url = $"{webURL}{publicCode}/pipe";
        StartCoroutine(DoGetRequest(url));
    }


    public static string CleanString(string s)
    {
        s = s.Replace("/", "");
        s = s.Replace("|", "");
        return s;
    }

    IEnumerator DoGetRequest(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = 6;
            Debug.Log($"Send {url}");
            GlobalEventAggregator.EventAggregator.Publish(new EventRequestSent { ForState = _curState });
            yield return www.SendWebRequest();

            _lastResponse = new Response(_curState, www.result, www.downloadHandler.text);
            Debug.Log($"Response {_lastResponse}");
            _curState = State.Idle;
            GlobalEventAggregator.EventAggregator.Publish(new EventResponseReceived { Response = _lastResponse});
        }
        
        
    }

    // note: Always return RecordsNumber items in a table
    public static ScoreTable ConvertResponseToTable(Response response)
    {
        if (response.State != State.DownloadScore)
        {
            Debug.LogError("Wrong state!");
            return null;
        }

        var scoreTable = new ScoreTable();
        string[] entries = response.Text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        scoreTable.Rows = new ScoreTable.TableRow[RecordsNumber];

        for (int i = 0; i < RecordsNumber; i++) 
        {
            var row = new ScoreTable.TableRow();

            if (i < entries.Length)
            {
                string[] entryInfo = entries[i].Split(new[] { '|' });
                row.Name = entryInfo[0].Split("_")[0];
                row.Score = int.Parse(entryInfo[1]);
            }
            
            scoreTable.Rows[i] = row;
        }

        return scoreTable;
    }

    [ContextMenu("DbgUpload")]
    void DbgUpload()
    {
        UploadScore("aaa", 300);
    }

    [ContextMenu("DbgDownload")]
    void DbgDownload()
    {
        DownloadScores();
    }
}

