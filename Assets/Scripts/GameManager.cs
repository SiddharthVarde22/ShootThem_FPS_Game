using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    bool gameOver;
    bool canSpawnEnemy;
    bool isConnected;

    [SerializeField]
    GameObject playerToSpawn;
    [SerializeField]
    GameObject enemyToSpawn;
    [SerializeField]
    List<Transform> spawnPoints;
    GameObject playerRefToGame;

    void Start()
    {
        gameOver = true;
        canSpawnEnemy = false;
        isConnected = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameOver)
            {
                if (isConnected)
                {
                    PhotonNetwork.Destroy(playerRefToGame);
                    PhotonNetwork.LeaveRoom();
                }
                gameOver = true;
                StopAllCoroutines();
            }
        }

        if(canSpawnEnemy && !gameOver)
        {
            canSpawnEnemy = false;
            StartCoroutine(EnemySpawningEnnumerator());
        }
    }

    public void ChangeGameOver()
    {
        gameOver = !gameOver;
        if(!gameOver)
        {
            if (!isConnected)
            {
                playerRefToGame = Instantiate(playerToSpawn);
                canSpawnEnemy = true;
            }
            else
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }
        else
        {
            StopAllCoroutines();
            if(isConnected)
            {
                PhotonNetwork.Destroy(playerRefToGame);
                PhotonNetwork.LeaveRoom();
            }
        }
    }

    public override void OnJoinedRoom()
    {
        playerRefToGame = PhotonNetwork.Instantiate(playerToSpawn.name, new Vector3(0, 10, 0), Quaternion.identity);
        canSpawnEnemy = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("could not Create Room");
    }

    public void QuitFunction()
    {
        Application.Quit();
    }

    public bool GetGameOver()
    {
        return gameOver;
    }

    public bool GetIsConnected()
    {
        return isConnected;
    }

    public override void OnConnectedToMaster()
    {
        isConnected = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnected = false;
    }

    IEnumerator EnemySpawningEnnumerator()
    {
        yield return new WaitForSeconds(Random.Range(3, 6));
        if (!gameOver)
        {
            int spawnPointChoose = Random.Range(0, 6);

            while (!canSpawnEnemy)
            {
                if (playerRefToGame != null)
                {
                    if (Vector3.Distance(playerRefToGame.transform.position, spawnPoints[spawnPointChoose].position) > 50)
                    {
                        Instantiate(enemyToSpawn, spawnPoints[spawnPointChoose].position, Quaternion.identity);
                        canSpawnEnemy = true;
                    }
                    else
                    {
                        spawnPointChoose++;
                        spawnPointChoose %= spawnPoints.Count;
                    }
                }
            }
        }
    }
}
