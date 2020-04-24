#region

using System.Windows.Forms;
using XadesNet.Signature;

#endregion

namespace TestXades
{
    public partial class FrmSeleccionarFirma : Form
    {
        private readonly SignatureDocument[] _firmas;

        public FrmSeleccionarFirma(SignatureDocument[] firmas)
        {
            InitializeComponent();

            _firmas = firmas;

            foreach (var firma in firmas)
            {
                var textoFirma = string.Format("{0} - {1}",
                    firma.XadesSignature.XadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties
                        .SigningTime,
                    firma.XadesSignature.GetSigningCertificate().Subject);

                lstFirmas.Items.Add(textoFirma);
            }

            lstFirmas.SelectedIndex = 0;
        }

        public SignatureDocument FirmaSeleccionada => _firmas[lstFirmas.SelectedIndex];
    }
}