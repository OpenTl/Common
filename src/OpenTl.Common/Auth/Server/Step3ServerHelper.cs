﻿namespace OpenTl.Common.Auth.Server
{
    using System.Collections;
    using System.Linq;

    using BarsGroup.CodeGuard;

    using OpenTl.Common.Crypto;
    using OpenTl.Schema;
    using OpenTl.Schema.Serialization;

    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Security;
    
    using MoreLinq;

    using OpenTl.Common.Extesions;
    using OpenTl.Common.GuardExtensions;

    public static class Step3ServerHelper
    {
        public static ISetClientDHParamsAnswer GetResponse(RequestSetClientDHParams setClientDhParams, byte[] newNonce, AsymmetricCipherKeyPair serverKeyPair, out BigInteger serverAgree, out byte[] serverSalt)
        {
            AesHelper.ComputeAESParameters(newNonce, setClientDhParams.ServerNonce, out var aesKeyData);
            
            var dhInnerData = DeserializeRequest(setClientDhParams, aesKeyData);

            var dhParameters = ((DHPrivateKeyParameters)serverKeyPair.Private).Parameters;
            
            var y = new BigInteger(dhInnerData.GBAsBinary);
            Guard.That(y).IsValidDhPublicKey(dhParameters.P);

            var clientPublicKey = new DHPublicKeyParameters(y, dhParameters);
                
            var serverKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            serverKeyAgree.Init(serverKeyPair.Private);

            serverAgree = serverKeyAgree.CalculateAgreement(clientPublicKey);

            serverSalt = new BitArray(newNonce.Take(8).ToArray()).Xor(new BitArray(setClientDhParams.ServerNonce.Take(8).ToArray())).ToByteArray();
            
            return SerializeResponse(setClientDhParams, newNonce, serverAgree);
        }

        private static TDhGenOk SerializeResponse(RequestSetClientDHParams setClientDhParams, byte[] newNonce, BigInteger agreement)
        {
            var newNonceHash = SHA1Helper.ComputeHashsum(newNonce).Skip(4).ToArray();
            
            var authKeyAuxHash = SHA1Helper.ComputeHashsum(agreement.ToByteArray()).Take(8).ToArray();

            return new TDhGenOk
                   {
                       Nonce = setClientDhParams.Nonce,
                       ServerNonce = setClientDhParams.ServerNonce,
                       NewNonceHash1 = newNonceHash.Concat((byte)1).Concat(authKeyAuxHash).ToArray()
                   };
        }
        
        private static TClientDHInnerData DeserializeRequest(RequestSetClientDHParams serverDhParams, AesKeyData aesKeyData)
        {
            var encryptedAnswer = serverDhParams.EncryptedDataAsBinary;
            var answerWithHash = AES.DecryptAes(aesKeyData, encryptedAnswer);

            var serverHashsum = answerWithHash.Take(20).ToArray();
            var answer = answerWithHash.Skip(20).ToArray();

            var clientDhInnerData = (TClientDHInnerData)Serializer.DeserializeObject(answer);

            var clearAnswer = Serializer.SerializeObject(clientDhInnerData);
            var hashsum = SHA1Helper.ComputeHashsum(clearAnswer);
            Guard.That(serverHashsum).IsItemsEquals(hashsum);

            return clientDhInnerData;

        }

    }
}