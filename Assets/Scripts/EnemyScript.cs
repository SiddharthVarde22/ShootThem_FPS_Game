using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    GameObject playerRefToEnemy;
    NavMeshAgent enemyAgent;
    PlayerScript playerScriptRefToEnemy;
    GameManager gameManagerRefToEnemy;
    GameObject firingParticlesRef;

    int enemyHelth;
    int pointsToGive;
    Animator enemyAnimatorRef;
    float currentStopDis;
    bool enemyCanFire;
    bool isAlive;
    Collider myColider;
    [SerializeField]
    AudioClip enemyShootClip;
    string playerTag;

    enum StatesOfEnemy
    {
        Walk,
        Shoot,
        Die
    }

    StatesOfEnemy currentEnemyState;

    void Start()
    {
        gameManagerRefToEnemy = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        enemyAnimatorRef = gameObject.GetComponent<Animator>();
        if(enemyAnimatorRef == null)
        {
            Debug.LogError("EnemyAnimatorRef Is Null");
        }

        enemyHelth = 100;
        pointsToGive = Random.Range(25, 36);
        currentStopDis = Random.Range(12, 16);
        enemyCanFire = true;
        isAlive = true;

        if (gameManagerRefToEnemy.GetIsConnected())
        {
            playerRefToEnemy = GameObject.FindGameObjectWithTag("MainPlayer");
        }
        else
        {
            playerRefToEnemy = GameObject.FindGameObjectWithTag("Player");
        }
        if (playerRefToEnemy == null)
        {
            Debug.LogError("PlayerRef IS Null");
        }
        playerScriptRefToEnemy = playerRefToEnemy.GetComponent<PlayerScript>();
        if(playerScriptRefToEnemy == null)
        {
            Debug.LogError("PlayerScriptRefToEnemy Is Null");
        }
        else
        {
            playerTag = playerRefToEnemy.tag;
        }

        enemyAgent = gameObject.GetComponent<NavMeshAgent>();
        if (enemyAgent == null)
        {
            Debug.LogError("NavMeshAgent is Null");
        }
        else
        {
            enemyAgent.stoppingDistance = currentStopDis;
        }
        firingParticlesRef = transform.GetChild(1).GetChild(0).gameObject;

        currentEnemyState = StatesOfEnemy.Walk;
    }

    void Update()
    {
        if (gameObject != null)
        {
            switch (currentEnemyState)
            {
                case StatesOfEnemy.Walk:
                    TheWalkState();
                    break;
                case StatesOfEnemy.Shoot:
                    TheShootState();
                    break;
                case StatesOfEnemy.Die:
                    TheDieState();
                    break;
            }
        }

        DestroyIfGameOver();
    }

    public void DestroyIfGameOver()
    {
        if (gameManagerRefToEnemy != null)
        {
            if (gameManagerRefToEnemy.GetGameOver())
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("GameManagerRefToEnemy Is null");
        }
    }

    public void TheWalkState()
    {
        if (enemyAgent != null)
        {
            enemyAnimatorRef.SetBool("WalkingPara", true);
            enemyAgent.SetDestination(playerRefToEnemy.transform.position);
            int curDis = (int)Vector3.Distance(transform.position, playerRefToEnemy.transform.position);
            if(curDis < currentStopDis)
            {
                enemyAnimatorRef.SetBool("WalkingPara", false);
                currentEnemyState = StatesOfEnemy.Shoot;
            }
        }
    }

    public void TheShootState()
    {
        Quaternion tempRotation = Quaternion.LookRotation(playerRefToEnemy.transform.position - transform.position);
        transform.rotation = Quaternion.Euler(0, tempRotation.eulerAngles.y, 0);

        Ray enemyFiringRay = new Ray(transform.position + (Vector3.up * 3), transform.TransformDirection(Vector3.forward));
        RaycastHit enemyRayHit;

        if(Physics.Raycast(enemyFiringRay,out enemyRayHit,20f))
        {
            if(enemyRayHit.collider.tag == playerTag && enemyCanFire)
            {
                enemyCanFire = false;
                enemyAnimatorRef.SetTrigger("ShootTrigPara");
                firingParticlesRef.SetActive(true);
                AudioSource.PlayClipAtPoint(enemyShootClip, transform.position);

                int isHitting = Random.Range(0, 2);
                if (isHitting == 1)
                {
                    playerScriptRefToEnemy.PlayerGetHit(Random.Range(5, 11));
                }
                StartCoroutine(EnemyFiring());
            }
        }
        else if ((enemyRayHit.collider == null || enemyRayHit.collider.tag != playerTag) &&
                transform.eulerAngles.y == tempRotation.eulerAngles.y)
        {
            currentEnemyState = StatesOfEnemy.Walk;
        }
    }

    public void TheDieState()
    {
        enemyAgent.isStopped = true;
        enemyAnimatorRef.SetTrigger("EnemyDieTrigPara");
        myColider = transform.GetComponent<CapsuleCollider>();
        if(myColider != null)
        {
            Destroy(myColider);
        }
        Destroy(gameObject, 3f);
    }

    public void EnemyGetHit(int damage)
    {
        if (isAlive)
        {
            enemyHelth -= damage;

            if (enemyHelth < 1)
            {
                playerScriptRefToEnemy.UpdateCurrentPoints(pointsToGive);
                isAlive = false;
                currentEnemyState = StatesOfEnemy.Die;
            }
        }
    }

    IEnumerator EnemyFiring()
    {
        yield return new WaitForSeconds(1f);
        firingParticlesRef.SetActive(false);
        enemyCanFire = true;
    }
}
