using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Overkill.Common.Enums;
using Overkill.Core;
using Overkill.Core.Connections;
using Overkill.Core.Interfaces;
using Overkill.Proxies;
using Overkill.Proxies.Interfaces;
using Overkill.PubSub;
using Overkill.PubSub.Interfaces;
using Overkill.Services;
using Overkill.Services.Interfaces.Services;
using Overkill.Services.Services;
using Overkill.Util.Helpers;
using Overkill.Websockets;
using Overkill.Websockets.Interfaces;
using Overkill.Websockets.MessageHandlers;
using Overkill.Websockets.MessageHandlers.Input;
using Overkill.Websockets.Messages;
using Overkill.Websockets.Messages.Input;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Overkill
{
    class Program
    {
        static void Main(string[] args)
        {
            OverkillConfiguration config = new OverkillConfiguration();

            AppDomain.CurrentDomain.AssemblyResolve += (sender, evt) =>
            {
                var assemblyFile = evt.Name.Contains(",") ? evt.Name.Substring(0, evt.Name.IndexOf(",")) : evt.Name;

                assemblyFile += ".dll";

                var absoluteFolder = new FileInfo((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).Directory.FullName;
                var targetPath = Path.Combine(absoluteFolder, assemblyFile);

                try
                {
                    return Assembly.LoadFile(targetPath);
                } catch(Exception)
                {
                    return null;
                }
            };

            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder
                    .AddJsonFile("configuration.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                })
                .ConfigureServices((hostContext, services) =>
                {
                    hostContext.Configuration.Bind(config);

                    Boot.SetupConfiguration(config);
                    Boot.LoadVehicleDriverWithDependencies(services);
                    Boot.LoadPluginsWithDependencies(services);

                    services.AddSingleton<ILoggerFactory, LoggerFactory>();
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    services.AddLogging(builder =>
                    {
                        builder
                            .AddConsole()
                            .AddDebug()
                            .SetMinimumLevel(LogLevel.Debug);
                    });

                    services.AddSingleton<ISoundOut, WaveOut>(x => new WaveOut() { Device = WaveOutDevice.DefaultDevice });
                    
                    //Core
                    services.AddSingleton<IOverkillConfiguration>(_ => new OverkillConfiguration(config));
                    services.AddSingleton<IInputService, InputService>();
                    services.AddSingleton<IPubSubService, PubSubService>();
                    services.AddSingleton<IVideoTransmissionService, FFmpegVideoTransmissionService>();
                    services.AddSingleton<IPluginService, PluginService>();

                    //Proxies
                    services.AddSingleton<IHttpProxy, HttpProxy>();
                    services.AddSingleton<IProcessProxy, ProcessProxy>();
                    services.AddSingleton<IFilesystemProxy, FilesystemProxy>();
                    services.AddSingleton<ISoundPlayerProxy, SoundPlayerProxy>();
                    services.AddSingleton<IThreadProxy, ThreadProxy>();
                    services.AddSingleton<ISerialProxy, SerialProxy>();

                    //Services
                    services.AddSingleton<IAudioService, AudioService>();
                    services.AddSingleton<INetworkingService, NetworkingService>();

                    //Websockets
                    services.AddSingleton<IWebsocketService, WebsocketService>();

                    //Handlers
                    services.AddSingleton<IWebsocketMessageHandler<GamepadButtonInputMessage>, GamepadButtonInputMessageHandler>();
                    services.AddSingleton<IWebsocketMessageHandler<GamepadJoystickInputMessage>, GamepadJoystickInputMessageHandler>();
                    services.AddSingleton<IWebsocketMessageHandler<GamepadTriggerInputMessage>, GamepadTriggerInputMessageHandler>();
                    services.AddSingleton<IWebsocketMessageHandler<KeyboardInputMessage>, KeyboardInputMessageHandler>();
                    services.AddSingleton<IWebsocketMessageHandler<LocomotionMessage>, LocomotionMessageHandler>();
                    services.AddSingleton<IWebsocketMessageHandler<VehiclePingMessage>, VehiclePingMessageHandler>();
                    services.AddSingleton<IWebsocketMessageHandler<VehiclePlaySoundMessage>, VehiclePlaySoundMessageHandler>();

                    //Positioning - optional, configurable
                    if (config.Positioning.Enabled)
                    {
                        switch (config.Positioning.Type)
                        {
                            case PositioningSystem.QuectelCM:
                                services.AddSingleton<IPositioningService, QuectelModemPositioningService>();
                                break;
                        }
                    }

                    //Connection protocol - configurable
                    switch(config.VehicleConnection.Type)
                    {
                        case CommunicationProtocol.TCP:
                            services.AddSingleton<IConnectionInterface, TcpConnectionInterface>();
                            break;
                        case CommunicationProtocol.GPIO:
                            services.AddSingleton<IConnectionInterface, GpioConnectionInterface>();
                            break;
                    }
                })
                .UseConsoleLifetime()
                .Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                //Do the actual bootup and run/initialize things
                Boot.SetupServiceProvider(services);
                Boot.LoadTopics();
                Boot.LoadConfiguredServices();
                Boot.Finish();

                while (true) 
                {

                }
            }
        }
    }
}
