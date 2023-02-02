using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace GrpcCert
{
    /** <summary>provides functions to create an x509 certificate
     * adopted and modified from: https://stackoverflow.com/questions/13806299/how-can-i-create-a-self-signed-certificate-using-c </summary>
     */
    public class CertificateUtil
    {
        /** <summary>create a self-signed certificate</summary>
         * <param name="certDir">directory where the certificate should be saved</param>
         * <param name="certName">name of the certificate file</param>
         */
        public static void GenerateRootCertificate(string certDir, string certName)
        {
            var ecdsa = ECDsa.Create(); // generate asymmetric key pair
            var req = new CertificateRequest($"cn=grpc_{certName}", ecdsa, HashAlgorithmName.SHA512);

            req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));

            //req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

            //req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension( new OidCollection
            //{
            //    new Oid("1.3.6.1.5.5.7.3.1")
            //}, true));

            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

            // Create PFX (PKCS #12) with private key            
            File.WriteAllBytes(Path.Join(certDir, AddOrChangeExtension(certName, ".pfx")), cert.Export(X509ContentType.Pfx, "P@55w0rd"));

            // Create Base 64 encoded CER (public key only)
            //File.WriteAllText(Path.Join(certDir, AddOrChangeExtension(certName, ".cer")),
            //    "-----BEGIN CERTIFICATE-----\r\n"
            //    + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
            //    + "\r\n-----END CERTIFICATE-----");
        }

        /** <summary>generates a client cert from the root cert</summary>
         * <param name="rootDir">path to the root cert</param>
         * <param name="rootFileName">name of the root cert</param>
         * <param name="targetDir">path of the generated file</param>
         * <param name="targetName">target file name and name in the certificate's dc</param>
         */
        public static void GenerateClientCertificate(string rootDir, string rootFileName, string targetDir, string targetName)
        {
            var rootCert = new X509Certificate2(Path.Join(rootDir, AddOrChangeExtension(rootFileName, ".pfx")), "P@55w0rd");

            var ecdsa = ECDsa.Create();

            var req = new CertificateRequest($"cn=grpc_{targetName}", ecdsa, HashAlgorithmName.SHA512);

            req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));

            //req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation, false));

            //req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
            //{
            //    new Oid("1.3.6.1.5.5.7.3.1")
            //}, true));

            byte[] serial = new byte[12];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(serial);
            }
            var clientCert = req.Create(rootCert, DateTimeOffset.Now, rootCert.NotAfter, serial);
            var clientCertWithPrivateKey = ECDsaCertificateExtensions.CopyWithPrivateKey(clientCert, ecdsa);
            File.WriteAllBytes(Path.Join(targetDir, AddOrChangeExtension(targetName, ".pfx")), clientCertWithPrivateKey.Export(X509ContentType.Pfx, "P@55w0rd"));
        }

        /**<summary>adds the extension name to the file if it does not have an extension, otherwise overwrite the existing extension</summary>
         */
        public static string AddOrChangeExtension(string path, string extension)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }
                path += extension;
            }
            else
            {
                path = Path.ChangeExtension(path, extension);
            }
            return path;
        }
    }
}
