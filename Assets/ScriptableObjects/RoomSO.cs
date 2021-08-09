using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/RoomScriptableObject")]
public class RoomSO : ScriptableObject
{
    public int height;
    public int width;
    public Sprite sprite;
}
