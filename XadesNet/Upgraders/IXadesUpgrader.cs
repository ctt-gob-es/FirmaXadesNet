#region

using XadesNet.Signature;
using XadesNet.Upgraders.Parameters;

#endregion

namespace XadesNet.Upgraders
{
    internal interface IXadesUpgrader
    {
        void Upgrade(SignatureDocument signatureDocument, UpgradeParameters parameters);
    }
}