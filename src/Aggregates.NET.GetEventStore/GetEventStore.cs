﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Aggregates.Contracts;
using Aggregates.Extensions;
using Aggregates.Internal;
using EventStore.ClientAPI;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.MessageInterfaces;
using NServiceBus.ObjectBuilder;
using NServiceBus.Settings;
using NServiceBus.Transport;
using NServiceBus.Unicast;

namespace Aggregates
{
    public class GetEventStore : NServiceBus.Features.Feature
    {
        public GetEventStore()
        {
            Defaults(s =>
            {
                s.Set("FlushInterval", TimeSpan.FromSeconds(60));
            });
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var settings = context.Settings;
            
            context.Container.ConfigureComponent(b =>
                new EventStoreDelayed(b.Build<IStoreEvents>(), settings.EndpointName(), settings.Get<int>("MaxDelayed"), settings.Get<int>("ReadSize"), settings.Get<TimeSpan>("FlushInterval"), settings.Get<StreamIdGenerator>("StreamGenerator")),
                DependencyLifecycle.InstancePerUnitOfWork);


            context.Container.ConfigureComponent(b =>
            {
                IEventStoreConnection[] connections;
                if (!settings.TryGet<IEventStoreConnection[]>("Shards", out connections))
                    connections = new[] { b.Build<IEventStoreConnection>() };
                return new StoreEvents(b.Build<IMessageMapper>(), settings.Get<int>("ReadSize"), settings.Get<Compression>("Compress"), connections);
            },
                DependencyLifecycle.InstancePerCall);
        }

    }

}