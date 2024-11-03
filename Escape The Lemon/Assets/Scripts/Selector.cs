using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Selector : MonoBehaviour
{
    [Serializable]
    public class SelectorOnChangeEvent : UnityEvent { }
    [SerializeField]
    private SelectorOnChangeEvent onChange = new SelectorOnChangeEvent();
    [SerializeField] private Button firstButtion;
    [SerializeField] private Button secondButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject firstSelection;
    [SerializeField] private GameObject secondSelection;
    [SerializeField] private bool inverted = false;
    public Action<bool> onConfirm;
    private bool isFirst;
    public bool IsFirst
    {
        get { return isFirst; }
        set
        {
            isFirst = value;
            isSecond = !value;
            OnChange();
        }
    }

    private bool isSecond;
    public bool IsSecond
    {
        get { return isSecond; }
        set
        {
            isSecond = value;
            isFirst = !value;
            OnChange();
        }
    }

    private void Start()
    {
        isFirst = true;
        isSecond = false;
        firstButtion.onClick.AddListener(FirstClicked);
        secondButton.onClick.AddListener(SecondClicked);
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirm);
        }
    }

    private void OnChange()
    {
        onChange.Invoke();
    }

    private void OnConfirm()
    {
        onConfirm?.Invoke(IsFirst);
    }

    private void FirstClicked()
    {
        if (!isFirst)
        {
            IsFirst = true;
        }
    }

    private void SecondClicked()
    {
        if (!isSecond)
        {
            IsSecond = true;
        }
    }

    private void Update()
    {
        if (isFirst)
        {
            firstSelection.SetActive(inverted);
            secondSelection.SetActive(!inverted);
        }

        if (isSecond)
        {
            firstSelection.SetActive(!inverted);
            secondSelection.SetActive(inverted);
        }
    }
}
