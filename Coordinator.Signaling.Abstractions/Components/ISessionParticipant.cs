using System;
using System.Threading.Tasks;
using Orleans;

namespace Coordinator.Signaling.Abstractions.Components
{
    public interface ISessionParticipant : IGrainWithGuidKey
    {
        Task<string> GetConnectionId();

        Task<Guid[]> GetSessionParticipants();

        Task ForwardRtcAnswer(Guid destination, string sdpAnswer);

        Task ForwardRtcOffer(Guid destination, string sdpOffer);

        Task ForwardIceCandidate(Guid destination, string payload);

        Task JoinSession(Guid sessionId);

        Task<bool> LeaveCurrentSession();

        Task OnParticipantIceCandidate(Guid origin, string payload);

        Task OnParticipantJoin(Guid id);

        Task OnParticipantLeave(Guid id);

        Task OnParticipantRtcAnswer(Guid origin, string sdpAnswer);

        Task OnParticipantRtcOffer(Guid origin, string sdpOffer);

        Task SetConnectionId(string connectionId);
    }
}