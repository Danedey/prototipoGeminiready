using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prototipoGeminiready
{
    public class AlertService
    {
        public async Task ShowAlertAsync(string title, string message, string cancel)
        {
            var mainPage = Application.Current?.MainPage;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert(title, message, cancel);
            }
        }
    }

}
