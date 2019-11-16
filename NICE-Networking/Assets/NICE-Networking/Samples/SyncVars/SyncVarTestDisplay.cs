using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SyncVarTestDisplay : MonoBehaviour
{
    public SyncVarTest syncVarTest;

    public Text syncInt;    
    public Text syncString;    
    public Text syncGameObject;    
    public Text syncVector3;    
    public Text syncVector2;    
    public Text syncQuaternion;    
    public Text syncColor;
    public Image syncColorDisplay;
    public Text syncBool;    
    public Text syncEnum;

    private void LateUpdate()
    {
        syncInt.text = "SyncInt = " + syncVarTest.syncInt;
        syncString.text = "SyncString = " + syncVarTest.syncString;
        syncGameObject.text = "SyncGameObject = " + (syncVarTest.syncGameObject ? syncVarTest.syncGameObject.name : "null");
        syncVector3.text = "SyncVector3 = " + syncVarTest.syncVector3;
        syncVector2.text = "SyncVector2 = " + syncVarTest.syncVector2;
        syncQuaternion.text = "SyncQuaternion = " + syncVarTest.syncQuaternion;
        syncColor.text = "SyncColor = " + syncVarTest.syncColor;
        syncColorDisplay.color = syncVarTest.syncColor;
        syncBool.text = "SyncBool = " + syncVarTest.syncBool;
        syncEnum.text = "SyncEnum = " + syncVarTest.syncEnum;
    }
}
