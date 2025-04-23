using BinaryMash.Extensions.Results;
using Shouldly;

namespace BinaryMash.Extensions.Tests.Results
{
    public class ResultTests
    {
        [Fact]
        public void OkResultConstructor()
        {
            // Act
            var result = new OkResult();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.ErrorDetail.ShouldBeNull();
        }

        [Fact]
        public void OkResultConstructorViaStatic()
        {
            // Act
            var result = Result.Ok;

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
            var result = new OkResult<MyClass>(myClass);

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
            var result = new ErrorResult(errorDetail);

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
            var result = Result.Error(errorDetail);

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
            var result = new ErrorResult<MyClass>(errorDetail);

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
            Result<MyClass> result = Result.Error<MyClass>(errorDetail);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Value.ShouldBeNull();
            result.ErrorDetail.ShouldBe(errorDetail);
        }

        private sealed record class MyClass(int SomeValue);
    }
}
