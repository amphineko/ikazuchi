using System;
using System.Threading.Tasks;
using Orleans;

namespace Coordinator.Signaling.Abstractions.Components
{
    public interface ISession : IGrainWithGuidKey
    {
        Task AddParticipant(ISessionParticipant participant);

        Task RemoveParticipant(ISessionParticipant participant);

        Task ForwardIceCandidate(Guid destination, Guid origin, string payload);

        Task ForwardRtcAnswer(Guid destination, Guid origin, string sdpAnswer);

        Task ForwardRtcOffer(Guid destination, Guid origin, string sdpOffer);

        Task<Guid[]> GetParticipants();
    }
}