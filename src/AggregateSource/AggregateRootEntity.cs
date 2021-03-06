﻿using System;
using System.Collections.Generic;

namespace AggregateSource {
  /// <summary>
  /// Base class for aggregate root entities that need some basic infrastructure for tracking state changes.
  /// </summary>
  public abstract class AggregateRootEntity : IAggregateRootEntity {
    readonly List<object> _changes;
    readonly Dictionary<Type, Action<object>> _handlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootEntity"/> class.
    /// </summary>
    protected AggregateRootEntity() {
      _handlers = new Dictionary<Type, Action<object>>();
      _changes = new List<object>();
    }

    /// <summary>
    /// Registers the specified handler to be invoked when the specified event is applied.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to register the handler for.</typeparam>
    /// <param name="handler">The handler.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="handler"/> is null.</exception>
    protected void Register<TEvent>(Action<TEvent> handler) {
      if (handler == null) throw new ArgumentNullException("handler");
      _handlers.Add(typeof (TEvent), @event => handler((TEvent) @event));
    }

    /// <summary>
    /// Initializes this instance using the specified events.
    /// </summary>
    /// <param name="events">The events to initialize with.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="events"/> are null.</exception>
    public void Initialize(IEnumerable<object> events) {
      if (events == null) throw new ArgumentNullException("events");
      if (HasChanges()) throw new InvalidOperationException("Initialize cannot be called on an instance with changes.");
      foreach (var @event in events) {
        Play(@event);
      }
    }

    /// <summary>
    /// Applies the specified event to this instance and invokes the associated state handler.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    protected void Apply(object @event) {
      if (@event == null) throw new ArgumentNullException("event");
      Play(@event);
      Record(@event);
    }

    void Play(object @event) {
      Action<object> handler;
      if (_handlers.TryGetValue(@event.GetType(), out handler)) {
        handler(@event);
      }
    }

    void Record(object @event) {
      _changes.Add(@event);
    }

    /// <summary>
    /// Determines whether this instance has state changes.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance has state changes; otherwise, <c>false</c>.
    /// </returns>
    public bool HasChanges() {
      return _changes.Count != 0;
    }

    /// <summary>
    /// Gets the state changes applied to this instance.
    /// </summary>
    /// <returns>A list of recorded state changes.</returns>
    public IEnumerable<object> GetChanges() {
      return _changes.ToArray();
    }

    /// <summary>
    /// Clears the state changes.
    /// </summary>
    public void ClearChanges() {
      _changes.Clear();
    }
  }
}
