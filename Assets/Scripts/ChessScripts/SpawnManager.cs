using UnityEngine;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("1. Clic detectado en SpawnManager");
            EventManager.onbuttonpressed?.Invoke();
        }
    }
}
