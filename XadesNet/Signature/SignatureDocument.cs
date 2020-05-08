#region

using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Microsoft.Xades;
using XadesNet.Utils;

#endregion

namespace XadesNet.Signature
{
    public class SignatureDocument
    {
        #region Private variables

        #endregion

        #region Public properties

        public XmlDocument Document { get; set; }

        public XadesSignedXml XadesSignature { get; set; }

        #endregion

        #region Public methods

        public byte[] GetDocumentBytes()
        {
            CheckSignatureDocument(this);

            using (var ms = new MemoryStream())
            {
                Save(ms);

                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Guardar la firma en el fichero especificado.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            CheckSignatureDocument(this);

            var settings = new XmlWriterSettings {Encoding = new UTF8Encoding()};
            using (var writer = XmlWriter.Create(fileName, settings))
            {
                Document.Save(writer);
            }
        }

        /// <summary>
        ///     Guarda la firma en el destino especificado
        /// </summary>
        /// <param name="output"></param>
        public void Save(Stream output)
        {
            var settings = new XmlWriterSettings {Encoding = new UTF8Encoding()};
            using (var writer = XmlWriter.Create(output, settings))
            {
                Document.Save(writer);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Actualiza el documento resultante
        /// </summary>
        internal void UpdateDocument()
        {
            if (Document == null) Document = new XmlDocument();

            if (Document.DocumentElement != null)
            {
                var xmlNode = Document.SelectSingleNode("//*[@Id='" + XadesSignature.Signature.Id + "']");

                if (xmlNode != null)
                {
                    var nm = new XmlNamespaceManager(Document.NameTable);
                    nm.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);
                    nm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

                    var xmlQPNode = xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties", nm);
                    var xmlUnsingedPropertiesNode =
                        xmlNode.SelectSingleNode("ds:Object/xades:QualifyingProperties/xades:UnsignedProperties", nm);

                    if (xmlUnsingedPropertiesNode != null)
                    {
                        xmlUnsingedPropertiesNode.InnerXml = XadesSignature.XadesObject.QualifyingProperties
                            .UnsignedProperties.GetXml().InnerXml;
                    }
                    else
                    {
                        xmlUnsingedPropertiesNode =
                            Document.ImportNode(
                                XadesSignature.XadesObject.QualifyingProperties.UnsignedProperties.GetXml(), true);
                        xmlQPNode.AppendChild(xmlUnsingedPropertiesNode);
                    }
                }
                else
                {
                    var xmlSigned = XadesSignature.GetXml();

                    var canonicalizedElement = XMLUtil.ApplyTransform(xmlSigned, new XmlDsigC14NTransform());

                    var doc = new XmlDocument {PreserveWhitespace = true};
                    doc.LoadXml(Encoding.UTF8.GetString(canonicalizedElement));

                    var canonSignature = Document.ImportNode(doc.DocumentElement, true);

                    XadesSignature.GetSignatureElement().AppendChild(canonSignature);
                }
            }
            else
            {
                Document.LoadXml(XadesSignature.GetXml().OuterXml);
            }
        }


        internal static void CheckSignatureDocument(SignatureDocument sigDocument)
        {
            if (sigDocument == null) throw new ArgumentNullException(nameof(sigDocument));

            if (sigDocument.Document == null || sigDocument.XadesSignature == null)
                throw new Exception("There is no information about the signature");
        }

        #endregion
    }
}