using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/*
 * Gestisce la selezione del personaggio locale all'interno del RoomLobbyPanel
 *
 * Responsabilità:
 * - collegare i pulsanti dei personaggi
 * - salvare la scelta nelle Player Custom Properties
 * - mostrare il glow sul personaggio locale selezionato
 * - aggiornarsi quando Photon sincronizza la proprietà
 * - bloccare la selezione quando il giocatore locale è Ready
 */

public sealed class UIRoomCharacterSelectionManager : MonoBehaviourPunCallbacks
{
    [Header("Character Selection")]
    [Tooltip("Pulsanti ordinati come gli ID dei personaggi: " + "Bomberman, Penguin, Slime1, Slime2, Slime3.")]
    [SerializeField] private Button[] characterButtons;
    [Tooltip("Glow ordinati nello stesso modo dei pulsanti.")]
    [SerializeField] private GameObject[] selectionGlows;

    [Header("Default Selection")]
    [Tooltip("Personaggio assegnato quando il player " + "non possiede ancora una selezione." )]
    [SerializeField] [Min(0)] private int defaultCharacterId = 0;

    private UnityAction[] buttonActions;


    // Registra le callback Photon, collega i pulsanti e inizializza la selezione locale
    public override void OnEnable()
    {
        base.OnEnable();

        if (!ValidateReferences())
        {
            return;
        }

        BindButtons();
        InitializeLocalSelection();
        RefreshInteractionState();
    }


    // Rimuove i listener e deregistra le callback Photon
    public override void OnDisable()
    {
        UnbindButtons();

        base.OnDisable();
    }


    // Collega ogni pulsante al relativo Character ID
    private void BindButtons()
    {
        buttonActions = new UnityAction[characterButtons.Length];

        for (int characterId = 0; characterId < characterButtons.Length; characterId++)
        {
            Button characterButton = characterButtons[characterId];

            if (characterButton == null)
            {
                continue;
            }

            int capturedCharacterId = characterId;

            UnityAction action = () => HandleCharacterClicked(capturedCharacterId);

            buttonActions[characterId] = action;

            characterButton.onClick.AddListener(action);
        }
    }


    // Rimuove i listener aggiunti da BindButtons
    private void UnbindButtons()
    {
        if (characterButtons == null || buttonActions == null)
        {
            return;
        }

        int count = Mathf.Min(characterButtons.Length, buttonActions.Length);

        for (int index = 0; index < count; index++)
        {
            Button characterButton = characterButtons[index];

            UnityAction action = buttonActions[index];

            if (characterButton != null && action != null)
            {
                characterButton.onClick.RemoveListener(action);
            }
        }

        buttonActions = null;
    }


