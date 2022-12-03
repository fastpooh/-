using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameCtrl : MonoBehaviourPunCallbacks
{
    public GameObject textteam1wins;
    public GameObject textteam2wins;
    public GameObject ExitButton;

    private int numOfPlayers;
    private bool gameEnds;

    private int player1_righthand;
    private int player1_lefthand;
    private int player2_righthand;
    private int player2_lefthand;


    void Start()
    {
        gameEnds = false;
        textteam1wins.SetActive(false);
        textteam2wins.SetActive(false);
        ExitButton.SetActive(false);
        
        player1_lefthand = 1;
        player1_righthand = 1;
        player2_lefthand = 1;
        player2_righthand = 1;
    }

    void Update()
    {
        if(numOfPlayers == 2)
        {
            if(player1_lefthand == 0 && player1_righthand == 0)
            {
                if(gameEnds == false)
                {
                    textteam1wins.SetActive(true);
                    ExitButton.SetActive(true);
                }
                gameEnds = true;
            }
            else if(player2_lefthand == 0 && player2_righthand == 0)
            {
                if(gameEnds == false)
                {
                    textteam2wins.SetActive(true);
                    ExitButton.SetActive(true);
                }
                gameEnds = true;
            }
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        if(coll.tag == "Player1")
        {
            numOfPlayers++;
        }
        
        if(coll.tag == "Player2")
        {
            numOfPlayers++;
        }
    }

    void OnTriggerStay(Collider coll)
    {
        if(coll.tag == "Player1")
        {
            player1_lefthand = coll.GetComponent<PlayerCtrl>().L_Finger_Number;
            player1_righthand = coll.GetComponent<PlayerCtrl>().R_Finger_Number;
        }

        if(coll.tag == "Player2")
        {
            player2_lefthand = coll.GetComponent<PlayerCtrl>().L_Finger_Number;
            player2_righthand = coll.GetComponent<PlayerCtrl>().R_Finger_Number;
        }
    }
}
