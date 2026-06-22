using UnityEngine;
using FMODUnity;

public enum E_Footstep
{
    Normal,
    Jelly
}
public class FootstepAudio : MonoBehaviour
{
    public void PlayFootstep()
    {
        AudioManager.Instance.PlayFootstep(GetComponent<PlayerController>().eFootstep);
    }
}
