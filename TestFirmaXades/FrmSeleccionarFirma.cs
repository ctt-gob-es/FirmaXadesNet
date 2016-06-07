using FirmaXadesNet.Signature;
using System.Windows.Forms;

namespace TestFirmaXades
{
    public partial class FrmSeleccionarFirma : Form
    {
        private SignatureDocument[] _firmas = null;

        public SignatureDocument FirmaSeleccionada
        {
            get
            {
                return _firmas[lstFirmas.SelectedIndex];
            }
        }

        public FrmSeleccionarFirma(SignatureDocument[] firmas)
        {
            InitializeComponent();

            _firmas = firmas;

            foreach (var firma in firmas)
            {
                string textoFirma = string.Format("{0} - {1}",
                    firma.XadesSignature.XadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties.SigningTime,
                    firma.GetSigningCertificate().Subject);

                lstFirmas.Items.Add(textoFirma);
            }

            lstFirmas.SelectedIndex = 0;
        }
    }
}
