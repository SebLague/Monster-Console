using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu ()]
public class Song : ScriptableObject {
    public List<int> keyIndices = new List<int> ();
    public List<float> durations = new List<float> ();
    public List<float> startTimes = new List<float> ();
}