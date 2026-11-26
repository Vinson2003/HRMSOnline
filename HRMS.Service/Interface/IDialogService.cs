using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Service.Interface
{
    public interface IDialogService
    {
        Task<bool> ConfirmAsync(string message);

        Task AlertAsync(string? message);
    }
}
