using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class Item : GameplayMonoBehaviour {
    public Transform cacheTransform;
    public GameObject itemPrefab;
    public bool isCurrent;

    private void Start()
    {
        OnItemCreated();
    }

    protected virtual void OnItemCreated()
    {
        cacheTransform = gameObject.transform;
    }

    private void OnDestroy()
    {
        OnItemDestroy();
    }

    public virtual void OnItemDestroy()
    {

    }
}

public class Inventory : GameplayMonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [SerializeField] private List<GameObject> uiSlots;
    [SerializeField] private List<GameObject> uiSelectSlots;
    [SerializeField] private Transform temp;

    private readonly int inventorySize = 3;
    private List<Item> inventory = new();
    private List<GameObject> inventoryDisplay = new();
    private List<Transform> inventoryDisplayCacheTransform = new();
    private int current;
    private List<InputAction> selector = new();
    private InputAction scroll;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            for (int i = 0; i < inventorySize; i++)
            {
                inventory.Add(null);
                inventoryDisplay.Add(null);
                inventoryDisplayCacheTransform.Add(null);
            }
        }
    }

    private void OnEnable()
    {
        selector.Add(GameManager.Instance.playerControls.Inventory._1);
        selector[0].Enable();
        selector[0].performed += ctx => Select(0);

        selector.Add(GameManager.Instance.playerControls.Inventory._2);
        selector[1].Enable();
        selector[1].performed += ctx => Select(1);

        selector.Add(GameManager.Instance.playerControls.Inventory._3);
        selector[2].Enable();
        selector[2].performed += ctx => Select(2);

        scroll = GameManager.Instance.playerControls.Inventory.Scroll;
        scroll.Enable();
        scroll.performed += ctx => {
            Vector2 value = ctx.ReadValue<Vector2>();
            if (value.y > 0)
            {
                PrevItem();
            } else if (value.y < 0)
            {
                NextItem();
            }
        };   
    }

    private void OnDisable()
    {
        selector[0].Disable();
        selector[1].Disable();
        selector[2].Disable();
        scroll.Disable();
    }

    protected override void PauseableUpdate()
    {
        Item current = inventory[this.current];
        if (current != null)
        {
            if (current.cacheTransform.parent != Player.Instance.HandItemContainerCacheTransform) current.cacheTransform.SetParent(Player.Instance.HandItemContainerCacheTransform, false);
            if (current.cacheTransform.localPosition != Vector3.zero) current.cacheTransform.localPosition = Vector3.zero;
        }

        for (int i = 0; i < inventorySize; i++)
        {
            uiSelectSlots[i].SetActive(this.current == i);

            if (current != inventory[i])
            {
                if (inventory[i] != null)
                {
                    if (inventory[i].cacheTransform.parent != temp) inventory[i].cacheTransform.SetParent(temp, false);
                    inventory[i].isCurrent = false;
                }
            } else
            {
                if (inventory[i] != null)
                {
                    inventory[i].isCurrent = true;
                }
            }

        }
    }

    public void SetItemAt(int index, Item item)
    {
        if (index >= inventorySize - 1 || index < 0)
        {
            return;
        }

        inventory[index] = item;
        OnListItemChange(index);
    }

    public void RemoveItem(int index)
    {
        if (index >= 0 && index < inventorySize)
        {
            Destroy(inventory[index]);
            inventory.RemoveAt(index);
        }
        OnListItemChange(index);
    }

    private void OnListItemChange(int changeIndex)
    {
        if (inventoryDisplay[changeIndex] != null) Destroy(inventoryDisplay[changeIndex]);
        inventoryDisplay[changeIndex] = Instantiate(inventory[changeIndex] != null ? inventory[changeIndex].itemPrefab : new GameObject("EmptyGameObject"));
        inventoryDisplayCacheTransform[changeIndex] = inventoryDisplay[changeIndex].transform;
        inventoryDisplayCacheTransform[changeIndex].SetParent(uiSlots[changeIndex].transform, false);
        inventoryDisplayCacheTransform[changeIndex].localPosition = new Vector3(14.5f, -10.3918133f, 40f);
    }

    public void NextItem()
    {
        current++;
        if (current == inventory.Count)
        {
            current = 0;
        }
    }

    public void PrevItem()
    {
        current--;
        if (current < 0)
        {
            current = inventory.Count - 1;
        } 
    }

    private void Select(int index)
    {
        if (index >= inventorySize || index < 0)
        {
            return;
        }

        current = index;
    }
}
