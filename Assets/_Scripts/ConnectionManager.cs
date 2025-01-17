using FishNet.Managing;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

    void Start()
    {
        Debug.Log($"Starting fishnet connection. NetworkManger is null: {networkManager == null}");

        if (networkManager != null)
            networkManager.ClientManager.StartConnection();
    }
}
