using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Extract the correlation trace token from incoming Angular interceptor headers, or initialize a new sequence
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // 2. Respond back with the header token to preserve upstream visibility tracing maps
            context.Response.Headers[CorrelationIdHeaderKey] = correlationId;

            // 3. Inject the identifier safely inside Serilog's diagnostic execution scope
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await _next(context);
            }
        }
    }
}
