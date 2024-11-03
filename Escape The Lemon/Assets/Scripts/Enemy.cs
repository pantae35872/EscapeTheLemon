using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Utilities;

public class Enemy : GameplayMonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;
    public ParticleSystem sparks;
    public ParticleSystem flash;
    public ParticleSystem fire;
    public ParticleSystem smoke;
    public ParticleSystem attack;
    public AudioSource source;
    public AudioClip clip;
    private Transform cacheTransform;
    public GameObject enemy;
    private int maxHealth;
    public int Health { get; private set; }

    public LayerMask whatIsGround, whatIsPlayer;
    public float groundDrag;
    bool grounded;
    public float enemyHeight;
    public bool shootAble;
    Rigidbody rb;

    public float timeBetweenAttacks;
    bool alreadyAttacked;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        cacheTransform = transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        agent = GetComponent<NavMeshAgent>();
        shootAble = true;
    }

    public Queue<HealthCommand> healthQueue = new();
    private bool doingHealth;

    private void Start()
    {
        Health = Settings.Instance.healthAmount;
    }
    public void DecreaseHealth(int amount)
    {
        healthQueue.Enqueue(new HealthCommand(amount, HealthCommands.Decrease));
    }

    public override void OnGameStart()
    {
        if (GameManager.Instance.currentDifficulty == Difficulties.EASY)
        {
            maxHealth = 300;
        }
        else if (GameManager.Instance.currentDifficulty == Difficulties.NORMAL)
        {
            maxHealth = 680;
        } else
        {
            maxHealth = 1000;
        }

        Health = maxHealth;
    }

    private IEnumerator SlowlyDecreaseHealth(int amount)
    {
        int totalHealth = Health - amount;
        while (Health > totalHealth && !GameManager.Instance.IsPause)
        {
            float elapsed = 0;
            while (elapsed < 0.1f && !GameManager.Instance.IsPause)
            {
                Health -= 1;
                elapsed += Time.deltaTime;

                yield return null;
            }
        }
        doingHealth = false;
    }

    private IEnumerator SlowlyIncreaseHealth(int amount)
    {
        int totalHealth = amount + Health;
        while (Health < totalHealth && !GameManager.Instance.IsPause)
        {
            float elapsed = 0;
            while (elapsed < 0.1f && !GameManager.Instance.IsPause)
            {
                Health += 1;
                elapsed += Time.deltaTime;

                yield return null;
            }
        }
        doingHealth = false;
    }

    private void DequeueEHC()
    {
        if (!doingHealth && healthQueue.TryDequeue(out HealthCommand command))
        {
            doingHealth = true;
            if (command.command == HealthCommands.Increase)
            {
                StartCoroutine(SlowlyIncreaseHealth(command.amount));
            }
            else if (command.command == HealthCommands.Decrease)
            {
                StartCoroutine(SlowlyDecreaseHealth(command.amount));
            }
        }
    }

    public void IncreaseHealth(int amount)
    {
        healthQueue.Enqueue(new HealthCommand(amount, HealthCommands.Increase));
    }

    protected override void PauseableUpdate()
    {
        GameMenu.Instance.bossbar.value = (float) Utils.MapValue(Health, 0, maxHealth, 0, 1);
        grounded = Physics.Raycast(cacheTransform.position, Vector3.down, enemyHeight * 0.5f + 0.2f, whatIsGround);
        playerInSightRange = Physics.CheckSphere(cacheTransform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(cacheTransform.position, attackRange, whatIsPlayer);
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
        DequeueEHC();
    }

    protected override void NotifyPause()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    protected override void NotifyResume()
    {
        agent.isStopped = false;
    }

    private void Dead()
    {
        Destroy(gameObject);
    }

    private void PlayDead()
    {
        shootAble = false;
        enemy.SetActive(false);
        agent.isStopped = true;
        GetComponent<CapsuleCollider>().enabled = false;
        int amount = 0;
        if (GameManager.Instance.currentDifficulty == Difficulties.EASY)
        {
            amount = Random.Range(5, 15);
        } else if (GameManager.Instance.currentDifficulty == Difficulties.NORMAL)
        {
            amount = Random.Range(20, 60);
        } else
        {
            amount = Random.Range(100, 150);
        }
        GameMenu.Instance.gameWinReward.text = "+" + amount.ToString() + " COINS";
        GameManager.Instance.GameWin();
        CoinSystem.Instance.IncreaseCoin(amount);
        if (!sparks.isPlaying) sparks.Play();
        if (!fire.isPlaying) fire.Play();
        if (!flash.isPlaying) flash.Play();
        if (!smoke.isPlaying) smoke.Play();
        Invoke(nameof(Dead), 5f);
    }

    public void GetShot(int amount)
    {
        DecreaseHealth(amount);
        if (Health <= 0)
        {
            PlayDead();
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(cacheTransform.position);

        cacheTransform.LookAt(player);

        if (!alreadyAttacked)
        {
            Player.Instance.DecreaseHealth(120);
            attack.Play();
            source.PlayOneShot(clip);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
