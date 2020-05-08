#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Xades;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using XadesNet.Clients;
using XadesNet.Signature;
using XadesNet.Upgraders.Parameters;
using XadesNet.Utils;
using CertificateStatus = XadesNet.Clients.CertificateStatus;
using DigestMethod = XadesNet.Crypto.DigestMethod;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

#endregion

namespace XadesNet.Upgraders
{
    internal class XadesXLUpgrader : IXadesUpgrader
    {
        #region Public methods

        public void Upgrade(SignatureDocument signatureDocument, UpgradeParameters parameters)
        {
            var signingCertificate = signatureDocument.XadesSignature.GetSigningCertificate();

            var unsignedProperties = signatureDocument.XadesSignature.UnsignedProperties;
            unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs = new CompleteCertificateRefs
            {
                Id = "CompleteCertificates-" + Guid.NewGuid()
            };

            unsignedProperties.UnsignedSignatureProperties.CertificateValues = new CertificateValues();
            var certificateValues = unsignedProperties.UnsignedSignatureProperties.CertificateValues;
            certificateValues.Id = "CertificatesValues-" + Guid.NewGuid();

            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs = new CompleteRevocationRefs
            {
                Id = "CompleteRev-" + Guid.NewGuid()
            };

            unsignedProperties.UnsignedSignatureProperties.RevocationValues = new RevocationValues
            {
                Id = "RevocationValues-" + Guid.NewGuid()
            };

            AddCertificate(signingCertificate, unsignedProperties, false, parameters.OCSPServers, parameters.CRL,
                parameters.DigestMethod);

            AddTSACertificates(unsignedProperties, parameters.OCSPServers, parameters.CRL, parameters.DigestMethod);

            signatureDocument.XadesSignature.UnsignedProperties = unsignedProperties;

            TimeStampCertRefs(signatureDocument, parameters);

            signatureDocument.UpdateDocument();
        }

        #endregion

        #region Private methods

        private string GetResponderName(ResponderID responderId, ref bool byKey)
        {
            var dt = (DerTaggedObject) responderId.ToAsn1Object();

            if (dt.TagNo == 1)
            {
                byKey = false;

                return new X500DistinguishedName(dt.GetObject().GetEncoded()).Name;
            }

            if (dt.TagNo == 2)
            {
                var tagger = (Asn1TaggedObject) responderId.ToAsn1Object();
                var pubInfo = (Asn1OctetString) tagger.GetObject();
                byKey = true;

                return Convert.ToBase64String(pubInfo.GetOctets());
            }

            return null;
        }

        /// <summary>
        ///     Comprueba si dos DN son equivalentes
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool EquivalentDN(X500DistinguishedName dn, X500DistinguishedName other)
        {
            return X509Name.GetInstance(Asn1Object.FromByteArray(dn.RawData))
                .Equivalent(X509Name.GetInstance(Asn1Object.FromByteArray(other.RawData)));
        }

        /// <summary>
        ///     Determina si un certificado ya ha sido añadido a la colección de certificados
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="unsignedProperties"></param>
        /// <returns></returns>
        private bool CertificateChecked(X509Certificate2 cert, UnsignedProperties unsignedProperties)
        {
            foreach (EncapsulatedX509Certificate item in unsignedProperties.UnsignedSignatureProperties
                .CertificateValues.EncapsulatedX509CertificateCollection)
            {
                var certItem = new X509Certificate2(item.PkiData);

                if (certItem.Thumbprint == cert.Thumbprint) return true;
            }

            return false;
        }

        /// <summary>
        ///     Inserta en la lista de certificados el certificado y comprueba la valided del certificado.
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="unsignedProperties"></param>
        /// <param name="addCertValue"></param>
        /// <param name="extraCerts"></param>
        private void AddCertificate(X509Certificate2 cert, UnsignedProperties unsignedProperties, bool addCert,
            IEnumerable<string> ocspServers,
            IEnumerable<X509Crl> crlList, DigestMethod digestMethod, X509Certificate2[] extraCerts = null)
        {
            if (addCert)
            {
                if (CertificateChecked(cert, unsignedProperties)) return;

                var guidCert = Guid.NewGuid().ToString();

                var chainCert = new Cert();
                chainCert.IssuerSerial.X509IssuerName = cert.IssuerName.Name;
                chainCert.IssuerSerial.X509SerialNumber = cert.GetSerialNumberAsDecimalString();
                DigestUtil.SetCertDigest(cert.GetRawCertData(), digestMethod, chainCert.CertDigest);
                chainCert.URI = "#Cert" + guidCert;
                unsignedProperties.UnsignedSignatureProperties.CompleteCertificateRefs.CertRefs.CertCollection
                    .Add(chainCert);

                var encapsulatedX509Certificate = new EncapsulatedX509Certificate
                {
                    Id = "Cert" + guidCert, PkiData = cert.GetRawCertData()
                };
                unsignedProperties.UnsignedSignatureProperties.CertificateValues.EncapsulatedX509CertificateCollection
                    .Add(encapsulatedX509Certificate);
            }

            var chain = CertUtil.GetCertChain(cert, extraCerts).ChainElements;

            if (chain.Count > 1)
            {
                var enumerator = chain.GetEnumerator();
                enumerator.MoveNext(); // el mismo certificado que el pasado por parametro

                enumerator.MoveNext();

                var valid = ValidateCertificateByCRL(unsignedProperties, cert, enumerator.Current.Certificate, crlList,
                    digestMethod);

                if (!valid)
                {
                    var ocspCerts = ValidateCertificateByOCSP(unsignedProperties, cert, enumerator.Current.Certificate,
                        ocspServers, digestMethod);

                    if (ocspCerts != null)
                    {
                        var startOcspCert = DetermineStartCert(ocspCerts);

                        if (!EquivalentDN(startOcspCert.IssuerName, enumerator.Current.Certificate.SubjectName))
                        {
                            var chainOcsp = CertUtil.GetCertChain(startOcspCert, ocspCerts);

                            AddCertificate(chainOcsp.ChainElements[1].Certificate, unsignedProperties, true,
                                ocspServers, crlList, digestMethod, ocspCerts);
                        }
                    }
                }

                AddCertificate(enumerator.Current.Certificate, unsignedProperties, true, ocspServers, crlList,
                    digestMethod, extraCerts);
            }
        }

