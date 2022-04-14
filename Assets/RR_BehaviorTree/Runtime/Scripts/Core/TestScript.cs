using RR.Serialization;
using RR.AI;
using UnityEngine;

[System.Serializable]
public class TestValue
{
    public int myInt;
    public bool myBool;
    public Camera camera;
}

public class TestScript : MonoBehaviour, IMyInterface
{
    public SerializableDictionary<int, ScriptableObject> _dict;
}
