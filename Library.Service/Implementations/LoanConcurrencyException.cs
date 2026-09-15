namespace Library.Service.Implementations;

public class LoanConcurrencyException : Exception
{
    public LoanConcurrencyException(string message) : base(message)
    {
    }
}
