using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Model
{
    public class ValidateResponse
    {
        public bool IsValid { get; set; }
        public List<string> ErrorList { get; set; }

        public ValidateResponse()
        {
            ErrorList = new List<string>();
        }
    }
}
