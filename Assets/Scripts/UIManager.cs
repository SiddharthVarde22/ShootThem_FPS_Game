using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    GameObject playingPanelRef;
    GameObject gameOverPanelRef;
    GameObject connectButton;
    GameObject disconnectButton;

    Image knobImage;
    Text bulletCountText;
    Text pointsText;
    Image buyAmmoImage;
    Text buyAmmoText;
    Image buyHelthImage;
    Text buyHelthText;
    Text networkStatusText;

    Slider helthSlider;
    GameManager gameManagerRef;

    void Start()
    {
        playingPanelRef = transform.GetChild(0).gameObject;
        gameOverPanelRef = transform.GetChild(1).gameObject;
        networkStatusText = transform.GetChild(2).GetComponent<Text>();

        knobImage = playingPanelRef.transform.GetChild(0).GetComponent<Image>();
        bulletCountText = playingPanelRef.transform.GetChild(1).GetComponent<Text>();
        pointsText = playingPanelRef.transform.GetChild(2).GetComponent<Text>();
        buyAmmoImage = playingPanelRef.transform.GetChild(3).GetChild(0).GetComponent<Image>();
        buyAmmoText = playingPanelRef.transform.GetChild(3).GetChild(1).GetComponent<Text>();
        buyHelthImage = playingPanelRef.transform.GetChild(3).GetChild(2).GetComponent<Image>();
        buyHelthText = playingPanelRef.transform.GetChild(3).GetChild(3).GetComponent<Text>();
        helthSlider = playingPanelRef.transform.GetChild(4).GetComponent<Slider>();
        connectButton = gameOverPanelRef.transform.GetChild(2).gameObject;
        disconnectButton = gameOverPanelRef.transform.GetChild(3).gameObject;

        gameManagerRef = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        if(gameManagerRef != null)
        {
            playingPanelRef.SetActive(!gameManagerRef.GetGameOver());
            gameOverPanelRef.SetActive(gameManagerRef.GetGameOver());
            if(gameManagerRef.GetGameOver())
            {
                connectButton.SetActive(!gameManagerRef.GetIsConnected());
                disconnectButton.SetActive(gameManagerRef.GetIsConnected());
            }
        }
    }

    public void ChangeKnobColor(Color knobColor)
    {
        knobImage.color = knobColor;
    }

    public void ChangeBulletCount(int bulletsInMag,int totalBullets)
    {
        bulletCountText.text = "Ammo : " + bulletsInMag + "\n" + totalBullets + "/ 60";
    }

    public void ChangePointsText(int points)
    {
        pointsText.text = "Points : " + points;
    }

    public void ChangeBuyAmmoOrMedColor(Color color)
    {
        buyAmmoImage.color = color;
        buyAmmoText.color = color;
        buyHelthImage.color = color;
        buyHelthText.color = color;
    }

    public void ChangeHelthSlider(int helth)
    {
        helthSlider.value = helth;
    }

    public void DisableHelthSlider(bool command)
    {
        helthSlider.gameObject.SetActive(command);
    }

    public void ConnectToMultiPlayer()
    {
        PhotonNetwork.ConnectUsingSettings();
        ChangeNetworkStatus("Connecting...");
    }

    public void DisconnectToMultiPlayer()
    {
        PhotonNetwork.Disconnect();
    }

    public void ChangeNetworkStatus(string currentStatus)
    {
        networkStatusText.text = currentStatus;
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        ChangeNetworkStatus("Connected");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ChangeNetworkStatus("Disconnected");
    }
}
