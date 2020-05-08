#region

using System.Collections.Generic;

#endregion

namespace XadesNet.Signature.Parameters
{
    public class SignatureXPathExpression
    {
        #region Constructors

        public SignatureXPathExpression()
        {
            Namespaces = new Dictionary<string, string>();
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        public string XPathExpression { get; set; }

        public Dictionary<string, string> Namespaces { get; }

        #endregion
    }
}