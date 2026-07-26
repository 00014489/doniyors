using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services.TelegramValidator
{
    public interface ITelegramValidator
    {
        TelegramUser Validate(string initData);
    }
}