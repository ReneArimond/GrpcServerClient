using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcClient.Protos;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Delay(2000); // give server some time to start
            using var channel = CreateChannelWithCert("https://localhost:5001", new X509Certificate2("GrpcClient.pfx", "P@55w0rd"));
            var client = new Test.TestClient(channel);
            await client.SendMessageAsync(new TestMessage { Payload = "test client" });
        }

        public static GrpcChannel CreateChannelWithCert(
            string baseAddress,
            X509Certificate2 certificate)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(certificate);

            var channel = GrpcChannel.ForAddress(baseAddress, new GrpcChannelOptions
            {
                HttpHandler = handler
            });

            return channel;
        }
    }
}
