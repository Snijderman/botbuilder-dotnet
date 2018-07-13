﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class Transcript_MiddlewareTests
    {
        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Transcript_LogActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;

            await new TestFlow(adapter, async (context) =>
                {
                    conversationId = context.Activity.Conversation.Id;
                    var typingActivity = new TypingActivity
                    {
                        Type = ActivityTypes.Typing
                    };
                    await context.SendActivityAsync(typingActivity);
                    await Task.Delay(500);
                    await context.SendActivityAsync("echo:" + (context.Activity as MessageActivity).Text);
                })
                .Send("foo")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:foo")
                .Send("bar")
                    .AssertReply((activity) => Assert.AreEqual(activity.Type, ActivityTypes.Typing))
                    .AssertReply("echo:bar")
                .StartTestAsync();

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(6, pagedResult.Items.Length);
            Assert.AreEqual("foo", (pagedResult.Items[0] as MessageActivity).Text);
            Assert.IsNotNull(pagedResult.Items[1] as TypingActivity);
            Assert.AreEqual("echo:foo", (pagedResult.Items[2] as MessageActivity).Text);
            Assert.AreEqual("bar", (pagedResult.Items[3] as MessageActivity).Text);
            Assert.IsNotNull(pagedResult.Items[4] as TypingActivity);
            Assert.AreEqual("echo:bar", (pagedResult.Items[5] as MessageActivity).Text);
            foreach (var activity in pagedResult.Items)
            {
                Assert.IsTrue(!String.IsNullOrWhiteSpace(activity.Id));
                Assert.IsTrue(activity.Timestamp > default(DateTime));
            }
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Transcript_LogUpdateActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;
            var updateResponseActivity = new MessageUpdateActivity
            {
                Text = "new response"
            };

            await new TestFlow(adapter, async (context) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if ((context.Activity as MessageActivity)?.Text == "update")
                {
                    await context.UpdateActivityAsync(updateResponseActivity);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);

                    updateResponseActivity.Id = response.Id;
                    updateResponseActivity.ApplyConversationReference(activity.GetConversationReference());
                }
            })
                .Send("foo")
                .Send("update")
                    .AssertReply("new response")
                .StartTestAsync();

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", (pagedResult.Items[0] as MessageActivity).Text);
            Assert.AreEqual("response", (pagedResult.Items[1] as MessageActivity).Text);
            Assert.AreEqual("update", (pagedResult.Items[2] as MessageActivity).Text);
            Assert.AreEqual("new response", (pagedResult.Items[3] as MessageUpdateActivity).Text);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[3].Id);
        }

        [TestMethod]
        [TestCategory("Middleware")]
        public async Task Transcript_LogDeleteActivities()
        {
            var transcriptStore = new MemoryTranscriptStore();
            TestAdapter adapter = new TestAdapter()
                .Use(new TranscriptLoggerMiddleware(transcriptStore));
            string conversationId = null;
            string activityId = null;
            await new TestFlow(adapter, async (context) =>
            {
                conversationId = context.Activity.Conversation.Id;
                if ((context.Activity as MessageActivity).Text == "deleteIt")
                {
                    await context.DeleteActivityAsync(activityId);
                }
                else
                {
                    var activity = context.Activity.CreateReply("response");
                    var response = await context.SendActivityAsync(activity);
                    activityId = response.Id;
                }
            })
                .Send("foo")
                    .AssertReply("response")
                .Send("deleteIt")
                .StartTestAsync();

            var pagedResult = await transcriptStore.GetTranscriptActivitiesAsync("test", conversationId);
            Assert.AreEqual(4, pagedResult.Items.Length);
            Assert.AreEqual("foo", (pagedResult.Items[0] as MessageActivity).Text);
            Assert.AreEqual("response", (pagedResult.Items[1] as MessageActivity).Text);
            Assert.AreEqual("deleteIt", (pagedResult.Items[2] as MessageActivity).Text);
            Assert.AreEqual(ActivityTypes.MessageDelete, pagedResult.Items[3].Type);
            Assert.AreEqual(pagedResult.Items[1].Id, pagedResult.Items[3].Id);
        }
    }
}
