﻿using System;
using System.IO;
using EventStore.ClientAPI;
using NUnit.Framework;
using ProtoBuf;

namespace AggregateSource.GEventStore {
  namespace RepositoryIntegrationTests {
    [TestFixture]
    public class Construction {
      [Test]
      public void FactoryCanNotBeNull() {
        Assert.Throws<ArgumentNullException>(() => 
          new Repository<AggregateRootEntityStub>(null, new UnitOfWork(), EmbeddedEventStore.Instance.Connection));
      }

      [Test]
      public void UnitOfWorkCanNotBeNull() {
        Assert.Throws<ArgumentNullException>(() => 
          new Repository<AggregateRootEntityStub>(AggregateRootEntityStub.Factory, null, EmbeddedEventStore.Instance.Connection));
      }

      [Test]
      public void EventStoreConnectionCanNotBeNull() {
        Assert.Throws<ArgumentNullException>(() =>
          new Repository<AggregateRootEntityStub>(AggregateRootEntityStub.Factory, new UnitOfWork(), null));
      }

      [Test]
      public void EventStoreReadConfigurationCanNotBeNull() {
        Assert.Throws<ArgumentNullException>(() =>
          new Repository<AggregateRootEntityStub>(AggregateRootEntityStub.Factory, new UnitOfWork(), EmbeddedEventStore.Instance.Connection, null));
      }
    }

    [TestFixture]
    public class WithEmptyStoreAndEmptyUnitOfWork {
      Repository<AggregateRootEntityStub> _sut;
      UnitOfWork _unitOfWork;
      Model _model;

      [SetUp]
      public void SetUp() {
        EmbeddedEventStore.Instance.Connection.DeleteAllStreams();
        _model = new Model();
        _unitOfWork = new UnitOfWork();
        _sut = new Repository<AggregateRootEntityStub>(AggregateRootEntityStub.Factory, _unitOfWork, EmbeddedEventStore.Instance.Connection);
      }

      [Test]
      public void GetThrows() {
        var exception =
          Assert.Throws<AggregateNotFoundException>(() => _sut.Get(_model.UnknownIdentifier));
        Assert.That(exception.Identifier, Is.EqualTo(_model.UnknownIdentifier));
        Assert.That(exception.Type, Is.EqualTo(typeof(AggregateRootEntityStub)));
      }

      [Test]
      public void GetOptionalReturnsEmpty() {
        var result = _sut.GetOptional(_model.UnknownIdentifier);

        Assert.That(result, Is.SameAs(Optional<AggregateRootEntityStub>.Empty));
      }

      [Test]
      public void AddAttachesToUnitOfWork() {
        var root = AggregateRootEntityStub.Factory();

        _sut.Add(_model.KnownIdentifier, root);

        Aggregate aggregate;
        var result = _unitOfWork.TryGet(_model.KnownIdentifier, out aggregate);
        Assert.That(result, Is.True);
        Assert.That(aggregate.Identifier, Is.EqualTo(_model.KnownIdentifier));
        Assert.That(aggregate.Root, Is.SameAs(root));
      }
    }

    [TestFixture]
    public class WithEmptyStoreAndFilledUnitOfWork {
      Repository<AggregateRootEntityStub> _sut;
      UnitOfWork _unitOfWork;
      AggregateRootEntityStub _root;
      Model _model;

      [SetUp]
      public void SetUp() {
        EmbeddedEventStore.Instance.Connection.DeleteAllStreams();
        _model = new Model();
        _root = AggregateRootEntityStub.Factory();
        _unitOfWork = new UnitOfWork();
        _unitOfWork.Attach(new Aggregate(_model.KnownIdentifier, 0, _root));
        _sut = new Repository<AggregateRootEntityStub>(AggregateRootEntityStub.Factory, _unitOfWork, EmbeddedEventStore.Instance.Connection);
      }

      [Test]
      public void GetThrowsForUnknownId() {
        var exception =
          Assert.Throws<AggregateNotFoundException>(() => _sut.Get(_model.UnknownIdentifier));
        Assert.That(exception.Identifier, Is.EqualTo(_model.UnknownIdentifier));
        Assert.That(exception.Type, Is.EqualTo(typeof(AggregateRootEntityStub)));
      }

      [Test]
      public void GetReturnsRootOfKnownId() {
        var result = _sut.Get(_model.KnownIdentifier);

        Assert.That(result, Is.SameAs(_root));
      }

      [Test]
      public void GetOptionalReturnsEmptyForUnknownId() {
        var result = _sut.GetOptional(_model.UnknownIdentifier);

        Assert.That(result, Is.SameAs(Optional<AggregateRootEntityStub>.Empty));
      }

      [Test]
      public void GetOptionalReturnsRootForKnownId() {
        var result = _sut.GetOptional(_model.KnownIdentifier);

        Assert.That(result, Is.EqualTo(new Optional<AggregateRootEntityStub>(_root)));
      }
    }

    [TestFixture]
    public class WithFilledStore {
      Repository<AggregateRootEntityStub> _sut;
      UnitOfWork _unitOfWork;
      AggregateRootEntityStub _root;
      Model _model;

      [SetUp]
      public void SetUp() {
        _model = new Model();
        using (var stream = new MemoryStream()) {
          Serializer.Serialize(stream, new Event());
          EmbeddedEventStore.Instance.Connection.AppendToStream(
            _model.KnownIdentifier,
            ExpectedVersion.NoStream,
            new EventData(
              Guid.NewGuid(),
              typeof (Event).AssemblyQualifiedName,
              false,
              stream.ToArray(),
              new byte[0]));
        }
        _root = AggregateRootEntityStub.Factory();
        _unitOfWork = new UnitOfWork();
        _sut = new Repository<AggregateRootEntityStub>(
          () => _root,
          _unitOfWork,
          EmbeddedEventStore.Instance.Connection);
      }

      [Test]
      public void GetThrowsForUnknownId() {
        var exception =
          Assert.Throws<AggregateNotFoundException>(() => _sut.Get(_model.UnknownIdentifier));
        Assert.That(exception.Identifier, Is.EqualTo(_model.UnknownIdentifier));
        Assert.That(exception.Type, Is.EqualTo(typeof(AggregateRootEntityStub)));
      }

      [Test]
      public void GetReturnsRootOfKnownId() {
        var result = _sut.Get(_model.KnownIdentifier);

        Assert.That(result, Is.SameAs(_root));
      }

      [Test]
      public void GetOptionalReturnsEmptyForUnknownId() {
        var result = _sut.GetOptional(_model.UnknownIdentifier);

        Assert.That(result, Is.SameAs(Optional<AggregateRootEntityStub>.Empty));
      }

      [Test]
      public void GetOptionalReturnsRootForKnownId() {
        var result = _sut.GetOptional(_model.KnownIdentifier);

        Assert.That(result, Is.EqualTo(new Optional<AggregateRootEntityStub>(_root)));
      }

      [ProtoContract]
      class Event {}
    }
  }
}
