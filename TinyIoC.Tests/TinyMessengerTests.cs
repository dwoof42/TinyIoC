﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyIoC.Tests.TestData;
using TinyMessenger;

namespace TinyIoC.Tests
{

    [TestClass]
    public class TinyMessengerTests
    {
        [TestMethod]
        public void TinyMessenger_Ctor_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
        }

        [TestMethod]
        public void Subscribe_ValidDeliverAction_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>));
        }

        [TestMethod]
        public void SubScribe_ValidDeliveryAction_ReturnsRegistrationObject()
        {
            var messenger = UtilityMethods.GetMessenger();

            var output = messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>));

            Assert.IsInstanceOfType(output, typeof(TinyMessageSubscription));
        }

        [TestMethod]
        public void Subscribe_ValidDeliverActionWIthStrongReferences_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), true);
        }

        [TestMethod]
        public void Subscribe_ValidDeliveryActionAndFilter_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Subscribe_NullDeliveryAction_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(null, new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Subscribe_NullFilter_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Unsubscribe_NullSubscriptionObject_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Unsubscribe<TestMessage>(null);
        }

        [TestMethod]
        public void Unsubscribe_PreviousSubscription_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
            var subscription = messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));

            messenger.Unsubscribe<TestMessage>(subscription);
        }

        [TestMethod]
        public void Subscribe_PreviousSubscription_ReturnsDifferentSubscriptionObject()
        {
            var messenger = UtilityMethods.GetMessenger();
            var sub1 = messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));
            var sub2 = messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));

            Assert.IsFalse(object.ReferenceEquals(sub1, sub2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Publish_NullMessage_Throws()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Publish<TestMessage>(null);
        }

        [TestMethod]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();

            messenger.Publish<TestMessage>(new TestMessage(this));
        }

        [TestMethod]
        public void Publish_Subscriber_DoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
            messenger.Subscribe<TestMessage>(new Action<TestMessage>(UtilityMethods.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(UtilityMethods.FakeMessageFilter<TestMessage>));

            messenger.Publish<TestMessage>(new TestMessage(this));
        }

        [TestMethod]
        public void Publish_SubscribedMessageNoFilter_GetsMessage()
        {
            var messenger = UtilityMethods.GetMessenger();
            bool received = false;
            messenger.Subscribe<TestMessage>((m) => { received = true; });

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsTrue(received);
        }

        [TestMethod]
        public void Publish_SubscribedThenUnsubscribedMessageNoFilter_DoesNotGetMessage()
        {
            var messenger = UtilityMethods.GetMessenger();
            bool received = false;
            var token = messenger.Subscribe<TestMessage>((m) => { received = true; });
            messenger.Unsubscribe<TestMessage>(token);

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsFalse(received);
        }

        [TestMethod]
        public void Publish_SubscribedMessageButFiltered_DoesNotGetMessage()
        {
            var messenger = UtilityMethods.GetMessenger();
            bool received = false;
            messenger.Subscribe<TestMessage>((m) => { received = true; }, (m) => false);

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsFalse(received);
        }

        [TestMethod]
        public void Publish_SubscribedMessageNoFilter_GetsActualMessage()
        {
            var messenger = UtilityMethods.GetMessenger();
            ITinyMessage receivedMessage = null;
            var payload = new TestMessage(this);
            messenger.Subscribe<TestMessage>((m) => { receivedMessage = m; });

            messenger.Publish<TestMessage>(payload);

            Assert.ReferenceEquals(payload, receivedMessage);
        }

        [TestMethod]
        public void GenericTinyMessage_String_SubscribeDoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
            var output = string.Empty;
            messenger.Subscribe<GenericTinyMessage<string>>((m) => { output = m._Content; });
        }

        [TestMethod]
        public void GenericTinyMessage_String_PubishDoesNotThrow()
        {
            var messenger = UtilityMethods.GetMessenger();
            messenger.Publish(new GenericTinyMessage<string>(this, "Testing"));
        }

        [TestMethod]
        public void GenericTinyMessage_String_PubishAndSubscribeDeliversContent()
        {
            var messenger = UtilityMethods.GetMessenger();
            var output = string.Empty;
            messenger.Subscribe<GenericTinyMessage<string>>((m) => { output = m._Content; });
            messenger.Publish(new GenericTinyMessage<string>(this, "Testing"));

            Assert.AreEqual("Testing", output);
        }
    }
}