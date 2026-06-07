using UnityEngine;


// Questa classe serve per ricordare la mappa scelta quando passo dal menu alla GameScene
// Ricordo anche il player. Bomberman come valore di default
public static class GameSession
{
    public static E_Map SelectedMap = E_Map.Spring;

    public static E_Character SelectedCharacter = E_Character.Bomberman;
}
