﻿using Coravel.Invocable;

namespace MemoDown.Services.Background
{
    public class UploadsCleanupService : IInvocable
    {
        private readonly MemoService _memoService;
        private readonly ILogger _logger;

        public UploadsCleanupService(MemoService memoService, ILogger<UploadsCleanupService> logger)
        {
            _memoService = memoService;
            _logger = logger;
        }

        public Task Invoke()
        {
            try
            {
                _memoService.CleanUpUploads();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up uploads");
            }
            return Task.CompletedTask;
        }
    }

    #region Previous Implementation
    //public class UploadsCleanupService : BackgroundService
    //{
    //    private const int CleanUpAt = 3;
    //    private const int CheckingIntervalMinutes = 60;

    //    private readonly IHostApplicationLifetime _lifetime;
    //    private readonly MemoService _service;

    //    public UploadsCleanupService(IHostApplicationLifetime lifetime, MemoService service)
    //    {
    //        _lifetime = lifetime;
    //        _service = service;
    //    }

    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        if (!await WaitForAppStartup(_lifetime, stoppingToken))
    //        {
    //            return;
    //        }

    //        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(CheckingIntervalMinutes));

    //        while (!stoppingToken.IsCancellationRequested &&
    //            await timer.WaitForNextTickAsync(stoppingToken))
    //        {
    //            if (DateTime.Now.Hour == CleanUpAt)
    //            {
    //                try
    //                {
    //                    _service.CleanUpUploads();
    //                }
    //                catch (Exception)
    //                {

    //                }
    //            }
    //        }
    //    }

    //    private static async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    //    {
    //        try
    //        {
    //            var tcs = new TaskCompletionSource();
    //            using var _ = lifetime.ApplicationStarted.Register(() => tcs.SetResult());
    //            await tcs.Task.WaitAsync(stoppingToken).ConfigureAwait(false);
    //            return true;
    //        }
    //        catch (TaskCanceledException)
    //        {
    //            return false;
    //        }
    //    }
    //}
    #endregion
}
