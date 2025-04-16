# aspire-payment-gateway

Playing around with .NET Aspire to build a simple payment gateway in order to explore a variety of modern .NET tech and system architectures in a distributed system.

## Requirements
- .NET 9
- Docker Desktop
- An AWS profile is configured on the machine. We don't use AWS, but we do run Dynamo locally and the integration requires the profile

## Running
- Run the `AspirePaymentGateway.AppHost` project to start everything and bring up the system dashboard. If it doesn't open in the browser automatically then find the link in the console output.

### System Resources

Here's a description of each of the systen resources visible in the dashboard.

- `payment-gateway` - the payment gateway service.
- `dynamodb` - an event store for the payment gateway.
- `keycloak`- an OAuth2 identity server used to issue tokens for securly accessing the payment gateway
- `fraud-api` - used by the payment gateway, this service provides fraud checks on payment requests. 
- `mock-bank-api` - used by the payment gateway, this service is a mock implementation of the 3rd party bank that will authorise payments

#### System Resources Endpoints
- The published reaource endpoints for the API services bring up the OpenAPI specs for these services.
- For dynamo-db and keycloak the published endpoints bring up an admin console.

### Submitting Payment Requests
- Open the OpenAPI spec for `payment-gateway` and go to `Make Payment` > `Test Request`
- The endpoint uses Bearer authentication; in the Authentication section select Auth Type Bearer
- Request a token from the identity server and paste it into the Bearer Token field
  - requesting a token is not part of this API spec; you should be able to send a request using the `AspirePaymentGateway.Api.http` file in the AspirePaymentGateway.Api project - use the `access_token` you get in the response from the identity server
- Send the request
  - The default values in the OpenAPI spec are for a nominakl payment request, which should be accepted. Explore the implementation of the fraud API and bank API to discover how to generate failure scenarios. 

## Solution Projects:

- `AspirePaymentGateway.Api` - a payment gateway that receives payment requests from our customers
- `AspirePaymentGateway.AppHost` - the .NET Aspire AppHost that defines the system deployment model and provides the run-time dashboard and service discovery
- `AspirePaymentGateway.FraudApi` - a fraud API that 
- `AspirePaymentGateway.MockBankApi` - a mock implementation of the 3rd party bank that authorises' payments 
- `AspirePaymentGateway.ServiceDefaults` - the .NET Aspire default service configuration


## Technical Points of Interest

### Architecture

- There is fundamental desire to avoid unnecessary complexity and over-engineering: SOLID principles are considered a guide rather than a rigid dogma. Uncle Bob might not like what he sees here, but we're aiming for the simplest pragmatic solution that doesn't stink, as this will be easier to maintain. 
- So, we have a clean architecture, organised by feature rather than by architectural layer. This means that we end up with a coherent structure where closely related files live alongside each other in the solution => less jumping around in code.
- And, we've ended up with a domain built using a fairly naive event-sourcing implementation. We didn't start with this; it has evolved this way as the domain got more complex. 

### Implementation details
- .NET 9 [no longer supports [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) out of the box; as a replacement [Microsoft introduced](https://github.com/dotnet/aspnetcore/issues/54599) `Microsoft.AspNetCore.OpenApi` to generate OpenAPI specs, but this offers no UI support. We're using [Scalar](https://github.com/scalar/scalar) to provide a UI in the development environment.
  - the OpenAPI spec is generated from the annotations on the DTOs.
- JSON serialization on the dependent APIs is optimised by using source generation
  - Eg, see `BankApiContractsContext`.
- Logging is optimised using source generation via the `LoggerMessage` attribute
- [Record](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) classes are used for value objects that are immutable - ie, DTOs, events
- [Refit](https://github.com/reactiveui/refit) is used to integrate with the bank API and fraud API. We need write virtually no code to integrate with the external services.
- An implementation of the Result pattern is used throughout; this allows us to build a consistent way of indicating success/failure with strongly-typed domain-specific error details whilst avoiding using exceptions for control flow
- We are making the most of [Open Telemetry](https://github.com/open-telemetry) functionality to improve observability.
  - in addition to the standard .net telemetry we get for free, we have domain-specific activities and domain-specific metrics. 

### Testing and Code Quality
- Most testing on AspirePaymentGateway.Api is component testing - ie, end-to-end testing the domain feature as a unit, rather than testing individual classes within the domain. Infrastucture concerns are mocked (API dependencies) or stubbed (in-memory DB).
  - Benefits: 
    - As we're effectively treating the feature implementation as a black box, our tests are only bound by the contracts on the domain boundaries, eg, API dependencies and storage contracts. This means that we can massively refactor the code without having to change the tests.
    - As we fake/stub the IO dependencies, we have no network latency and so the tests still run on a par with traditional class-level unit tests
    - And, because we're using the real implementations for pretty much everything, we know that the tests accurately represent the bejabopir of the system, and the intent of the test is pretty clear
  - Trade-offs:
    - If a test fails it might be more difficult to discover exactly what is causing the problem.
    - Edge-cases are more difficult to test. In these cases we might want to write class-specific unit tests. However, as our copmonent tests are using real implementations of all classes in a given feature, these edge conditions are often scenarios that can never actually arise - eg, null parames in constructors, etc.
- We're using [Verify](https://github.com/VerifyTests/Verify) quite a lot to provide assertion tests. This lets us easily assert on complex objects. Stil not sure how I feel about this.
- A code coverage report can be generated with the `./run-tests.sh` script.
- Static code analysis is provided by [Roslynator](https://github.com/dotnet/roslynator), and applied automatically to all projects. This gives us immediate feedback in the IDE at development time.
- [.editorconfig](https://editorconfig.org/) is used for consistent styling across IDEs


- [Central package management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) using `Directory.package.props`; this means we only need to define package versions in a single place, and so should never have to consolidate nuget packages.
