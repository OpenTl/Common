﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace OpenTl.Common.UnitTests.Old.MTProto.Crypto
{
    public class AuthKey
    {
        private readonly ulong _auxHash;

        public byte[] Data { get; }

        public ulong Id { get; }

        public AuthKey(BigInteger gab)
        {
            Data = gab.ToByteArrayUnsigned();
            using (var hash = SHA1.Create())
            {
                using (var hashStream = new MemoryStream(hash.ComputeHash(Data), false))
                {
                    using (var hashReader = new BinaryReader(hashStream))
                    {
                        _auxHash = hashReader.ReadUInt64();
                        hashReader.ReadBytes(4);
                        Id = hashReader.ReadUInt64();
                    }
                }
            }
        }

        public byte[] CalcNewNonceHash(byte[] newNonce, int number)
        {
            using (var stream = new MemoryStream(100))
            {
                using (var bufferWriter = new BinaryWriter(stream))
                {
                    bufferWriter.Write(newNonce);
                    bufferWriter.Write((byte)number);
                    bufferWriter.Write(_auxHash);
                    using (var sha1 = SHA1.Create())
                    {
                        stream.TryGetBuffer(out var buffer);

                        var hash = sha1.ComputeHash(buffer.Array, 0, (int)stream.Position);
                        var newNonceHash = new byte[16];
                        Array.Copy(hash, 4, newNonceHash, 0, 16);
                        return newNonceHash;
                    }
                }
            }
        }
    }
}