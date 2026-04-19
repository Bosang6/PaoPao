using UnityEngine;

public class TileBreakEvent : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
