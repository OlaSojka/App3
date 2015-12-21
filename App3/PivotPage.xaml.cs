using App3.Common;
using App3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
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
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Documents;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace App3
{
    public sealed partial class PivotPage : Page, IFileOpenPickerContinuable 
    {
        public static PivotPage Current;

        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private List<Question> questions;
        private ListView answeredQuestions;
        private ListView fututreConclusionsListView;
      //  private List<KeyValuePair<Question, String>> answeredQuestions;
        private List<Question> futureConclusions;
        private Question presentQuestion;

        private List<int> answeredQuestionsIdList; 

        public PivotPage()
        {
            Current = this;
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            questions = new List<Question>();
            //answeredQuestions = new List<KeyValuePair<Question, string>>();
            answeredQuestions = new ListView();
            answeredQuestions.MaxHeight = questionsGrid.Height - 20;
            fututreConclusionsListView = new ListView();
            fututreConclusionsListView.MaxHeight = questionsGrid.Height - 20;
            futureConclusions = new List<Question>();
            answeredQuestionsIdList = new List<int>();
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
                openPicker.FileTypeFilter.Add(".jpg");

                //
                //
                // XML !!!!!!!!!!!!!!!!!!!!!!!!!!!
                //
                //

                // Launch file open picker and caller app is suspended and may be terminated if required
                openPicker.PickSingleFileAndContinue();
            }
            else if (!Frame.Navigate(typeof(ItemPage), list))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                XmlReader xmlReader = XmlReader.Create("test.xml", new XmlReaderSettings()); // args.Files[0].Name zamiast chestpain.xml
                XmlSerializer x = new XmlSerializer(typeof(ExpertSystem), "http://tempuri.org/XMLSchema.xsd");
                ExpertSystem expertSystem = null;
                try
                {
                    expertSystem = (ExpertSystem) x.Deserialize(xmlReader);
                }
                catch (Exception exception)
                {
                    MessageDialog md1 = new MessageDialog("Invalid XML file");
                    await md1.ShowAsync();
                }
                if (expertSystem != null)
                {
                    MessageDialog md1 = new MessageDialog("Selected file" + args.Files[0].Name);
                    await md1.ShowAsync();
                    processFile(expertSystem.questions);
                }
            }
            else
            {
                MessageDialog md = new MessageDialog("Operation cancelled");
                await md.ShowAsync();
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            //przy załadowaniu nowego pliku czyszczenie historii i przyszłych wniosków
            //właściwie to oba mogą byc bezpośrednio na jakieś listview, nie muszą być jeszcze listami
            if (Frame.BackStack.Count != 0 && Frame.BackStack.Last().SourcePageType == typeof(ItemPage))
            {
               processFile((List<Question>)e.Parameter);   
            }
        }

        private void processFile(List<Question> list)
        {
            List<String> headers = new List<string>();
            String test = "";
            if (answeredQuestions.Items.Count > 0)
                answeredQuestions.Items.Clear();

            if (fututreConclusionsListView.Items.Count > 0)
                fututreConclusionsListView.Items.Clear();

            futureConclusions.Clear();
            answeredQuestionsIdList.Clear();

            foreach (PivotItem pi in this.pivot.Items)
            {
                headers.Add(pi.Header.ToString());
            }

            if (!(headers.Contains("History") && headers.Contains("Conclusions")))
            {
                PivotItem history = new PivotItem();
                history.Header = "History";
                StackPanel stackpanel = new StackPanel();
                stackpanel.Children.Add(answeredQuestions);
                history.Content = stackpanel;

                PivotItem conclusions = new PivotItem();
                conclusions.Header = "Conclusions";
                StackPanel stackpanel2 = new StackPanel();
                stackpanel2.Children.Add(fututreConclusionsListView);
                conclusions.Content = stackpanel2;

                this.pivot.Items.Insert(2, history);
                this.pivot.Items.Insert(4, conclusions);
            }

            questions = list;

            yesButton.Visibility = Visibility.Visible;
            noButton.Visibility = Visibility.Visible;
            backButton.Visibility = Visibility.Visible;
            presentQuestion = findQuestion(0);

            Run run = new Run();
            run.Text = presentQuestion.content;
            Paragraph p = new Paragraph();
            p.Inlines.Add(run);
            questionsTextBlock.Blocks.Clear();
            questionsTextBlock.Blocks.Add(p);

            foreach (Question q in questions)
            {
                if (q.isConclusion)
                {
                    futureConclusions.Add(q);
                    addConclusionToListView(q.content + "\n");
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private Question findQuestion(int id)
        {
            return questions.Find(x => x.ID == id);
        }

        private bool setQuestion(Question q)
        {
            questionsTextBlock.Blocks.Clear();
            Paragraph p = new Paragraph();
            p.TextIndent = 0;
            Run run = new Run();
            run.Text = q.content;

            bool conclusionFlag = false;

            if (q.isConclusion)
            {
                Run conclusion = new Run();
                conclusion.Text = "Conclusion:\n";

                p.Inlines.Add(conclusion);

                noButton.Visibility = Visibility.Collapsed;
                yesButton.Visibility = Visibility.Collapsed;
                conclusionFlag = true;
            }

            presentQuestion = q;

            p.Inlines.Add(run);

            questionsTextBlock.Blocks.Add(p);
            return conclusionFlag;
        }

        private void addConclusionToListView(String text)
        {
            ListViewItem item = new ListViewItem();
            TextBlock tb = new TextBlock();
            tb.TextWrapping = TextWrapping.WrapWholeWords;
            tb.FontSize = 15;
            tb.Text = text;
            item.Content = tb;
            fututreConclusionsListView.Items.Add(item);
        }

        public static List<List<int>> listOLists = new List<List<int>>();

        public void test(List<int> prev, Question q)
        {
            Question q1 = findQuestion(q.no);
            Question q2 = findQuestion(q.yes);

            if (q.isConclusion)
            {
                futureConclusions.Add(q);
                return;
            }

            List<int> newVector = new List<int>(prev);
            newVector.Add(q1.ID);

            listOLists.Add(newVector);
            test(newVector, q1);

            prev.Add(q2.ID);
            test(prev, q2);
        }

        private void checkConclusions()
        {
            if (fututreConclusionsListView.Items.Count > 0)
                fututreConclusionsListView.Items.Clear();

            futureConclusions.Clear();
            listOLists.Clear();
            List<int> nowa = new List<int>();
            nowa.Add(presentQuestion.ID);
            listOLists.Add(nowa);

            test(nowa, presentQuestion);

            foreach (Question q in futureConclusions.Distinct().ToList())
            {
                addConclusionToListView(q.content + "\n");
            }
        }

        private void updateConclusions(Question q)
        {
            if (!setQuestion(q))
                checkConclusions();
            else
            {
                fututreConclusionsListView.Items.Clear();
                addAnsweredQuestion(q);
            }
        }

        private void noButton_Click(object sender, RoutedEventArgs e)
        {
            answeredQuestionsIdList.Add(presentQuestion.ID);
            Question q = findQuestion(presentQuestion.no);

            addAnsweredQuestion(presentQuestion, "No");

            updateConclusions(q);
        }

        private void yesButton_Click(object sender, RoutedEventArgs e)
        {
            answeredQuestionsIdList.Add(presentQuestion.ID);
            Question q = findQuestion(presentQuestion.yes);

            addAnsweredQuestion(presentQuestion, "Yes");

            updateConclusions(q);
        }


        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (answeredQuestionsIdList.Count > 0)
            {
                if (presentQuestion.isConclusion)
                {
                    noButton.Visibility = Visibility.Visible;
                    yesButton.Visibility = Visibility.Visible;
                }
                Question q = findQuestion(answeredQuestionsIdList.Last());
                answeredQuestionsIdList.Remove(answeredQuestionsIdList.Last());

                addAnsweredQuestion(null, "Back");
                updateConclusions(q);
            }
        }

        private void addAnsweredQuestion(Question q=null, String answer="")
        {
            ListViewItem item = new ListViewItem();
            RichTextBlock tb = new RichTextBlock();

            tb.Foreground = new SolidColorBrush(Colors.WhiteSmoke);

            Paragraph p = new Paragraph();
            tb.FontSize = 16;
            Run r = new Run();
            Run r2 = new Run();
            if (q != null)
            {
                r.Text = q.content.Trim();
                if (!answer.Equals(""))
                {
                    r.Text += ": ";
                    r2.Text = answer + "\n";
                    r2.FontWeight = FontWeights.Bold;
                    p.Inlines.Add(r);
                    p.Inlines.Add(r2);
                }
                else
                {
                    r2.Text = "Conclusion: ";
                    r2.FontWeight = FontWeights.Bold;
                    p.Inlines.Add(r2);
                    p.Inlines.Add(r);
                }
            }
            else
            {
                r.Text = answer + "\n";
                r.FontWeight = FontWeights.Bold;
                p.Inlines.Add(r);
            }

            tb.Blocks.Add(p);

            item.Content = tb;
            answeredQuestions.Items.Add(item);
        }
    }
}