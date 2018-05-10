using System;
using System.Threading;
using UIKit;
using Xamarin.iOS_BAC_.Services;

namespace Xamarin.iOS_BAC_.Views
{
    public partial class BackgroundThreadViewController : UIViewController
    {
        private readonly PrimeService _primeService;
        public BackgroundThreadViewController (IntPtr handle) : base (handle)
        {
            _primeService = new PrimeService();
        }

        partial void UIButton6078_TouchUpInside(UIButton sender)
        {
            StartThread();
        }

        private void StartThread()
        {
            new Thread(() =>
            {
                var res = _primeService.GetPrime();

                this.InvokeOnMainThread(() =>
                {
                    this.ResultLabel.Text = res.ToString();
                });
            }).Start();
        }
    }
}