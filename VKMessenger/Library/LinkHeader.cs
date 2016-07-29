using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Utils;

namespace VKMessenger.Library
{
    public sealed class LinkHeader
    {
        public Link Link { get; set; }

        public string Title { get; set; }

        public TextWrapping TitleWrapping { get; set; }

        public string Domain { get; set; }

        public string Description { get; set; }

        public Visibility DescriptionVisibility { get; set; }

        public string Thumbnail { get; set; }

        public SolidColorBrush ThumbnailBackground { get; set; }

        public string ThumbnailPlaceholderLetter { get; set; }

        public string Url { get; set; }

        public LinkHeader(Link link)
        {
            this.Link = link;
            this.Title = Extensions.ForUI(link.title);
            this.Description = Extensions.ForUI(link.description);
            this.Domain = Extensions.ForUI(link.url);
            this.Url = link.url;
            if (this.Title.Length > 55)
            {
                this.DescriptionVisibility = Visibility.Collapsed;
                this.TitleWrapping = TextWrapping.Wrap;
            }
            else
            {
                this.DescriptionVisibility = Visibility.Visible;
                this.TitleWrapping = TextWrapping.NoWrap;
                if (string.IsNullOrWhiteSpace(this.Description))
                {
                    this.Description = "...";
                }
            }
            this.Thumbnail = link.image_src;
            if (this.Thumbnail != null)
            {
                this.ThumbnailBackground = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"];
                this.ThumbnailPlaceholderLetter = "";
            }
            else
            {
                this.ThumbnailBackground = (SolidColorBrush)Application.Current.Resources["PhonePollSliderBackgroundBrush"];
                this.ThumbnailPlaceholderLetter = this.Domain[0].ToString().ToUpper();
            }
        }
    }
}
