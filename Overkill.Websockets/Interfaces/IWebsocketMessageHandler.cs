using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Overkill.Websockets.Interfaces
{
    public interface IWebsocketMessageHandler<T> where T: IWebsocketMessage
    {
        Task<IWebsocketMessage> Handle(T msg);
    }
}
