using LiteNinja.SOA.Events;
using UnityEngine;
using UnityEngine.Events;

namespace LiteNinja.Audio
{
    [AddComponentMenu("LiteNinja/Event Listeners/SoundCollection Event Listener")]
    public class SoundCollectionListener : ASOEventListener<SoundCollection>
    {
        [SerializeField] private EventResponse[] _eventResponses;
        protected override EventResponse<SoundCollection>[] EventResponses => _eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<SoundCollection>
        {
            [SerializeField] private SoundCollectionEvent _soEvent;
            public override ASOEvent<SoundCollection> SOEvent => _soEvent;

            [SerializeField] private UnityEventSoundCollection _response;
            public override UnityEvent<SoundCollection> Response => _response;
        }
    }
}