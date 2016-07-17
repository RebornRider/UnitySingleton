using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    protected override void Reset()
    {
        base.Reset();
        // Custom Reset behaiviours
        Debug.Log("Reset");
    }
}
