namespace nuke_sandbox_lib;

public class BasicArithmeticOperations
{
    public decimal Addition(decimal a, decimal b)
    {
        return a + b;
    }

    public decimal Subtraction(decimal a, decimal b)
    {
        return a - b;
    }

    public decimal Multiplication(decimal a, decimal b)
    {
        return a * b;
    }

    public decimal Division(decimal a, decimal b)
    {
        if (b == 0)
        {
            throw new InvalidOperationException();
        }
        return a / b;
    }
}
