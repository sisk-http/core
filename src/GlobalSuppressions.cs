// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   GlobalSuppressions.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage ( "Usage", "CA2225:Operator overloads have named alternates", Scope = "member", Target = "~M:Sisk.Core.Http.HttpStatusInformation.op_Implicit(System.Net.HttpStatusCode)~Sisk.Core.Http.HttpStatusInformation" )]
[assembly: SuppressMessage ( "Usage", "CA2225:Operator overloads have named alternates", Scope = "member", Target = "~M:Sisk.Core.Http.HttpStatusInformation.op_Implicit(System.Int32)~Sisk.Core.Http.HttpStatusInformation" )]
[assembly: SuppressMessage ( "Json", "SYSLIB0020:JsonSerializerOptions.IgnoreNullValues is obsolete" )]