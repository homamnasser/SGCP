// RoundRobinHandler.cs
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SGCP.IService; // تأكد من تعديل مساحة الاسم لتناسب مشروعك

namespace SGCP.Handlers
{
    /// <summary>
    /// معالج مخصص يستخدم خدمة Round Robin لتحديد عنوان الخادم الخلفي لكل طلب.
    /// </summary>
    public class RoundRobinHandler : DelegatingHandler
    {
        private readonly IRoundRobinDispatcherService _dispatcherService;

        public RoundRobinHandler(IRoundRobinDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // 1. الحصول على عنوان الخادم التالي من خدمة Round Robin
            var nextServerBaseUrl = _dispatcherService.GetNextServer();

            // 2. بناء عنوان URL الجديد للطلب
            // يجمع بين BaseAddress الجديد والمسار الأصلي للطلب
            // نستخدم Uri(string, string) لدمج العنوان الأساسي مع المسار النسبي
            var newUri = new Uri(new Uri(nextServerBaseUrl), request.RequestUri.PathAndQuery);

            // 3. تعيين عنوان URL الجديد للطلب
            request.RequestUri = newUri;

            // 4. تمرير الطلب إلى المعالج التالي (عادةً HttpClientHandler)
            return base.SendAsync(request, cancellationToken);
        }
    }
}
