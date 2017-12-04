﻿#region License
// Copyright (C) 2010-2016 Devexperts LLC
//
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at
// http://mozilla.org/MPL/2.0/.
#endregion

using System;

namespace com.dxfeed.api.events
{
    /// <summary>
    ///     Represents time-series snapshots of some process that is evolving in time or actual 
    ///     events in some external system that have an associated time stamp and can be uniquely 
    ///     identified.
    ///     For example, <see cref="IDxTimeAndSale"/> events represent the actual sales that 
    ///     happen on a market exchange at specific time moments, while <see cref="IDxCandle"/> 
    ///     events represent snapshots of aggregate information about trading over a specific time 
    ///     period.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Time-series events can be used with <see cref="IDXFeedTimeSeriesSubscription{E}"/>
    ///         to receive a time-series history of past events. Time-series events
    ///         are conflated based on unique <see cref="IndexedEvent.Index"/> for each symbol.
    ///         Last indexed event for each symbol and index is always
    ///         delivered to event listeners on subscription, but intermediate (next-to-last) 
    ///         events for each symbol+index pair are not queued anywhere, they are simply 
    ///         discarded as stale events.
    ///     </para>
    ///     <para>
    ///         Timestamp of an event is available via <see cref="TimeSeriesEvent.TimeStamp"/> 
    ///         property with a millisecond precision. Some events may happen multiple times per 
    ///         millisecond.
    ///         In this case they have the same <see cref="TimeSeriesEvent.TimeStamp"/>, but 
    ///         different <see cref="IndexedEvent.Index"/>. An ordering of 
    ///         <see cref="IndexedEvent.Index"/> is the same as an ordering of 
    ///         <see cref="TimeSeriesEvent.TimeStamp"/>. That is, an collection of time-series
    ///         events that is ordered by <see cref="IndexedEvent.Index"/> is ascending order will 
    ///         be also ordered by <see cref="TimeSeriesEvent.TimeStamp"/> in ascending order.
    ///     </para>
    ///     <para>
    ///         Time-series events are a more specific subtype of <see cref="IndexedEvent"/>.
    ///         All general documentation and Event Flags section, in particular,
    ///         applies to time-series events. However, the time-series events never come from 
    ///         multiple sources for the same symbol and their <see cref="IndexedEvent.Source"/> 
    ///         is always <see cref="IndexedEventSource.DEFAULT"/>.
    ///     </para>
    ///     <para>
    ///         Unlike a general <see cref="IndexedEvent"/> that is subscribed to via 
    ///         <see cref="IDXFeedSubscription{E}"/> using a plain symbol to receive all events 
    ///         for all indices, time-series events are typically subscribed to using
    ///         {@link TimeSeriesSubscriptionSymbol} class to specific time range of the 
    ///         subscription. There is a dedicated <see cref="IDXFeedTimeSeriesSubscription{E}"/> 
    ///         class that is designed to simplify the task of subscribing for time-series events.
    ///     </para>
    ///     <para>
    ///         {@link TimeSeriesEventModel} class handles all the snapshot and transaction logic 
    ///         and conveniently represents a list of current time-series events ordered by their 
    ///         <see cref="TimeSeriesEvent.TimeStamp"/>.
    ///         It relies on the code of {@link AbstractIndexedEventModel} to handle this logic.
    ///         Use the source code of {@link AbstractIndexedEventModel} for clarification on 
    ///         transactions and snapshot logic.
    ///     </para>
    ///     <para>
    ///         Classes that implement this interface may also implement
    ///         <see cref="LastingEvent"/> interface, which makes it possible to
    ///         use {@link DXFeed#getLastEvent(LastingEvent) DXFeed.getLastEvent} method to
    ///         retrieve the last event for the corresponding symbol.
    ///     </para>
    ///     <para>
    ///         Publishing time-series
    ///     </para>
    ///     <para>
    ///         When publishing time-series event with {@link DXPublisher#publishEvents(Collection) DXPublisher.publishEvents}
    ///         method on incoming {@link TimeSeriesSubscriptionSymbol} the snapshot of currently known events for the
    ///         requested time range has to be published first.
    ///     </para>
    ///     <para>
    ///         A snapshot is published in the descending order of <see cref="IndexedEvent.Index"/>
    ///         (which is the same as the descending order of 
    ///         <see cref="TimeSeriesEvent.TimeStamp"/>), starting with
    ///         an event with the largest index and marking it with <see cref="EventFlag.SnapshotBegin"/> 
    ///         bit in <see cref="IndexedEvent.EventFlags"/>.
    ///         All other event follow with default, zero <see cref="IndexedEvent.EventFlags"/>.
    ///         If there is no actual event at the time that was specified in subscription
    ///         <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> property, then event 
    ///         with the corresponding time has to be created anyway and published. To distinguish 
    ///         it from the actual event, it has to be marked with <see cref="EventFlag.RemoveEvent"/> 
    ///         bit in <see cref="IndexedEvent.EventFlags"/>.
    ///         <see cref="EventFlag.SnapshotEnd"/> bit in this event's 
    ///         <see cref="IndexedEvent.EventFlags"/> is optional during publishing. It will be 
    ///         properly set on receiving end anyway. Note, that publishing any event with time 
    ///         that is below subscription <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> 
    ///         also works as a legal indicator for the end of the snapshot.
    ///     </para>
    ///     <para>
    ///         Both <see cref="IDxTimeAndSale"/> and <see cref="IDxCandle"/> time-series events 
    ///         define a sequence property that is mixed into an <see cref="IndexedEvent.Index"/> 
    ///         property. It provides a way to distinguish different events at the same time.
    ///         For a snapshot end event the sequence has to be left at its default zero value. It 
    ///         means, that if there is an actual event to be published at subscription 
    ///         <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> with non-zero 
    ///         sequence, then generation of an additional snapshot end event with the subscription
    ///         <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> and zero sequence is 
    ///         still required.
    ///     </para>
    /// </remarks>
    public interface TimeSeriesEvent : IndexedEvent
    {
        /// <summary>
        /// Returns timestamp of this event.
        /// The timestamp is in milliseconds from midnight, January 1, 1970 UTC.
        /// </summary>
        long TimeStamp { get; }

