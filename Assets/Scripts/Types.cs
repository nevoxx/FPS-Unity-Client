using UnityEngine;
using FPS_Bindings;
using Quaternion = FPS_Bindings.Quaternion;
using Vector3 = FPS_Bindings.Vector3;

public class Types
{
    public static PlayerRect[] players = new PlayerRect[Constants.MAX_PLAYERS];
    
    public struct PlayerRect
    {
        public GameObject playerPref;
        
        public int connectionId;          
        public int health;          
        public Vector3 position;
        public Vector3 collider;
        public Quaternion rotation;
    }
}
