using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NICE_Networking;

public class SyncVarTest : NetworkBehaviour
{
    [SyncVar]
    public int syncInt = 0;

    [SyncVar]
    public string syncString = "sync this";

    [SyncVar]
    public GameObject syncGameObject;

    [SyncVar]
    public Vector3 syncVector3;

    [SyncVar]
    public Vector2 syncVector2;

    [SyncVar]
    public Quaternion syncQuaternion;

    [SyncVar]
    public Color syncColor;

    [SyncVar]
    public bool syncBool;

    [SyncVar]
    public NetworkAuthority syncEnum;

    // Start is called before the first frame update
    private void Start()
    {
        initialize(this);
    }
}