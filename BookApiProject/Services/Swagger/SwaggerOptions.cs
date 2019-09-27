using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApiProject.Services.Swagger
{
    public class SwaggerOptions
    {
        public string JsonRoute { get; set; }
        public string Description { get; set; }
        public string UiEndpoint { get; set; }
    }
}
