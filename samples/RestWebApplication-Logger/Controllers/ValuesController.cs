// \\     |/\  /||
//  \\ \\ |/ \/ ||
//   \//\\/|  \ || 
// Copyright © Artem Paskhin 2018. All rights reserved.
// Wallsmedia LTD 2018:{Artem Paskhin}
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Dot Net Sample logger for Microsoft.Extensions.Logging.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RestWebApplication.Controllers
{

    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private static string Component { get; } = typeof(ValuesController).Assembly.GetName().Name;
        private static string ProcessName { get; } = nameof(ValuesController);

        ILoggerFactory _logFactory;
        ILogger _logInfo;

        public ValuesController(ILoggerFactory logFactory)
        {
            _logFactory = logFactory;
            _logInfo = _logFactory?.CreateLogger(Component) ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logInfo.LogInformation("IEnumerable<string> Get() has been invoked");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            _logInfo.LogInformation("Get(int id) has been invoked");
            if (id  == 0)
            {
                _logInfo.LogError($"Component:{Component} Process :{ProcessName}  the  value should be more then zero");
            }
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            _logInfo.LogInformation("Post([FromBody] string value) has been invoked");
            try
            {
                throw new Exception("Demo exception for NLog ");
            }
            catch (Exception ex)
            {
                _logInfo.LogCritical(ex, $"Component:{Component} Process :{ProcessName}  thrown the Exception");
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            _logInfo.LogInformation("Put(int id, [FromBody] string value) has been invoked");

            if(value == null)
            {
                _logInfo.LogError($"Component:{Component} Process :{ProcessName}  the  value should not null for method PUT");
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _logInfo.LogInformation("Delete(int id) has been invoked");
        }
    }
}
