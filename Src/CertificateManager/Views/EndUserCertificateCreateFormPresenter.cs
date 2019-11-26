﻿using System;
using System.Windows.Forms;
using CertificateManager.Services;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace CertificateManager.Views
{
    public interface IEndUserCertificateCreateFormPresenter : ICertificateCreateView
    {
        string[] AlternateNames { get; }

        string CaUri { get; }
        string CrlUri { get; }
        string OcspUri { get; }

        string CAFile { get; }
        string CAPrivateKeyFile { get; }
    }

    public class EndUserCertificateCreateFormPresenter :
        CertificateCreatePresenter<IEndUserCertificateCreateFormPresenter>
    {
        public override void OnViewReady()
        {
            base.OnViewReady();

            View.AvailableKeyUsage = KeyUsageUtils.All;
            View.DefaultKeyUsage = Constants.DefaultClientKeyUsage;
            View.AvailablePurpose = KeyPurposeUtils.All;
            View.DefaultPurpose = Constants.DefaultClientExtendUsages;
        }

        public bool Generate()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter =
                    @"X.509 Certificate files (*.cer;*.crt;*.cert;*.pem)|*.cer;*.crt;*.cert;*.pem|Pem files (*.pem)|*.pem|Crt files (*.crt)|*.crt|All files (*.*)|*.*";
                dlg.FilterIndex = 0;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var certificateGenerator = new DefaultClientCertificateGenerator(new DefaultUrlEvaluator());

                    DateTime beginDate = View.From;
                    DateTime endDate = beginDate.AddDays(View.Duration);

                    X509Certificate caFile = IOPEMUtils.LoadPemCertificate(View.CAFile);
                    AsymmetricCipherKeyPair caPriveKeyFile = IOPEMUtils.LoadPemPrivateKey(View.CAPrivateKeyFile);

                    CertificateResult result = certificateGenerator.Create(
                        View.Algoritm,
                        View.KeyLength,
                        View.Subject,
                        View.AlternateNames,
                        beginDate,
                        endDate,
                        View.KeyUsage,
                        View.Purpose,
                        caFile,
                        caPriveKeyFile,
                        new CertificateEndPoints
                        {
                            CaDistributionEndPoint = View.CaUri,
                            CrlDistributionEndPoint = View.CrlUri,
                            OcspEndPoint = View.OcspUri
                        }
                        );

                    ResultFileNames files = IOPEMUtils.SaveCertificateAndPrivateKey(result.Certificate, result.KeyPair,
                        dlg.FileName);

                    MessageBox.Show(
                        String.Format("Foram gravados os ficheiros:\nCertificado como '{0}'\nChave privada como '{1}'",
                            files.Certificate,
                            files.PrivateKey), @"Informação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                        );

                    return true;
                }

                return false;
            }
        }
    }
}