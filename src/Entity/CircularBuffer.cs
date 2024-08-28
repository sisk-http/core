// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
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
public sealed class CircularBuffer<T> : IEnumerable<T>, IReadOnlyList<T>
{
    private T[] items;

    int capacity = 0,
        addedItems = 0;

    /// <summary>
    /// Creates an new instance of the <see cref="CircularBuffer{T}"/> with the specified
    /// capacity.
    /// </summary>
    /// <param name="capacity">The circular buffer capacity.</param>
    public CircularBuffer(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException("Capacity cannot be less or equals to zero.");
        this.capacity = capacity;
        this.items = new T[capacity];
    }

    /// <summary>
    /// Adds an object of <typeparamref name="T"/> into the beginning of the circular buffer and pushes
    /// old items to end.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        lock (this.items)
        {
            for (int i = this.capacity - 1; i > 0; i--)
            {
                this.items[i] = this.items[i - 1];
            }

            this.addedItems = Math.Min(this.Capacity, this.addedItems + 1);
            this.items[0] = item;
        }
    }

    /// <summary>
    /// Clears the circular buffer contents and recreates the array.
    /// </summary>
    public void Clear()
    {
        this.items = new T[this.capacity];
        this.addedItems = 0;
    }

    /// <summary>
    /// Changes the capacity of elements in this circular buffer to the specified new size.
    /// </summary>
    /// <param name="capacity">The new size for this circular buffer.</param>
    public void Resize(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException("Capacity cannot be less or equals to zero.");
        Array.Resize(ref this.items, capacity);
        this.capacity = capacity;
    }

    /// <summary>
    /// Returns an array representation of this <see cref="CircularBuffer{T}"/> items in their defined
    /// positions within the capacity.
    /// </summary>
    public T[] ToArray() => this.items;

    /// <summary>
    /// Creates an new <see cref="ReadOnlySpan{T}"/> over the circular buffer.
    /// </summary>
    public ReadOnlySpan<T> ToSpan() => new ReadOnlySpan<T>(this.items);

    /// <summary>
    /// Gets the current capacity of this <see cref="CircularBuffer{T}"/>.
    /// </summary>
    public int Capacity { get => this.capacity; }

    /// <summary>
    /// Gets the amount of added items in this circular buffer.
    /// </summary>
    public int Count => this.addedItems;

    /// <inheritdoc/>
    public T this[int index] => this.items[index];

    /// <inheritdoc/>
    /// <exclude/>
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)this.items).GetEnumerator();
    }

    /// <inheritdoc/>
    /// <exclude/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.items.GetEnumerator();
    }
}