using UnityEngine;

public enum E_Footstep
{
    Normal,
    Jelly
}

public class PlayerAudio : MonoBehaviour
{
    private bool muted = false;

    public void Initialize() { muted = true; }

    public void Initialize(PlayerBombHandler bombHandler)
    {
        //Altri eventi (es morte)
        bombHandler.OnBombPlaced += PlayBombPlaced;
    }

    private void PlayBombPlaced() { if (!muted) { AudioManager.Instance.PlaySFX(AudioEvent.BombTimer); } }
    
    public void PlayFootstep()
    {
        if(!muted)
            AudioManager.Instance.PlayFootstep(GetComponent<PlayerController>().eFootstep);
    }

    //Per interrompere eventuali suoni looppati o altro
    private void OnDestroy() { }
}
