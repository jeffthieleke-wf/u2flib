﻿/*
 * Code was copied from java example so there is a .NET version 
 * to work with.
 * 
 * Copyright 2014 Yubico.
 * Copyright 2014 Google Inc. All rights reserved.
 *
 * Use of this source code is governed by a BSD-style
 * license that can be found in the LICENSE file or at
 * https://developers.google.com/open-source/licenses/bsd
 */

using System;
using System.Linq;
using Newtonsoft.Json;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using u2flib.Exceptions;

namespace u2flib.Data
{
    public class DeviceRegistration : DataObject
    {
        private long serialVersionUID = -142942195464329902L;
        public static uint INITIAL_COUNTER_VALUE = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceRegistration"/> class.
        /// </summary>
        /// <param name="keyHandle">The key handle.</param>
        /// <param name="publicKey">The public key.</param>
        /// <param name="attestationCert">The attestation cert.</param>
        /// <param name="counter">The counter.</param>
        /// <exception cref="U2fException">Invalid attestation certificate</exception>
        public DeviceRegistration(byte[] keyHandle, byte[] publicKey, byte[] attestationCert, uint counter)
        {
            KeyHandle = keyHandle;
            PublicKey = publicKey;
            Counter = counter;

            try
            {
                AttestationCert = attestationCert;
            }
            catch (CertificateEncodingException e)
            {
                throw new U2fException("Invalid attestation certificate", e);
            }
        }

        /// <summary>
        /// Gets the key handle.
        /// </summary>
        /// <value>
        /// The key handle.
        /// </value>
        public byte[] KeyHandle { get; private set; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <value>
        /// The public key.
        /// </value>
        public byte[] PublicKey { get; private set; }

        /// <summary>
        /// Gets the attestation cert.
        /// </summary>
        /// <value>
        /// The attestation cert.
        /// </value>
        public byte[] AttestationCert { get; private set; }

        /// <summary>
        /// Usage counter from the device, this should be incremented by 1 every use.
        /// </summary>
        /// <value>
        /// Number of device uses.
        /// </value>
        public uint Counter { get; private set; }

        public X509Certificate GetAttestationCertificate()
        {
            X509CertificateParser x509CertificateParser = new X509CertificateParser();

            return x509CertificateParser.ReadCertificate(AttestationCert);
        }

        /// <summary>
        /// Froms the json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static DeviceRegistration FromJson(String json)
        {
            return JsonConvert.DeserializeObject<DeviceRegistration>(json);
        }

        /// <summary>
        /// To the json with out attestion cert.
        /// </summary>
        /// <returns></returns>
        public String ToJsonWithOutAttestionCert()
        {
            return JsonConvert.SerializeObject(new DeviceWithoutCertificate(KeyHandle, PublicKey, Counter));
        }

        /// <summary>
        /// Checks the and increment counter.
        /// </summary>
        /// <param name="clientCounter">The client counter.</param>
        /// <exception cref="U2fException">Counter value smaller than expected!</exception>
        public void CheckAndUpdateCounter(uint clientCounter)
        {
            if (clientCounter <= Counter)
            {
                throw new U2fException("Counter value smaller than expected!");
            }
            Counter = clientCounter;
        }
        
        public override int GetHashCode()
        {
            int hash = PublicKey.Sum(b => b + 31);
            hash += AttestationCert.Sum(b => b + 31);
            hash += KeyHandle.Sum(b => b + 31);

            return hash;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is DeviceRegistration))
            {
                return false;
            }
            DeviceRegistration that = (DeviceRegistration)obj;
            return Arrays.AreEqual(KeyHandle, that.KeyHandle)
                   && Arrays.AreEqual(PublicKey, that.PublicKey)
                   && Arrays.AreEqual(AttestationCert, that.AttestationCert);
        }
    }

    internal class DeviceWithoutCertificate
    {
        internal DeviceWithoutCertificate(byte[] keyHandle, byte[] publicKey, uint counter)
        {
            KeyHandle = keyHandle;
            PublicKey = publicKey;
            Counter = counter;
        }

        public byte[] PublicKey { get; private set; }

        public byte[] KeyHandle { get; private set; }

        public uint Counter { get; private set; }
    }
}