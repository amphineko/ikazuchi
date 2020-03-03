using System;
using System.Threading.Tasks;
using Orleans;

namespace Coordinator.Signaling.Abstractions.Components
{
    public interface ISessionParticipant : IGrainWithGuidKey
    {
        Task<string> GetConnectionId();

        Task SetConnectionId(string connectionId);

        Task ForwardRtcAnswer(Guid destination, string sdpAnswer);

        Task ForwardRtcOffer(Guid destination, string sdpOffer);

        Task JoinSession(Guid sessionId);

        Task<bool> LeaveCurrentSession();

        Task PushRtcAnswerToClient(Guid origin, string sdpAnswer);

        Task PushRtcOfferToClient(Guid origin, string sdpOffer);
    }
}