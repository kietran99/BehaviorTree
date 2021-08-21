using UnityEngine;
using System;

public interface IMyInterface {}

[Serializable]
public class TestData
{
    public int myInt;
    public string myString;
    public Camera camera;
    public IMyInterface myInterface;
}

public class TestTaskDataContainer : ScriptableObject
{
    public TestData data;
    public System.Collections.Generic.Dictionary<string, TestData> dict;
}
