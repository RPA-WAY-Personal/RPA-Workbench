using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPA_Workbench.Views.StartupMenu;
using System.Windows.Input;
using System.Windows;
using Newtonsoft.Json.Linq;
using RPA_Workbench.Utilities;
using System.IO;
using System.ComponentModel;
using RPA_Workbench.Project_Types;
using static RPA_Workbench.Project_Types.WorkflowTypes;
using System.Windows.Media.Imaging;
using ActiproSoftware.Windows.DocumentManagement;
using ActiproSoftware.Windows.Controls.Ribbon.Controls;
using System.Windows.Forms;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using RPA_Workbench.ViewModels;
using RPA_Workbench.Properties;
using System.Windows.Media;

namespace RPA_Workbench.ViewModels.StartupMenu
{
    public class StartPageViewModel: INotifyPropertyChanged
    {
        #region Variables

       
        static private ParentStartupWindow localParentStartupWindow = new ParentStartupWindow(localMainwindow);
        static private StartPageViewModel staticStartPageViewModel = new StartPageViewModel(); // Page0 VIEWMODEL (Used in Static Mode)
        static private StartPage startPage;
        static private Views.ProjectView projectView;
        static public Models.BackstageMenuModel backstageMenuModel = new Models.BackstageMenuModel();
        static private MainWindow localMainwindow;
        static public Models.StartPageModel startPageModel = new Models.StartPageModel();
        static private ViewModels.StartupMenu.ParentStartupViewModel parentStartupViewModel = new ParentStartupViewModel();
        static public Models.ParentStartupWindowModel parentStartupWindowModel = new Models.ParentStartupWindowModel();
        static public StartPageTemplateDialog startPageTemplateDialog = new StartPageTemplateDialog();
        static public Views.StartupMenu.StartPageCreateFromTemplateDialog startPageCreateFromTemplateDialog = new Views.StartupMenu.StartPageCreateFromTemplateDialog();
        static private ProjectWindowViewModel projectWindowViewModel = new ProjectWindowViewModel();

        //Local
        private static int LocalPageNumber;
        static RecentDocumentManager LocalRecentDocumentManager;
        static RecentDocumentMenu LocalRecentDocumentMenu;
        #endregion

        #region Constructors
        //Constructor

        public StartPageViewModel()
        {
            RelaySetup();
        }
        public void LoadStartPage(ParentStartupWindow parentStartupWindow, MainWindow mainWindow)
        {
            startPage = new StartPage(this);
            startPage.DataContext = this;
         
            localParentStartupWindow = parentStartupWindow;
            parentStartupWindow.RootDockPanel.Children.Add(startPage);
            localMainwindow = mainWindow;
            //Properties.Settings.Default.RecentDocuments = string.Empty;
            //Properties.Settings.Default.Save();
            LoadRecentDocuments();
        }

        //Optional Constructor
        public void SetupRecentDocuments(RecentDocumentManager recentDocumentManager, RecentDocumentMenu recentDocumentMenu)
        {
            LocalRecentDocumentManager = recentDocumentManager;
            LocalRecentDocumentMenu = recentDocumentMenu;
            LocalRecentDocumentManager.Documents.CollectionChanged += Documents_CollectionChanged; // When a item gets removed/added it saves the state
            LocalRecentDocumentMenu.MouseEnter += LocalRecentDocumentMenu_MouseEnter;
        }
        IDocumentReference documentReference;
        private void LocalRecentDocumentMenu_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }


        void RelaySetup()
        {
            ContinueNoCodeCommand = new RelayCommand(new Action<object>(ContinueWithoutCode));
            CreateNewProjectFromStartPageCommmand = new RelayCommand(new Action<object>(CreateNewProjectFromStartPage));
            CreateNewProjectFromTemplatePageCommmand = new RelayCommand(new Action<object>(CreateNewProjectFromTemplatePage));
            OpenProjectCommand = new RelayCommand(new Action<object>(OpenProject));
            CreateProjectCommand = new RelayCommand(new Action<object>(CreateProject));
            RemoveRecentDocumentCommand = new RelayCommand(new Action<object>(RemoveRecentDocument));
            PreviousPageCommand = new RelayCommand(new Action<object>(PreviousPage));
            ClearRecentDocumentsCommand = new RelayCommand(new Action<object>(ClearRecentDocuments));
        }

