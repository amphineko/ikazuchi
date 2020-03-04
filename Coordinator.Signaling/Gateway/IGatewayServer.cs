using System;
using System.Threading.Tasks;

namespace Coordinator.Signaling.Gateway
{
    public interface IGatewayServer
    {
        Task<Guid> GetParticipantId();

        Task<Guid[]> GetSessionParticipants();

        Task LeaveCurrentSession();

        Task JoinSession(Guid sessionId);

        Task SendIceCandidate(Guid destination, string payload);

        Task SendRtcAnswer(Guid destination, string sdpAnswer);

        Task SendRtcOffer(Guid destination, string sdpOffer);
    }
}