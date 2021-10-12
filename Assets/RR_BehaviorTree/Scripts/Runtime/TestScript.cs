using RR.Serialization;
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
    [TagField]
    public string someTag;
    [LayerMaskField]
    public int someLayer;
    // public SerializableDictionary<string, int> intDict;
    public SerializableDictionary<string, int[]> intArrDict;
    public SerializableDictionary<string, TestValue> testValueDict;
    void Awake() => Debug.Log(someLayer);
}
