using RR.Serialization;
using UnityEngine;

[System.Serializable]
public class TestValue
{
    public int myInt;
    public bool myBool;
    public Camera camera;
}

public class TestScript : MonoBehaviour
{
    // public SerializableDictionary<string, int> intDict;
    public SerializableDictionary<string, int[]> intArrDict;
    public SerializableDictionary<string, TestValue> testValueDict;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
