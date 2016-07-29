using Microsoft.Phone.Scheduler;
using System;
using System.Diagnostics;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Social;
using VKClient.Audio.Base.Utils;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.ScheduledUpdater
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        public ScheduledAgent()
        {
            if (ScheduledAgent._classInitialized)
                return;
            ScheduledAgent._classInitialized = true;
            Deployment.Current.Dispatcher.BeginInvoke((Action)(() => Application.Current.UnhandledException += new EventHandler<ApplicationUnhandledExceptionEventArgs>(this.ScheduledAgent_UnhandledException)));
        }

        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Logger.Instance.Error("UNHANDLED exception in ScheduledAgent" + e.ExceptionObject);
            if (!Debugger.IsAttached)
                return;
            Debugger.Break();
        }

        protected override async void OnInvoke(ScheduledTask task)
        {
            Logger.Instance.Info("Entering ScheduledAgent.OnInvoke, task.Name=" + task.Name ?? "");
            try
            {
                AppGlobalStateManager.Current.Initialize(true);
                if (AppGlobalStateManager.Current.LoggedInUserId == 0)
                    return;
                if (task.Name == "ExtensibilityTaskAgent")
                {
                    await SocialDataManager.Instance.ProcessSocialOperationsQueue();
                    this.NotifyComplete();
                }
                else
                    SecondaryTileManager.Instance.UpdateAllExistingTiles((Action<bool>)(resSecondary => CountersService.Instance.GetCountersWithLastMessage((Action<BackendResult<CountersWithMessageInfo, ResultCode>>)(res =>
                    {
                        if (res.ResultCode == ResultCode.Succeeded)
                        {
                            string content1 = "";
                            string content2 = "";
                            string content3 = "";
                            if (res.ResultData.LastMessage != null && BaseFormatterHelper.UnixTimeStampToDateTime((double)res.ResultData.LastMessage.date, true) > AppGlobalStateManager.Current.GlobalState.LastDeactivatedTime)
                                MessageHeaderFormatterHelper.FormatForTileIntoThreeStrings(res.ResultData.LastMessage, res.ResultData.User, out content1, out content2, out content3);
                            int messages = res.ResultData.Counters.messages;
                            TileManager.Instance.SetContentAndCount(content1, content2, content3, messages, (Action)(() => this.NotifyComplete()));
                        }
                        else
                            this.NotifyComplete();
                    }))));
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("ScheduledAgent.OnInvoke failed", ex);
                this.NotifyComplete();
            }
        }
    }
}