    // Legge la selezione già presente sul giocatore locale.
    // Se manca, assegna il personaggio predefinito.
    private void InitializeLocalSelection()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            SetSelectionVisible(-1);
            SetButtonsInteractable(false);
            return;
        }

        if (PhotonPlayerProperties.TryGetCharacterId(PhotonNetwork.LocalPlayer, out int storedCharacterId) && IsValidCharacterId(storedCharacterId))
        {
            SetSelectionVisible(storedCharacterId);
            return;
        }

        if (!IsValidCharacterId(defaultCharacterId))
        {
            Debug.LogError("[UIRoomCharacterSelectionManager] " + "Default Character ID non valido.", this);

            SetSelectionVisible(-1);
            return;
        }

        SetLocalCharacter(defaultCharacterId);
    }


    // Gestisce il click su un personaggio.
    private void HandleCharacterClicked(int characterId)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            Debug.LogWarning("[UIRoomCharacterSelectionManager] " + "Impossibile selezionare il personaggio: " + "il client non è dentro una Room.", this);
            return;
        }

        if (IsLocalPlayerReady())
        {
            Debug.LogWarning("[UIRoomCharacterSelectionManager] " + "Impossibile cambiare personaggio " + "mentre il player è Ready.", this);
            return;
        }

        if (!IsValidCharacterId(characterId))
        {
            Debug.LogWarning("[UIRoomCharacterSelectionManager] " + $"Character ID non valido: {characterId}.", this);
            return;
        }

        SetLocalCharacter(characterId);
    }


    // Salva il personaggio nelle Player Custom Properties.
    private void SetLocalCharacter(int characterId)
    {
        bool requestAccepted = PhotonPlayerProperties.SetCharacterId( PhotonNetwork.LocalPlayer, characterId);

        if (!requestAccepted)
        {
            Debug.LogError("[UIRoomCharacterSelectionManager] " + "Photon non ha accettato l'aggiornamento del personaggio locale.", this);

            return;
        }

        // Aggiornamento immediato del glow locale.
        SetSelectionVisible(characterId);

        Debug.Log("[UIRoomCharacterSelectionManager] " + $"Actor {PhotonNetwork.LocalPlayer.ActorNumber} " + $"ha selezionato il Character {characterId}.");
    }


    // Reagisce al cambiamento del personaggio e dello stato Ready del giocatore locale
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProperties)
    {
        if (targetPlayer == null || !targetPlayer.IsLocal || changedProperties == null)
        {
            return;
        }

        bool characterChanged = changedProperties.ContainsKey(PhotonPlayerProperties.CharacterIdKey);

        bool readyChanged = changedProperties.ContainsKey(PhotonPlayerProperties.ReadyKey);

        if (characterChanged)
        {
            if (PhotonPlayerProperties.TryGetCharacterId(targetPlayer, out int characterId) && IsValidCharacterId(characterId))
            {
                SetSelectionVisible(characterId);
            }
        }

        if (readyChanged)
        {
            RefreshInteractionState();

            Debug.Log("[UIRoomCharacterSelectionManager] " + $"Selezione personaggio " + $"{(IsLocalPlayerReady() ? "bloccata" : "riabilitata")}.");
        }
    }


    // Inizializza la selezione quando il client entra nella Room.
    public override void OnJoinedRoom()
    {
        InitializeLocalSelection();
        RefreshInteractionState();
    }


    // Pulisce la selezione quando il client lascia la Room.
    public override void OnLeftRoom()
    {
        SetSelectionVisible(-1);
        SetButtonsInteractable(false);
    }


    // Accende soltanto il glow del personaggio selezionato, con -1 spegne tutti i glow.
    private void SetSelectionVisible(int selectedCharacterId)
    {
        if (selectionGlows == null)
        {
            return;
        }

        for (int characterId = 0; characterId < selectionGlows.Length; characterId++)
        {
            GameObject glow = selectionGlows[characterId];

            if (glow != null)
            {
                glow.SetActive(characterId == selectedCharacterId);
            }
        }
    }


    // Abilita o disabilita tutti i pulsanti dei personaggi
    private void SetButtonsInteractable(bool interactable)
    {
        if (characterButtons == null)
        {
            return;
        }

        foreach (Button characterButton in characterButtons)
        {
            if (characterButton != null)
            {
                characterButton.interactable = interactable;
            }
        }
    }


    // Restituisce true se il giocatore locale è Ready.
    private bool IsLocalPlayerReady()
    {
        return
            PhotonNetwork.LocalPlayer != null &&
            PhotonPlayerProperties.TryGetReady(PhotonNetwork.LocalPlayer, out bool isReady) && isReady;
    }


    // Consente di cambiare personaggio a qualsiasi giocatore presente nella Room, purché non sia Ready
    private void RefreshInteractionState()
    {
        bool canSelectCharacter = PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null && !IsLocalPlayerReady();

        SetButtonsInteractable(canSelectCharacter);
    }


    // Controlla che il Character ID sia valido.
    private bool IsValidCharacterId(int characterId)
    {
        return
            characterId >= 0 &&
            characterButtons != null &&
            characterId < characterButtons.Length;
    }


    // Controlla che pulsanti e glow siano configurati.
    private bool ValidateReferences()
    {
        if (characterButtons == null || selectionGlows == null)
        {
            Debug.LogError("[UIRoomCharacterSelectionManager] " + "Array dei pulsanti o dei glow non assegnato.", this);
            return false;
        }

        if (characterButtons.Length == 0 || characterButtons.Length != selectionGlows.Length)
        {
            Debug.LogError("[UIRoomCharacterSelectionManager] " + "Pulsanti e glow devono avere " + "la stessa quantità di elementi.", this);

            return false;
        }

        if (!IsValidCharacterId(defaultCharacterId))
        {
            Debug.LogError("[UIRoomCharacterSelectionManager] " + "Default Character ID fuori intervallo.", this);

            return false;
        }

        return true;
    }
}