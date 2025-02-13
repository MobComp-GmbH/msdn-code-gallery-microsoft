//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Navigation;


namespace SDKTemplate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : SDKTemplate.Common.LayoutAwarePage
    {
        public const string FEATURE_NAME = "File picker contracts C# sample";

        public event System.EventHandler ScenarioLoaded;
        public event EventHandler<MainPageSizeChangedEventArgs> MainPageResized;
        public bool AutoSizeInputSectionWhenSnapped = true;

        public Windows.ApplicationModel.Activation.LaunchActivatedEventArgs LaunchArgs;

        public static MainPage Current;

        private Frame HiddenFrame = null;
        
        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title = "File Open Picker contract", ClassType = typeof(FilePickerContracts.MainPage_PickFile) },
            new Scenario() { Title = "File Save Picker contract", ClassType = typeof(FilePickerContracts.MainPage_SaveFile) },
            new Scenario() { Title = "Cached File Updater contract", ClassType = typeof(FilePickerContracts.MainPage_CachedFile) },
        };

        internal bool EnsureUnsnapped()
        {
            // FilePicker APIs will not work if the application is in a snapped state.
            // If an app wants to show a FilePicker while snapped, it must attempt to unsnap first
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped)
            {
                NotifyUser("Cannot unsnap the sample.", NotifyType.StatusMessage);
            }

            return unsnapped;
        }

        internal void ResetScenarioOutput(TextBlock output)
        {
            // clear Error/Status
            NotifyUser("", NotifyType.ErrorMessage);
            NotifyUser("", NotifyType.StatusMessage);
            // clear scenario output
            output.Text = "";
        }

        public MainPage()
        {
            this.InitializeComponent();

            // This is a static public property that will allow downstream pages to get
            // a handle to the MainPage instance in order to call methods that are in this class.
            Current = this;

            // This frame is hidden, meaning it is never shown.  It is simply used to load
            // each scenario page and then pluck out the input and output sections and
            // place them into the UserControls on the main page.
            HiddenFrame = new Windows.UI.Xaml.Controls.Frame();
            HiddenFrame.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            LayoutRoot.Children.Add(HiddenFrame);

            // Populate the sample title from the constant in the GlobalVariables.cs file.
            SetFeatureName(FEATURE_NAME);

            Scenarios.SelectionChanged += Scenarios_SelectionChanged;
            SizeChanged += MainPage_SizeChanged;

        }

        void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateSize();
            if (MainPageResized != null)
            {
                MainPageSizeChangedEventArgs args = new MainPageSizeChangedEventArgs();
                args.ViewState = ApplicationView.Value;
                MainPageResized(this, args);
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PopulateScenarios();
            InvalidateViewState();
        }

        private void InvalidateSize()
        {
            // Get the window width
            double windowWidth = this.ActualWidth;

            if (windowWidth != 0.0)
            {
                // Get the width of the ListBox.
                double listBoxWidth = Scenarios.ActualWidth;

                // Is the ListBox using any margins that we need to consider?
                double listBoxMarginLeft = Scenarios.Margin.Left;
                double listBoxMarginRight = Scenarios.Margin.Right;

                // Figure out how much room is left after considering the list box width
                double availableWidth = windowWidth - listBoxWidth;

                // Is the top most child using margins?
                double layoutRootMarginLeft = ContentRoot.Margin.Left;
                double layoutRootMarginRight = ContentRoot.Margin.Right;

                // We have different widths to use depending on the view state
                if (ApplicationView.Value != ApplicationViewState.Snapped)
                {
                    // Make us as big as the the left over space, factoring in the ListBox width, the ListBox margins.
                    // and the LayoutRoot's margins
                    InputSection.Width = ((availableWidth) - (layoutRootMarginLeft + layoutRootMarginRight + listBoxMarginLeft + listBoxMarginRight));
                }
                else
                {
                    // Make us as big as the left over space, factoring in just the LayoutRoot's margins.
                    if (AutoSizeInputSectionWhenSnapped)
                    {
                        InputSection.Width = (windowWidth - (layoutRootMarginLeft + layoutRootMarginRight));
                    }
                }
            }
            InvalidateViewState();
        }

        private void InvalidateViewState()
        {

            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                Grid.SetRow(DescriptionText, 3);
                Grid.SetColumn(DescriptionText, 0);

                Grid.SetRow(InputSection, 4);
                Grid.SetColumn(InputSection, 0);

                Grid.SetRow(FooterPanel, 2);
                Grid.SetColumn(FooterPanel, 0);
            }
            else
            {
                Grid.SetRow(DescriptionText, 1);
                Grid.SetColumn(DescriptionText, 1);

                Grid.SetRow(InputSection, 2);
                Grid.SetColumn(InputSection, 1);

                Grid.SetRow(FooterPanel, 1);
                Grid.SetColumn(FooterPanel, 1);
            }
        }

        private void PopulateScenarios()
        {
            System.Collections.ObjectModel.ObservableCollection<object> ScenarioList = new System.Collections.ObjectModel.ObservableCollection<object>();
            int i = 0;

            // Populate the ListBox with the list of scenarios as defined in Constants.cs.
            foreach (Scenario s in scenarios)
            {
                ListBoxItem item = new ListBoxItem();
                s.Title = (++i).ToString() + ") " + s.Title;
                item.Content = s;
                item.Name = s.ClassType.FullName;
                ScenarioList.Add(item);
            }

            // Bind the ListBox to the scenario list.
            Scenarios.ItemsSource = ScenarioList;

            // Starting scenario is the first or based upon a previous selection.
            int startingScenarioIndex = -1;

            if (SuspensionManager.SessionState.ContainsKey("SelectedScenarioIndex"))
            {
                int selectedScenarioIndex = Convert.ToInt32(SuspensionManager.SessionState["SelectedScenarioIndex"]);
                startingScenarioIndex = selectedScenarioIndex;
           }

            Scenarios.SelectedIndex = startingScenarioIndex != -1 ? startingScenarioIndex : 0;
        }

        /// <summary>
        /// This method is responsible for loading the individual input and output sections for each scenario.  This
        /// is based on navigating a hidden Frame to the ScenarioX.xaml page and then extracting out the input
        /// and output sections into the respective UserControl on the main page.
        /// </summary>
        /// <param name="scenarioName"></param>
        public void LoadScenario(Type scenarioClass)
        {
            AutoSizeInputSectionWhenSnapped = true;

            // Load the ScenarioX.xaml file into the Frame.
            HiddenFrame.Navigate(scenarioClass, this);

            // Get the top element, the Page, so we can look up the elements
            // that represent the input and output sections of the ScenarioX file.
            Page hiddenPage = HiddenFrame.Content as Page;

            // Get each element.
            UIElement input = hiddenPage.FindName("Input") as UIElement;
            UIElement output = hiddenPage.FindName("Output") as UIElement;

            if (input == null)
            {
                // Malformed input section.
                NotifyUser(String.Format(
                    "Cannot load scenario input section for {0}.  Make sure root of input section markup has x:Name of 'Input'",
                    scenarioClass.Name), NotifyType.ErrorMessage);
                return;
            }

            if (output == null)
            {
                // Malformed output section.
                NotifyUser(String.Format(
                    "Cannot load scenario output section for {0}.  Make sure root of output section markup has x:Name of 'Output'",
                    scenarioClass.Name), NotifyType.ErrorMessage);
                return;
            }

            // Find the LayoutRoot which parents the input and output sections in the main page.
            Panel panel = hiddenPage.FindName("LayoutRoot") as Panel;

            if (panel != null)
            {
                // Get rid of the content that is currently in the intput and output sections.
                panel.Children.Remove(input);
                panel.Children.Remove(output);

                // Populate the input and output sections with the newly loaded content.
                InputSection.Content = input;
                OutputSection.Content = output;

                if (ScenarioLoaded != null)
                {
                    ScenarioLoaded(this, new EventArgs());
                }
            }
            else
            {
                // Malformed Scenario file.
                NotifyUser(String.Format(
                    "Cannot load scenario: '{0}'.  Make sure root tag in the '{0}' file has an x:Name of 'LayoutRoot'",
                    scenarioClass.Name), NotifyType.ErrorMessage);
            }

        }

        void Scenarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Scenarios.SelectedItem != null)
            {
                NotifyUser("", NotifyType.StatusMessage);

                ListBoxItem selectedListBoxItem = Scenarios.SelectedItem as ListBoxItem;
                SuspensionManager.SessionState["SelectedScenarioIndex"] = Scenarios.SelectedIndex;

                Scenario scenario = selectedListBoxItem.Content as Scenario;
                LoadScenario(scenario.ClassType);
                InvalidateSize();
            }
        }

        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                // Use the status message style.
                case NotifyType.StatusMessage:
                    StatusBlock.Style = Resources["StatusStyle"] as Style;
                    break;
                // Use the error message style.
                case NotifyType.ErrorMessage:
                    StatusBlock.Style = Resources["ErrorStyle"] as Style;
                    break;
            }
            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            if (StatusBlock.Text != String.Empty)
            {
                StatusBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                StatusBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        async void Footer_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(((HyperlinkButton)sender).Tag.ToString()));
        }

        private void SetFeatureName(string str)
        {
            FeatureName.Text = str;
        }
    }

    public class MainPageSizeChangedEventArgs : EventArgs
    {
        private ApplicationViewState viewState;

        public ApplicationViewState ViewState
        {
            get { return viewState; }
            set { viewState = value; }
        }
    }
}
