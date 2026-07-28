using UnityEngine;

public enum E_Footstep
{
    Normal,
    Jelly
}

public class PlayerAudio : MonoBehaviour
{
    private bool muted;

    private AudioEvent deathSound;
    private E_Footstep footstepType;

    private PlayerBombHandler subscribedBombHandler;


    // Inizializzazione utilizzata da AI e giocatori remoti.
    public void Initialize(CharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogError($"[PlayerAudio] CharacterData non assegnato su {gameObject.name}.", this);
            return;
        }

        muted = true;

        deathSound = characterData.DeathSound;
        footstepType = characterData.eFootstep;
    }


    // Inizializzazione utilizzata dal giocatore locale.
    public void Initialize(CharacterData characterData, PlayerBombHandler bombHandler)
    {
        if (characterData == null)
        {
            Debug.LogError($"[PlayerAudio] CharacterData non assegnato su {gameObject.name}.", this);
            return;
        }

        muted = false;

        deathSound = characterData.DeathSound;
        footstepType = characterData.eFootstep;

        subscribedBombHandler = bombHandler;

        if (subscribedBombHandler != null)
        {
            subscribedBombHandler.OnBombPlaced += PlayBombPlaced;
        }
        else
        {
            Debug.LogWarning($"[PlayerAudio] PlayerBombHandler non assegnato su {gameObject.name}.", this);
        }
    }


    private void PlayBombPlaced()
    {
        if (muted || AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(AudioEvent.BombTimer);
    }


    public void PlayFootstep()
    {
        if (muted || AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayFootstep(footstepType);
    }


    public void PlayDeath()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayDeath(deathSound);
    }


    private void OnDestroy()
    {
        if (subscribedBombHandler != null)
        {
            subscribedBombHandler.OnBombPlaced -= PlayBombPlaced;
        }
    }
}