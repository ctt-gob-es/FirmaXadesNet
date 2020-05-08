#region

using System;
using System.Collections;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Utilities;
using XadesNet.Crypto;
using XadesNet.Signature;
using XadesNet.Utils;

#endregion

namespace XadesNet.Validation
{
    internal class XadesValidator
    {
        #region Public methods

        public ValidationResult Validate(SignatureDocument sigDocument)
        {
            /* Los elementos que se validan son:
             * 
             * 1. Las huellas de las referencias de la firma.
             * 2. Se comprueba la huella del elemento SignedInfo y se verifica la firma con la clave pública del certificado.
             * 3. Si la firma contiene un sello de tiempo se comprueba que la huella de la firma coincide con la del sello de tiempo.
             * 
             * La validación de perfiles -C, -X, -XL y -A esta fuera del ámbito de este proyecto.
             */

            var result = new ValidationResult();

            try
            {
                // Verifica las huellas de las referencias y la firma
                sigDocument.XadesSignature.CheckXmldsigSignature();
            }
            catch (Exception)
            {
                result.IsValid = false;
                result.Message = "La verificación de la firma no ha sido satisfactoria";

                return result;
            }

            if (sigDocument.XadesSignature.UnsignedProperties.UnsignedSignatureProperties.SignatureTimeStampCollection
                .Count > 0)
            {
                // Se comprueba el sello de tiempo

                var timeStamp = sigDocument.XadesSignature.UnsignedProperties.UnsignedSignatureProperties
                    .SignatureTimeStampCollection[0];
                var token = new TimeStampToken(new CmsSignedData(timeStamp.EncapsulatedTimeStamp.PkiData));

                var tsHashValue = token.TimeStampInfo.GetMessageImprintDigest();
                var tsDigestMethod = DigestMethod.GetByOid(token.TimeStampInfo.HashAlgorithm.ObjectID.Id);

                var signatureValueElementXpaths = new ArrayList {"ds:SignatureValue"};
                var signatureValueHash = DigestUtil.ComputeHashValue(
                    XMLUtil.ComputeValueOfElementList(sigDocument.XadesSignature, signatureValueElementXpaths),
                    tsDigestMethod);

                if (!Arrays.AreEqual(tsHashValue, signatureValueHash))
                {
                    result.IsValid = false;
                    result.Message = "La huella del sello de tiempo no se corresponde con la calculada";

                    return result;
                }
            }

            result.IsValid = true;
            result.Message = "Verificación de la firma satisfactoria";

            return result;
        }

        #endregion
    }
}