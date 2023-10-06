using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public string playerPrefabLocation;
    public PlayerController[] players;
    public Transform[] spawnPoints;
    public int alivePlayers;

    private int playersInGame;
    public float postGameTime;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start ()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }


    public PlayerController GetPlayer (int playerId)
    {
        foreach(PlayerController player in players)
        {
            if (player != null && player.id == playerId)
                return player;
        }
        return null;
    }

    public PlayerController GetPlayer (GameObject playerObj)
    {
        foreach(PlayerController player in players)
        {
            if (player != null && player.gameObject == playerObj)
                return player;
        }
        return null;
    }
    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
            photonView.RPC("SpawnPlayer", RpcTarget.All);
    }

    [PunRPC]
    void SpawnPlayer()
    {
        GameObject playerobj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0,spawnPoints.Length)].position,Quaternion.identity);
        playerobj.GetComponent<PlayerController>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }
    public void CheckWinCondition()
    {
        if (alivePlayers == 1)
            photonView.RPC("WinGame", RpcTarget.All, players.First(x => !x.dead).id);
    }

    [PunRPC]
    void WinGame(int winningPlayer)
    {
        Invoke("GoBackToMenu", postGameTime);
        GameUI.instance.SetWinText(GetPlayer(winningPlayer).photonPlayer.NickName);
    }

    void GoBackToMenu ()
    {
        NetworkManager.instance.ChangeScene("Menu");
    }
}
