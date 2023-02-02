using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcCert.Protos;

namespace GrpcCert.Services
{
    public class TestService:Test.TestBase
    {

        public override Task<TestMessage> SendMessage(TestMessage request, ServerCallContext context)
        {
            return Task.FromResult(new TestMessage { Payload = "test" });
        }
    }
}
