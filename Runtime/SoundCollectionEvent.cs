using System;
using LiteNinja.SOA.Events;
using UnityEngine;

namespace LiteNinja.Audio
{
    [CreateAssetMenu(menuName = "LiteNinja/Events/SoundCollection Event")]
    [Serializable]
    public class SoundCollectionEvent : ASOEvent<SoundCollection>
    {
        
    }
}