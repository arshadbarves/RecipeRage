using Unity.Netcode;
using UnityEngine;

namespace TestScripts
{
    public class NetworkAutoHost : MonoBehaviour
    {
        private void Start()
        {
            NetworkManager.Singleton.StartHost();
        }
    }
}