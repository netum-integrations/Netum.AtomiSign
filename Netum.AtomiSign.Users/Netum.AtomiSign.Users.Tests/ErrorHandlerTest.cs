using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Netum.AtomiSign.Users.Tests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        Func<Task> action = async () => await AtomiSign.Users(
            DefaultInput(),
            DefaultConnection(),
            DefaultOptions(),
            CancellationToken.None);

        var ex = Assert.ThrowsAsync<Exception>(action);

        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = await AtomiSign.Users(DefaultInput(), DefaultConnection(), options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        Func<Task> action = async () => await AtomiSign.Users(
            DefaultInput(),
            DefaultConnection(),
            options,
            CancellationToken.None);

        var ex = Assert.ThrowsAsync<Exception>(action);

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}
