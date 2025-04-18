using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspirePaymentGateway.Tests
{
    public class PaymentGatewayTestClient
    {
        private readonly HttpClient httpClient;

        public PaymentGatewayTestClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

    }
}
