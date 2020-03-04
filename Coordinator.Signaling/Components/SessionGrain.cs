using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task AddParticipant(ISessionParticipant participant)
        {
            await Task.WhenAll(
                _participants
                    .Select(pair => pair.Value.OnParticipantJoin(participant.GetPrimaryKey()))
            );

            _participants.Add(participant.GetPrimaryKey(), participant);
        }

        public Task RemoveParticipant(ISessionParticipant participant)
        {
            _participants.Remove(participant.GetPrimaryKey());
            return Task.CompletedTask;
        }

        public Task ForwardIceCandidate(Guid destination, Guid origin, string payload)
        {
            if (!_participants.TryGetValue(destination, out var participant))
                throw new KeyNotFoundException("Participant not joined");

            return participant.OnParticipantIceCandidate(origin, payload);
        }

        public Task ForwardRtcAnswer(Guid destination, Guid origin, string sdpAnswer)
        {
            if (!_participants.TryGetValue(destination, out var participant))
                throw new KeyNotFoundException("Participant not joined");

            return participant.OnParticipantRtcAnswer(origin, sdpAnswer);
        }

        public Task ForwardRtcOffer(Guid destination, Guid origin, string sdpOffer)
        {
            if (!_participants.TryGetValue(destination, out var participant))
                throw new KeyNotFoundException("Participant not joined");

            return participant.OnParticipantRtcOffer(origin, sdpOffer);
        }

        public Task<Guid[]> GetParticipants()
        {
            return Task.FromResult(_participants.Keys.ToArray());
        }
    }
}