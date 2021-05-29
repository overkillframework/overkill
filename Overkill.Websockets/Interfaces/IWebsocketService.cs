using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Overkill.Websockets.Interfaces
{
    public interface IWebsocketService
    {
        Task Start();
        Task Connect();
        void RegisterMessages();
        void SendMessage(IWebsocketMessage message);
        Task Handle(string json);
    }
}
