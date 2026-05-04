using System;
using FMODUnity;

/*
 * Collega ogni nome di AudioEvent ad un evento in FMOD assegnato nell'Inspector
 */


[Serializable]
public class  AudioEventReference
{
    public AudioEvent audioEvent;
    public EventReference eventReference;
  
}
