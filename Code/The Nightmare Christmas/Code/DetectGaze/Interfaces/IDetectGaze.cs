using UnityEngine;

public interface IDetectGaze
{
    void OnGazeDetected(Transform observer);
    void OnGazeLost();                      
}