using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHUDMatchInitializer : MonoBehaviour
{
    [System.Serializable]
    private class CharacterHUDSprite
    {
        public CharacterData cData;
        public Sprite hudSprite;
    }

    // Array di Image per i player nell'HUD da assegnare in Inspector
    [Header("HUD Player Images")]
    [SerializeField] private Image[] hudPlayerImages;

    // Lista di associazioni tra character e sprite HUD da assegnare in Inspector
    [Header("Character HUD Sprites")]
    [SerializeField] private List<CharacterHUDSprite> characterHUDSprites = new List<CharacterHUDSprite>();

    private void Start()
    {
        InitializeHUD();
    }

    // Inizializza l'HUD dei player in base alla configurazione dei player slots in GameSession
    private void InitializeHUD()
    {
        if (GameSession.PlayerSlots == null || GameSession.PlayerSlots.Count == 0)
        {
            Debug.LogWarning("UIHUDMatchInitializer: nessuna configurazione player trovata in GameSession.");
            return;
        }

        foreach (PlayerSlotConfig slot in GameSession.PlayerSlots)
        {
            if (slot.slotIndex < 0 || slot.slotIndex >= hudPlayerImages.Length)
            {
                Debug.LogWarning("UIHUDMatchInitializer: slotIndex non valido: " + slot.slotIndex);
                continue;
            }

            Image hudImage = hudPlayerImages[slot.slotIndex];

            if (hudImage == null)
            {
                Debug.LogWarning("UIHUDMatchInitializer: Image HUD mancante per lo slot " + slot.slotIndex);
                continue;
            }

            Sprite hudSprite = GetHUDSprite(slot.cType);

            if (hudSprite == null)
            {
                Debug.LogWarning("UIHUDMatchInitializer: sprite HUD non trovata per " + slot.cType);
                continue;
            }

            hudImage.sprite = hudSprite;

            //Debug.Log("HUD slot " + slot.slotIndex + " aggiornato con " + slot.character + " usando sprite " + hudSprite.name);
        }
    }

    // Restituisce lo sprite HUD associato al character specificato
    private Sprite GetHUDSprite(CharacterData.E_Character cDataType)
    {
        foreach (CharacterHUDSprite entry in characterHUDSprites)
        {
            if (entry.cData.type == cDataType)
            {
                return entry.hudSprite;
            }
        }

        return null;
    }

}
