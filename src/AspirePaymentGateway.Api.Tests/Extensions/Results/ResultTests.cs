using BinaryMash.Extensions.Results;
using Shouldly;

namespace AspirePaymentGateway.Api.Tests.Extensions.Results
{
    public class ResultTests
    {
        [Fact]
        public void OkResultConstructor()
        {
            // Act
            var result = new SuccessResult();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.ErrorDetail.ShouldBeNull();
        }

        [Fact]
        public void OkResultConstructorViaStatic()
        {
            // Act
            var result = Result.Success;

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.IsFailure.ShouldBeFalse();
            result.ErrorDetail.ShouldBeNull();
        }

        [Fact]
        public void OkResultWithValueConstructor()
        {
            // Arrange
            MyClass myClass = new(123);

            // Act
            var result = new SuccessResult<MyClass>(myClass);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.IsFailure.ShouldBeFalse();
            result.Value.ShouldBe(myClass);
            result.ErrorDetail.ShouldBeNull();
        }

        [Fact]
        public void OkResultWithValueViaImplicitConstruction()
        {
            // Arrange
            MyClass myClass = new(456);

            // Act
            Result<MyClass> result = myClass;

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.IsFailure.ShouldBeFalse();
            result.Value.ShouldBe(myClass);
            result.ErrorDetail.ShouldBeNull();
        }

        [Fact]
        public void ErrorResultConstructor()
        {
            // Arrange
            ErrorDetail errorDetail = new("SomeError", "Some error message");

            // Act
            var result = new FailureResult(errorDetail);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.ErrorDetail.ShouldBe(errorDetail);
        }

        [Fact]
        public void ErrorResultConstructorViaStaticConstruction()
        {
            // Arrange
            ErrorDetail errorDetail = new("SomeError", "Some error message");

            // Act
            var result = Result.Failure(errorDetail);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.ErrorDetail.ShouldBe(errorDetail);
        }

        [Fact]
        public void ErrorResultWithValueConstructor()
        {
            // Arrange
            ErrorDetail errorDetail = new("SomeError", "Some error message");

            // Act
            var result = new FailureResult<MyClass>(errorDetail);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Value.ShouldBeNull();
            result.ErrorDetail.ShouldBe(errorDetail);
        }

        [Fact]
        public void ErrorResultWithValueViaStaticConstruction()
        {
            // Arrange
            ErrorDetail errorDetail = new("SomeError", "Some error message");

            // Act
            Result<MyClass> result = Result.Failure<MyClass>(errorDetail);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Value.ShouldBeNull();
            result.ErrorDetail.ShouldBe(errorDetail);
        }

        private sealed record class MyClass(int SomeValue);
    }
}
