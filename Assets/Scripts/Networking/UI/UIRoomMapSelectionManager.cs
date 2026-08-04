using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/*
 * Gestisce la selezione della mappa nel RoomLobbyPanel 
 * 
 * Responsabilità:
 * - permettere la selezione solamente al Master Client
 * - salvare la mappa nelle Room Custom Properties
 * - mostrare lo stesso glow su tutti i client
 * - aggiornare l'interazione quando cambia il Master Client
 * 
 */

public sealed class UIRoomMapSelectionManager : MonoBehaviourPunCallbacks
{
    [Header("Map Selection")]
    [Tooltip("Pulsanti ordinati secondo i Map ID: Spring, Winter e Cave.")]
    [SerializeField] private Button[] mapButtons;
    [Tooltip("Glow ordinati nello stesso modo dei pulsanti.")]
    [SerializeField] private GameObject[] selectionGlows;

    private UnityAction[] buttonActions;

    // Quando il pannello viene attivato:
    // 1. registra le callback di Photon
    // 2. collega i pulsanti
    // 3. assicura che la proprietà della mappa esista
    // 4. legge la selezione corrente
    // 5. abilita i pulsanti solo per il Master Client  
    public override void OnEnable()
    {
        base.OnEnable();

        if (!ValidateReferences())
        {
            return;
        }

        BindButtons();
        EnsureMapPropertyExists();
        RefreshSelectionFromRoom();
        RefreshInteractionState();
    }


    // Rimuove i listener aggiunti ai pulsanti e disattiva correttamnte le Callback di Photon
    public override void OnDisable()
    {
        UnbindButtons();

        base.OnDisable();
    }



    // Collega ogni pulsante al proprio Map ID
    // L'indice nell'array rappresenta:
    // 0 = spring
    // 1 = winter
    // 2 = cave
    private void BindButtons()
    {
        buttonActions = new UnityAction[mapButtons.Length];

        for (int mapId = 0; mapId < mapButtons.Length; mapId++)
        {
            Button mapButton = mapButtons[mapId];

            if (mapButton == null)
            {
                continue;
            }

            int capturedMapId = mapId;

            UnityAction action = () => HandleMapClicked(capturedMapId);

            buttonActions[mapId] = action;

            mapButton.onClick.AddListener(action);
        }
    }


    // Rimuove i listener creati da BindButtons, evita che chiudendo e riaprendo il pannello,
    // lo stesso click venga eseguito più volte
    private void UnbindButtons()
    {
        if (mapButtons == null || buttonActions == null)
        {
            return;
        }

        int count = Mathf.Min(mapButtons.Length, buttonActions.Length);

        for (int index = 0; index < count; index++)
        {
            Button mapButton = mapButtons[index];
            UnityAction action = buttonActions[index];

            if (mapButton != null && action != null)
            {
                mapButton.onClick.RemoveListener(action);
            }
        }

        buttonActions = null;
    }


