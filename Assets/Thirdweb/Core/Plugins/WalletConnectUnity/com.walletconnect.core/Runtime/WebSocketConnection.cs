using System;
using System.IO;
using WalletConnectUnity.NativeWebSocket;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectUnity.Core
{
    public sealed class WebSocketConnection : IModule, IJsonRpcConnection
    {
        private bool _disposed;

        private WebSocket _socket;

        public WebSocketConnection(string url)
        {
            if (!Validation.IsWsUrl(url))
                throw new ArgumentException($"[WebSocketConnection] Provided URL is not compatible with WebSocket connection: {url}");

            Context = Guid.NewGuid().ToString();
            Url = url;
        }

        public bool Connected { get; private set; }

        public bool Connecting { get; private set; }

        public string Url { get; private set; }

        public bool IsPaused { get; set; }

        public event EventHandler<string> PayloadReceived;
        public event EventHandler Closed;
        public event EventHandler<Exception> ErrorReceived;
        public event EventHandler<object> Opened;
#pragma warning disable 67
        public event EventHandler<Exception> RegisterErrored;
#pragma warning restore 67

        Task IJsonRpcConnection.Open()
        {
            Register();
            return Task.CompletedTask;
        }

        Task IJsonRpcConnection.Open<T>(T options)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WebSocketConnection));

            if (typeof(string).IsAssignableFrom(typeof(T)))
            {
                var newUrl = options as string;

                if (!Validation.IsWsUrl(newUrl))
                    throw new ArgumentException($"[WebSocketConnection] Provided URL is not compatible with WebSocket connection: {newUrl}");

                Url = newUrl;
            }

            Register();
            return Task.CompletedTask;
        }

        async Task IJsonRpcConnection.Close()
        {
            if (!Connected || _socket == null)
                throw new IOException("Connection already closed");

            if (_disposed)
                throw new ObjectDisposedException(nameof(WebSocketConnection));

            await _socket.Close();
            OnClose(WebSocketCloseCode.Normal);
        }

        async Task IJsonRpcConnection.SendRequest<T>(IJsonRpcRequest<T> requestPayload, object context)
        {
            if (_socket == null || (!Connected && !Connecting))
                throw new IOException("Connection is not open");

            if (_disposed)
                throw new ObjectDisposedException(nameof(WebSocketConnection));

            try
            {
                var json = JsonConvert.SerializeObject(requestPayload);

                await _socket.SendText(json);

                PayloadReceived?.Invoke(this, json);
            }
#if ENABLE_IL2CPP
            catch (JsonSerializationException e) when (e.Message.Contains("Unable to find a constructor"))
            {
                Debug.LogError(
                    $"[WebSocketConnection-{Context}] Failed to serialize request payload. Make sure the managed code stripping is set to 'Minimal'. Error message: {e.Message}");
            }
#endif
            catch (Exception e)
            {
                OnError<T>(requestPayload, e);
            }
        }

        async Task IJsonRpcConnection.SendResult<T>(IJsonRpcResult<T> responsePayload, object context)
        {
            if (_socket == null)
                throw new IOException("Connection is not open");

            if (_disposed)
                throw new ObjectDisposedException(nameof(WebSocketConnection));

            try
            {
                await _socket.SendText(JsonConvert.SerializeObject(responsePayload));
            }
            catch (Exception e)
            {
                OnError<T>(responsePayload, e);
            }
        }

        async Task IJsonRpcConnection.SendError(IJsonRpcError errorPayload, object context)
        {
            if (_socket == null)
                throw new IOException("Connection is not open");

            try
            {
                await _socket.SendText(JsonConvert.SerializeObject(errorPayload));
            }
            catch (Exception e)
            {
                OnError<object>(errorPayload, e);
            }
        }

        public string Name => "websocket-connection";

        public string Context { get; }

        private void Register()
        {
            var unitySyncContext = WalletConnect.UnitySyncContext;
            var isOnMainThread = SynchronizationContext.Current == WalletConnect.UnitySyncContext;

            if (isOnMainThread)
                RegisterCore();
            else
                unitySyncContext.Post(_ => Register(), null);
        }

        private void RegisterCore()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WebSocketConnection));

            if (!Validation.IsWsUrl(Url))
                throw new ArgumentException($"Provided URL is not compatible with WebSocket connection: {Url}");

            if (Connecting || Connected)
                throw new InvalidOperationException("WebSocket is already connecting or connected");

            Connecting = true;

            _socket = new WebSocket(Url);

            _socket.OnOpen += OnOpen;
            _socket.OnMessage += OnMessage;
            _socket.OnClose += OnDisconnect;
            _socket.OnError += OnError;

#if !UNITY_WEBGL || UNITY_EDITOR
            UnityEventsDispatcher.Instance.Tick += OnTick;
#endif

            try
            {
                Connect();
            }
            catch (Exception e)
            {
                OnError<IJsonRpcPayload>(null, e);
                throw;
            }
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void OnTick()
        {
            try
            {
                if (_socket == null || !Connected || Connecting || _disposed)
                    return;

                _socket.DispatchMessageQueue();
            }
#if ENABLE_IL2CPP
            catch (JsonSerializationException e) when (e.Message.Contains("Unable to find a constructor"))
            {
                Debug.LogError(
                    $"[WebSocketConnection-{Context}] Failed to serialize request payload. Make sure the managed code stripping is set to 'Minimal'. Error message: {e.Message}");
            }
#endif
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
#endif

        private void OnOpen()
        {
            Connected = true;
            Connecting = false;

            Opened?.Invoke(this, _socket);
        }

        private void OnMessage(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);

            if (string.IsNullOrWhiteSpace(json))
                return;

            PayloadReceived?.Invoke(this, json);
        }

        private void OnDisconnect(WebSocketCloseCode obj)
        {
            if (obj != WebSocketCloseCode.Normal)
                ErrorReceived?.Invoke(this, new Exception($"Websocket closed with code {obj}"));

            OnClose(obj);
        }

        private void OnError(string message)
        {
            ErrorReceived?.Invoke(this, new WebSocketException(message));

            if (Connecting)
                Connecting = false;

            WCLogger.LogError(Connecting ? $"[{Name}-{Context}] Error happened during connection. Error message: {message}" : $"[{Name}-{Context}] Error: {message}");
        }

        private void OnError<T>(IJsonRpcPayload ogPayload, Exception e)
        {
            if (ogPayload != null)
            {
                var payload = new JsonRpcResponse<T>(
                    ogPayload.Id,
                    new Error
                    {
                        Code = e.HResult,
                        Data = null,
                        Message = e.Message
                    },
                    default
                );

                var json = JsonConvert.SerializeObject(payload);

                //Trigger the payload event, converting the new JsonRpcResponse object to JSON string
                PayloadReceived?.Invoke(this, json);
            }

            WCLogger.LogError(e);
        }

        private void OnClose(WebSocketCloseCode obj)
        {
            Connected = false;
            Connecting = false;

            if (_socket == null)
                return;

#if !UNITY_WEBGL || UNITY_EDITOR
            UnityEventsDispatcher.Instance.Tick -= OnTick;
#endif

            _socket = null;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        // This is a workaround for NativeWebSocket.Connect() blocking the main thread
        private async void Connect()
        {
            await _socket.Connect();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                UnityEventsDispatcher.Instance.Tick -= OnTick;
#endif
                _socket?.Dispose();
            }

            _disposed = true;
        }
    }
}
