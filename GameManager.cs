using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    private int i;
    public Button exitBtn;

    void Awake()
    {
        CreatePlayer();
        exitBtn.onClick.AddListener(() => OnExitClick());
    }

    void CreatePlayer()
    {
        i = 0;
        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{player.Value.NickName}, {player.Value.ActorNumber}");
            i++;
        }

        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        Debug.Log($"i : {i}");
        if(i == 1)
            PhotonNetwork.Instantiate("Player1", points[i].position, points[i].rotation, 0);
        else if(i == 2)
        {
            PhotonNetwork.Instantiate("Player2", points[i].position, points[i].rotation, 0);
        }
        else
            Debug.LogError("Error!");
    }


    void Update()
    {
        
    }


    private void OnExitClick()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("StartUI");
    }
}