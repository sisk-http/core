// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CertificateHelper.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides a set of useful functions to issue self-signed development certificates.
/// </summary>
public static class CertificateHelper {
    private const string PfxPassword = "sisk";

    // -> https://github.com/dotnet/corefx/blob/a10890f4ffe0fadf090c922578ba0e606ebdd16c/src/Common/src/System/Text/StringOrCharArray.cs#L140
    // we need to make sure each hashcode for each string is the same.
    static int GetDeterministicHashCode ( string str ) {
        str = str.ToLowerInvariant ();
        unchecked {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2) {
                hash1 = ((hash1 << 5) + hash1) ^ str [ i ];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str [ i + 1 ];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    static int ComputeArrayHash ( string [] array ) {
        int i = 3321581;
        for (int j = 0; j < array.Length; j++) {
            i ^= GetDeterministicHashCode ( array [ j ] );
            i *= j + 1;
        }
        return Math.Abs ( i );
    }

    /// <summary>
    /// Creates a self-signed certificate for the specified DNS names and adds them
    /// to the local user's certificate store.
    /// </summary>
    /// <param name="dnsNames">The certificate DNS names.</param>
    public static X509Certificate2 CreateTrustedDevelopmentCertificate ( params string [] dnsNames ) {
        int dnsHash = ComputeArrayHash ( dnsNames );
        string basePath = Path.Combine (
            Environment.GetFolderPath ( Environment.SpecialFolder.LocalApplicationData ),
            ".sisk", "development-certs" );

        Directory.CreateDirectory ( basePath );

        string fileName = $"SiskDevelopment_{dnsHash}.pfx";
        string pfxPath = Path.Combine ( basePath, fileName );

        X509Certificate2 certificate;

        if (File.Exists ( pfxPath )) {
            certificate = LoadPfxFromDisk ( pfxPath );
        }
        else {
            using var fresh = CreateDevelopmentCertificate ( dnsNames );

            File.WriteAllBytes (
                pfxPath,
                fresh.Export ( X509ContentType.Pfx, PfxPassword ) );

            certificate = LoadPfxFromDisk ( pfxPath );
        }

        EnsureTrusted ( certificate );

        return certificate;
    }

    private static X509Certificate2 LoadPfxFromDisk ( string path ) {
#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadPkcs12 (
            File.ReadAllBytes ( path ),
            PfxPassword,
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.UserKeySet );
#else
        return new X509Certificate2 (
            path,
            PfxPassword,
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.UserKeySet );
#endif
    }

    private static void EnsureTrusted ( X509Certificate2 certificate ) {
        // deixa a confiança local (TrustedPeople) ou raiz (Root)
        using var trusted = new X509Store ( StoreName.Root, StoreLocation.CurrentUser );
        trusted.Open ( OpenFlags.ReadWrite );
        if (trusted.Certificates.Cast<X509Certificate2> ().FirstOrDefault ( c => c.Thumbprint == certificate.Thumbprint ) is null) {
            trusted.Add ( certificate );
        }
    }

    /// <summary>
    /// Creates a self-signed certificate for the specified DNS names.
    /// </summary>
    /// <param name="dnsNames">The certificate DNS names.</param>
    public static X509Certificate2 CreateDevelopmentCertificate ( params string [] dnsNames ) {
        if (dnsNames.Length == 0)
            throw new ArgumentException ( "At least one DNS name must be specified.", nameof ( dnsNames ) );

        var sanBuilder = new SubjectAlternativeNameBuilder ();
        sanBuilder.AddIpAddress ( IPAddress.Loopback );
        sanBuilder.AddIpAddress ( IPAddress.IPv6Loopback );

        foreach (string dnsName in dnsNames.Distinct ( StringComparer.OrdinalIgnoreCase ))
            sanBuilder.AddDnsName ( dnsName.ToLowerInvariant () );

        var distinguishedName = new X500DistinguishedName (
            $"CN = Sisk Development CA #{ComputeArrayHash ( dnsNames )},OU = IT,O = Sao Paulo,L = Brazil,S = Sao Paulo,C = Brazil" );

        using RSA rsa = RSA.Create ( 2048 );
        var request = new CertificateRequest ( distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1 );

        request.CertificateExtensions.Add (
            new X509EnhancedKeyUsageExtension ( new OidCollection { new Oid ( "1.3.6.1.5.5.7.3.1" ) }, false ) );
        request.CertificateExtensions.Add ( sanBuilder.Build () );

        using var certificate = request.CreateSelfSigned (
            DateTimeOffset.UtcNow.AddDays ( -1 ),
            DateTimeOffset.UtcNow.AddYears ( 10 ) );

        var pfxBytes = certificate.Export ( X509ContentType.Pfx, PfxPassword );

#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadPkcs12 (
            pfxBytes,
            PfxPassword,
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.UserKeySet );
#else
        return new X509Certificate2(
            pfxBytes,
            PfxPassword,
            X509KeyStorageFlags.Exportable |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.UserKeySet);
#endif
    }
}