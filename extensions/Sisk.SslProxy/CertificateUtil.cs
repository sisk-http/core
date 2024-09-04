// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CertificateUtil.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Sisk.Ssl;

/// <summary>
/// Provides a set of useful functions to issue development certificates for the <see cref="SslProxy"/>.
/// </summary>
public static class CertificateUtil
{
    // -> https://github.com/dotnet/corefx/blob/a10890f4ffe0fadf090c922578ba0e606ebdd16c/src/Common/src/System/Text/StringOrCharArray.cs#L140
    // we need to make sure each hashcode for each string is the same.
    static int GetDeterministicHashCode(string str)
    {
        str = str.ToLower();
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    static int ComputeArrayHash(string[] array)
    {
        int i = array.GetHashCode();
        for (int j = 0; j < array.Length; j++)
        {
            i ^= GetDeterministicHashCode(array[j]);
        }
        return Math.Abs(i);
    }

    static string GetIssuerName(string[] dnsNames)
    {
        int hash = ComputeArrayHash(dnsNames);
        return $"CN = Sisk Development CA {hash},OU = IT,O = Sao Paulo,L = Brazil,S = Sao Paulo,C = Brazil";
    }

    /// <summary>
    /// Creates a self-signed certificate for the specified DNS names and adds them
    /// to the local user's certificate store.
    /// </summary>
    /// <param name="dnsNames">The certificate DNS names.</param>
    public static X509Certificate2 CreateTrustedDevelopmentCertificate(string[] dnsNames)
    {
        X509Certificate2 x509Certificate2;
        using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
        {
            store.Open(OpenFlags.ReadWrite);

            var siskCert = store.Certificates.FirstOrDefault(c => c.Issuer.Contains($"Sisk Development CA {ComputeArrayHash(dnsNames)}"));
            if (siskCert is null)
            {
                x509Certificate2 = CreateDevelopmentCertificate(dnsNames);
                store.Add(x509Certificate2);
            }
            else
            {
                x509Certificate2 = siskCert;
            }
        }
        return x509Certificate2;
    }

    /// <summary>
    /// Creates a self-signed certificate for the specified DNS names.
    /// </summary>
    /// <param name="dnsNames">The certificate DNS names.</param>
    public static X509Certificate2 CreateDevelopmentCertificate(string[] dnsNames)
    {
        SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);

        foreach (string dnsName in dnsNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            sanBuilder.AddDnsName(dnsName.ToLower());
        }

        X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN = Sisk Development CA {ComputeArrayHash(dnsNames)},OU = IT,O = Sao Paulo,L = Brazil,S = Sao Paulo,C = Brazil");

        using (RSA rsa = RSA.Create(2048))
        {
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));
            request.CertificateExtensions.Add(
                sanBuilder.Build());

            var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
            return new X509Certificate2(certificate.Export(X509ContentType.Pfx, "sisk"), "sisk", X509KeyStorageFlags.MachineKeySet);
        }
    }
}