        #endregion

        #region Properties
        public static int PageNumber
        {
            get
            {
                return LocalPageNumber;
            }
            set
            {
                LocalPageNumber = value;
            }
        }

        private string _projectName;
        public string ProjectName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_projectName))
                {
                    _projectName = "Blank Process";
                }
                return _projectName;
            }
            set
            {
                _projectName = value;
                RaisePropertyChanged("_projectName");
                RaisePropertyChanged("ProjectName");
            }
        }

        private string _projectDescription;
        public string ProjectDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_projectDescription))
                {
                    _projectDescription = "Blank RPA process";
                }
                return _projectDescription;
            }
            set
            {
                _projectDescription = value;
                RaisePropertyChanged("ProjectDescription");
            }
        }

        #endregion

        #region ICommands

        
        private ICommand mOpenProject;
        public ICommand OpenProjectCommand
        {
            get
            {
                return mOpenProject;
            }
            set
            {
                mOpenProject = value;
            }
        }


        //Continue Without Code Command
        private ICommand mContinueWithoutCode;
        public ICommand ContinueNoCodeCommand
        {
            get
            {
                return mContinueWithoutCode;
            }
            set
            {
                mContinueWithoutCode = value;
            }
        }


        //Create New Project From start page Command - Page 1
        private ICommand mCreateNewProject;
        public ICommand CreateNewProjectFromStartPageCommmand
        {
            get
            {
                return mCreateNewProject;
            }
            set
            {
                mCreateNewProject = value;
            }
        }



        //Create New Project From Template Page Command - Page 2
        private ICommand mCreateNewProjectFromTemplate;
        public ICommand CreateNewProjectFromTemplatePageCommmand
        {
            get
            {
                return mCreateNewProjectFromTemplate;
            }
            set
            {
                mCreateNewProjectFromTemplate = value;
            }
        }



        private ICommand mPreviousPage;
        public ICommand PreviousPageCommand
        {
            get
            {
                return mPreviousPage;
            }
            set
            {
                mPreviousPage = value;
            }
        }



        private ICommand mCreateProject;
        public ICommand CreateProjectCommand
        {
            get
            {
                return mCreateProject;
            }
            set
            {
                mCreateProject = value;
            }
        }


        private ICommand mRemoveRecentDocument;
        public ICommand RemoveRecentDocumentCommand
        {
            get
            {
                return mRemoveRecentDocument;
            }
            set
            {
                mRemoveRecentDocument = value;
            }
        }

        private ICommand mClearRecentDocuments;
        public ICommand ClearRecentDocumentsCommand
        {
            get
            {
                return mClearRecentDocuments;
            }
            set
            {
                mClearRecentDocuments = value;
            }
        }

        #endregion

        #region ICommand Functions/Methods

        public void PreviousPage(object parameter)
        {
            //localParentStartupWindow.PageControls.Visibility = Visibility.Visible;
            PageNumber--;
            switch (PageNumber)
            {
                case 0:
                    localParentStartupWindow.RootDockPanel.Children.Clear();
                    startPage = new StartPage(staticStartPageViewModel);
                    localParentStartupWindow.RootDockPanel.Children.Add(startPage);
                    break;

                case 1:
                    localParentStartupWindow.RootDockPanel.Children.Clear();
                    startPageTemplateDialog = new StartPageTemplateDialog();
                    localParentStartupWindow.RootDockPanel.Children.Add(startPageTemplateDialog);
                    //System.Windows.Forms.MessageBox.Show("1 Hit");
                    break;


                case 2:
                    localParentStartupWindow.RootDockPanel.Children.Clear();
                    startPageCreateFromTemplateDialog = new StartPageCreateFromTemplateDialog();
                    localParentStartupWindow.RootDockPanel.Children.Add(startPageCreateFromTemplateDialog);
                    //System.Windows.Forms.MessageBox.Show("2 Hit");
                    break;
            }
        }

        public void ContinueWithoutCode(object parameter)
        {
            backstageMenuModel.CanBackstageClose = false;
            backstageMenuModel.IsBackStageOpen = true;
            localMainwindow.Visibility = System.Windows.Visibility.Visible;
            localMainwindow.Show();
            App.Current.MainWindow = localMainwindow;
            localMainwindow.MainRibbon.ApplicationMenu = localMainwindow.MainBackStageMenu;

            localMainwindow.MainBackStageMenu.CanClose = backstageMenuModel.CanBackstageClose;
            localMainwindow.MainRibbon.IsApplicationMenuOpen = backstageMenuModel.IsBackStageOpen;
            localParentStartupWindow.Close();
        }
        //Page 1
        public void CreateNewProjectFromStartPage(object parameter)
        {
            localParentStartupWindow.RootDockPanel.Children.Clear();
            localParentStartupWindow.RootDockPanel.Children.Add(startPageTemplateDialog);
            PageNumber = 1;
        }
        //Page 2
        public void CreateNewProjectFromTemplatePage(object parameter)
        {
            localParentStartupWindow.RootDockPanel.Children.Clear();
            localParentStartupWindow.RootDockPanel.Children.Add(startPageCreateFromTemplateDialog);

            startPageCreateFromTemplateDialog.txtProcessLocation.Text =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RPA Workbench");

            PageNumber = 2;
        }

        public void OpenProject(object parameter)
        {
            try
            {
                //System.Windows.Forms.MessageBox.Show("Search for the project you want to open (PLACE HOLDER)");
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Project File|*.json";
                openFileDialog.ShowDialog();

                JToken token = JObject.Parse(File.ReadAllText(@openFileDialog.FileName));


                //Get Selected Directory
                System.Uri uri = new System.Uri(openFileDialog.FileName);
                string path = uri.LocalPath;
                string ProjectCurrentDirectory = System.IO.Path.GetDirectoryName(path);
                //System.Windows.Forms.MessageBox.Show(ProjectCurrentDirectory);



                //Check whether the path is the original path or not
                string json = File.ReadAllText(openFileDialog.FileName);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (jsonObj["ProjectPath"] != ProjectCurrentDirectory)
                {

                    jsonObj["ProjectPath"] = ProjectCurrentDirectory;
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(@openFileDialog.FileName, output);

                }

                JsonControls jsonControls = new JsonControls();
                jsonControls.ReadJsonFile(@openFileDialog.FileName);
                jsonControls.DeserializeJsonObject();

                string ProjectPath = jsonControls.GetKeyValue("ProjectPath");
                string ProjectName = (string)token.SelectToken("Name");
                string ProjectDescription = (string)token.SelectToken("Description");
                string FullProjectDirectory = jsonObj["ProjectPath"];



                //AddDocumentReference(LocalRecentDocumentManager, new Uri(openFileDialog.FileName), ProjectName, ProjectDescription, true);
                // mainWindow = new MainWindow();
                if (LocalRecentDocumentManager.Documents.Count > 0)
                {
                    foreach (var item in LocalRecentDocumentManager.Documents)
                    {
                        if (item.Name.Contains(ProjectName))
                        {
                            continue;
                        }
                        if (item.Name.Contains(ProjectName) == false)
                        {
                            LocalRecentDocumentMenu.IsEnabled = true;
                            LocalRecentDocumentMenu.Focus();
                            LocalRecentDocumentMenu.BringIntoView();
                            AddDocumentReference(LocalRecentDocumentManager, new Uri(openFileDialog.FileName), ProjectName, ProjectDescription, true);
                        }
                    }
                }
                else
                {
                    LocalRecentDocumentMenu.IsEnabled = true;
                    LocalRecentDocumentMenu.Focus();
                    LocalRecentDocumentMenu.BringIntoView();
                    AddDocumentReference(LocalRecentDocumentManager, new Uri(@openFileDialog.FileName), ProjectName, ProjectDescription, true);

                    Properties.Settings.Default.RecentDocuments = LocalRecentDocumentManager.Serialize();
                    //  .Items.Add(docRef);
                }
                if (localMainwindow.IsInitialized)
                {
                    //if (mainWindow.Visibility == Visibility.Visible)
                    //{
                    //    mainWindow = new MainWindow();
                    //    //MainWindow mainWindow = new MainWindow();
                    //    mainWindow.Visibility = Visibility.Visible;
                    //    App.Current.MainWindow = mainWindow;
                    //    mainWindow.WindowState = WindowState.Maximized;
                    //    //mainWindow.Visibility = Visibility.Hidden;
                    //    mainWindow.appMenu.Height = mainWindow.Height;
                    //    mainWindow.appMenu.Width = mainWindow.Width;
                    //    mainWindow.appMenu.Ribbon = mainWindow.ribbon;
                    //    mainWindow.appMenu.CanClose = true;
                    //    mainWindow.ribbon.IsApplicationMenuOpen = false;
                    //    mainWindow.ShowInTaskbar = true;
                    //    mainWindow.BringIntoView();
                    //}
                    //  documentReferences.Save();
                    //  mainWindow = new MainWindow();
                    //MainWindow mainWindow = new MainWindow();
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.Visibility = Visibility.Visible;
                    App.Current.MainWindow = localMainwindow;
                    App.Current.MainWindow.WindowState = WindowState.Maximized;
                    localMainwindow.WindowState = WindowState.Maximized;
                    //mainWindow.Visibility = Visibility.Hidden;
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.MainBackStageMenu.CanClose = backstageMenuModel.CanBackstageClose;
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.MainRibbon.ApplicationMenu = localMainwindow.MainBackStageMenu;
                    localMainwindow.MainRibbon.IsApplicationMenuOpen = false;
                    localMainwindow.ShowInTaskbar = true;
                    localMainwindow.BringIntoView();
                    Properties.Settings.Default.Save();
                    localMainwindow.DocumentName = ProjectName;


                    // WffControls.projectWindowLight.mainWindowLocal = mainWindow;
                    projectWindowViewModel.ProjectDirectory = ProjectPath;
                    projectWindowViewModel.ProjectRootFolder = ProjectPath;
                    projectView = new Views.ProjectView(projectWindowViewModel, localMainwindow);
                    projectWindowViewModel.LoadWindow(FullProjectDirectory);
                    localMainwindow.ProjectToolWindow.HorizontalAlignment = HorizontalAlignment.Stretch;
                    localMainwindow.ProjectToolWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    projectView.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    projectView.HorizontalAlignment = HorizontalAlignment.Stretch;
                    localMainwindow.ProjectToolWindow.Content = projectView;


                    //Close();
                }
                localMainwindow.Visibility = Visibility.Visible;
                localMainwindow.WindowState = WindowState.Maximized;
                localMainwindow.BringIntoView();
                localMainwindow.Show();
                Window window = Window.GetWindow(localParentStartupWindow);
                if (window != null)
                {
                    window.Close();
                }
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(e.Message);
                //  throw;
            }
        }

        public void CreateProject(object parameter)
        {
            string projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RPA Workbench\\" + _projectName);
            //string projectRootFolder= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RPA Workbench\\" + _projectName + "\\.root");
            //string projectDependanciesFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RPA Workbench\\" + _projectName + "\\.root\\Dependancies.json");
            Directory.CreateDirectory(projectDirectory);
            //Directory.CreateDirectory(projectRootFolder);
            //File.Create(projectDependanciesFile);
            JsonControls jsonControls = new JsonControls();
            JObject ProjectJson = new JObject(
                new JProperty("Name", ProjectName),
                new JProperty("ProjectPath", projectDirectory),
                new JProperty("Description", ProjectDescription),
                new JProperty("Main", "Main")
           //new JProperty("ExpressionLanguage", cmbProjectExpression.SelectionBoxItem.ToString())
           );
            File.WriteAllText(projectDirectory + "\\Project.json", ProjectJson.ToString());
            File.WriteAllText(projectDirectory + "\\Main.xaml", CleanProjectView("Main"));

            //DEBUGGING
            //MessageBox.Show("Name: " + ProjectName + Environment.NewLine +
            //            "Desc: " + ProjectDescription);


            OpenTheNewProject(ProjectName, projectDirectory, ProjectDescription);
        }

        public void RemoveRecentDocument(object parameter)
        {
            //LocalRecentDocumentManager.Documents.Remove();
        }

        public void ClearRecentDocuments(object parameter)
        {
            localParentStartupWindow.Opacity = 0.2f;
            localParentStartupWindow.IsEnabled = false;
            localParentStartupWindow.Background = new SolidColorBrush(Colors.Gray);
            var messageBoxResult = CustomControls.Views.CustomMessageBox.Show("Clear recent projects list", "Are you sure you want to clear all your recent projects?",
                         CustomControls.Views.CustomMessageBox.MessageBoxButtons.YesNo);
            if (messageBoxResult == CustomControls.Views.CustomMessageBox.MessageBoxResults.Yes)
            {
                LocalRecentDocumentManager.Documents.Clear();
                Settings.Default.Save();
            }
            localParentStartupWindow.Opacity = 100;
            localParentStartupWindow.IsEnabled = true;
            localParentStartupWindow.Background = new SolidColorBrush(Colors.White);
        }

        #endregion

        #region Events
        private void Documents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Properties.Settings.Default.RecentDocuments = LocalRecentDocumentManager.Serialize();
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Methods/Functions
        void LoadRecentDocuments()
        {
            if (Properties.Settings.Default.RecentDocuments != string.Empty)
            {
                LocalRecentDocumentManager.Deserialize(Properties.Settings.Default.RecentDocuments); // Load recent Document List
            }
        }

        void OpenTheNewProject(string ProjectName, string ProjectPath, string ProjectDescription)
        {
            try
            {
                if (localMainwindow.IsInitialized)
                {
                    if (Directory.Exists(ProjectPath + "\\.root") == false)
                    {
                        Directory.CreateDirectory(ProjectPath + "//.root");
                    }

                    if (File.Exists(ProjectPath + "\\.root" + "\\Dependencies.json") == false)
                    {
                        StreamWriter streamWriter = new StreamWriter(ProjectPath + "\\.root" + "\\Dependencies.json");
                        streamWriter.Write("");
                        streamWriter.Close();
                        //NOTE: Do not use File.Create(ProjectPath + "\\.root" + "\\Dependencies.json"), as it does not close the stream.

                    }

                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.Visibility = Visibility.Visible;
                    App.Current.MainWindow = localMainwindow;
                    App.Current.MainWindow.WindowState = WindowState.Maximized;
                    localMainwindow.WindowState = WindowState.Maximized;
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.MainBackStageMenu.CanClose = backstageMenuModel.CanBackstageClose;
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.MainRibbon.ApplicationMenu = localMainwindow.MainBackStageMenu;
                    localMainwindow.MainRibbon.IsApplicationMenuOpen = false;
                    localMainwindow.ShowInTaskbar = true;
                    localMainwindow.BringIntoView();
                    Properties.Settings.Default.Save();
                    localMainwindow.DocumentName = ProjectName;

                    projectWindowViewModel.ProjectDirectory = ProjectPath;
                    projectWindowViewModel.ProjectRootFolder = ProjectPath;
                    projectView = new Views.ProjectView(projectWindowViewModel, localMainwindow);
                    projectWindowViewModel.LoadWindow(ProjectPath);
                    localMainwindow.ReloadMainWindowModel(ProjectPath);
                    localMainwindow.ProjectToolWindow.HorizontalAlignment = HorizontalAlignment.Stretch;
                    localMainwindow.ProjectToolWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    projectView.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    projectView.HorizontalAlignment = HorizontalAlignment.Stretch;
                    localMainwindow.ProjectToolWindow.Content = projectView;

                    if (LocalRecentDocumentManager.Documents.Count > 0)
                    {
                        foreach (var item in LocalRecentDocumentManager.Documents)
                        {
                            //if (item.Name.Contains(ProjectName))
                            //{
                            //    continue;
                            //}
                            if (item.Name.Contains(ProjectName) == false)
                            {
                                LocalRecentDocumentMenu.IsEnabled = true;
                                LocalRecentDocumentMenu.Focus();
                                LocalRecentDocumentMenu.BringIntoView();
                                AddDocumentReference(LocalRecentDocumentManager, new Uri(ProjectPath + "\\Project.json"), ProjectName, ProjectDescription, true);
                                break;
                            }
                        }
                    }
                    else
                    {
                        LocalRecentDocumentMenu.IsEnabled = true;
                        LocalRecentDocumentMenu.Focus();
                        LocalRecentDocumentMenu.BringIntoView();
                        AddDocumentReference(LocalRecentDocumentManager, new Uri(ProjectPath + "\\Project.json"), ProjectName, ProjectDescription, true);

                        Properties.Settings.Default.RecentDocuments = LocalRecentDocumentManager.Serialize();
                    }
                }
                localMainwindow.Visibility = Visibility.Visible;
                localMainwindow.WindowState = WindowState.Maximized;
                localMainwindow.BringIntoView();
                localMainwindow.Show();
                Window window = Window.GetWindow(localParentStartupWindow);
                if (window != null)
                {
                    window.Close();
                }
            }
            catch (Exception e)
            {
                //System.Windows.Forms.MessageBox.Show(e.Message);
               throw;
            }
        }

        void OpenProjectFromRecentDocuments(Uri PathUri)
        {
            try
            {
                if (localMainwindow.IsInitialized)
                {
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.Visibility = Visibility.Visible;
                    App.Current.MainWindow = localMainwindow;
                    App.Current.MainWindow.WindowState = WindowState.Maximized;
                    localMainwindow.WindowState = WindowState.Maximized;
                    //mainWindow.Visibility = Visibility.Hidden;
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.MainBackStageMenu.CanClose = backstageMenuModel.CanBackstageClose;
                    backstageMenuModel.CanBackstageClose = true;
                    localMainwindow.MainRibbon.ApplicationMenu = localMainwindow.MainBackStageMenu;
                    localMainwindow.MainRibbon.IsApplicationMenuOpen = false;
                    localMainwindow.ShowInTaskbar = true;
                    localMainwindow.BringIntoView();
                    Properties.Settings.Default.Save();
                    localMainwindow.DocumentName = ProjectName;

                    LoadProjectOnOpen(PathUri); // Loads the project selected intom the Studio interface


                    //Close();
                }
                localMainwindow.Visibility = Visibility.Visible;
                localMainwindow.WindowState = WindowState.Maximized;
                localMainwindow.BringIntoView();
                localMainwindow.Show();
                Window window = Window.GetWindow(localParentStartupWindow);
                if (window != null)
                {
                    window.Close();
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        void LoadProjectOnOpen(Uri PathUri)
        {
            try
            {
                //Get Selected Directory
                System.Uri uri = PathUri;
                string path = "";
                if (uri != null)
                {
                   path = uri.LocalPath;
                }
               
                string ProjectCurrentDirectory = System.IO.Path.GetDirectoryName(path);
                    //DialogResult OpenProject = System.Windows.Forms.MessageBox.Show("Are you sure you want to close current project and open this project", "Project shift", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    //if (OpenProject == System.Windows.Forms.DialogResult.Yes)
                    //{
                    JToken token = JObject.Parse(File.ReadAllText(PathUri.LocalPath));

                    string json = File.ReadAllText(PathUri.LocalPath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    if (jsonObj["ProjectPath"] != ProjectCurrentDirectory)
                    {
                        DialogResult Warning = System.Windows.Forms.MessageBox.Show("The selected Project is not in original directory, update it?", "Directory Changed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (Warning == System.Windows.Forms.DialogResult.Yes)
                        {
                            jsonObj["ProjectPath"] = ProjectCurrentDirectory;
                            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                            File.WriteAllText(PathUri.LocalPath, output);
                        }
                    }

                    string ProjectPath = (string)token.SelectToken("ProjectPath");
                    string ProjectName = (string)token.SelectToken("Name");
                    string ProjectDescription = (string)token.SelectToken("Description");
                    string FullProjectDirectory = jsonObj["ProjectPath"];



                    // mainWindow = new MainWindow();
                    LocalRecentDocumentMenu.IsEnabled = true;
                    LocalRecentDocumentMenu.Focus();
                    LocalRecentDocumentMenu.BringIntoView();
                    //Close all previous project MDI tabs
                    foreach (var item in localMainwindow.MDIHost.GetDocuments())
                    {
                        item.Close();
                    }

                    if (localMainwindow.IsInitialized)
                    {
                        backstageMenuModel.CanBackstageClose = true;
                        localMainwindow.Visibility = Visibility.Visible;
                        App.Current.MainWindow = localMainwindow;
                        App.Current.MainWindow.WindowState = WindowState.Maximized;
                        localMainwindow.WindowState = WindowState.Maximized;
                        //mainWindow.Visibility = Visibility.Hidden;
                        backstageMenuModel.CanBackstageClose = true;
                        localMainwindow.MainBackStageMenu.CanClose = backstageMenuModel.CanBackstageClose;
                        backstageMenuModel.CanBackstageClose = true;
                        localMainwindow.MainRibbon.ApplicationMenu = localMainwindow.MainBackStageMenu;
                        localMainwindow.MainRibbon.IsApplicationMenuOpen = false;
                        localMainwindow.ShowInTaskbar = true;
                        localMainwindow.BringIntoView();
                        Properties.Settings.Default.Save();
                        localMainwindow.DocumentName = ProjectName;
                        

                        projectWindowViewModel.ProjectDirectory = ProjectPath;
                        projectWindowViewModel.ProjectRootFolder = ProjectPath;
                        projectView = new Views.ProjectView(projectWindowViewModel, localMainwindow);
                        projectWindowViewModel.LoadWindow(ProjectPath);
                        localMainwindow.mainWindowViewModel.CurrentProjectPath = FullProjectDirectory;
                        localMainwindow.ReloadMainWindowModel(FullProjectDirectory);
                        localMainwindow.ProjectToolWindow.HorizontalAlignment = HorizontalAlignment.Stretch;
                        localMainwindow.ProjectToolWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        projectView.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        projectView.HorizontalAlignment = HorizontalAlignment.Stretch;
                        localMainwindow.ProjectToolWindow.Content = projectView;
                        

                        //Close();
                    }
                    localMainwindow.Visibility = Visibility.Visible;
                    localMainwindow.WindowState = WindowState.Maximized;
                    localMainwindow.BringIntoView();
                    localMainwindow.Show();

                    Window window = Window.GetWindow(localParentStartupWindow);
                    if (window != null)
                    {
                        window.Close();
                    }
                }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddDocumentReference(RecentDocumentManager manager, Uri Path, String Name, string description, bool isPinned)
        {
            ActiproSoftware.Windows.DocumentManagement.DocumentReference docRef = new ActiproSoftware.Windows.DocumentManagement.DocumentReference(Path);
            docRef.LastOpenedDateTime = DateTime.Now.AddDays(-1 * manager.Documents.Count); ;
            docRef.IsPinnedRecentDocument = isPinned;
            docRef.Description = description;
            docRef.Name = Name;

            /* Themeing
             * if (Properties.Settings.Default.ThemeType == 0)//Light Theme
            {
                docRef.ImageSourceSmall = new BitmapImage(new Uri("/Resources/Project Icon Dark.png", UriKind.Relative));
                docRef.ImageSourceLarge = new BitmapImage(new Uri("/Resources/Project Icon Dark.png", UriKind.Relative));
            }
            else if (Properties.Settings.Default.ThemeType == 1) //Dark Theme
            {
                docRef.ImageSourceSmall = new BitmapImage(new Uri("/Resources/Project Icon Light.png", UriKind.Relative));
                docRef.ImageSourceLarge = new BitmapImage(new Uri("/Resources/Project Icon Light.png", UriKind.Relative));
            }
            */

            docRef.ImageSourceSmall = new BitmapImage(new Uri("/RPA-Workbench-Revision2;component/1. Resources/StartPage Images/Project Icon Dark.png", UriKind.Relative));
            docRef.ImageSourceLarge = new BitmapImage(new Uri("/RPA-Workbench-Revision2;component/1. Resources/StartPage Images/Project Icon Dark.png", UriKind.Relative));
            
            manager.Documents.Add(docRef);
            Properties.Settings.Default.RecentDocuments = manager.Serialize();
            Properties.Settings.Default.Save();
        }

        public void OnOpenExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is IDocumentReference)
            {
                // Process recent document clicks
                //System.Windows.Forms.MessageBox.Show("Open document '" + ((IDocumentReference)e.Parameter).Name + "' here.", "Open Recent Document", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //System.Windows.Forms.MessageBox.Show(((IDocumentReference)e.Parameter).Location.OriginalString);
                if (File.Exists(((IDocumentReference)e.Parameter).Location.OriginalString)){ 
                    Uri location = new Uri(((IDocumentReference)e.Parameter).Location.OriginalString);
                    OpenProjectFromRecentDocuments(location);
                }
                else
                {
                    //DialogResult warnMessage = System.Windows.Forms.MessageBox.Show("This project cannot open, remove from list", "remove", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    //if (warnMessage == System.Windows.Forms.DialogResult.Yes)
                    //{
                    //    LocalRecentDocumentManager.Documents.Remove((IDocumentReference)e.Parameter);
                    //    Properties.Settings.Default.Save();
                    //}
                    localParentStartupWindow.Opacity = 0.2f;
                    localParentStartupWindow.IsEnabled = false;
                    localParentStartupWindow.Background = new SolidColorBrush(Colors.Gray);
                    var messageBoxResult = CustomControls.Views.CustomMessageBox.Show("Could not locate project", $"Do you want to remove: '{((IDocumentReference)e.Parameter).Name}' from the list or" + Environment.NewLine + "try and locate it again, by browsing for it ?",
                        CustomControls.Views.CustomMessageBox.MessageBoxButtons.YesNoBrowse);
                    string json;
                    dynamic jsonObj;
                    Uri location = null;
                    //MessageBox.Show();
                    if (messageBoxResult == CustomControls.Views.CustomMessageBox.MessageBoxResults.Browse)
                    {
                        try
                        {
                            //MessageBox.Show("Browsed");
                            OpenFileDialog openFileDialog = new OpenFileDialog();
                            openFileDialog.Filter = "Project file (*.json)|*.json";
                            openFileDialog.ShowDialog();
                         
                            if (File.Exists(openFileDialog.FileName) ==true)
                            {
                                location = new Uri(openFileDialog.FileName);
                                string ProjectCurrentDirectory = System.IO.Path.GetDirectoryName(location.LocalPath);
                                LocalRecentDocumentManager.Documents.Remove((IDocumentReference)e.Parameter);

                                ((IDocumentReference)e.Parameter).Location = location;
                                LocalRecentDocumentManager.Documents.Add((IDocumentReference)e.Parameter);
                                json = File.ReadAllText(location.LocalPath);
                                jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                                jsonObj["ProjectPath"] = ProjectCurrentDirectory;
                                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                                File.WriteAllText(location.LocalPath, output);

                                Properties.Settings.Default.RecentDocuments = LocalRecentDocumentManager.Serialize();
                                Properties.Settings.Default.Save();
                                OpenProjectFromRecentDocuments(location);
                                // MessageBox.Show();
                            }

                            
                           
                        }
                        catch (Exception)
                        {

                           // throw;
                        }
                       
                    }
                    else if(messageBoxResult == CustomControls.Views.CustomMessageBox.MessageBoxResults.Yes)
                    {
                        LocalRecentDocumentManager.Documents.Remove((IDocumentReference)e.Parameter);
                        Properties.Settings.Default.RecentDocuments = LocalRecentDocumentManager.Serialize();
                        Properties.Settings.Default.Save();
                    }
                    localParentStartupWindow.Background = new SolidColorBrush(Colors.White);
                    localParentStartupWindow.Opacity = 100;
                    localParentStartupWindow.IsEnabled = true;
                }
          
                return;
            }
        }
        #endregion

        #region PropertyChangedEvents
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}

