using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraScript : MonoBehaviour
{
    GameObject playerRefToCam;
    [SerializeField]
    float rotationSpeed = 3f;
    GameManager gameManagerToCam;
    Camera camComponenant;

    void Start()
    {
        gameManagerToCam = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (gameManagerToCam.GetIsConnected())
        {
            playerRefToCam = transform.parent.gameObject;
            if(playerRefToCam.GetPhotonView().IsMine)
            {
                gameObject.tag = "MainCamera";
                camComponenant = GetComponent<Camera>();
                if(camComponenant != null)
                {
                    camComponenant.depth = 1;
                }
            }
        }
        else
        {
            playerRefToCam = GameObject.FindGameObjectWithTag("Player");
            gameObject.tag = "MainCamera";
        }
    }

    void Update()
    {
        if (gameManagerToCam.GetIsConnected())
        {
            if (playerRefToCam.GetPhotonView().IsMine)
            {
                MovingWithPlayer();
            }
        }
        else
        {
            MovingWithPlayer();
        }
    }

    void MovingWithPlayer()
    {
        float mouseInputY = Input.GetAxis("Mouse Y");

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x - mouseInputY * rotationSpeed, playerRefToCam.transform.eulerAngles.y, 0);

        if(transform.eulerAngles.x <= 180 && transform.eulerAngles.x > 45)
        {
            transform.rotation = Quaternion.Euler(45, playerRefToCam.transform.eulerAngles.y, 0);
        }
        else if(transform.eulerAngles.x > 180 && transform.eulerAngles.x < 315)
        {
            transform.rotation = Quaternion.Euler(315, playerRefToCam.transform.eulerAngles.y, 0);
        }
    }
}
