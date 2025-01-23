// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpAction.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Cadente;

/// <summary>
/// Represents a method that takes an <see cref="HttpSession"/> as a parameter and does not return a value.
/// </summary>
/// <param name="session">The HTTP session associated with the action.</param>
public delegate Task HttpAction ( HttpSession session );