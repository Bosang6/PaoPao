using UnityEngine;

public enum E_Footstep
{
    Normal,
    Jelly
}

public class PlayerAudio : MonoBehaviour
{
    private bool muted = false;
    private AudioEvent deathSound;


    public void Initialize(CharacterData characterData)
    {
        muted = true;
        deathSound = characterData.DeathSound;
    }

    public void Initialize(CharacterData characterData, PlayerBombHandler bombHandler)
    {
        muted = false;
        deathSound = characterData.DeathSound;
        bombHandler.OnBombPlaced += PlayBombPlaced;
    }

    private void PlayBombPlaced() 
    { 
        if (!muted) AudioManager.Instance.PlaySFX(AudioEvent.BombTimer);  
    }
    
    public void PlayFootstep()
    {
        if(!muted) AudioManager.Instance.PlayFootstep(GetComponent<PlayerController>().eFootstep);
    }

    public void PlayDeath()
    {
        AudioManager.Instance.PlayDeath(deathSound);
    }

    //Per interrompere eventuali suoni looppati o altro
    private void OnDestroy() { }

    
}
