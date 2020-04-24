#region

using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace XadesNet.Signature.Parameters
{
    public class SignerRole
    {
        #region Constructors

        public SignerRole()
        {
            CertifiedRoles = new List<X509Certificate>();
            ClaimedRoles = new List<string>();
        }

        #endregion

        #region Private variables

        #endregion


        #region Public properties

        public List<X509Certificate> CertifiedRoles { get; }

        public List<string> ClaimedRoles { get; }

        #endregion
    }
}