/*
 * Copyright (C) 2011 uhttpsharp project - http://github.com/raistlinthewiz/uhttpsharp
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace uhttpsharp
{
    public sealed class HttpServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _port;
        
        private TcpListener _listener;
        private bool _isActive;

        private readonly IList<IHttpRequestHandler> _handlers = new List<IHttpRequestHandler>();

        private readonly IHttpRequestProvider _requestProvider;


        public HttpServer(int port, IHttpRequestProvider requestProvider)
        {
            _port = port;
            _requestProvider = requestProvider;
        }

        public void Use(IHttpRequestHandler handler)
        {
            _handlers.Add(handler);
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();

            _isActive = true;

            Task.Factory.StartNew(Listen);
        }

        private async void Listen()
        {
            Logger.InfoFormat("Embedded uhttpserver started @ {0}:{1}", IPAddress.Loopback, _port);

            while (_isActive)
            {
                new HttpClient(await _listener.AcceptTcpClientAsync(), _handlers, _requestProvider);
            }

            Logger.InfoFormat("Embedded uhttpserver stopped @ {0}:{1}", IPAddress.Loopback, _port);
        }

        public void Dispose()
        {
            _isActive = false;
        }
    }
}