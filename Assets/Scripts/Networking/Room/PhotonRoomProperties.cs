using System;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/*
 * Raccoglie le Room Custom Properties utilizzate dal Multiplayer
 * 
 * Gestisce:
 * - la mappa selezionata per la partita
 */

public static class PhotonRoomProperties
{
    public const string MapIdKey = "map_id";

    // spring = 0
    // winter = 1
    public const int DefaultMapId = 0;

    // Aggiunge la proprietà iniziale della mappa alla Hashtable usata dutante la creazione della Room
    public static void AddInitialMapProperty(Hashtable roomProperties, int mapId = DefaultMapId)
    {
        if (roomProperties == null)
        {
            return;
        }

        roomProperties[MapIdKey] = mapId;
    }

    // Legge la mappa selezionata dalle Custom Properties della Room
    // Restituisce true se la proprietà esiste e contiene un valore convertibile in int
    public static bool TryGetMapId(Room room, out int mapId)
    {
        mapId = -1;

        if (room == null)
        {
            return false;
        }

        if (!room.CustomProperties.TryGetValue(MapIdKey, out object storedValue)
        )
        {
            return false;
        }

        if (storedValue == null)
        {
            return false;
        }

        try
        {
            mapId = Convert.ToInt32(storedValue);
            return true;
        }
        catch
        {
            mapId = -1;
            return false;
        }
    }


    // Richiede a Photon di aggiornare la mappa selezionata nella Room corrente.
    public static bool SetMapId(Room room, int mapId)
    {
        if (room == null || mapId < 0)
        {
            return false;
        }

        Hashtable properties = new Hashtable
        {
            { MapIdKey, mapId }
        };

        return room.SetCustomProperties(properties);
    }




}
