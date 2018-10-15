using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkPlayerController : MonoBehaviour
{
    public int PlayerConnectionId;
    
    private void LateUpdate()
    {
        if (PlayerConnectionId == ClientManager.instance.myConnectionId)
        {
            ClientTCP.SendMovement(transform.position, transform.rotation);
        }
    }
}
