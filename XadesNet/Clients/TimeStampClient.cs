#region

using System;
using System.IO;
using System.Net;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using XadesNet.Crypto;

#endregion

namespace XadesNet.Clients
{
    public class TimeStampClient
    {
        #region Public methods

        /// <summary>
        ///     Make the request to stamp the hash that is passed as a parameter and return the
        ///     server response.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="digestMethod"></param>
        /// <param name="certReq"></param>
        /// <returns></returns>
        public byte[] GetTimeStamp(byte[] hash, DigestMethod digestMethod, bool certReq)
        {
            var tsrq = new TimeStampRequestGenerator();
            tsrq.SetCertReq(certReq);

            var nonce = BigInteger.ValueOf(DateTime.Now.Ticks);

            var tsr = tsrq.Generate(digestMethod.Oid, hash, nonce);
            var data = tsr.GetEncoded();

            var req = (HttpWebRequest) WebRequest.Create(_url);
            req.Method = "POST";
            req.ContentType = "application/timestamp-query";
            req.ContentLength = data.Length;

            if (!string.IsNullOrEmpty(_user) && !string.IsNullOrEmpty(_password))
            {
                var auth = $"{_user}:{_password}";
                req.Headers["Authorization"] =
                    "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(auth), Base64FormattingOptions.None);
            }

            var reqStream = req.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            var res = (HttpWebResponse) req.GetResponse();
            if (res.StatusCode != HttpStatusCode.OK)
                throw new Exception("The server returned an invalid response");

            Stream resStream = new BufferedStream(res.GetResponseStream());
            var tsRes = new TimeStampResponse(resStream);
            resStream.Close();

            tsRes.Validate(tsr);

            if (tsRes.TimeStampToken == null) throw new Exception("The server has not returned any time stamps");

            return tsRes.TimeStampToken.GetEncoded();
        }

        #endregion

        #region Private variables

        private readonly string _url;
        private readonly string _user;
        private readonly string _password;

        #endregion

        #region Constructors

        public TimeStampClient(string url)
        {
            _url = url;
        }

        public TimeStampClient(string url, string user, string password)
            : this(url)
        {
            _user = user;
            _password = password;
        }

        #endregion
    }
}