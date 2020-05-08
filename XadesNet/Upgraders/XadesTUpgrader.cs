#region

using System;
using System.Collections;
using Microsoft.Xades;
using XadesNet.Signature;
using XadesNet.Upgraders.Parameters;
using XadesNet.Utils;

#endregion

namespace XadesNet.Upgraders
{
    internal class XadesTUpgrader : IXadesUpgrader
    {
        #region Public methods

        public void Upgrade(SignatureDocument signatureDocument, UpgradeParameters parameters)
        {
            var unsignedProperties = signatureDocument.XadesSignature.UnsignedProperties;

            try
            {
                if (unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Count > 0)
                    throw new Exception("The signature already contains a time stamp");

                var signatureValueElementXpaths = new ArrayList {"ds:SignatureValue"};
                var signatureValueHash = DigestUtil.ComputeHashValue(
                    XMLUtil.ComputeValueOfElementList(signatureDocument.XadesSignature, signatureValueElementXpaths),
                    parameters.DigestMethod);

                var tsa = parameters.TimeStampClient.GetTimeStamp(signatureValueHash, parameters.DigestMethod, true);

                var signatureTimeStamp = new TimeStamp("SignatureTimeStamp")
                {
                    Id = "SignatureTimeStamp-" + signatureDocument.XadesSignature.Signature.Id,
                    EncapsulatedTimeStamp = {PkiData = tsa, Id = "SignatureTimeStamp-" + Guid.NewGuid()}
                };

                unsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection.Add(signatureTimeStamp);

                signatureDocument.XadesSignature.UnsignedProperties = unsignedProperties;

                signatureDocument.UpdateDocument();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while inserting the time stamp.", ex);
            }
        }

        #endregion
    }
}