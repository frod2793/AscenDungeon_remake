using UnityEngine;

public class Timer
{
    public float WaitTime;
    public float WaitTimeSum;

    public void Start(float waitTime = 0f)
    {
        WaitTime = waitTime.Equals(0f) ? WaitTime : waitTime;

        WaitTimeSum = 0.0f;
    }

    public void Update()
    {
        WaitTimeSum += Time.deltaTime * Time.timeScale;
    }

    public bool IsOver() => WaitTimeSum >= WaitTime;
}
