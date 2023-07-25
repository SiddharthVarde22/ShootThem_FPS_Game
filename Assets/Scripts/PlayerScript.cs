using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    GameManager gameManagerRefToPlayer;

    [SerializeField]
    float rotationSpeedCap = 3f;
    [SerializeField]
    float movingSpeedCap = 5f;
    float maxMoveSpeed = 10f;
    float minMoveSpeed = 5f;
    GameObject playerRefrenceToCap;
    Animator playerAnimator;
    bool isAimingIn = false;
    GameObject firingParticleSystems;
    bool canShoot = true;

    [SerializeField]
    int ammoInClip, totalAmmo, currentPoints,currentHelth;

    const int maxAmmoInClip = 12;
    const int maxTotalAmmo = 60;
    const int maxHelth = 100;

    UIManager uiManagerRefToCap;

    [SerializeField]
    AudioClip shootSound;
    [SerializeField]
    AudioClip reloadSound;
    [SerializeField]
    AudioClip outOfAmmoReloadSound;
    [SerializeField]
    AudioClip aimInSound;

    void Start()
    {
        gameManagerRefToPlayer = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if(gameManagerRefToPlayer.GetIsConnected())
        {
            if(photonView.IsMine)
            {
                gameObject.tag = "MainPlayer";
            }
        }

        ammoInClip = maxAmmoInClip;
        totalAmmo = maxTotalAmmo;
        currentPoints = 0;
        currentHelth = maxHelth;

        playerRefrenceToCap = gameObject.transform.GetChild(0).GetChild(0).gameObject;
        playerAnimator = playerRefrenceToCap.GetComponent<Animator>();

        if(playerRefrenceToCap != null)
        {
            firingParticleSystems = playerRefrenceToCap.transform.GetChild(1).GetChild(0).GetChild(2).gameObject;
        }

        if (gameManagerRefToPlayer.GetIsConnected())
        {
            if (photonView.IsMine)
            {
                uiManagerRefToCap = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
            }
        }
        else
        {
            uiManagerRefToCap = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
        }

        if (uiManagerRefToCap != null)
        {
            uiManagerRefToCap.ChangeBulletCount(ammoInClip, totalAmmo);
            uiManagerRefToCap.ChangePointsText(currentPoints);
            uiManagerRefToCap.ChangeHelthSlider(currentHelth);
        }
    }

    void Update()
    {
        if(gameManagerRefToPlayer != null)
        {
            if(!gameManagerRefToPlayer.GetGameOver())
            {
                if(Input.GetKey(KeyCode.LeftShift))
                {
                    movingSpeedCap = maxMoveSpeed;
                }
                else
                {
                    movingSpeedCap = minMoveSpeed;
                }
                PlayerMovement();
                Firing();
                UsePoints();
                if(currentHelth < 1)
                {
                    gameManagerRefToPlayer.ChangeGameOver();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("GameManagerRefToPlayer Is Null");
        }
    }

    void PlayerMovement()
    {
        float inputXRot = Input.GetAxis("Mouse X");
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + inputXRot * rotationSpeedCap, 0);

        transform.position += Input.GetAxis("Vertical") * transform.TransformDirection(Vector3.forward) * 
            Time.deltaTime * movingSpeedCap;

        transform.position += Input.GetAxis("Horizontal") * transform.TransformDirection(Vector3.right) *
            Time.deltaTime * movingSpeedCap;
    }

    void Firing()
    {
        Ray firingRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit firingRayHitInfo;
        if(Physics.Raycast(firingRay,out firingRayHitInfo,20f))
        {
            if(firingRayHitInfo.collider.tag == "Enemy")
            {
                if (uiManagerRefToCap != null)
                {
                    uiManagerRefToCap.ChangeKnobColor(Color.green);
                }
            }
            else
            {
                if (uiManagerRefToCap != null)
                {
                    uiManagerRefToCap.ChangeKnobColor(Color.red);
                }
            }
        }
        else
        {
            if (uiManagerRefToCap != null)
            {
                uiManagerRefToCap.ChangeKnobColor(Color.red);
            }
        }

        if(Input.GetKeyDown(KeyCode.Mouse1) && !isAimingIn)
        {
            AimIn();
        }
        else if(Input.GetKeyDown(KeyCode.Mouse1) && isAimingIn)
        {
            AimOut();
        }

        if(firingParticleSystems != null)
        {
            if(Input.GetKeyDown(KeyCode.Mouse0) && canShoot && ammoInClip >= 1)
            {
                firingParticleSystems.SetActive(canShoot);

                AudioSource.PlayClipAtPoint(shootSound, transform.position);

                playerAnimator.SetTrigger("HandGunFireTrigPara");
                ammoInClip--;
                if (uiManagerRefToCap != null)
                {
                    uiManagerRefToCap.ChangeBulletCount(ammoInClip, totalAmmo);
                }
                if(firingRayHitInfo.collider != null)
                {
                    if (firingRayHitInfo.collider.tag == "Enemy")
                    {
                        EnemyScript hittingEnemyScript = firingRayHitInfo.transform.GetComponent<EnemyScript>();
                        if(hittingEnemyScript != null)
                        {
                            hittingEnemyScript.EnemyGetHit(Random.Range(25,36));
                        }
                    }
                }
                
                if(ammoInClip < 1)
                {
                    playerAnimator.SetTrigger("HandGunOutOfAmmoTrigPara");
                }
                canShoot = false;
                StartCoroutine(ResetTheFire());
            }
            else if(Input.GetKeyDown(KeyCode.R))
            {
                canShoot = false;
                playerAnimator.SetTrigger("HandGunReloadingTrigPara");
                StartCoroutine(ReloadingCoroutine());
                if(ammoInClip < 1)
                {
                    AudioSource.PlayClipAtPoint(outOfAmmoReloadSound, transform.position);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(reloadSound, transform.position);
                }
            }
        }
    }

    void AimIn()
    {
        isAimingIn = true;
        playerAnimator.SetBool("IsAimingInPara", isAimingIn);
        AudioSource.PlayClipAtPoint(aimInSound, transform.position);
        firingParticleSystems.transform.localPosition = new Vector3(0, -1.07f, 0.6f);
    }

    void AimOut()
    {
        isAimingIn = false;
        playerAnimator.SetBool("IsAimingInPara", isAimingIn);
        AudioSource.PlayClipAtPoint(aimInSound, transform.position);
        firingParticleSystems.transform.localPosition = new Vector3(0.105f, -1.005f, 0.6f);
    }

    public void UpdateCurrentPoints(int pointsToIncrease)
    {
        currentPoints += pointsToIncrease;
        if (uiManagerRefToCap != null)
        {
            uiManagerRefToCap.ChangePointsText(currentPoints);
        }
    }

    void UsePoints()
    {
        if (uiManagerRefToCap != null)
        {
            uiManagerRefToCap.ChangePointsText(currentPoints);
            uiManagerRefToCap.ChangeHelthSlider(currentHelth);
        }

        if(currentPoints >= 100)
        {
            if (uiManagerRefToCap != null)
            {
                uiManagerRefToCap.ChangeBuyAmmoOrMedColor(Color.white);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                currentPoints -= 100;
                totalAmmo += 30;
                if (totalAmmo > maxTotalAmmo)
                {
                    totalAmmo = maxTotalAmmo;
                }
                if (uiManagerRefToCap != null)
                {
                    uiManagerRefToCap.ChangeBulletCount(ammoInClip, totalAmmo);
                }
            }
            else if(Input.GetKeyDown(KeyCode.H))
            {
                currentPoints -= 100;
                currentHelth += 50;
                if(currentHelth > maxHelth)
                {
                    currentHelth = maxHelth;
                }
                if (uiManagerRefToCap != null)
                {
                    uiManagerRefToCap.ChangeHelthSlider(currentHelth);
                }
            }
        }
        else
        {
            if (uiManagerRefToCap != null)
            {
                uiManagerRefToCap.ChangeBuyAmmoOrMedColor(Color.black);
            }
        }
    }

    public void PlayerGetHit(int damage)
    {
        if (!gameManagerRefToPlayer.GetIsConnected())
        {
            currentHelth -= damage;
            if (uiManagerRefToCap != null)
            {
                uiManagerRefToCap.ChangeHelthSlider(currentHelth);
            }
        }
        else
        {
            if(photonView.IsMine)
            {
                currentHelth -= damage;
                if (uiManagerRefToCap != null)
                {
                    uiManagerRefToCap.ChangeHelthSlider(this.currentHelth);
                }
            }
        }
    }

    IEnumerator ResetTheFire()
    {
        yield return new WaitForSeconds(0.1f);
        firingParticleSystems.SetActive(canShoot);
        canShoot = true;
    }

    IEnumerator ReloadingCoroutine()
    {
        float secondsToWait;
        if(ammoInClip > 0)
        {
            secondsToWait = 1.6f;
        }
        else
        {
            secondsToWait = 2.3f;
        }

        yield return new WaitForSeconds(secondsToWait);
        int difAmmoInClip = maxAmmoInClip - ammoInClip;
        int difTotalAmmo = totalAmmo - difAmmoInClip;

        if(difTotalAmmo >= 0)
        {
            ammoInClip += difAmmoInClip;
            totalAmmo = difTotalAmmo;
        }
        else
        {
            ammoInClip += totalAmmo;
            totalAmmo -= totalAmmo;
        }
        if (uiManagerRefToCap != null)
        {
            uiManagerRefToCap.ChangeBulletCount(ammoInClip, totalAmmo);
        }
        canShoot = true;
    }
}