    // Viene eseguito quando viene cliccata l'immagine di una mappa
    // (solo per il Master Client)
    private void HandleMapClicked(int mapId)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("[UIRoomMapSelectionManager] " + "Il client non è dentro una Room.", this);
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[UIRoomMapSelectionManager] " + "Soltanto il Master Client può " + "selezionare la mappa.", this);
            return;
        }

        if (IsLocalPlayerReady())
        {
            Debug.LogWarning("[UIRoomMapSelectionManager] " + "Impossibile cambiare mappa mentre il Master Client è Ready.", this);
            return;
        }

        if (!IsValidMapId(mapId))
        {
            Debug.LogWarning("[UIRoomMapSelectionManager] " + $"Map ID non valido: {mapId}.", this);
            return;
        }


        // Se la mappa è già selezionata, non invia una modifica utile
        if (PhotonRoomProperties.TryGetMapId(PhotonNetwork.CurrentRoom, out int currentMapId) && currentMapId == mapId)
        {
            return;
        }

        bool requestAccepted = PhotonRoomProperties.SetMapId(PhotonNetwork.CurrentRoom, mapId);

        if (!requestAccepted)
        {
            Debug.LogError("[UIRoomMapSelectionManager] " + "Photon non ha accettato la richiesta " + "di modifica della mappa.", this);
            return;
        }

        Debug.Log("[UIRoomMapSelectionManager] " + $"Il Master Client ha richiesto " + $"la selezione della mappa {mapId}.");

    }


    // Se la proprietà map_id non esiste, il Master Client assegna Spring come default
    private void EnsureMapPropertyExists()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null || !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (PhotonRoomProperties.TryGetMapId(PhotonNetwork.CurrentRoom, out int existingMapId) && IsValidMapId(existingMapId))
        {
            return;
        }

        PhotonRoomProperties.SetMapId(PhotonNetwork.CurrentRoom, PhotonRoomProperties.DefaultMapId);
    }


    /*
     * Legge map_id dalla Room corrente
     * e accende soltanto il glow corrispondente.
     */
    // Legge le map_id della Room corrente
    private void RefreshSelectionFromRoom()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            SetSelectionVisible(-1);
            return;
        }

        if (PhotonRoomProperties.TryGetMapId(PhotonNetwork.CurrentRoom, out int mapId) && IsValidMapId(mapId))
        {
            SetSelectionVisible(mapId);
            return;
        }

        SetSelectionVisible(PhotonRoomProperties.DefaultMapId);
    }


    // Callback Photon eseguita quando cambiano le room Custom Propertis
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null || !propertiesThatChanged.ContainsKey(PhotonRoomProperties.MapIdKey))
        {
            return;
        }

        RefreshSelectionFromRoom();

        if (PhotonRoomProperties.TryGetMapId(PhotonNetwork.CurrentRoom, out int mapId))
        {
            Debug.Log("[UIRoomMapSelectionManager] " + $"Mappa sincronizzata: {mapId}.");
        }
    }


    // La mappa può essere selezionata soltanto dal Master Client quando il suo stato Ready è disattivato.
    private void RefreshInteractionState()
    {
        bool canSelectMap = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && !IsLocalPlayerReady();

        if (mapButtons == null)
        {
            return;
        }

        foreach (Button mapButton in mapButtons)
        {
            if (mapButton != null)
            {
                mapButton.interactable = canSelectMap;
            }
        }
    }


    // Restituisce true quando il giocatore locale possiede lo stato Ready attivo
    private bool IsLocalPlayerReady()
    {
        return
            PhotonNetwork.LocalPlayer != null && PhotonPlayerProperties.TryGetReady(PhotonNetwork.LocalPlayer, out bool isReady) && isReady;
    }


    // Aggiorna solamente il glow della mappa attualmente selezionata
    // -1 spegne tutti i glow
    private void SetSelectionVisible(int selectedMapId)
    {
        if (selectionGlows == null)
        {
            return;
        }

        for (int mapId = 0; mapId < selectionGlows.Length; mapId++)
        {
            GameObject glow = selectionGlows[mapId];

            if (glow != null)
            {
                glow.SetActive(mapId == selectedMapId);
            }
        }
    }

    // Callback ricevuta quando il client entra correttamente nella Room
    public override void OnJoinedRoom()
    {
        EnsureMapPropertyExists();
        RefreshSelectionFromRoom();
        RefreshInteractionState();
    }



     // Quando cambia il Master Client:
     // - aggiorna chi può cliccare le mappe
     // - il nuovo Master verifica la proprietà
     // - rilegge il glow corrente
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        EnsureMapPropertyExists();
        RefreshSelectionFromRoom();
        RefreshInteractionState();
    }


    // Quando il client lascia la room spegne tutti i glow
    public override void OnLeftRoom()
    {
        SetSelectionVisible(-1);
        RefreshInteractionState();
    }


    // Controlla che il Map ID corrisponda ad uno degli elementi configurati
    private bool IsValidMapId(int mapId)
    {
        return
            mapId >= 0 &&
            Enum.IsDefined(typeof(E_Map), mapId) &&
            mapButtons != null &&
            selectionGlows != null &&
            mapId < mapButtons.Length &&
            mapId < selectionGlows.Length;
    }


    // Verifica che pulsanti e glow siano configurati correttamente
    private bool ValidateReferences()
    {
        if (mapButtons == null || selectionGlows == null)
        {
            Debug.LogError("[UIRoomMapSelectionManager] " + "Array dei pulsanti o dei glow " + "non assegnato.", this);
            return false;
        }

        if (mapButtons.Length == 0 || mapButtons.Length != selectionGlows.Length)
        {
            Debug.LogError("[UIRoomMapSelectionManager] " + "Pulsanti e glow devono avere " + "la stessa quantità di elementi.", this);
            return false;
        }

        if (!IsValidMapId( PhotonRoomProperties.DefaultMapId))
        {
            Debug.LogError("[UIRoomMapSelectionManager] " + "Default Map ID fuori intervallo.", this);
            return false;
        }

        return true;
    }



    // Aggiorna l'interazione della Map Selection quando cambia lo stato Ready del giocatore locale
    // È importante soprattutto per il Master Client:
    // READY blocca le mappe, CANCEL READY le riabilita.
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProperties)
    {
        if (targetPlayer == null || !targetPlayer.IsLocal || changedProperties == null || !changedProperties.ContainsKey(PhotonPlayerProperties.ReadyKey))
        {
            return;
        }

        RefreshInteractionState();

        Debug.Log("[UIRoomMapSelectionManager] " + $"Selezione mappa " + $"{(IsLocalPlayerReady() ? "bloccata" : "riabilitata")}.");
    }




}
