#region

using XadesNet.Signature;
using XadesNet.Upgraders.Parameters;

#endregion

namespace XadesNet.Upgraders
{
    public class XadesUpgraderService
    {
        #region Public methods

        public void Upgrade(SignatureDocument sigDocument, SignatureFormat toFormat, UpgradeParameters parameters)
        {
            XadesTUpgrader xadesTUpgrader;

            SignatureDocument.CheckSignatureDocument(sigDocument);

            if (toFormat == SignatureFormat.XAdES_T)
            {
                xadesTUpgrader = new XadesTUpgrader();
                xadesTUpgrader.Upgrade(sigDocument, parameters);
            }
            else
            {
                if (sigDocument.XadesSignature.UnsignedProperties.UnsignedSignatureProperties
                    .SignatureTimeStampCollection.Count == 0)
                {
                    xadesTUpgrader = new XadesTUpgrader();
                    xadesTUpgrader.Upgrade(sigDocument, parameters);
                }

                var xadesXLUpgrader = new XadesXLUpgrader();
                xadesXLUpgrader.Upgrade(sigDocument, parameters);
            }
        }

        #endregion
    }
}