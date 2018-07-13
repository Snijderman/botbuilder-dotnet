﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Represents a store for recording conversations.
    /// </summary>
    public interface ITranscriptStore : ITranscriptLogger
    {
        /// <summary>
        /// Gets from the store activities that match a set of criteria.
        /// </summary>
        /// <param name="channelId">The ID of the channel the conversation is in.</param>
        /// <param name="conversationId">The ID of the conversation.</param>
        /// <param name="continuationToken"></param>
        /// <param name="startDate">A cutoff date. Activities older than this date are not included.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If the task completes successfully, the result contains the matching activities.</remarks>
        Task<PagedResult<Activity>> GetTranscriptActivitiesAsync(string channelId, string conversationId, string continuationToken = null, DateTime startDate = default(DateTime));

        /// <summary>
        /// Gets the conversations on a channel from the store.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="continuationToken"></param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks></remarks>
        Task<PagedResult<Transcript>> ListTranscriptsAsync(string channelId, string continuationToken = null);

        /// <summary>
        /// Deletes conversation data from the store.
        /// </summary>
        /// <param name="channelId">The ID of the channel the conversation is in.</param>
        /// <param name="conversationId">The ID of the conversation to delete.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task DeleteTranscriptAsync(string channelId, string conversationId);
    }
}