        private bool ExistsCRL(CRLRefCollection collection, string issuer)
        {
            foreach (CRLRef crlRef in collection)
                if (crlRef.CRLIdentifier.Issuer == issuer)
                    return true;

            return false;
        }

        private long? GetCRLNumber(X509Crl crlEntry)
        {
            var extValue = crlEntry.GetExtensionValue(X509Extensions.CrlNumber);

            if (extValue != null)
            {
                var asn1Value = X509ExtensionUtilities.FromExtensionValue(extValue);

                return DerInteger.GetInstance(asn1Value).PositiveValue.LongValue;
            }

            return null;
        }

        private bool ValidateCertificateByCRL(UnsignedProperties unsignedProperties, X509Certificate2 certificate,
            X509Certificate2 issuer,
            IEnumerable<X509Crl> crlList, DigestMethod digestMethod)
        {
            var clientCert = certificate.ToBouncyX509Certificate();
            var issuerCert = issuer.ToBouncyX509Certificate();

            foreach (var crlEntry in crlList)
                if (crlEntry.IssuerDN.Equivalent(issuerCert.SubjectDN) && crlEntry.NextUpdate.Value > DateTime.Now)
                {
                    if (!crlEntry.IsRevoked(clientCert))
                    {
                        if (!ExistsCRL(
                            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.CRLRefs
                                .CRLRefCollection,
                            issuer.Subject))
                        {
                            var idCrlValue = "CRLValue-" + Guid.NewGuid();

                            var crlRef = new CRLRef();
                            crlRef.CRLIdentifier.UriAttribute = "#" + idCrlValue;
                            crlRef.CRLIdentifier.Issuer = issuer.Subject;
                            crlRef.CRLIdentifier.IssueTime = crlEntry.ThisUpdate.ToLocalTime();

                            var crlNumber = GetCRLNumber(crlEntry);
                            if (crlNumber.HasValue) crlRef.CRLIdentifier.Number = crlNumber.Value;

                            var crlEncoded = crlEntry.GetEncoded();
                            DigestUtil.SetCertDigest(crlEncoded, digestMethod, crlRef.CertDigest);

                            var crlValue = new CRLValue {PkiData = crlEncoded, Id = idCrlValue};

                            unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.CRLRefs
                                .CRLRefCollection.Add(crlRef);
                            unsignedProperties.UnsignedSignatureProperties.RevocationValues.CRLValues.CRLValueCollection
                                .Add(crlValue);
                        }

                        return true;
                    }

                    throw new Exception("Certificate revoked");
                }

            return false;
        }

