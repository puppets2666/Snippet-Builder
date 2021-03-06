﻿using EventBus.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus
{
	public sealed class InMemoryEventBus : IEventBus
	{
		private readonly ICollection<Type> _handlers;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public InMemoryEventBus(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_handlers = new List<Type>();
		}


		public void Subscribe<T, TH>()
			where T : IEvent
			where TH : IEventHandler<T>
		{
			var eventName = typeof(T).Name;
			var handlerType = typeof(TH);

			if (_handlers.Contains(handlerType))
			{
				throw new ArgumentException($"Handler Type {handlerType.Name} already is registered for '{eventName}'", nameof(handlerType));
			}

			_handlers.Add(handlerType);
		}


		public void Publish<TEvent>(TEvent @event)
			where TEvent : IEvent
		{
			foreach (var handler in _handlers.Where(IsEventHandler<TEvent>).Select(CreateHandlerFromType<TEvent>))
			{
				handler.Handle(@event);
			}
		}

		private bool IsEventHandler<TEvent>(Type handlerType)
			where TEvent : IEvent
		{
			return typeof(IEventHandler<TEvent>).IsAssignableFrom(handlerType);
		}

		private IEventHandler<TEvent> CreateHandlerFromType<TEvent>(Type handlerType)
			where TEvent : IEvent
		{
			using var serviceScope = _serviceScopeFactory.CreateScope();

			return serviceScope.ServiceProvider.GetService(handlerType) as IEventHandler<TEvent>;
		}
	}
}
