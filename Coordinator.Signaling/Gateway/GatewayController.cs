using System;
using System.Threading.Tasks;
using Coordinator.Signaling.Abstractions.Components;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace Coordinator.Signaling.Gateway
{
    public class GatewayController : Hub<IGatewayClient>, IGatewayServer
    {
        private const string ParticipantIdKey = "participantId";

        private readonly IGrainFactory _grainFactory;

        private ISessionParticipant _participant;

        public GatewayController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        private ISessionParticipant Participant
        {
            get
            {
                if (_participant != null)
                    return _participant;

                if (!Context.Items.TryGetValue(ParticipantIdKey, out var participantId))
                    Context.Items[ParticipantIdKey] = participantId = Guid.NewGuid();

                return _participant = _grainFactory.GetGrain<ISessionParticipant>((Guid) participantId);
            }
        }

        public Task<Guid> GetParticipantId()
        {
            return Task.FromResult(Participant.GetPrimaryKey());
        }

        public Task<Guid[]> GetSessionParticipants()
        {
            return Participant.GetSessionParticipants();
        }

        public Task LeaveCurrentSession()
        {
            return Participant.LeaveCurrentSession();
        }

        public Task JoinSession(Guid sessionId)
        {
            return Participant.JoinSession(sessionId);
        }

        public Task SendIceCandidate(Guid destination, string payload)
        {
            return Participant.ForwardIceCandidate(destination, payload);
        }

        public Task SendRtcAnswer(Guid destination, string sdpAnswer)
        {
            return Participant.ForwardRtcAnswer(destination, sdpAnswer);
        }

        public Task SendRtcOffer(Guid destination, string sdpOffer)
        {
            return Participant.ForwardRtcOffer(destination, sdpOffer);
        }

        public override async Task OnConnectedAsync()
        {
            await Participant.SetConnectionId(Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Participant.LeaveCurrentSession();
            await base.OnDisconnectedAsync(exception);
        }
    }
}