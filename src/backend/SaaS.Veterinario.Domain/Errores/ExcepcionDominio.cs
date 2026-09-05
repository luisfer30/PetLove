namespace SaaS.Veterinario.Domain.Errores;

public sealed class ExcepcionDominio : Exception
{
    public ExcepcionDominio()
    {
    }

    public ExcepcionDominio(string mensaje) : base(mensaje)
    {
    }

    public ExcepcionDominio(string mensaje, Exception innerException) : base(mensaje, innerException)
    {
    }
}
