using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coordinator.Signaling.Abstractions.Components;
using Orleans;

namespace Coordinator.Signaling.Components
{
    public class SessionGrain : Grain, ISession
    {
        private readonly IDictionary<Guid, ISessionParticipant> _participants;

        public SessionGrain()
        {
            _participants = new Dictionary<Guid, ISessionParticipant>();
        }

        public Task AddParticipant(ISessionParticipant participant)
        {
            _participants.Add(participant.GetPrimaryKey(), participant);
            return Task.CompletedTask;
        }

        public Task RemoveParticipant(ISessionParticipant participant)
        {
            _participants.Remove(participant.GetPrimaryKey());
            return Task.CompletedTask;
        }

        public Task ForwardRtcAnswer(Guid destination, Guid origin, string sdpAnswer)
        {
            if (!_participants.TryGetValue(destination, out var participant))
                throw new KeyNotFoundException("Participant not joined");

            return participant.PushRtcAnswerToClient(origin, sdpAnswer);
        }

        public Task ForwardRtcOffer(Guid destination, Guid origin, string sdpOffer)
        {
            if (!_participants.TryGetValue(destination, out var participant))
                throw new KeyNotFoundException("Participant not joined");

            return participant.PushRtcOfferToClient(origin, sdpOffer);
        }
    }
}