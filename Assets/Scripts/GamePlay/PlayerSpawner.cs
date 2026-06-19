using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 *  Questa classe instanzia i character dei player in base alla configurazione di GameSession e alle posizioni di spawn fornite dal MapManager. 
 */


public class PlayerSpawner : MonoBehaviour
{
    [System.Serializable]
    private class CharacterPrefabEntry
    {
        public CharacterData cData;
        public GameObject prefab;
    }

    [Header("Character Prefab")]
    [SerializeField] private List<CharacterPrefabEntry> characterPrefabs = new List<CharacterPrefabEntry>();

    [Header("HUD")]
    [SerializeField] private UIPlayerLives[] uiPlayerLives;

    [Header("Manager")]
    [SerializeField] private GameManager gameManager;

    private IEnumerator Start()
    {
        // Aspetta un frame per essere sicuri che MapManager abbia caricato la mappa
        yield return null;

        SpawnPlayersFromSession();
    }

    private void SpawnPlayersFromSession()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager non trovato in scena.");
            return;
        }

        // Vettore con le posizioni di spawn dei giocatori, ottenute da MapManager
        Vector2Int[] spawnPositions = MapManager.Instance.GetPlayerSpawnPositions();


        if (spawnPositions == null || spawnPositions.Length == 0)
        {
            Debug.LogError("Nessuna spawn position trovata nella mappa.");
            return;
        }

        if (GameSession.PlayerSlots == null || GameSession.PlayerSlots.Count == 0)
        {
            Debug.LogError("Nessuna configurazione player trovata in GameSession.");
            return;
        }

        foreach (PlayerSlotConfig slot in GameSession.PlayerSlots)
        {
            if (slot.slotIndex < 0 || slot.slotIndex >= spawnPositions.Length)
            {
                Debug.LogWarning("Spawn position non valida per lo slot: " + slot.slotIndex);
                continue;
            }

            GameObject prefab = GetPrefabByCharacter(slot.cType);

            if (prefab == null)
            {
                Debug.LogWarning("Prefab non trovato per il character: " + slot.cType);
                continue;
            }

            // Calcola la posizione di spawn in base all'indice dello slot
            Vector2Int spawnGridPosition = spawnPositions[slot.slotIndex];

            // Converti la posizione di spawn da coordinate di griglia a coordinate di mondo
            Vector3 spawnWorldPosition = new Vector3(spawnGridPosition.x, spawnGridPosition.y, 0f);

            GameObject spawnedPlayer = Instantiate(prefab, spawnWorldPosition, Quaternion.identity);

            PlayerMove playerMove = spawnedPlayer.GetComponent<PlayerMove>();

            if (playerMove != null)
            {
                playerMove.SetSpawnPosition(spawnWorldPosition, Quaternion.identity);
            }

            PlayerController playerController = spawnedPlayer.GetComponent<PlayerController>();

            if (playerController != null && gameManager != null)
            {
                gameManager.RegisterPlayer(playerController);
            }

            BindPlayerToHUD(slot.slotIndex, playerController);

            //Debug.Log("Spawnato slot " + slot.slotIndex + " - " + slot.character + " - " + slot.slotType);
        }


    }

    // Metodo helper per ottenere il prefab corrispondente a un character
    private GameObject GetPrefabByCharacter(CharacterData.E_Character cType)
    {
        foreach (CharacterPrefabEntry entry in characterPrefabs)
        {
            if (entry.cData.type == cType)
            {
                return entry.prefab;
            }
        }

        return null;
    }

    // Collega il PlayerController del player appena spawnato all'HUD delle vite corrispondente al suo slot
    private void BindPlayerToHUD(int slotIndex, PlayerController playerController)
    {
        if (playerController == null)
        {
            Debug.LogWarning("PlayerSpawner: impossibile collegare HUD, PlayerController nullo.");
            return;
        }

        if (uiPlayerLives == null || uiPlayerLives.Length == 0)
        {
            Debug.LogWarning("PlayerSpawner: array UIPlayerLives non assegnato.");
            return;
        }

        if (slotIndex < 0 || slotIndex >= uiPlayerLives.Length)
        {
            Debug.LogWarning("PlayerSpawner: slotIndex HUD non valido: " + slotIndex);
            return;
        }

        UIPlayerLives playerLivesUI = uiPlayerLives[slotIndex];

        if (playerLivesUI == null)
        {
            Debug.LogWarning("PlayerSpawner: UIPlayerLives mancante per slot " + slotIndex);
            return;
        }

        playerLivesUI.Bind(playerController);

        //Debug.Log("HUD vite collegato allo slot " + slotIndex + " per player " + playerController.name);
    }

}
