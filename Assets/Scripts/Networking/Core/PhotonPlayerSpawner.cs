using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


/*
 * Crea i personaggi della partita Multiplayer utilizzando la config ricostruita dentro GameSession
 * 
 * Responsabilità:
 * - attendere il caricamento della mappa
 * - creare soltanto il giocatore umano locale
 * - non creare manualmente i NetworkHuman
 * - creare le AI soltanto sul Master Client
 * - passare slotIndex e tipo di player al prefab Photon
 * 
 */


public sealed class PhotonPlayerSpawner : MonoBehaviour
{
    [Serializable] private class CharacterPrefabEntry
    {
        [Tooltip("Personaggio associato al prefab Photon.")]
        public CharacterData.E_Character characterType;

        [Tooltip("Prefab multiplayer registrato nel DefaultPool di Photon.")]
        public GameObject prefab;
    }


    [Header("Photon Character Prefabs")]
    [Tooltip("Associazione tra personaggio e percorso del relativo prefab Photon.")]
    [SerializeField] private List<CharacterPrefabEntry> characterPrefabs =  new List<CharacterPrefabEntry>();

    [Header("Initialization")]
    [Tooltip("Tempo massimo concesso al MapManager per caricare " + "la mappa e rendere disponibili gli spawn point.")]
    [SerializeField] [Min(1f)] private float mapInitializationTimeout = 5f;


    private bool hasSpawned;



    // Registra tutti i prefab multiplayer nel DefaultPool di Photon prima che qualsiasi client possa richiederne l'istanziazione
    private void Awake()
    {
        RegisterPhotonPrefabs();
    }


    // Attende che MapManager abbia caricato la mappa, poi avvia la creazione dei personaggi Multiplayer
    private IEnumerator Start()
    {
        float elapsedTime = 0f;

        while (elapsedTime < mapInitializationTimeout)
        {
            if (MapManager.Instance != null)
            {
                Vector2Int[] spawnPositions = MapManager.Instance.GetPlayerSpawnPositions();

                if (spawnPositions != null && spawnPositions.Length > 0)
                {
                    SpawnPlayers(spawnPositions);
                    yield break;
                }
            }

            elapsedTime += Time.unscaledDeltaTime;

            yield return null;
        }

        Debug.LogError("[PhotonPlayerSpawner] " + "Impossibile avviare lo spawn: la mappa non è stata " + $"inizializzata entro {mapInitializationTimeout} secondi.", this);
    }


    // Legge la configurazione degli slot e decide quali personaggi devono essere creati localmente da questo client
    private void SpawnPlayers(Vector2Int[] spawnPositions)
    {
        if (hasSpawned)
        {
            return;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError("[PhotonPlayerSpawner] " + "Impossibile creare i player: il client non è dentro una Room Photon.", this);
            return;
        }

        if (GameSession.PlayerSlots == null || GameSession.PlayerSlots.Count == 0)
        {
            Debug.LogError("[PhotonPlayerSpawner] " + "Nessuna configurazione multiplayer presente in GameSession.", this);
            return;
        }

        foreach (PlayerSlotConfig slot in GameSession.PlayerSlots)
        {
            if (slot == null)
            {
                continue;
            }

            if (slot.slotIndex < 0 || slot.slotIndex >= spawnPositions.Length)
            {
                Debug.LogWarning("[PhotonPlayerSpawner] " + $"Spawn position non valida per lo Slot " + $"{slot.slotIndex}.", this);
                continue;
            }

            switch (slot.pType)
            {
                case PlayerInstanceData.E_PlayerSlotType.LocalHuman: 
                    SpawnSlot(slot, spawnPositions[slot.slotIndex], false);
                    break;

                case PlayerInstanceData.E_PlayerSlotType.NetworkHuman:
                    /*
                     * Il NetworkHuman non viene creato localmente.
                     *
                     * Verrà ricevuto automaticamente quando il client
                     * proprietario eseguirà PhotonNetwork.Instantiate().
                     */
                    break;

                case PlayerInstanceData.E_PlayerSlotType.AI:
                    /*
                     * Soltanto il Master Client crea le AI.
                     *
                     * Le copie verranno replicate automaticamente
                     * agli altri client presenti nella Room.
                     */
                    if (PhotonNetwork.IsMasterClient)
                    {
                        SpawnSlot(slot, spawnPositions[slot.slotIndex], true);
                    }
                    break;

                default:
                    Debug.LogWarning("[PhotonPlayerSpawner] " + $"Tipo di slot non riconosciuto: {slot.pType}.", this);
                    break;
            }
        }

        hasSpawned = true;
    }


