using EPiServer.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Wangkanai.Detection;

namespace AlloyMVC.Business.Channels
{
    //<summary>
    // Defines the 'Mobile' content channel
    //</summary>
    public class MobileChannel : DisplayChannel
    {
        public const string Name = "mobile";

        public override string ChannelName => Name;

        public override string ResolutionId => typeof(IphoneVerticalResolution).FullName;

        public override bool IsActive(HttpContext context)
        {
            var detection = context.RequestServices.GetRequiredService<IDetection>();
            return detection.Device.Type == DeviceType.Mobile;
        }
    }
}
