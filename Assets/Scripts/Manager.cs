using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Manager : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject buttonPanel;
    public GameObject confirmationPanel;
    public GameObject loadingScreen;

    [Header("Texts and IFs")]
    public TMP_InputField passCodeIF;
    public TMP_Text notificationText;


    [Header("Variables")]
    public int gameId = -1;
    public int leaderboardSize = 15;
    public float loadingTime = 5f;
    private const string API_URL = "https://leaderboard-backend.mern.singularitybd.net/api/v1/leaderboard?game=";
    private const string API_TOKEN = "9b1de5f407f1463e7b2a921bbce364";



    public IEnumerator SubmitScore(string playerName, int playerScore, int g_id)
    {
        string url = "https://leaderboard-backend.mern.singularitybd.net/api/v1/score";

        // Create a PlayerScore object
        PlayerScore postData = new PlayerScore(playerName, playerScore, g_id);

        // Serialize to JSON
        string jsonBody = JsonUtility.ToJson(postData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", API_TOKEN);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error submitting score: {request.error}");
            Debug.LogError($"Response: {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log("Score submitted successfully.");
            Debug.Log($"Response: {request.downloadHandler.text}");
            try
            {
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing response: {ex.Message}");
            }
            //Response: {"resState":201,"success":true,"message":"player inserted successfully","data":{"position":1,"id":"6785f039698f33be978b6622","name":"response","score":370}}
        }
    }



    public IEnumerator ClearLeaderboard(int g_id)
    {        
        UnityWebRequest request = UnityWebRequest.Delete(API_URL + g_id.ToString());
        request.SetRequestHeader("x-token", API_TOKEN);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error clearing leaderboard: {request.error}");
        }
        else
        {
            Debug.Log("Leaderboard cleared successfully.");
            notificationText.text = "Leaderboard resetting...";
        }
    }



    IEnumerator EktuTimeDen()
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(loadingTime);
        notificationText.text = "Leaderboard reset successfull";
        loadingScreen.SetActive(false);
        gameId = -1;
        passCodeIF.text = "";
        confirmationPanel.SetActive(false);
    }

    public void ResetCarGame()
    {
        gameId = 0;
        confirmationPanel.SetActive(true);
    }


    public void ResetTrivia()
    {
        gameId = 2;
        confirmationPanel.SetActive(true);
    }

    public void ResetARVenture()
    {
        gameId = 3;
        confirmationPanel.SetActive(true);
    }

    public void ConfirmButton()
    {
        if(passCodeIF.text == "pareht" && gameId > -1)
        {
            notificationText.text = "Leaderboard reset on progess";
            StartCoroutine(ClearLeaderboard(gameId));
            for (int i = 0; i < leaderboardSize + 2; i++)
            {
                StartCoroutine(SubmitScore(" ", -1, gameId));
            }
            StartCoroutine(EktuTimeDen());            
            
        }
        else
        {
            StartCoroutine(Notification());
        }
    }

    IEnumerator Notification()
    {
        notificationText.text = "Wrong Pass Code!";
        yield return new WaitForSeconds(1f);
        notificationText.text = "";
    }
}

[Serializable]
public class PlayerScore
{
    public string name;
    public int score;
    public int game;

    public PlayerScore(string playerName, int playerScore, int gID)
    {
        name = playerName;
        score = playerScore;
        game = gID;
    }
}
[Serializable]
public class ApiResponse
{
    public int resState;
    public bool success;
    public string message;
    public Data data;
}

[Serializable]
public class Data
{
    public int position;
    public string id;
    public string name;
    public int score;
    public int game;
}
