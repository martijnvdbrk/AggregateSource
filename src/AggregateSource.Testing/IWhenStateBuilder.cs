﻿using System;

namespace AggregateSource.Testing {
  /// <summary>
  /// The when state within the test specification building process.
  /// </summary>
  public interface IWhenStateBuilder {
    /// <summary>
    /// Then events should have occurred.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="events">The events that should have occurred.</param>
    /// <returns>A builder continuation.</returns>
    IThenStateBuilder Then(string identifier, params object[] events);
    /// <summary>
    /// Throws an exception.
    /// </summary>
    /// <param name="exception">The exception thrown.</param>
    /// <returns>A builder continuation.</returns>
    IThrowStateBuilder Throws(Exception exception);
    /// <summary>
    /// Builds the test specification thus far.
    /// </summary>
    /// <returns>The test specification.</returns>
    EventCentricTestSpecification Build();
  }
}