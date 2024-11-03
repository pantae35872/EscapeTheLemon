using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : Item
{
    [SerializeField] private uint maxAmmo;
    [SerializeField] private float fireRate;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptyShootSound;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private ParticleSystem flash;
    [SerializeField] private ParticleSystem fire;
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject modelMagazine;
    public int damage;

    private float timer;
    private bool ready_to_fire;
    private InputAction shoot;
    private InputAction reload;
    private int ammoAmount;
    private bool reloading;

    private void OnEnable()
    {
        shoot = GameManager.Instance.playerControls.Player.Shoot;
        shoot.Enable();
        shoot.performed += Shoot;

        reload = GameManager.Instance.playerControls.Player.Reload;
        reload.Enable();
        reload.performed += Reload;
    }

    private void OnDisable()
    {
        shoot.Disable();
    }

    protected override void OnItemCreated()
    {
        base.OnItemCreated();
        ready_to_fire = true;
        ammoAmount = (int) maxAmmo;
        ResetTimer();
    }

    private void ResetTimer()
    {
        timer = fireRate;
    }

    private IEnumerator ReloadAndAnimate()
    {
        model.transform.LeanMoveLocalY(0.476f, 0.5f);
        LeanTween.value(model, 0, 60f, 0.5f).setOnUpdate(
            (float val) =>
            {
                Quaternion rot = transform.localRotation;
                rot.eulerAngles = new Vector3(val, 0.0f, 0.0f);
                model.transform.localRotation = rot;
            }
        );
        yield return new WaitForSeconds(1f);
        modelMagazine.transform.LeanMoveLocal(new Vector3(1.93700004f, -1.70500004f, 0.0189999994f), 0.8f);
        source.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(1.8f);
        modelMagazine.transform.LeanMoveLocal(new Vector3(1.41144979f, -0.0866985321f, -0.00226058438f), 0.8f);
        yield return new WaitForSeconds(0.8f);
        source.PlayOneShot(reloadSound);
        LeanTween.value(model, 60, 0f, 0.5f).setOnUpdate(
            (float val) =>
            {
                Quaternion rot = transform.localRotation;
                rot.eulerAngles = new Vector3(val, 0.0f, 0.0f);
                model.transform.localRotation = rot;
            }
        );
        yield return new WaitForSeconds(0.5f);
        model.transform.LeanMoveLocalY(0, 0.5f);
        if ((int)maxAmmo - ammoAmount < Player.Instance.ammoAmount)
        {
            Player.Instance.ammoAmount -= (int)maxAmmo - ammoAmount;
            ammoAmount += (int)maxAmmo - ammoAmount;
        } else
        {
            ammoAmount += Player.Instance.ammoAmount;
            Player.Instance.ammoAmount = 0;
        }
        reloading = false;
    }

    private void Reload(InputAction.CallbackContext ctx)
    {
        if (!reloading && ammoAmount != maxAmmo && Player.Instance.ammoAmount > 0)
        {
            reloading = true;
            StartCoroutine(ReloadAndAnimate());
        }
    }

    private void UpdateTimerDisplay(float time)
    {
        GameMenu.Instance.cooldown.fillAmount = (float)Utils.MapValue(time, fireRate, 0, 0, 1);
    }

    private void Flash()
    {
        if (timer != 0)
        {
            timer = 0;
            UpdateTimerDisplay(timer);
        }
        ready_to_fire = true;
        ResetTimer();
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        if (!GameManager.Instance.IsPause && ready_to_fire && isCurrent && ammoAmount > 0 && !reloading)
        {
            source.PlayOneShot(shootSound);

            sparks.Play();
            flash.Play();
            fire.Play();
            smoke.Play();
            StartCoroutine(ShootAnim());

            RaycastHit[] raycastHits = Physics.RaycastAll(Player.Instance.shootPos.position, Player.Instance.shootPos.forward, 1000f, whatIsEnemy);
            for (int i = 0; i < raycastHits.Length; i++)
            {
                RaycastHit hit = raycastHits[i];
                Enemy enemy = hit.transform.GetComponent<Enemy>();
                if (enemy != null && enemy.shootAble)
                {
                    enemy.GetShot(damage);
                    break;
                }
            }

            ready_to_fire = false;
            ammoAmount -= 1;
        } else if (ammoAmount <= 0 && !GameManager.Instance.IsPause && isCurrent && !reloading)
        {
            source.PlayOneShot(emptyShootSound);
        }
    }

    private IEnumerator ShootAnim()
    {
        LeanTween.value(model, 0, -20f, 0.2f).setOnUpdate(
            (float val) =>
            {
                Quaternion rot = transform.localRotation;
                rot.eulerAngles = new Vector3(0.0f, 0.0f, val);
                model.transform.localRotation = rot;
            }
        );
        yield return new WaitForSeconds(0.2f);
        LeanTween.value(model, -20f, 0f, 0.1f).setOnUpdate(
            (float val) =>
            {
                Quaternion rot = transform.localRotation;
                rot.eulerAngles = new Vector3(0.0f, 0.0f, val);
                model.transform.localRotation = rot;
            }
        );
        yield return new WaitForSeconds(0.1f);
    }

    protected override void PauseableUpdate()
    {
        GameMenu.Instance.ammoAmount.text = "Ammo: " + ammoAmount.ToString() + "/" + maxAmmo.ToString();
        if (!ready_to_fire)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                UpdateTimerDisplay(timer);
            }
            else
            {
                Flash();
            }
        }
    }
}
