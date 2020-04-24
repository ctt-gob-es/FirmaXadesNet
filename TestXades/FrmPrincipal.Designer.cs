﻿namespace TestXades
{
    partial class FrmPrincipal
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnFirmar = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSeleccionarFichero = new System.Windows.Forms.Button();
            this.txtFichero = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rbEnveloped = new System.Windows.Forms.RadioButton();
            this.rbExternallyDetached = new System.Windows.Forms.RadioButton();
            this.rbInternnallyDetached = new System.Windows.Forms.RadioButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.txtURLSellado = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOCSP = new System.Windows.Forms.TextBox();
            this.btnXadesT = new System.Windows.Forms.Button();
            this.btnXadesXL = new System.Windows.Forms.Button();
            this.btnGuardarFirma = new System.Windows.Forms.Button();
            this.btnCargarFirma = new System.Windows.Forms.Button();
            this.btnCoFirmar = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtIdentificadorPolitica = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtHashPolitica = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtURIPolitica = new System.Windows.Forms.TextBox();
            this.btnContraFirma = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnFirmar
            // 
            this.btnFirmar.Location = new System.Drawing.Point(12, 370);
            this.btnFirmar.Name = "btnFirmar";
            this.btnFirmar.Size = new System.Drawing.Size(75, 23);
            this.btnFirmar.TabIndex = 0;
            this.btnFirmar.Text = "Sign";
            this.btnFirmar.UseVisualStyleBackColor = true;
            this.btnFirmar.Click += new System.EventHandler(this.btnFirmar_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSeleccionarFichero);
            this.groupBox1.Controls.Add(this.txtFichero);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.rbEnveloped);
            this.groupBox1.Controls.Add(this.rbExternallyDetached);
            this.groupBox1.Controls.Add(this.rbInternnallyDetached);
            this.groupBox1.Location = new System.Drawing.Point(12, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(606, 176);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Signature format";
            // 
            // btnSeleccionarFichero
            // 
            this.btnSeleccionarFichero.Location = new System.Drawing.Point(425, 128);
            this.btnSeleccionarFichero.Name = "btnSeleccionarFichero";
            this.btnSeleccionarFichero.Size = new System.Drawing.Size(28, 23);
            this.btnSeleccionarFichero.TabIndex = 5;
            this.btnSeleccionarFichero.Text = "...";
            this.btnSeleccionarFichero.UseVisualStyleBackColor = true;
            this.btnSeleccionarFichero.Click += new System.EventHandler(this.btnSeleccionarFichero_Click);
            // 
            // txtFichero
            // 
            this.txtFichero.Location = new System.Drawing.Point(13, 129);
            this.txtFichero.Name = "txtFichero";
            this.txtFichero.Size = new System.Drawing.Size(412, 20);
            this.txtFichero.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Original file";
            // 
            // rbEnveloped
            // 
            this.rbEnveloped.AutoSize = true;
            this.rbEnveloped.Location = new System.Drawing.Point(13, 75);
            this.rbEnveloped.Name = "rbEnveloped";
            this.rbEnveloped.Size = new System.Drawing.Size(76, 17);
            this.rbEnveloped.TabIndex = 2;
            this.rbEnveloped.Text = "Enveloped";
            this.rbEnveloped.UseVisualStyleBackColor = true;
            // 
            // rbExternallyDetached
            // 
            this.rbExternallyDetached.AutoSize = true;
            this.rbExternallyDetached.Location = new System.Drawing.Point(13, 51);
            this.rbExternallyDetached.Name = "rbExternallyDetached";
            this.rbExternallyDetached.Size = new System.Drawing.Size(118, 17);
            this.rbExternallyDetached.TabIndex = 1;
            this.rbExternallyDetached.Text = "Externally detached";
            this.rbExternallyDetached.UseVisualStyleBackColor = true;
            // 
            // rbInternnallyDetached
            // 
            this.rbInternnallyDetached.AutoSize = true;
            this.rbInternnallyDetached.Checked = true;
            this.rbInternnallyDetached.Location = new System.Drawing.Point(13, 27);
            this.rbInternnallyDetached.Name = "rbInternnallyDetached";
            this.rbInternnallyDetached.Size = new System.Drawing.Size(115, 17);
            this.rbInternnallyDetached.TabIndex = 0;
            this.rbInternnallyDetached.TabStop = true;
            this.rbInternnallyDetached.Text = "Internally detached";
            this.rbInternnallyDetached.UseVisualStyleBackColor = true;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "XML|*.xml";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 195);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Time-stamped server URL";
            // 
            // txtURLSellado
            // 
            this.txtURLSellado.Location = new System.Drawing.Point(16, 211);
            this.txtURLSellado.Name = "txtURLSellado";
            this.txtURLSellado.Size = new System.Drawing.Size(255, 20);
            this.txtURLSellado.TabIndex = 3;
            this.txtURLSellado.Text = "http://tss.accv.es:8318/tsa";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(274, 195);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(197, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "default Online Certificate Status Protocol";
            // 
            // txtOCSP
            // 
            this.txtOCSP.Location = new System.Drawing.Point(277, 211);
            this.txtOCSP.Name = "txtOCSP";
            this.txtOCSP.Size = new System.Drawing.Size(336, 20);
            this.txtOCSP.TabIndex = 5;
            this.txtOCSP.Text = "http://ocsp.dnie.es";
            // 
            // btnXadesT
            // 
            this.btnXadesT.Location = new System.Drawing.Point(252, 370);
            this.btnXadesT.Name = "btnXadesT";
            this.btnXadesT.Size = new System.Drawing.Size(118, 23);
            this.btnXadesT.TabIndex = 6;
            this.btnXadesT.Text = "Expand to XADES-T";
            this.btnXadesT.UseVisualStyleBackColor = true;
            this.btnXadesT.Click += new System.EventHandler(this.btnXadesT_Click);
            // 
            // btnXadesXL
            // 
            this.btnXadesXL.Location = new System.Drawing.Point(376, 370);
            this.btnXadesXL.Name = "btnXadesXL";
            this.btnXadesXL.Size = new System.Drawing.Size(134, 23);
            this.btnXadesXL.TabIndex = 7;
            this.btnXadesXL.Text = "Expand to XADES-XL";
            this.btnXadesXL.UseVisualStyleBackColor = true;
            this.btnXadesXL.Click += new System.EventHandler(this.btnXadesXL_Click);
            // 
            // btnGuardarFirma
            // 
            this.btnGuardarFirma.Location = new System.Drawing.Point(516, 369);
            this.btnGuardarFirma.Name = "btnGuardarFirma";
            this.btnGuardarFirma.Size = new System.Drawing.Size(97, 23);
            this.btnGuardarFirma.TabIndex = 8;
            this.btnGuardarFirma.Text = "Save signature";
            this.btnGuardarFirma.UseVisualStyleBackColor = true;
            this.btnGuardarFirma.Click += new System.EventHandler(this.btnGuardarFirma_Click);
            // 
            // btnCargarFirma
            // 
            this.btnCargarFirma.Location = new System.Drawing.Point(516, 320);
            this.btnCargarFirma.Name = "btnCargarFirma";
            this.btnCargarFirma.Size = new System.Drawing.Size(97, 23);
            this.btnCargarFirma.TabIndex = 9;
            this.btnCargarFirma.Text = "Upload signature";
            this.btnCargarFirma.UseVisualStyleBackColor = true;
            this.btnCargarFirma.Click += new System.EventHandler(this.btnCargarFirma_Click);
            // 
            // btnCoFirmar
            // 
            this.btnCoFirmar.Location = new System.Drawing.Point(93, 370);
            this.btnCoFirmar.Name = "btnCoFirmar";
            this.btnCoFirmar.Size = new System.Drawing.Size(75, 23);
            this.btnCoFirmar.TabIndex = 10;
            this.btnCoFirmar.Text = "Co-Sign";
            this.btnCoFirmar.UseVisualStyleBackColor = true;
            this.btnCoFirmar.Click += new System.EventHandler(this.btnCoFirmar_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 251);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(124, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Signature policy identifier";
            // 
            // txtIdentificadorPolitica
            // 
            this.txtIdentificadorPolitica.Location = new System.Drawing.Point(16, 267);
            this.txtIdentificadorPolitica.Name = "txtIdentificadorPolitica";
            this.txtIdentificadorPolitica.Size = new System.Drawing.Size(255, 20);
            this.txtIdentificadorPolitica.TabIndex = 12;
            this.txtIdentificadorPolitica.Text = "urn:oid:2.16.724.1.3.1.1.2.1.8";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(274, 251);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Hash value (base64)";
            // 
            // txtHashPolitica
            // 
            this.txtHashPolitica.Location = new System.Drawing.Point(277, 267);
            this.txtHashPolitica.Name = "txtHashPolitica";
            this.txtHashPolitica.Size = new System.Drawing.Size(336, 20);
            this.txtHashPolitica.TabIndex = 14;
            this.txtHashPolitica.Text = "V8lVVNGDCPen6VELRD1Ja8HARFk=";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 307);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Policy URI";
            // 
            // txtURIPolitica
            // 
            this.txtURIPolitica.Location = new System.Drawing.Point(16, 323);
            this.txtURIPolitica.Name = "txtURIPolitica";
            this.txtURIPolitica.Size = new System.Drawing.Size(494, 20);
            this.txtURIPolitica.TabIndex = 16;
            this.txtURIPolitica.Text = "http://administracionelectronica.gob.es/es/ctt/politicafirma/politica_firma_AGE_v" +
    "1_8.pdf";
            // 
            // btnContraFirma
            // 
            this.btnContraFirma.Location = new System.Drawing.Point(171, 370);
            this.btnContraFirma.Name = "btnContraFirma";
            this.btnContraFirma.Size = new System.Drawing.Size(75, 23);
            this.btnContraFirma.TabIndex = 17;
            this.btnContraFirma.Text = "Countersign";
            this.btnContraFirma.UseVisualStyleBackColor = true;
            this.btnContraFirma.Click += new System.EventHandler(this.btnContraFirma_Click);
            // 
            // FrmPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 436);
            this.Controls.Add(this.btnContraFirma);
            this.Controls.Add(this.txtURIPolitica);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtHashPolitica);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtIdentificadorPolitica);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnCoFirmar);
            this.Controls.Add(this.btnCargarFirma);
            this.Controls.Add(this.btnGuardarFirma);
            this.Controls.Add(this.btnXadesXL);
            this.Controls.Add(this.btnXadesT);
            this.Controls.Add(this.txtOCSP);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtURLSellado);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnFirmar);
            this.Name = "FrmPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "XadesNet test application";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnFirmar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbExternallyDetached;
        private System.Windows.Forms.RadioButton rbInternnallyDetached;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rbEnveloped;
        private System.Windows.Forms.Button btnSeleccionarFichero;
        private System.Windows.Forms.TextBox txtFichero;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtURLSellado;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtOCSP;
        private System.Windows.Forms.Button btnXadesT;
        private System.Windows.Forms.Button btnXadesXL;
        private System.Windows.Forms.Button btnGuardarFirma;
        private System.Windows.Forms.Button btnCargarFirma;
        private System.Windows.Forms.Button btnCoFirmar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtIdentificadorPolitica;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtHashPolitica;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtURIPolitica;
        private System.Windows.Forms.Button btnContraFirma;
    }
}

