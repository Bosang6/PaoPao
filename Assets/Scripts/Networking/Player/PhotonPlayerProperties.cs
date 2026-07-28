using System;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/*
 * Chiave utilizzata nelle Player Custom Properties per memorizzare l'identificativo del personaggio
 * 
 * 
 */



public static class PhotonPlayerProperties
{
    public const string CharacterIdKey = "character_id";

    // Chiave per memorizzare lo stato Ready del player
    public const string ReadyKey = "is_ready";

    // Stato Ready utilizzato esclusivamente nella schermata post-partita
    public const string RematchReadyKey = "rematch_ready";

    // Prova a leggere il personaggio selezionato dalle Custom Properties di un Player Photon
    // Restituisce true se il valore esiste ed è valido
    public static bool TryGetCharacterId(Player player, out int characterId)
    {
        characterId = -1;

        if (player == null)
        {
            return false;
        }

        if (!player.CustomProperties.TryGetValue(CharacterIdKey, out object storedValue))
        {
            return false;
        }

        if (storedValue == null)
        {
            return false;
        }

        try
        {
            characterId = Convert.ToInt32(storedValue);
            return true;
        }
        catch
        {
            characterId = -1;
            return false;
        }
    }


    // Imposta e sincronizza il personaggio scelto per uno specifico Player Photon
    public static bool SetCharacterId(Player player, int characterId)
    {
        if (player == null || characterId < 0)
        {
            return false;
        }

        Hashtable properties = new Hashtable
        {
            { CharacterIdKey, characterId }
        };

        return player.SetCustomProperties(properties);
    }


    // Legge lo stato Ready dalle Player Custom Propertis
    public static bool TryGetReady(Player player, out bool isReady)
    {
        isReady = false;

        if (player == null)
        {
            return false;
        }

        if (!player.CustomProperties.TryGetValue(ReadyKey, out object storedValue))
        {
            return false;
        }

        if (storedValue == null)
        {
            return false;
        }

        try
        {
            isReady = Convert.ToBoolean(storedValue);
            return true;
        }
        catch
        {
            isReady = false;
            return false;
        }
    }


    // Imposta e sincronizza lo stato Ready di uno specifico Player
    public static bool SetReady(Player player, bool isReady)
    {
        if (player == null)
        {
            return false;
        }

        Hashtable properties = new Hashtable{{ ReadyKey, isReady }};

        return player.SetCustomProperties(properties);
    }


    // Legge lo stato Ready della rivincita
    public static bool TryGetRematchReady(Player player, out bool isReady)
    {
        isReady = false;

        if (player == null) return false;

        if (!player.CustomProperties.TryGetValue(RematchReadyKey, out object storedValue))
        {
            return false;
        }

        if (storedValue == null)
        {
            return false;
        }

        try
        {
            isReady = Convert.ToBoolean(storedValue);
            return true;
        }
        catch
        {
            isReady = false;
            return false;
        }
    }


    // Imposta lo stato Ready della rivincita
    public static bool SetRematchReady(Player player, bool isReady    )
    {
        if (player == null) return false;

        Hashtable properties = new Hashtable{{ RematchReadyKey, isReady }};

        return player.SetCustomProperties(properties);
    }


}
