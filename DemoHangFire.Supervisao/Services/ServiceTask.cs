using System;

namespace DemoHangFire.Supervisao.Services
{
    public class ServiceTask : IServiceTask
    {
        public void Processar(string nomeServico)
        {
            Console.WriteLine($"Executando serviço: {nomeServico}");
        }
    }
}
