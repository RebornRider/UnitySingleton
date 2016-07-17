using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        // Custom Awake Behaiviour
        Debug.Log("Awake");
    }

    protected override void Reset()
    {
        base.Reset();
        // Custom Reset Behaiviour
        Debug.Log("Reset");
    }
}
