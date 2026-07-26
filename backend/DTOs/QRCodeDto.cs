using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class QRCodeDto
    {
        public string QrToken { get; set; } = string.Empty;
        public int Points { get; set; }
    }
}