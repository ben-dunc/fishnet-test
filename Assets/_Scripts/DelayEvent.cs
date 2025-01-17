using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayEvent : MonoBehaviour
{
    [SerializeField] float delay = 1f;
    [SerializeField] bool invokeOnStart = true;
    [SerializeField] UnityEvent onEvent = new();

    void Start()
    {
        if (invokeOnStart)
            Invoke();
    }

    void Invoke()
    {
        Invoke("DelayedInvoke", delay);
    }

    void DelayedInvoke()
    {
        onEvent.Invoke();
    }
}
