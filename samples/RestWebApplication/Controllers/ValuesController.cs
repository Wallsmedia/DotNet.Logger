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
        ILoggerFactory _logFactory;
        ILogger _logInfo;
        ILogger _logFatalError;

        public ValuesController(ILoggerFactory logFactory)
        {
            _logFactory = logFactory;
            _logInfo = _logFactory?.CommonInfoLogger();
            _logFatalError = _logFactory?.FatalErrorLogger();
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
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            _logInfo.LogInformation("Post([FromBody] string value) has been invoked");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            _logInfo.LogInformation("Put(int id, [FromBody] string value) has been invoked");
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _logInfo.LogInformation("Delete(int id) has been invoked");
        }
    }
}
