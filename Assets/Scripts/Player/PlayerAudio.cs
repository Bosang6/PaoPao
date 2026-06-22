using UnityEngine;

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

    //Per interrompere eventuali suoni looppati o altro
    private void OnDestroy() { }
}
