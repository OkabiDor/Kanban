using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class Response
    {
        public string? ErrorMessage { get; set; }
        public object? ReturnValue { get; set; }

        public Response() { }

        public Response(string errorMessage, object returnValue)
        {
            ErrorMessage = errorMessage;
            ReturnValue = returnValue;
        }
        /// <summary>
        /// returns true iff the error message is null
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty() { return ErrorMessage == null; }
    }

}
