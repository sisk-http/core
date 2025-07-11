﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CircularBuffer.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an thread-safe, fixed-capacity circular buffer that stores elements of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
/// <exclude/>
public sealed class CircularBuffer<T> : IEnumerable<T>, IReadOnlyList<T> {
    private T [] items;

    int capacity,
        addedItems;

    /// <summary>
    /// Creates an new instance of the <see cref="CircularBuffer{T}"/> with the specified
    /// capacity.
    /// </summary>
    /// <param name="capacity">The circular buffer capacity.</param>
    public CircularBuffer ( int capacity ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( capacity );

        this.capacity = capacity;
        items = new T [ capacity ];
    }

    /// <summary>
    /// Adds an object of <typeparamref name="T"/> into the beginning of the circular buffer and pushes
    /// old items to end.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add ( T item ) {
        lock (items) {
            for (int i = capacity - 1; i > 0; i--) {
                items [ i ] = items [ i - 1 ];
            }

            addedItems = Math.Min ( Capacity, addedItems + 1 );
            items [ 0 ] = item;
        }
    }

    /// <summary>
    /// Clears the circular buffer contents and recreates the array.
    /// </summary>
    public void Clear () {
        items = new T [ capacity ];
        addedItems = 0;
    }

    /// <summary>
    /// Changes the capacity of elements in this circular buffer to the specified new size.
    /// </summary>
    /// <param name="capacity">The new size for this circular buffer.</param>
    public void Resize ( int capacity ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( capacity );

        Array.Resize ( ref items, capacity );
        this.capacity = capacity;
    }

    /// <summary>
    /// Returns an array representation of this <see cref="CircularBuffer{T}"/> items in their defined
    /// positions within the capacity.
    /// </summary>
    public T [] ToArray () => items [ 0..addedItems ];

    /// <summary>
    /// Creates an new <see cref="ReadOnlySpan{T}"/> over the circular buffer.
    /// </summary>
    public ReadOnlySpan<T> ToSpan () => new ReadOnlySpan<T> ( items );

    /// <summary>
    /// Gets the current capacity of this <see cref="CircularBuffer{T}"/>.
    /// </summary>
    public int Capacity { get => capacity; }

    /// <summary>
    /// Gets the amount of added items in this circular buffer.
    /// </summary>
    public int Count => addedItems;

    /// <inheritdoc/>
    public T this [ int index ] => items [ index ];

    /// <inheritdoc/>
    /// <exclude/>
    public IEnumerator<T> GetEnumerator () {
        return ((IEnumerable<T>) items).GetEnumerator ();
    }

    /// <inheritdoc/>
    /// <exclude/>
    IEnumerator IEnumerable.GetEnumerator () {
        return items.GetEnumerator ();
    }
}