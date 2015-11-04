using App3.Common;
using App3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace App3
{
    public sealed partial class PivotPage : Page
    {
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private List<Question> questions;
        private List<KeyValuePair<Question, String>> answeredQuestions;
        private List<Question> futureConclusions;
        private Question presentQuestion;

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            questions = new List<Question>();
            answeredQuestions = new List<KeyValuePair<Question, string>>();
            futureConclusions = new List<Question>();
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
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-1");
            this.DefaultViewModel[FirstGroupName] = sampleDataGroup;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            var itemDescription = ((SampleDataItem) e.ClickedItem).Description;
            List<String> list = new List<string>();
            list.Add(itemId);
            list.Add(itemDescription);
            if (itemId.Equals("file"))
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".xml");

                // Launch file open picker and caller app is suspended and may be terminated if required
                openPicker.PickSingleFileAndContinue();
            }
            else if (!Frame.Navigate(typeof(ItemPage), list))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
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

            //przy załadowaniu nowego pliku czyszczenie historii i przyszłych wniosków
            //właściwie to oba mogą byc bezpośrednio na jakieś listview, nie muszą być jeszcze listami

            if (Frame.BackStack.Count != 0)
            {
                if (Frame.BackStack.Last().SourcePageType == typeof (ItemPage))
                {
                    List<String> headers = new List<string>();
                    String test = "";

                    foreach (PivotItem pi in this.pivot.Items)
                    {
                        headers.Add(pi.Header.ToString());
                    //    test += pi.Header.ToString() + "\n";
                    }
                  //  MessageDialog md = new MessageDialog(test);
                 //   await md.ShowAsync();
                    if (!(headers.Contains("History") && headers.Contains("Conclusions")))
                    {
                        PivotItem history = new PivotItem();
                        history.Header = "History";

                        PivotItem conclusions = new PivotItem();
                        conclusions.Header = "Conclusions";


                        this.pivot.Items.Insert(2, history);
                        this.pivot.Items.Insert(4, conclusions);
                    }
                    questions = (List<Question>) e.Parameter;

                    yesButton.Visibility = Visibility.Visible;
                    noButton.Visibility = Visibility.Visible;
                    presentQuestion = questions.ElementAt(0);
                    questionsWebView.NavigateToString(presentQuestion.content);

                    foreach (Question q in questions)
                    {
                        if(q.isConclusion)
                            futureConclusions.Add(q);
                    }
                    
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        //1. dodawanie pytań i odpowiedzi do answeredQuestions, 
        //2. sprawdzanie czy jakiś wniosek już nie może być accessed (jakiś sprytny algorytm) i jeśli nie może, to kasowanie z futureConclusions
        //3. zmiana pytania w questionsWbView, jeśli pytanie jest wnioskiem, to buttony na collapsed


        private void noButton_Click(object sender, RoutedEventArgs e)
        {
           addAnsweredQuestion(presentQuestion, "No");
        }

        private void yesButton_Click(object sender, RoutedEventArgs e)
        {
            addAnsweredQuestion(presentQuestion, "Yes");
        }

        private void addAnsweredQuestion(Question q, String answer)
        {

        }

    }
}