﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Autumn.Http.Sessions;

namespace Autumn.Http.Interceptors;

public interface IHttpRequestInterceptor {

    bool Intercept(HttpListenerRequest request, IHttpSession session);

}
