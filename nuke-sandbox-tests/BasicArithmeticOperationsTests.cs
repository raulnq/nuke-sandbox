using Microsoft.VisualStudio.TestTools.UnitTesting;
using nuke_sandbox_lib;
using System;

namespace nuke_sandbox_tests;

[TestClass]
public class BasicArithmeticOperationsTests
{
    [TestMethod]
    public void adding_1_plus_1_should_be_2()
    {
        var sut = new BasicArithmeticOperations();

        var result = sut.Addition(1, 1);

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void subtracting_1_minus_1_should_be_0()
    {
        var sut = new BasicArithmeticOperations();

        var result = sut.Subtraction(1, 1);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void dividing_4_by_2_should_be_2()
    {
        var sut = new BasicArithmeticOperations();

        var result = sut.Division(4, 2);

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void dividing_4_by_0_should_throw_an_exception()
    {
        var sut = new BasicArithmeticOperations();

        Assert.ThrowsException<InvalidOperationException>(() => sut.Division(4, 0));
    }
}
