#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;

#endregion

namespace XadesNet.Clients
{
    public class OcspClient
    {
        #region Private variables

        private Asn1OctetString _nonceAsn1OctetString;

        #endregion

        #region Public methods

        /// <summary>
        ///     Método que comprueba el estado de un certificado
        /// </summary>
        /// <param name="eeCert"></param>
        /// <param name="issuerCert"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public byte[] QueryBinary(X509Certificate eeCert, X509Certificate issuerCert, string url)
        {
            var req = GenerateOcspRequest(issuerCert, eeCert.SerialNumber);

            var binaryResp = PostData(url, req.GetEncoded(), "application/ocsp-request", "application/ocsp-response");

            return binaryResp;
        }

        /// <summary>
        ///     Devuelve la URL del servidor OCSP que contenga el certificado
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public string GetAuthorityInformationAccessOcspUrl(X509Certificate cert)
        {
            var ocspUrls = new List<string>();

            try
            {
                var obj = GetExtensionValue(cert, X509Extensions.AuthorityInfoAccess.Id);

                if (obj == null) return null;

                // Switched to manual parse 
                var s = (Asn1Sequence) obj;
                var elements = s.GetEnumerator();

                while (elements.MoveNext())
                {
                    var element = (Asn1Sequence) elements.Current;
                    var oid = (DerObjectIdentifier) element[0];

                    if (oid.Id.Equals("1.3.6.1.5.5.7.48.1")) // Is Ocsp? 
                    {
                        var taggedObject = (Asn1TaggedObject) element[1];
                        var gn = GeneralName.GetInstance(taggedObject);
                        ocspUrls.Add(DerIA5String.GetInstance(gn.Name).GetString());
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return ocspUrls[0];
        }

        /// <summary>
        ///     Procesa la respuesta del servidor OCSP y devuelve el estado del certificado
        /// </summary>
        /// <param name="binaryResp"></param>
        /// <returns></returns>
        public CertificateStatus ProcessOcspResponse(byte[] binaryResp)
        {
            if (binaryResp.Length == 0) return CertificateStatus.Unknown;

            var r = new OcspResp(binaryResp);
            var cStatus = CertificateStatus.Unknown;

            if (r.Status == OcspRespStatus.Successful)
            {
                var or = (BasicOcspResp) r.GetResponseObject();

                if (or.GetExtensionValue(OcspObjectIdentifiers.PkixOcspNonce).ToString() !=
                    _nonceAsn1OctetString.ToString())
                    throw new Exception("Bad nonce value");

                if (or.Responses.Length == 1)
                {
                    var resp = or.Responses[0];

                    var certificateStatus = resp.GetCertStatus();

                    if (certificateStatus == Org.BouncyCastle.Ocsp.CertificateStatus.Good)
                        cStatus = CertificateStatus.Good;
                    else if (certificateStatus is RevokedStatus)
                        cStatus = CertificateStatus.Revoked;
                    else if (certificateStatus is UnknownStatus) cStatus = CertificateStatus.Unknown;
                }
            }
            else
            {
                throw new Exception("Unknow status '" + r.Status + "'.");
            }

            return cStatus;
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Construye la petición web y devuelve el resultado de la misma
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="accept"></param>
        /// <returns></returns>
        private byte[] PostData(string url, byte[] data, string contentType, string accept)
        {
            byte[] resp;

            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = data.Length;
            request.Accept = accept;
            var stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            var response = (HttpWebResponse) request.GetResponse();
            var respStream = response.GetResponseStream();
            using (var ms = new MemoryStream())
            {
                respStream.CopyTo(ms);
                resp = ms.ToArray();
                respStream.Close();
            }

            return resp;
        }


        protected static Asn1Object GetExtensionValue(X509Certificate cert,
            string oid)
        {
            if (cert == null) return null;

            var bytes = cert.GetExtensionValue(new DerObjectIdentifier(oid)).GetOctets();

            if (bytes == null) return null;

            var aIn = new Asn1InputStream(bytes);

            return aIn.ReadObject();
        }


        private OcspReq GenerateOcspRequest(X509Certificate issuerCert, BigInteger serialNumber)
        {
            var id = new CertificateID(CertificateID.HashSha1, issuerCert, serialNumber);
            return GenerateOcspRequest(id);
        }

        private OcspReq GenerateOcspRequest(CertificateID id)
        {
            var ocspRequestGenerator = new OcspReqGenerator();

            ocspRequestGenerator.AddRequest(id);

            var oids = new ArrayList();
            var values = new Hashtable();

            oids.Add(OcspObjectIdentifiers.PkixOcspNonce);

            _nonceAsn1OctetString =
                new DerOctetString(new DerOctetString(BigInteger.ValueOf(DateTime.Now.Ticks).ToByteArray()));

            values.Add(OcspObjectIdentifiers.PkixOcspNonce, new X509Extension(false, _nonceAsn1OctetString));
            ocspRequestGenerator.SetRequestExtensions(new X509Extensions(oids, values));

            return ocspRequestGenerator.Generate();
        }

        #endregion
    }
}