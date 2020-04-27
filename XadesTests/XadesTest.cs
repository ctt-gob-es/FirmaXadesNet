using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XadesNet;
using XadesNet.Utils;

namespace XadesTests
{
    [TestClass]
    public class XadesTest
    {
        [TestMethod]
        public void TestSign()
        {
            var inputPath = "../../input.xml";
            var outputPath = "../../output.xml";

            var xml = new XmlDocument();
            xml.Load(inputPath);
            var certificate = CertUtil.SelectCertificate();
            var xmlDocument = XadesService.Sign(inputPath, certificate);
            xmlDocument.Save(outputPath);
        }
    }
}