        private X509Certificate2[] ValidateCertificateByOCSP(UnsignedProperties unsignedProperties,
            X509Certificate2 client, X509Certificate2 issuer,
            IEnumerable<string> ocspServers, DigestMethod digestMethod)
        {
            var byKey = false;
            var finalOcspServers = new List<string>();
            var clientCert = client.ToBouncyX509Certificate();
            var issuerCert = issuer.ToBouncyX509Certificate();

            var ocsp = new OcspClient();
            var certOcspUrl = ocsp.GetAuthorityInformationAccessOcspUrl(issuerCert);

            if (!string.IsNullOrEmpty(certOcspUrl)) finalOcspServers.Add(certOcspUrl);

            foreach (var ocspUrl in ocspServers) finalOcspServers.Add(ocspUrl);

            foreach (var ocspUrl in finalOcspServers)
            {
                var resp = ocsp.QueryBinary(clientCert, issuerCert, ocspUrl);

                var status = ocsp.ProcessOcspResponse(resp);

                if (status == CertificateStatus.Revoked) throw new Exception("Certificate revoked");

                if (status == CertificateStatus.Good)
                {
                    var r = new OcspResp(resp);
                    var rEncoded = r.GetEncoded();
                    var or = (BasicOcspResp) r.GetResponseObject();

                    var guidOcsp = Guid.NewGuid().ToString();

                    var ocspRef = new OCSPRef {OCSPIdentifier = {UriAttribute = "#OcspValue" + guidOcsp}};
                    DigestUtil.SetCertDigest(rEncoded, digestMethod, ocspRef.CertDigest);

                    var rpId = or.ResponderId.ToAsn1Object();
                    ocspRef.OCSPIdentifier.ResponderID = GetResponderName(rpId, ref byKey);
                    ocspRef.OCSPIdentifier.ByKey = byKey;

                    ocspRef.OCSPIdentifier.ProducedAt = or.ProducedAt.ToLocalTime();
                    unsignedProperties.UnsignedSignatureProperties.CompleteRevocationRefs.OCSPRefs.OCSPRefCollection
                        .Add(ocspRef);

                    var ocspValue = new OCSPValue {PkiData = rEncoded, Id = "OcspValue" + guidOcsp};
                    unsignedProperties.UnsignedSignatureProperties.RevocationValues.OCSPValues.OCSPValueCollection.Add(
                        ocspValue);

                    return (from cert in or.GetCerts()
                        select new X509Certificate2(cert.GetEncoded())).ToArray();
                }
            }

            throw new Exception("The certificate could not be validated");
        }

        private X509Certificate2 DetermineStartCert(X509Certificate2[] certs)
        {
            X509Certificate2 currentCert = null;
            var isIssuer = true;

            for (var i = 0; i < certs.Length && isIssuer; i++)
            {
                currentCert = certs[i];

                isIssuer = certs.Any(cert => EquivalentDN(cert.IssuerName, currentCert.SubjectName));
            }

            return currentCert;
        }

        /// <summary>
        ///     Inserts and validates time stamp server certificates.
        /// </summary>
        /// <param name="unsignedProperties"></param>
        /// <param name="crlList"></param>
        /// <param name="digestMethod"></param>
        private void AddTSACertificates(UnsignedProperties unsignedProperties, IEnumerable<string> ocspServers,
            IEnumerable<X509Crl> crlList, DigestMethod digestMethod)
        {
            var token = new TimeStampToken(new CmsSignedData(unsignedProperties.UnsignedSignatureProperties
                .SignatureTimeStampCollection[0].EncapsulatedTimeStamp.PkiData));
            var store = token.GetCertificates("Collection");

            var signerId = token.SignerID;

            var tsaCerts = new List<X509Certificate2>();
            foreach (var tsaCert in store.GetMatches(null))
            {
                var cert = new X509Certificate2(((X509Certificate) tsaCert).GetEncoded());
                tsaCerts.Add(cert);
            }

            var startCert = DetermineStartCert(tsaCerts.ToArray());
            AddCertificate(startCert, unsignedProperties, true, ocspServers, crlList, digestMethod, tsaCerts.ToArray());
        }

        private static void TimeStampCertRefs(SignatureDocument signatureDocument, UpgradeParameters parameters)
        {
            var nodoFirma = signatureDocument.XadesSignature.GetSignatureElement();

            var nm = new XmlNamespaceManager(signatureDocument.Document.NameTable);
            nm.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);
            nm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            var xmlCompleteCertRefs = nodoFirma.SelectSingleNode(
                "ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteCertificateRefs",
                nm);

            if (xmlCompleteCertRefs == null) signatureDocument.UpdateDocument();

            var signatureValueElementXpaths = new ArrayList
            {
                "ds:SignatureValue",
                "ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:SignatureTimeStamp",
                "ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteCertificateRefs",
                "ds:Object/xades:QualifyingProperties/xades:UnsignedProperties/xades:UnsignedSignatureProperties/xades:CompleteRevocationRefs"
            };
            var signatureValueHash = DigestUtil.ComputeHashValue(
                XMLUtil.ComputeValueOfElementList(signatureDocument.XadesSignature, signatureValueElementXpaths),
                parameters.DigestMethod);

            var tsa = parameters.TimeStampClient.GetTimeStamp(signatureValueHash, parameters.DigestMethod, true);

            var xadesXTimeStamp = new TimeStamp("SigAndRefsTimeStamp")
            {
                Id = "SigAndRefsStamp-" + signatureDocument.XadesSignature.Signature.Id,
                EncapsulatedTimeStamp = {PkiData = tsa, Id = "SigAndRefsStamp-" + Guid.NewGuid()}
            };
            var unsignedProperties = signatureDocument.XadesSignature.UnsignedProperties;

            unsignedProperties.UnsignedSignatureProperties.RefsOnlyTimeStampFlag = false;
            unsignedProperties.UnsignedSignatureProperties.SigAndRefsTimeStampCollection.Add(xadesXTimeStamp);


            signatureDocument.XadesSignature.UnsignedProperties = unsignedProperties;
        }

        #endregion
    }
}