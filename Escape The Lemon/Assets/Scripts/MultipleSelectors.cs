using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Selector;

public class MultipleSelectors : MonoBehaviour
{
    [Serializable] public class MultipleSelectorOnChangeEvent : UnityEvent { }
    [SerializeField] private List<GameObject> selectors;
    [SerializeField] private List<Button> selectButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private MultipleSelectorOnChangeEvent onChange = new();
    [SerializeField] private MultipleSelectorOnChangeEvent onConfirm = new();

    public int currentSelect { get; private set; } = 0;

    private void Start()
    {
        currentSelect = 0;
        for (int i = 0; i < selectButton.Count; i++)
        {
            int index = i;
            selectButton[i].onClick.AddListener(() => Select(index));
        }
        confirmButton.onClick.AddListener(() => onConfirm?.Invoke());
    }

    private void Select(int index)
    {
        currentSelect = index;
        onChange?.Invoke();
    }

    private void Update()
    {
        for (int i = 0; i < selectors.Count; i++)
        {
            if (currentSelect != i)
            {
                selectors[i].SetActive(true);
            } else
            {
                selectors[i].SetActive(false);
            }
        }
    }
}
