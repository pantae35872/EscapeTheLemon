using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HealthCommands
{
    Increase,
    Decrease,
}
public class HealthCommand {
    public int amount;
    public HealthCommands command;

    public HealthCommand(int amount, HealthCommands command)
    {
        this.amount = amount;
        this.command = command;
    }
}

public class Player : GameplayMonoBehaviour
{
    public static Player Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public LayerMask whatIsEnemy;
    public int Health { get; private set; }
    public Queue<HealthCommand> healthQueue = new();
    private bool doingHealth;
    [SerializeField] private GameObject handItemContainer;
    public Transform shootPos;
    public AudioSource source;
    public AudioClip injuredSound;
    public GameObject gunPrefab;
    public int ammoAmount;

    public GameObject HandItemContainer
    {
        get
        {
            return handItemContainer;
        }
        
        private set
        {
            handItemContainer = value;
        }
    }
    public Transform HandItemContainerCacheTransform { get; private set; }

    private void Start()
    {
        Health = Settings.Instance.healthAmount;
        HandItemContainerCacheTransform = HandItemContainer.transform;
        Inventory.Instance.SetItemAt(0, Instantiate(gunPrefab).GetComponent<Gun>());
        gunPrefab.GetComponent<Gun>().damage = Settings.Instance.gunDamage;
    }
    public void DecreaseHealth(int amount)
    {
        source.PlayOneShot(injuredSound);
        healthQueue.Enqueue(new HealthCommand(amount, HealthCommands.Decrease));
    }

    private IEnumerator SlowlyDecreaseHealth(int amount)
    {
        int totalHealth = Health - amount;
        while (Health > totalHealth && !GameManager.Instance.IsPause) {
            float elapsed = 0;
            while (elapsed < 0.01f && !GameManager.Instance.IsPause)
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
            while (elapsed < 0.01f && !GameManager.Instance.IsPause)
            {
                Health += 1;
                elapsed += Time.deltaTime;

                yield return null;
            }
        }
        doingHealth = false;
    }

    private void DequeuePHC()
    {
        if (!doingHealth && healthQueue.TryDequeue(out HealthCommand command))
        {
            doingHealth = true;
            if (command.command == HealthCommands.Increase)
            {
                StartCoroutine(SlowlyIncreaseHealth(command.amount));
            } else if (command.command == HealthCommands.Decrease) 
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
        GameMenu.Instance.health.fillAmount = (float) Utils.MapValue(Health, 0, 1000, 0, 1);
        if (Health <= 0)
        {
            int amount = Random.Range(1, 20);
            GameMenu.Instance.gameOverReward.text = "-" + amount.ToString() + " COINS";
            CoinSystem.Instance.DecreaseCoin(amount);
            GameManager.Instance.GameOver();
        }
        DequeuePHC();
        GameMenu.Instance.globalAmmoText.text = "GLOBAL Ammo: " + ammoAmount;
    }
}