        /// <summary>
        /// Returns UTC date and time of this event.
        /// </summary>
        DateTime Time { get; }
    }

    /// <summary>
    ///     Represents time-series snapshots of some process that is evolving in time or actual 
    ///     events in some external system that have an associated time stamp and can be uniquely 
    ///     identified.
    ///     For example, <see cref="IDxTimeAndSale"/> events represent the actual sales that 
    ///     happen on a market exchange at specific time moments, while <see cref="IDxCandle"/> 
    ///     events represent snapshots of aggregate information about trading over a specific time 
    ///     period.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Time-series events can be used with <see cref="IDXFeedTimeSeriesSubscription{E}"/>
    ///         to receive a time-series history of past events. Time-series events
    ///         are conflated based on unique <see cref="IndexedEvent.Index"/> for each symbol.
    ///         Last indexed event for each symbol and index is always
    ///         delivered to event listeners on subscription, but intermediate (next-to-last) 
    ///         events for each symbol+index pair are not queued anywhere, they are simply 
    ///         discarded as stale events.
    ///     </para>
    ///     <para>
    ///         Timestamp of an event is available via <see cref="TimeSeriesEvent.TimeStamp"/> 
    ///         property with a millisecond precision. Some events may happen multiple times per 
    ///         millisecond.
    ///         In this case they have the same <see cref="TimeSeriesEvent.TimeStamp"/>, but 
    ///         different <see cref="IndexedEvent.Index"/>. An ordering of 
    ///         <see cref="IndexedEvent.Index"/> is the same as an ordering of 
    ///         <see cref="TimeSeriesEvent.TimeStamp"/>. That is, an collection of time-series
    ///         events that is ordered by <see cref="IndexedEvent.Index"/> is ascending order will 
    ///         be also ordered by <see cref="TimeSeriesEvent.TimeStamp"/> in ascending order.
    ///     </para>
    ///     <para>
    ///         Time-series events are a more specific subtype of <see cref="IndexedEvent"/>.
    ///         All general documentation and Event Flags section, in particular,
    ///         applies to time-series events. However, the time-series events never come from 
    ///         multiple sources for the same symbol and their <see cref="IndexedEvent.Source"/> 
    ///         is always <see cref="IndexedEventSource.DEFAULT"/>.
    ///     </para>
    ///     <para>
    ///         Unlike a general <see cref="IndexedEvent"/> that is subscribed to via 
    ///         <see cref="IDXFeedSubscription{E}"/> using a plain symbol to receive all events 
    ///         for all indices, time-series events are typically subscribed to using
    ///         {@link TimeSeriesSubscriptionSymbol} class to specific time range of the 
    ///         subscription. There is a dedicated <see cref="IDXFeedTimeSeriesSubscription{E}"/> 
    ///         class that is designed to simplify the task of subscribing for time-series events.
    ///     </para>
    ///     <para>
    ///         {@link TimeSeriesEventModel} class handles all the snapshot and transaction logic 
    ///         and conveniently represents a list of current time-series events ordered by their 
    ///         <see cref="TimeSeriesEvent.TimeStamp"/>.
    ///         It relies on the code of {@link AbstractIndexedEventModel} to handle this logic.
    ///         Use the source code of {@link AbstractIndexedEventModel} for clarification on 
    ///         transactions and snapshot logic.
    ///     </para>
    ///     <para>
    ///         Classes that implement this interface may also implement
    ///         <see cref="LastingEvent"/> interface, which makes it possible to
    ///         use {@link DXFeed#getLastEvent(LastingEvent) DXFeed.getLastEvent} method to
    ///         retrieve the last event for the corresponding symbol.
    ///     </para>
    ///     <para>
    ///         Publishing time-series
    ///     </para>
    ///     <para>
    ///         When publishing time-series event with {@link DXPublisher#publishEvents(Collection) DXPublisher.publishEvents}
    ///         method on incoming {@link TimeSeriesSubscriptionSymbol} the snapshot of currently known events for the
    ///         requested time range has to be published first.
    ///     </para>
    ///     <para>
    ///         A snapshot is published in the descending order of <see cref="IndexedEvent.Index"/>
    ///         (which is the same as the descending order of 
    ///         <see cref="TimeSeriesEvent.TimeStamp"/>), starting with
    ///         an event with the largest index and marking it with <see cref="EventFlag.SnapshotBegin"/> 
    ///         bit in <see cref="IndexedEvent.EventFlags"/>.
    ///         All other event follow with default, zero <see cref="IndexedEvent.EventFlags"/>.
    ///         If there is no actual event at the time that was specified in subscription
    ///         <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> property, then event 
    ///         with the corresponding time has to be created anyway and published. To distinguish 
    ///         it from the actual event, it has to be marked with <see cref="EventFlag.RemoveEvent"/> 
    ///         bit in <see cref="IndexedEvent.EventFlags"/>.
    ///         <see cref="EventFlag.SnapshotEnd"/> bit in this event's 
    ///         <see cref="IndexedEvent.EventFlags"/> is optional during publishing. It will be 
    ///         properly set on receiving end anyway. Note, that publishing any event with time 
    ///         that is below subscription <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> 
    ///         also works as a legal indicator for the end of the snapshot.
    ///     </para>
    ///     <para>
    ///         Both <see cref="IDxTimeAndSale"/> and <see cref="IDxCandle"/> time-series events 
    ///         define a sequence property that is mixed into an <see cref="IndexedEvent.Index"/> 
    ///         property. It provides a way to distinguish different events at the same time.
    ///         For a snapshot end event the sequence has to be left at its default zero value. It 
    ///         means, that if there is an actual event to be published at subscription 
    ///         <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> with non-zero 
    ///         sequence, then generation of an additional snapshot end event with the subscription
    ///         <see cref="IDXFeedTimeSeriesSubscription{E}.FromTimeStamp"/> and zero sequence is 
    ///         still required.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">Type of the event symbol for this event type.</typeparam>
    public interface TimeSeriesEvent<T> : TimeSeriesEvent, IndexedEvent<T>
    {
        //Note: no-extra fields here
    }
}