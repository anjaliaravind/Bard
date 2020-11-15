using System;
using Bard.Infrastructure;
using Bard.Internal.When;

namespace Bard.Internal.Then
{
    internal class PerformanceMonitor
    {
        private readonly LogWriter _logWriter;

        public PerformanceMonitor(LogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public void AssertElapsedTime(Func<IResponse>? apiRequest, ApiResult apiResult, int? maxElapsedTime)
        {
            const int retryCount = 3;

            IResponse RetryApiCall(Func<IResponse> api, int i)
            {
                var response = api();

                _logWriter.LogMessage(
                    $"Retry #{i + 1} Response Time: {response.ElapsedTime?.TotalMilliseconds} (milliseconds)");
                _logWriter.BlankLine();
                _logWriter.LogLineBreak();
                _logWriter.BlankLine();
                return response;
            }

            if (apiRequest == null || apiResult.ExceededElapsedTime(maxElapsedTime) == false)
                return;

            _logWriter.LogHeaderMessage(
                $"The API response took longer than {maxElapsedTime} milliseconds. ({apiResult.ElapsedTime?.TotalMilliseconds})");

            var totalTime = new TimeSpan();

            for (var i = 0; i < retryCount; i++)
            {
                var response = RetryApiCall(apiRequest, i);

                totalTime = totalTime.Add(response.ElapsedTime.GetValueOrDefault());
            }

            var averageTime = totalTime.Divide(retryCount);

            _logWriter.LogMessage($"Average Response Time: {averageTime.TotalMilliseconds} (milliseconds)");

            apiResult.AssertElapsedTime(averageTime, maxElapsedTime);
        }
    }
}