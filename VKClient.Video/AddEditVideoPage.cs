using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Video.Library;
using Windows.Storage;

namespace VKClient.Video
{
    public partial class AddEditVideoPage : PageBase
    {
        private ApplicationBarIconButton _appBarButtonCommit = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/check.png", UriKind.Relative),
            Text = CommonResources.AppBarMenu_Save
        };
        private ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
        {
            IconUri = new Uri("Resources/appbar.cancel.rest.png", UriKind.Relative),
            Text = CommonResources.AppBar_Cancel
        };
        private ApplicationBar _appBar = new ApplicationBar()
        {
            BackgroundColor = VKConstants.AppBarBGColor,
            ForegroundColor = VKConstants.AppBarFGColor
        };
        private bool _isInitialized;
        //private bool _isSaving;

        private AddEditVideoViewModel VM
        {
            get
            {
                return this.DataContext as AddEditVideoViewModel;
            }
        }

        public AddEditVideoPage()
        {
            this.InitializeComponent();
            this.BuildAppBar();
            this.SuppressMenu = true;
            this.Loaded += new RoutedEventHandler(this.AddEditVideoPage_Loaded);
            this.ucPrivacyHeaderView.OnTap = new Action(this.PrivacyViewTap);
            this.ucPrivacyHeaderComment.OnTap = new Action(this.PrivacyCommentTap);
            this.ucHeader.HideSandwitchButton = true;
        }

        private void AddEditVideoPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void PrivacyCommentTap()
        {
            Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
            {
                PrivacyForEdit = this.VM.CommentVideoPrivacyVM,
                UpdatePrivacyCallback = (Action<PrivacyInfo>)(pi => this.VM.CommentVideoPrivacyVM = new EditPrivacyViewModel(this.VM.CommentVideoPrivacyVM.PrivacyQuestion, pi, "", (List<string>)null))
            });
        }

        private void PrivacyViewTap()
        {
            Navigator.Current.NavigateToEditPrivacy(new EditPrivacyPageInputData()
            {
                PrivacyForEdit = this.VM.ViewVideoPrivacyVM,
                UpdatePrivacyCallback = (Action<PrivacyInfo>)(pi => this.VM.ViewVideoPrivacyVM = new EditPrivacyViewModel(this.VM.ViewVideoPrivacyVM.PrivacyQuestion, pi, "", (List<string>)null))
            });
        }

        private void BuildAppBar()
        {
            this._appBarButtonCommit.Click += new EventHandler(this._appBarCommit_Click);
            this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
            this._appBar.Buttons.Add((object)this._appBarButtonCommit);
            this._appBar.Buttons.Add((object)this._appBarButtonCancel);
            this.ApplicationBar = (IApplicationBar)this._appBar;
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            if (this.VM.IsSaving)
                this.VM.Cancel();
            else
                this.NavigationService.GoBackSafe();
        }

        private void _appBarCommit_Click(object sender, EventArgs e)
        {
            //this._isSaving = true;
            this._appBarButtonCommit.IsEnabled = false;
            this.VM.Description = this.textBoxDescription.Text;
            this.VM.Name = this.textBoxName.Text;
            this.Focus();
            this.VM.Save((Action<bool>)(res => Execute.ExecuteOnUIThread((Action)(() =>
            {
                this._appBarButtonCommit.IsEnabled = true;
                //this._isSaving = false;
                this.UpdateAppBar();
                if (res)
                {
                    this.NavigationService.GoBackSafe();
                }
                else
                {
                    if (this.VM.C != null && this.VM.C.IsSet)
                        return;
                    new GenericInfoUC().ShowAndHideLater(CommonResources.Error, (FrameworkElement)null);
                }
            }))));
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (this._isInitialized)
                return;
            if (this.NavigationContext.QueryString.ContainsKey("VideoToUploadPath"))
            {
                this.DataContext = (object)AddEditVideoViewModel.CreateForNewVideo(this.NavigationContext.QueryString["VideoToUploadPath"], long.Parse(this.NavigationContext.QueryString["OwnerId"]));
            }
            else
            {
                long ownerId = long.Parse(this.NavigationContext.QueryString["OwnerId"]);
                long num = long.Parse(this.NavigationContext.QueryString["VideoId"]);
                VKClient.Common.Backend.DataObjects.Video video1 = ParametersRepository.GetParameterForIdAndReset("VideoForEdit") as VKClient.Common.Backend.DataObjects.Video;
                long videoId = num;
                VKClient.Common.Backend.DataObjects.Video video2 = video1;
                this.DataContext = (object)AddEditVideoViewModel.CreateForEditVideo(ownerId, videoId, video2);
            }
            this.UpdateAppBar();
            this._isInitialized = true;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            AddEditVideoViewModel.PickedExternalFile = (StorageFile)null;
        }

        private void textBoxName_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            this._appBarButtonCommit.IsEnabled = !string.IsNullOrWhiteSpace(this.textBoxName.Text);
        }

    }
}
