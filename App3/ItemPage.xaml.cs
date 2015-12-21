﻿using App3.Common;
using App3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Xml.Serialization;
using Windows.System;
using Windows.UI.Xaml.Documents;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace App3
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private XmlReader xmlReader;
        private ExpertSystem expertSystem;

        public ItemPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

        } 

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data.
            var list = (List<String>) e.NavigationParameter;
            var item = await SampleDataSource.GetItemAsync((list.ElementAt(0)));
            this.DefaultViewModel["Item"] = item;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            var list = (List<String>)e.Parameter;
            String fileName = list.ElementAt(1);
            xmlReader = XmlReader.Create(fileName, new XmlReaderSettings());

            XmlSerializer x = new XmlSerializer(typeof(ExpertSystem), "http://tempuri.org/XMLSchema.xsd");
            expertSystem = (ExpertSystem)x.Deserialize(xmlReader);

            /*  this.WebViewDescription.NavigateToString(expertSystem.description);

              this.textBlock1.Text = expertSystem.description;

              String sourceshtml = "";

              foreach (Source s in expertSystem.sources)
              {
                  sourceshtml += s.linkDescription + "</br><font color=\"blue\">" + s.link + "</font></br></br>";
              }

              this.WebViewSources.NavigateToString(sourceshtml);*/
            DescriptionTextBlock.Blocks.Clear();
            SourcesTextBlock.Blocks.Clear();

            Run r = new Run();
            r.Text = expertSystem.description;
            Paragraph p = new Paragraph();
            p.Inlines.Add(r);

            DescriptionTextBlock.Blocks.Add(p);

            foreach (var s in expertSystem.sources)
            {
                Run description = new Run();
                description.Text = s.linkDescription + "\n";

                Run linklink = new Run();
                linklink.Text = s.link + "\n";
                
                Hyperlink link = new Hyperlink();
                link.Inlines.Add(linklink);
                link.NavigateUri = new System.Uri(s.link);

                Paragraph p1 = new Paragraph();
                p1.Inlines.Add(description);
                p1.Inlines.Add(link);

                SourcesTextBlock.Blocks.Add(p1);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (PivotPage), expertSystem.questions);
        }
    }
}