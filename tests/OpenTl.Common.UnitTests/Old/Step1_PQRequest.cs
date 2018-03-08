﻿using System;
using System.Collections.Generic;
using System.IO;

namespace OpenTl.Common.UnitTests.Old
{
    public class Step1Response
    {
        public byte[] Nonce { get; set; }

        public byte[] ServerNonce { get; set; }

        public List<byte[]> Fingerprints { get; set; }
    }

    public class Step1PqRequest
    {
        private readonly byte[] _nonce;

        public Step1PqRequest()
        {
            _nonce = new byte[16];
        }

//        public Step1Response FromBytes(byte[] bytes)
//        {
//            var fingerprints = new List<byte[]>();
//
//            using (var memoryStream = new MemoryStream(bytes, false))
//            {
//                using (var binaryReader = new BinaryReader(memoryStream))
//                {
//                    const int responseConstructorNumber = 0x05162463;
//                    var responseCode = binaryReader.ReadInt32();
//                    if (responseCode != responseConstructorNumber)
//                    {
//                        throw new InvalidOperationException($"invalid response code: {responseCode}");
//                    }
//
//                    var nonceFromServer = binaryReader.ReadBytes(16);
//
//                    if (!nonceFromServer.SequenceEqual(_nonce))
//                    {
//                        throw new InvalidOperationException("invalid nonce from server");
//                    }
//
//                    var serverNonce = binaryReader.ReadBytes(16);
//
//                    var pqbytes = Serializers.Bytes.Read(binaryReader);
//                    var pq = new BigInteger(1, pqbytes);
//
//                    var vectorId = binaryReader.ReadInt32();
//                    const int vectorConstructorNumber = 0x1cb5c415;
//                    if (vectorId != vectorConstructorNumber)
//                    {
//                        throw new InvalidOperationException($"Invalid vector constructor number {vectorId}");
//                    }
//
//                    var fingerprintCount = binaryReader.ReadInt32();
//                    for (var i = 0; i < fingerprintCount; i++)
//                    {
//                        var fingerprint = binaryReader.ReadBytes(8);
//                        fingerprints.Add(fingerprint);
//                    }
//
//                    return new Step1Response
//                           {
//                               Fingerprints = fingerprints,
//                               Nonce = _nonce,
//                               Pq = pq,
//                               ServerNonce = serverNonce
//                           };
//                }
//            }
//        }

        public byte[] ToBytes()
        {
            new Random().NextBytes(_nonce);
            const int ConstructorNumber = 0x60469778;

            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(ConstructorNumber);
                    binaryWriter.Write(_nonce);
                }

                return memoryStream.ToArray();
            }
        }
    }
}