    // Crea un singolo personaggio tramite PhotonNetwork.Instantiate e passa al prefab lo slot e le info Human/AI
    private void SpawnSlot(PlayerSlotConfig slot, Vector2Int spawnGridPosition, bool isAI)
    {
        string prefabId = GetPrefabId(slot.cType);

        if (string.IsNullOrWhiteSpace(prefabId))
        {
            Debug.LogError("[PhotonPlayerSpawner] " + $"Nessun prefab Photon configurato per " + $"{slot.cType}.", this);
            return;
        }

        Vector3 spawnWorldPosition = new Vector3(spawnGridPosition.x, spawnGridPosition.y, 0f);

        object[] instantiationData =
        {
            slot.slotIndex,
            isAI
        };

        GameObject spawnedPlayer =
            PhotonNetwork.Instantiate(
                prefabId,
                spawnWorldPosition,
                Quaternion.identity,
                0,
                instantiationData
            );

        if (spawnedPlayer == null)
        {
            Debug.LogError("[PhotonPlayerSpawner] " + $"Creazione fallita per lo Slot {slot.slotIndex}, " + $"Character {slot.cType}.", this);
            return;
        }

        Debug.Log(
            "[PhotonPlayerSpawner] " +
            $"Creato Slot {slot.slotIndex}: " +
            $"{slot.cType} - " +
            $"{(isAI ? "AI" : "LocalHuman")} - " +
            $"Prefab ID '{prefabId}'."
        );
    }



    // Restituisce l'identificativo del prefab Photon associato al personaggio richiesto
    private string GetPrefabId(CharacterData.E_Character characterType)
    {
        if (characterPrefabs == null)
        {
            return null;
        }

        foreach (CharacterPrefabEntry entry in characterPrefabs)
        {
            if (entry != null && entry.characterType == characterType && entry.prefab != null)
            {
                return entry.prefab.name;
            }
        }

        return null;
    }


    // Inserisce i prefab configurati nell'Inspector nella cache del DefaultPool, evitando che Photon debba cercarli con Resources.Load
    private void RegisterPhotonPrefabs()
    {
        DefaultPool defaultPool = PhotonNetwork.PrefabPool as DefaultPool;

        if (defaultPool == null)
        {
            Debug.LogError("[PhotonPlayerSpawner] " + "Il PrefabPool corrente non è un DefaultPool.", this);
            return;
        }

        if (characterPrefabs == null || characterPrefabs.Count == 0)
        {
            Debug.LogError("[PhotonPlayerSpawner] " + "Nessun prefab multiplayer configurato.", this);
            return;
        }

        foreach (CharacterPrefabEntry entry in characterPrefabs)
        {
            if (entry == null || entry.prefab == null)
            {
                Debug.LogError("[PhotonPlayerSpawner] " + "È presente un elemento senza prefab assegnato.", this);
                continue;
            }

            PhotonView photonView = entry.prefab.GetComponent<PhotonView>();

            if (photonView == null)
            {
                Debug.LogError("[PhotonPlayerSpawner] " + $"Il prefab '{entry.prefab.name}' " + "non possiede un PhotonView.", entry.prefab);
                continue;
            }

            string prefabId = entry.prefab.name;

            if (defaultPool.ResourceCache.TryGetValue(prefabId, out GameObject registeredPrefab))
            {
                if (registeredPrefab != entry.prefab)
                {
                    Debug.LogError("[PhotonPlayerSpawner] " + $"Esiste già un altro prefab registrato " + $"con ID '{prefabId}'.", this);
                }

                continue;
            }

            defaultPool.ResourceCache.Add(prefabId, entry.prefab);

            Debug.Log("[PhotonPlayerSpawner] " + $"Prefab Photon registrato: '{prefabId}'.");
        }
    }




}
