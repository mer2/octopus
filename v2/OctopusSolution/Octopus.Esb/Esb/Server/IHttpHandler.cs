﻿using Microsoft.AspNetCore.Http;

namespace Octopus.Esb.Server
{
    public interface IHttpHandler
    {
		bool IsHandleable(HttpContext context);
		void ProcessRequest(HttpContext context);
	}
}