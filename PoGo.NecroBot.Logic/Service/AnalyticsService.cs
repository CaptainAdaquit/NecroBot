﻿using Google.Apis.Analytics.v3.Data;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.State;
using System.Threading;
using System.Threading.Tasks;

namespace PoGo.NecroBot.Logic.Service
{
    public class AnalyticsService
    {
        private const int POLLING_INTERVAL = 5000;
        private Google.Apis.Analytics.v3.AnalyticsService service;
        
        private static AnalyticsData clientData = new AnalyticsData();
        
        public AnalyticsService()
        {
            service = new Google.Apis.Analytics.v3.AnalyticsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = "JQkzeCAktUQEpp8arKipoJvmdjJbKURUuvHJfwkziWmJLjePfaXczjFdoDGyiyPe", ApplicationName = "NecroBot"
            });
        }

        public class AnalyticsEvent : IEvent
        {
            public int EventType { get; set; }
            public object Data { get; set; }
        }

        public async Task StartAsync(Session session, CancellationToken cancellationToken)
        {
            await Task.Delay(10000, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            service.OnData += (eventType, eventData) =>
            {
                var analyticsEvent = new AnalyticsEvent { EventType = eventType, Data = eventData };
                session.EventDispatcher.Send(analyticsEvent);
            };

            while (true)
            {
                var request = service.Data.Ga.Get(service, clientData);
                await request.ExecuteRequest(cancellationToken).ConfigureAwait(false);
                await Task.Delay(POLLING_INTERVAL).ConfigureAwait(false);
            }
        }
        
        public static void HandleEvent(IEvent evt, ISession session)
        {
        }
        
        public static void Listen(IEvent evt, ISession session)
        {
            dynamic eve = evt;

            try
            {
                HandleEvent(eve, session);
            }
            catch
            {
            }
        }

        public static void HandleEvent(SnipeFailedEvent e, ISession sesion)
        {
            clientData.Removes.Enqueue(e.ToPokemon());
        }

        private static void HandleEvent(EncounteredEvent e, ISession session)
        {
            if (e.IsRecievedFromSocket)
                return;

            clientData.Adds.Enqueue(e.ToPokemon());
        }
    }
}