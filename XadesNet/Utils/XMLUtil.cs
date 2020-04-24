#region

using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Microsoft.Xades;
using Transform = System.Security.Cryptography.Xml.Transform;

#endregion

namespace XadesNet.Utils
{
    internal class XMLUtil
    {
        #region Public methods

        /// <summary>
        ///     Aplica una transformación al elemento especificado
        /// </summary>
        /// <param name="element"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static byte[] ApplyTransform(XmlElement element, Transform transform)
        {
            var buffer = Encoding.UTF8.GetBytes(element.OuterXml);

            using (var ms = new MemoryStream(buffer))
            {
                transform.LoadInput(ms);
                using (var transformedStream = (MemoryStream) transform.GetOutput(typeof(Stream)))
                {
                    return transformedStream.ToArray();
                }
            }
        }

        /// <summary>
        ///     Obtiene el valor canonicalizado de los elementos especificados en elementXpaths
        /// </summary>
        /// <param name="xadesSignedXml"></param>
        /// <param name="elementXpaths"></param>
        /// <returns></returns>
        public static byte[] ComputeValueOfElementList(XadesSignedXml xadesSignedXml, ArrayList elementXpaths)
        {
            XmlDocument xmlDocument;
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList searchXmlNodeList;

            var signatureXmlElement = xadesSignedXml.GetSignatureElement();
            var namespaces = xadesSignedXml.GetAllNamespaces(signatureXmlElement);

            xmlDocument = signatureXmlElement.OwnerDocument;
            xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            xmlNamespaceManager.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);

            using (var msResult = new MemoryStream())
            {
                foreach (string elementXpath in elementXpaths)
                {
                    searchXmlNodeList = signatureXmlElement.SelectNodes(elementXpath, xmlNamespaceManager);

                    if (searchXmlNodeList.Count == 0)
                        throw new CryptographicException(
                            "Element " + elementXpath + " not found while calculating hash");

                    foreach (XmlNode xmlNode in searchXmlNodeList)
                    {
                        var clonedElement = (XmlElement) xmlNode.Clone();

                        clonedElement.SetAttribute("xmlns:" + XadesSignedXml.XmlDSigPrefix,
                            SignedXml.XmlDsigNamespaceUrl);

                        foreach (var attr in namespaces) clonedElement.SetAttribute(attr.Name, attr.Value);

                        var canonicalizedElement = ApplyTransform(clonedElement, new XmlDsigC14NTransform());
                        msResult.Write(canonicalizedElement, 0, canonicalizedElement.Length);
                    }
                }

                return msResult.ToArray();
            }
        }

        /// <summary>
        ///     Carga un documento XML
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static XmlDocument LoadDocument(Stream input)
        {
            var document = new XmlDocument {PreserveWhitespace = true};
            document.Load(input);

            return document;
        }

        #endregion
    }
}