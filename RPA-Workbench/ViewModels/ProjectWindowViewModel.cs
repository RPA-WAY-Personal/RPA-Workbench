using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using ActiproSoftware.Windows.Controls.Docking;
using ActiproSoftware.Windows.Controls.Grids;
using RPA_Workbench.Utilities.TreeNodeClasses;
using RPA_Workbench.Utilities;
using System.Windows.Media.Effects;
using ActiproSoftware.Windows.Controls.Ribbon.Controls;
using ActiproSoftware.Windows.Controls.Ribbon;
using Button = ActiproSoftware.Windows.Controls.Ribbon.Controls.Button;
using Newtonsoft.Json;
using Meziantou.Framework;

namespace RPA_Workbench.ViewModels
{
    public class ProjectWindowViewModel
    {
        #region Variables
        //Moving
        private Point _lastMouseDown;
        private TreeViewItem draggedItem, _target;


        private MainWindow mainWindowLocal;
        private object dummyNode = null;
        public string SelectedImagePath { get; set; }

        public string SelectedFilePath;
        public string SelectedFileName;

        public string ProjectDirectory;
        public string ProjectRootFolder;
        public DockSite dockSite;
        bool exist = false;
        DockingWindow dockingWindow = null;

        public ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu LocalcontextMenu = new ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu();

        private ViewModels.WorkflowStudioIntegration.MainWindowViewModel mainWindowViewModelLocal;
        #endregion

        #region MVVM
        void RelaySetup()
        {
            OpenProjectSettingsCommand = new RelayCommand(new Action<object>(OpenProjectSettings));
            OpenProjectFolderCommand = new RelayCommand(new Action<object>(OpenProjectFolder));
            DeleteFileorFolderCommand = new RelayCommand(new Action<object>(DeleteFileorFolder));
            RefreshProjectListCommand = new RelayCommand(new Action<object>(RefreshProjectList));
            OpenRenameDialogCommand = new RelayCommand(new Action<object>(OpenRenameDialog));
            SetAsMainCommand = new RelayCommand(new Action<object>(SetAsMain));
            RemoveDepedencyCommand = new RelayCommand(new Action<object>(RemoveDependency));
            AddDepedencyCommand = new RelayCommand(new Action<object>(AddDependency));
        }
        #endregion

        #region ICommands
        private TreeView _solutionTreeview;
        public TreeView SolutionTreeView 
        {
            get { return _solutionTreeview; }
            set { _solutionTreeview = value; }
        }
        private ICommand iOpenSettings;
        public ICommand OpenProjectSettingsCommand
        {
            get
            {
                return iOpenSettings;
            }
            set
            {
                iOpenSettings = value;
            }
        }

        private ICommand iOpenProjectFolder;
        public ICommand OpenProjectFolderCommand
        {
            get
            {
                return iOpenProjectFolder;
            }
            set
            {
                iOpenProjectFolder = value;
            }
        }

        private ICommand iDeleteFileorFolder;
        public ICommand DeleteFileorFolderCommand
        {
            get
            {
                return iDeleteFileorFolder;
            }
            set
            {
                iDeleteFileorFolder = value;
            }
        }

        private ICommand iRefreshProjectList;
        public ICommand RefreshProjectListCommand
        {
            get
            {
                return iRefreshProjectList;
            }
            set
            {
                iRefreshProjectList = value;
            }
        }

        private ICommand iOpenRenameDialog;
        public ICommand OpenRenameDialogCommand
        {
            get
            {
                return iOpenRenameDialog;
            }
            set
            {
                iOpenRenameDialog = value;
            }
        }

        private ICommand iSetAsMain;
        public ICommand SetAsMainCommand
        {
            get
            {
                return iSetAsMain;
            }
            set
            {
                iSetAsMain = value;
            }
        }


        private ICommand iRemoveDepedency;
        public ICommand RemoveDepedencyCommand
        {
            get
            {
                return iRemoveDepedency;
            }
            set
            {
                iRemoveDepedency = value;
            }
        }

        private ICommand iAddDepedency;
        public ICommand AddDepedencyCommand
        {
            get
            {
                return iAddDepedency;
            }
            set
            {
                iAddDepedency = value;
            }
        }

        #endregion

        #region Methods/Functions Commands
        void OpenProjectSettings(object parameter)
        {
            //ProjectSettings projectSettings = new ProjectSettings();
            //projectSettings.CurrentDirectory = ProjectDirectory;
            //mainWindowLocal.Opacity = 0.5;
            //// blur the whole window

            //mainWindowLocal.Effect = new BlurEffect();
            //// Open the child window
            //projectSettings.ShowDialog();
            ////restore Opacity and remove blur after closing the child window
            //mainWindowLocal.Opacity = 1;
            //mainWindowLocal.Effect = null;
        }
        void OpenProjectFolder(object parameter)
        {
            Process.Start(ProjectDirectory);
        }
        void DeleteFileorFolder(object parameter)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(SelectedFilePath);
                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)  // Delete Directory
                {
                    System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to Delete this Folder?", "Delete Folder", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        Directory.Delete(SelectedFilePath);
                        RefreshSolutionList();
                    }
                }
                else// Delete File
                {
                    System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to Delete this File?", "Delete File", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        File.Delete(SelectedFilePath);
                        string filename = System.IO.Path.GetFileName(SelectedFilePath);
                        foreach (var item in mainWindowLocal.MainDockSite.DocumentWindows)
                        {
                            if (item.Title.Contains(filename))
                            {
                                item.Close();
                            }
                        }

                        RefreshSolutionList();
                    }

                }

            }
            catch (Exception)
            {

            }
        }
        void RefreshProjectList(object parameter)
        {
            RefreshSolutionList();
        }
        void OpenRenameDialog(object parameter)
        {
            Views.ProjectSolutionControls.RenameDialog renameDialog = new Views.ProjectSolutionControls.RenameDialog();
            ViewModels.ProjectSolutionControls.RenameDialogViewModel renameDialogViewModel = new ProjectSolutionControls.RenameDialogViewModel(renameDialog);
            renameDialog.DataContext = renameDialogViewModel;
            renameDialogViewModel.ProjectRootFolder = ProjectRootFolder;
            renameDialogViewModel.OldFileName = SelectedImagePath;
            mainWindowLocal.MainRibbon.ApplicationMenu.IsEnabled = false;
            mainWindowLocal.Opacity = 0.2f;

            renameDialog.ShowDialog();
            JsonControls jsonControls = new JsonControls();
            jsonControls.ReadJsonFile(ProjectRootFolder +"\\Project.json");
            jsonControls.DeserializeJsonObject();
            string cleanedFileName = SelectedFileName.Remove(SelectedFileName.Length - 5);
            if (jsonControls.GetKeyValue("Main") == cleanedFileName)
            {
                jsonControls.ChangeKeyString("Main", renameDialogViewModel.NewFileName);
            } 
          
            RefreshSolutionList();
            mainWindowLocal.MainRibbon.ApplicationMenu.IsEnabled = true;
            mainWindowLocal.Opacity = 100;
   
        }
        void SetAsMain(object parameter)
        {
            FileInfo fileInfo = null;
            string cleanedFileName = "";
            JsonControls jsonControls = new JsonControls();
            jsonControls.ReadJsonFile(ProjectRootFolder + "\\Project.json");
            jsonControls.DeserializeJsonObject();
            if (SelectedFileName.Contains(".xaml"))
            {
                fileInfo = new FileInfo(SelectedFilePath);
               // MessageBox.Show(fileInfo.Directory.Name);
            
                if (fileInfo.Directory.FullName == jsonControls.GetKeyValue("ProjectPath"))
                {
                    //SelectedImagePath.Remove(jsonControls.GetKeyValue("Name").Length);
                    jsonControls.ChangeKeyString("Main", SelectedFileName);
                }
                else
                {
                    SelectedImagePath.Remove(jsonControls.GetKeyValue("Name").Length);
                    //MessageBox.Show(SelectedImagePath);
                    jsonControls.ChangeKeyString("Main", SelectedImagePath);
                }

            }
            int nodeCount = 0;
            foreach (TreeViewItem item in SolutionTreeView.Items)
            {
                //MessageBox.Show("Item header: " + item.Header.ToString());
                //MessageBox.Show("Item header with project path: " + jsonControls.GetKeyValue("ProjectPath") + "\\" + item.Header.ToString());
                //MessageBox.Show("fileInfo FullName: " + fileInfo.FullName);
                string FullNameCleaned = "";
                if (fileInfo.FullName.Contains(".xaml"))
                {
                    FullNameCleaned = fileInfo.FullName.Remove(fileInfo.FullName.Length - 5);
                }
                if (item.Header.ToString().Contains(".xaml"))
                {
                    item.Header = item.Header.ToString().Remove(item.Header.ToString().Length - 5);
                }
                //MessageBox.Show("Item header after cleaning: " + FullNameCleaned);
                //MessageBox.Show("Item header with project path after cleaning: " + jsonControls.GetKeyValue("ProjectPath") + "\\" + item.Header.ToString());
                //MessageBox.Show("fileInfo FullName: " + fileInfo.FullName);
                if (jsonControls.GetKeyValue("ProjectPath") + "\\" + item.Header.ToString() == FullNameCleaned)
                {
                    MessageBox.Show(item.ToString());
                    RefreshSolutionList(2);
                }
              
            }
           // RefreshSolutionList();
        }
        void RemoveDependency(object parameter)
        {
            StreamReader streamReader = new StreamReader(ProjectDirectory + "\\.root" + "\\Dependencies.json");
            var depedencyTree = new TreeViewItem { Header = "Dependencies" };
            string DependecyFileContents = streamReader.ReadToEnd();
            if (SelectedFileName.Contains(".dll"))
            {
                SelectedFileName = SelectedFileName.Replace(".dll", "");
            }
            List<Reference> originalReferences = JsonConvert.DeserializeObject<List<Reference>>(DependecyFileContents);
            Reference CurrentItteratedReference = new Reference();
            if (originalReferences != null)
            {
                foreach (var item in originalReferences)
                {
                    if (item.Name == SelectedFileName)
                    {
                        depedencyTree.Items.Remove(item);
                        CurrentItteratedReference = item;
                    }
                }
                if (CurrentItteratedReference.Name == SelectedFileName)
                {
                    originalReferences.Remove(CurrentItteratedReference);
                }
            }

            streamReader.Close();
            string json = JsonConvert.SerializeObject(originalReferences, Formatting.Indented);
            TextWriter tsw = new StreamWriter(ProjectRootFolder + "\\.root" + "\\Dependencies.json");
            tsw.Write(json);
            tsw.Close();

            //MessageBox.Show(ProjectDirectory + "\\" + SelectedFileName);

            mainWindowViewModelLocal.ActivitiesView.Categories.Clear();
            mainWindowViewModelLocal.SelectedDependencyPath.Add(ProjectDirectory + "\\" + SelectedFileName + ".dll");
            mainWindowLocal.btnDeletePackage.Command.Execute(mainWindowLocal.btnDeletePackage.CommandParameter);
            mainWindowViewModelLocal.RefreshToolbox();
            //RefreshSolutionList();
           // SolutionTreeView.Items.Remove(GetDependencies());
   
            foreach (TreeViewItem item in SolutionTreeView.Items)
            {
                if (item.Header == "Dependencies")
                {
                    item.IsExpanded = true;
                    SolutionTreeView.Items.Refresh();
                    item.IsExpanded = true;
                }
            }
          //  SolutionTreeView.Items.Add(GetDependencies());
      
           // RefreshSolutionList();
            //GetDependencies();
            //GetDependencies().IsExpanded = true;
            //GetDependencies().Items.Refresh();
        }
        void AddDependency(object parameter)
        {
            //mainWindowViewModelLocal.ActivitiesView.Categories.Clear();
            mainWindowLocal.btnAddReference.Command.Execute(mainWindowLocal.btnAddReference.CommandParameter);
            // mainWindowViewModelLocal.AddReference();
            //  mainWindowViewModelLocal.RefreshToolbox();
            RefreshSolutionList();
            GetDependencies();
            GetDependencies().IsExpanded = true;
            GetDependencies().Items.Refresh();
          //  mainWindowViewModelLocal.AddAllAddReferencesToFileToToolBox(ProjectDirectory);
        }
        #endregion

        #region Events

        private void SolutionTreeViewBox_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);
            
            if (temp == null)
                return;
            SelectedImagePath = "";
            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                SelectedImagePath = temp1 + temp2 + SelectedImagePath;
                if (temp.Parent.GetType().Equals(typeof(TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }

            #region Debugging
        
            //MessageBox.Show(temp.ToString());
            //MessageBox.Show(ProjectRootFolder + "\\" + fileName);
            //MessageBox.Show(jsonControls.GetKeyValue("Name"));
            #endregion


            string fileName = (string)((TreeViewItem)SolutionTreeView.SelectedItem).Header;

            SelectedFileName = fileName;
            JsonControls jsonControls = new JsonControls();
            jsonControls.ReadJsonFile(ProjectRootFolder + "//" + "project.json");

            jsonControls.DeserializeJsonObject();

            if (SelectedImagePath.Contains(jsonControls.GetKeyValue("Name")))
            {
                SelectedImagePath = SelectedImagePath.Replace(jsonControls.GetKeyValue("Name"), "");
                SelectedFilePath = ProjectRootFolder + SelectedImagePath;
            }
           // MessageBox.Show("SelectedImagePath : " + SelectedFilePath);
            //  MessageBox.Show("Ruwe: " + ProjectDirectory + SelectedImagePath) ;
        }

        private void SolutionTreeview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string FilenameWithExtension;
                // WorkflowStudioIntegration.MainWindowViewModel mainWindowViewModel1 = new WorkflowStudioIntegration.MainWindowViewModel(mainWindowLocal.MainDockSite, mainWindowLocal.ActivitiesToolWindow, mainWindowLocal.PropertiesToolWindow, mainWindowLocal.OutlineToolwindow, mainWindowLocal.ErrorListToolWindow, mainWindowLocal.OutputToolWindow, mainWindowLocal.MDIHost);
                //mainWindowViewModel1.Filename = SelectedFilePath;
                mainWindowViewModelLocal.Filename = SelectedFilePath;
                FilenameWithExtension = SelectedFileName;
                if (FilenameWithExtension != null && FilenameWithExtension.Contains(".xaml"))
                {
                    if (mainWindowLocal.MDIHost.IsEmpty == false)
                    {
                        foreach (var documentTab in mainWindowLocal.MDIHost.GetDocuments())
                        {

                            if (documentTab.Title.Contains(SelectedFileName))
                            {
                                dockingWindow = documentTab;
                                exist = true;
                                break;
                            }
                            else
                            {
                                exist = false;
                                continue;
                            }

                        }

                        if (exist == false)
                        {
                            mainWindowViewModelLocal.OpenWorkflow();
                            mainWindowViewModelLocal.content.Activate();
                        }
                        else if (exist == true)
                        {
                            dockingWindow.Activate(true);
                        }
                    }
                    else
                    {
                        mainWindowViewModelLocal.OpenWorkflow();
                        mainWindowViewModelLocal.content.Activate();
                    }

                }
            }
            catch (NullReferenceException)
            {

            }

        }

        #endregion

        #region Methods
        public void ProjectWindowHookup(TreeView treeView,
            MainWindow mainWindow, ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu contextMenu, ViewModels.WorkflowStudioIntegration.MainWindowViewModel mainWindowViewModel)
        {
            mainWindowLocal = mainWindow;
            mainWindowViewModelLocal = mainWindowViewModel;
            _solutionTreeview = treeView;
            //Button Events
            RelaySetup();


           


            //Solution Tree View Events
            SolutionTreeView.MouseDown += SolutionTreeview_MouseDown;
            SolutionTreeView.MouseMove += SolutionTreeview_MouseMove;
            SolutionTreeView.DragOver += SolutionTreeview_DragOver;
            SolutionTreeView.Drop += SolutionTreeview_Drop;
            SolutionTreeView.SelectedItemChanged += SolutionTreeViewBox_SelectedItemChanged;
            SolutionTreeView.MouseDoubleClick += SolutionTreeview_MouseDoubleClick;
            LocalcontextMenu = contextMenu;


        }
        public void LoadWindow(string ProjectDirectory)
        {
            CreateContextMenus();
            ListDirectory(SolutionTreeView, ProjectDirectory);
            RefreshSolutionList();
          
        }

        ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu XamlcontextMenu;
        ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu DependencycontextMenu;
        void CreateContextMenus()
        {
            //XAML ContextMenu
            XamlcontextMenu = new ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu();


            ActiproSoftware.Windows.Controls.Ribbon.Controls.Button deleteButton = new Button();
            deleteButton.Label = "Delete";
            deleteButton.ImageSourceSmall = new BitmapImage(new Uri(@"/RPA-Workbench-Revision2;component/1. Resources/ProjectWindow Images/DeleteIcon.png", UriKind.RelativeOrAbsolute));
            deleteButton.Command = DeleteFileorFolderCommand;

            ActiproSoftware.Windows.Controls.Ribbon.Controls.Button createFolderButton = new Button();
            createFolderButton.Label = "Create Folder";
            createFolderButton.ImageSourceSmall = new BitmapImage(new Uri(@"/RPA-Workbench-Revision2;component/1. Resources/ProjectWindow Images/Folder Dark -32.png", UriKind.RelativeOrAbsolute));
    

            ActiproSoftware.Windows.Controls.Ribbon.Controls.Button renameButton = new Button();
            renameButton.Label = "Rename";   
            renameButton.Command = OpenRenameDialogCommand;

            ActiproSoftware.Windows.Controls.Ribbon.Controls.Button setAsMainButton = new Button();
            setAsMainButton.Label = "Set as Main";
            setAsMainButton.Command = SetAsMainCommand;


            XamlcontextMenu.Items.Add(deleteButton);
            XamlcontextMenu.Items.Add(createFolderButton);
            XamlcontextMenu.Items.Add(renameButton);
            XamlcontextMenu.Items.Add(setAsMainButton);


            //Depedency ContextMenu
            DependencycontextMenu = new ActiproSoftware.Windows.Controls.Ribbon.Controls.ContextMenu();

            ActiproSoftware.Windows.Controls.Ribbon.Controls.Button addDependancy = new Button();
            addDependancy.ImageSourceSmall = new BitmapImage(new Uri(@"/RPA-Workbench-Revision2;component/1. Resources/ProjectWindow Images/Add-Package - 32.png", UriKind.RelativeOrAbsolute));
            addDependancy.Label = "Add Dependency";
            addDependancy.Command = AddDepedencyCommand;

            ActiproSoftware.Windows.Controls.Ribbon.Controls.Button deleteDependancy = new Button();
            deleteDependancy.ImageSourceSmall = new BitmapImage(new Uri(@"/RPA-Workbench-Revision2;component/1. Resources/ProjectWindow Images/Remove-Package - 32.png", UriKind.RelativeOrAbsolute));
            deleteDependancy.Label = "Remove Dependency";
            deleteDependancy.Command = RemoveDepedencyCommand;


            // addDependancy.Click += AddDependancy_Click;

            DependencycontextMenu.Items.Add(addDependancy);
            DependencycontextMenu.Items.Add(deleteDependancy);

           

            // CreateDirectoryNode().ContextMenu = contextMenu;
        }
        public void RefreshSolutionList(int MainFileLevel = 0)
        {
            ListDirectory(SolutionTreeView, ProjectDirectory);
           // var header  = (string)((TreeViewItem)SolutionTreeView.SelectedItem).Header;
            foreach (TreeViewItem item in SolutionTreeView.Items)
            {
                item.IsExpanded = true;
            }
        }
        private void ListDirectory(TreeView treeView, string path)
        {
            try
            {
                treeView.Items.Clear();
                var rootDirectoryInfo = new DirectoryInfo(path);
                TreeViewItem createDirectoryNode = CreateDirectoryNode(rootDirectoryInfo);

                //createDirectoryNode.Header = createDirectoryNode.Header + ".project"; //To give top node solution icon
                treeView.Items.Add(createDirectoryNode);

                createDirectoryNode.Items.Insert(0, GetDependencies());
            }
            catch (Exception)
            {

                //throw;
            }

        }
        #endregion

        #region Functions

        private class Reference
        {
            string _name;
            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }


            string _location;
            public string Location
            {
                get { return _location; }
                set { _location = value; }
            }

            string _fullname;
            public string FullName
            {
                get { return _fullname; }
                set { _fullname = value; }
            }

            //Version _version;
            //public Version Version
            //{
            //    get { return _version; }
            //    set { _version = value; }
            //}
        }
        private TreeViewItem GetDependencies()
        {
            //Get Depedencies and show them
            var depedencyTree = new TreeViewItem { Header = "Dependencies" };
            depedencyTree.AllowDrop = false;

            depedencyTree.ContextMenu = DependencycontextMenu;
           
            StreamReader streamReader = new StreamReader(ProjectDirectory + "\\.root" + "\\Dependencies.json");

            string DependecyFileContents = streamReader.ReadToEnd();
            List<Reference> originalReferences = JsonConvert.DeserializeObject<List<Reference>>(DependecyFileContents);
            if (originalReferences != null)
            {
                foreach (var item in originalReferences)
                {
                    depedencyTree.Items.Add(new TreeViewItem { Header = item.Name + ".dll" });
                }
            }
            streamReader.Close();

            return depedencyTree;
        }
        private TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo = null)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name };
            directoryNode.ContextMenu = XamlcontextMenu;

           

            foreach (var directory in directoryInfo.GetDirectories())
            {
                if (directory.Name.Contains("json") || directory.Name.StartsWith("."))
                {
                    continue;
                }
                directoryNode.Items.Add(CreateDirectoryNode(directory));
            }


            JsonControls jsonControls = new JsonControls();
            jsonControls.ReadJsonFile(ProjectRootFolder + "\\Project.json");
            jsonControls.DeserializeJsonObject();

            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Name.Contains("json") || file.Name.StartsWith("."))
                {
                    continue;
                }
             

                //Process to Bolden MAIN File
                string _cleanedFileName = file.FullName;
                string MainFile;

                if (jsonControls.GetKeyValue("Main").Contains("\\") == false) //If in root of Solution
                {
                    MainFile = jsonControls.GetKeyValue("ProjectPath") + "\\" + jsonControls.GetKeyValue("Main");
                }
                else // If in other folder in solution
                {
                    MainFile = jsonControls.GetKeyValue("ProjectPath") + jsonControls.GetKeyValue("Main");
                }
                if (_cleanedFileName == MainFile)
                {
                    directoryNode.Items.Add(new TreeViewItem { Header = file.Name, FontWeight = FontWeights.Bold });
                }
                else
                {
                    directoryNode.Items.Add(new TreeViewItem { Header = file.Name, FontWeight = FontWeights.Normal });
                }
            }


          


            return directoryNode;

        }

        #endregion

        #region Drag/Drop Events
        //EVENTS
        private void SolutionTreeview_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                TreeViewItem TargetItem = GetNearestContainer
                    (e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;
                }
            }
            catch (Exception)
            {
            }
        }

        private void SolutionTreeview_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                Point currentPosition = e.GetPosition(SolutionTreeView);

                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                   (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    TreeViewItem item = GetNearestContainer
                    (e.OriginalSource as UIElement);
                    if (CheckDropTarget(draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                e.Handled = true;
            }
            catch (Exception)
            {
            }
        }

        private void SolutionTreeview_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPosition = e.GetPosition(SolutionTreeView);
                   
                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    {
                        draggedItem = (TreeViewItem)SolutionTreeView.SelectedItem;
                        if (draggedItem != null && draggedItem.Header.ToString().Contains("Dependencies") == false && draggedItem.Header.ToString().Contains("dll") == false)
                        {
                            DragDropEffects finalDropEffect =
                DragDrop.DoDragDrop(SolutionTreeView,
                    SolutionTreeView.SelectedValue,
                                DragDropEffects.Move);
                            //Checking target is not null and item is
                            //dragging(moving)
                            if ((finalDropEffect == DragDropEffects.Move) &&
                    (_target != null))
                            {
                                // A Move drop was accepted
                                if (!draggedItem.Header.ToString().Equals
                    (_target.Header.ToString()))
                                {
                                    CopyItem(draggedItem, _target);
                                    _target = null;
                                    draggedItem = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void SolutionTreeview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(SolutionTreeView);
            }
        }

        //Methods
        private bool CheckDropTarget(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            bool _isEqual = false;
            try
            {
                //if ((_targetItem == null || _sourceItem == null)
                //{
                //    return;
                //}
              //  else  //Check whether the target item is meeting your condition
                if (_targetItem != null || _sourceItem != null || string.IsNullOrEmpty(_targetItem.Header.ToString()) == false)
                {
                    if (!_sourceItem.Header.ToString().Equals(_targetItem.Header.ToString()) && _targetItem.Header.ToString().Contains("xaml") == false &&
                        _targetItem.Header.ToString().Contains("dll") == false || _targetItem.Header.ToString().Contains("Dependencies") == false ||
                        _sourceItem.Header.ToString().Contains("Dependencies") == false)
                    {
                        _isEqual = true;
                        //RefreshSolutionList();
                        //	ListDirectory(SolutionTreeViewBox, ProjectDirectory);
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return _isEqual;

        }
        private void CopyItem(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {

            //Asking user wether he want to drop the dragged TreeViewItem here or not
            if (MessageBox.Show("Would you like to drop " + _sourceItem.Header.ToString() + " into " + _targetItem.Header.ToString() + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    //adding dragged TreeViewItem in target TreeViewItem
                    addChild(_sourceItem, _targetItem);

                    //finding Parent TreeViewItem of dragged TreeViewItem 
                    TreeViewItem ParentItem = FindVisualParent<TreeViewItem>(_sourceItem);
                    // if parent is null then remove from TreeView else remove from Parent TreeViewItem
                    if (ParentItem == null)
                    {
                        LocalcontextMenu.Items.Remove(_sourceItem);
                    }
                    else
                    {
                        ParentItem.Items.Remove(_sourceItem);
                    }
                }
                catch
                {

                }
            }
            RefreshSolutionList();
            //MessageBox.Show("Tried To Move to " + _targetItem.Header.ToString());
        }
        public void addChild(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            // add item in target TreeViewItem 
            TreeViewItem item1 = new TreeViewItem();
            item1.Header = _sourceItem.Header;
            _targetItem.Items.Add(item1);
            string JustFolderName = new System.IO.DirectoryInfo(ProjectRootFolder).Name;

            //Root Directory Move Does not WORK YET
            if (_targetItem.Header.ToString() == JustFolderName) // Move File To Root Directory/Parent
            {
                JsonControls jsonControls = new JsonControls();
                jsonControls.ReadJsonFile(ProjectRootFolder + "//" + "project.json");
                jsonControls.DeserializeJsonObject();
                if (SelectedImagePath.Contains(jsonControls.GetKeyValue("Name")))
                {
                    SelectedImagePath = SelectedImagePath.Replace(jsonControls.GetKeyValue("Name"), "");
                    SelectedFilePath = ProjectRootFolder + "\\" + SelectedImagePath;
                }
                string FileToMove = System.IO.Path.GetFullPath(_sourceItem.Header.ToString());
                string FileDestination = ProjectDirectory + "\\" + _sourceItem.Header.ToString();
                MessageBox.Show("Moving File: " + SelectedFilePath + Environment.NewLine +
                                "To" + Environment.NewLine +
                                "File Destination" + FileDestination);
                foreach (TreeViewItem item in _sourceItem.Items)
                {
                    addChild(item, item1);
                }

                FileAttributes attr = File.GetAttributes(SelectedFilePath);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)  // Move Directory
                {
                    Directory.Move(SelectedFilePath, FileDestination);
                }
                else
                {
                    File.Move(SelectedFilePath, FileDestination); // Move File
                }
                    

            }
            else //If Not Moving to Root Directory/Parent
            {
                MessageBox.Show(SelectedFilePath);
                
                string FileToMove = ProjectDirectory + SelectedImagePath;
                // SelectedImagePath = SelectedImagePath.TrimEnd(SelectedFileName.ToCharArray());
                int index = SelectedFilePath.LastIndexOf(SelectedFileName); // Character to remove "?"
                
                string FileDestination = SelectedFilePath +  _target.Header + "\\" + SelectedFileName;
                //string FileDestination = ProjectDirectory + SelectedImagePath;
                
                //MessageBox.Show("File To Move: " + FileToMove);
                //MessageBox.Show("File Move To: " + FileDestination);

                //MessageBox.Show("Moving File: " + FileToMove + Environment.NewLine +
                //              "To" + Environment.NewLine +
                //              "File Destination: " + FileDestination);


                FileAttributes attr = File.GetAttributes(SelectedFilePath); //CHECK if Dir or File

                if (index > 0)
                {
                    SelectedFilePath = SelectedFilePath.Replace(SelectedFileName, ""); // This will remove all text after character ?                                                        // MessageBox.Show("New FilePath without SelectedFileName: " + SelectedFilePath);
                }
                FileDestination = SelectedFilePath + _target.Header + "\\" + SelectedFileName; //Remove Selected File name again after checking if it is a file or directory

                MessageBox.Show("FileDestination Should be: " + FileDestination);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)  // Move Directory
                {
                    //string input = "text?here";
                    //int index = SelectedFilePath.LastIndexOf(SelectedFileName); // Character to remove "?"

                    MessageBox.Show("FOLDER " + Environment.NewLine +
                       "Moving File: " + FileToMove + Environment.NewLine +
                         "To" + Environment.NewLine +
                         "File Destination: " + FileDestination);


                    //if (index > 0)
                    //{
                    //    SelectedFilePath = SelectedFilePath.Substring(0, index); // This will remove all text after character ?
                    //    MessageBox.Show("New FilePath without SelectedFileName: " + SelectedFilePath);
                    //}
                       

                    //SelectedFilePath = SelectedFilePath.Remove()
                    FileDestination = SelectedFilePath + _target.Header;
                    MessageBox.Show("Moving File: " + FileToMove + Environment.NewLine +
                            "To" + Environment.NewLine +
                            "File Destination: " + FileDestination);
                    Directory.Move(FileToMove, FileDestination);
                }
                else
                {
                    MessageBox.Show("FILE " + Environment.NewLine +
                        "Moving File: " + FileToMove + Environment.NewLine +
                          "To" + Environment.NewLine +
                          "File Destination: " + FileDestination);
                    File.Move(FileToMove, FileDestination); // Move File
                }
               // File.Move(FileToMove, FileDestination);
            }
            foreach (TreeViewItem item in _sourceItem.Items)
            {
                addChild(item, item1);
            }

            //ListDirectory();
            // RefreshSolutionList();
            //MessageBox.Show("Moving File: " + FileToMove + Environment.NewLine +
            //				"To" + Environment.NewLine +
            //				"File Destination" + FileDestination);
        }

        public void addChildAsDependancy(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            // add item in target TreeViewItem 
            TreeViewItem item1 = new TreeViewItem();
            item1.Header = _sourceItem.Header;
            _targetItem.Items.Add(item1);
            string JustFolderName = new System.IO.DirectoryInfo(ProjectRootFolder).Name;

                string FileToMove = ProjectDirectory + "\\" + _sourceItem.Header;
                string FileDestination = ProjectDirectory + "\\" + _targetItem.Header + "\\" + _sourceItem.Header;
                foreach (TreeViewItem item in _sourceItem.Items)
                {
                addChildAsDependancy(item, item1);
                }

            //ListDirectory();
            RefreshSolutionList();
            //MessageBox.Show("Moving File: " + FileToMove + Environment.NewLine +
            //				"To" + Environment.NewLine +
            //				"File Destination" + FileDestination);
        }
        static TObject FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                TObject found = parent as TObject;
                if (found != null)
                {
                    return found;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }

            return null;
        }
        private TreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            return container;
        }
        public DragDropEffects InitializeDataObject(TreeListBox sourceControl, IDataObject dataObject, IEnumerable<object> items)
        {
            if (sourceControl == null)
                throw new ArgumentNullException("sourceControl");
            if (dataObject == null)
                throw new ArgumentNullException("dataObject");
            if (items == null)
                throw new ArgumentNullException("items");

            // Store the full paths to items in case we drop on the tree itself... 
            //   Each item needs to have a unique path, which comes from adapter GetPath() calls
            var fullPaths = new StringBuilder();
            foreach (var item in items)
                fullPaths.AppendLine(sourceControl.GetFullPath(item));
            if (fullPaths.Length > 0)
                dataObject.SetData(TreeListBox.ItemDataFormat, fullPaths.ToString());

            // If there is one item, store its text so that it can be dropped elsewhere
            if (items.Count() == 1)
            {
                var viewModel = items.First() as TreeNodeModel;
                if (viewModel != null)
                    dataObject.SetData(DataFormats.Text, viewModel.Name);
            }

            return DragDropEffects.Move;
        }
        public TreeItemDropArea OnDragOver(DragEventArgs e, TreeListBox targetControl, object targetItem, TreeItemDropArea dropArea)
        {
            // If the drag is over an item and there is item data present...
            if ((targetItem != null) && (dropArea != TreeItemDropArea.None) && (e.Data.GetDataPresent(TreeListBox.ItemDataFormat)))
            {
                var fullPaths = e.Data.GetData(TreeListBox.ItemDataFormat) as string;
                if (!string.IsNullOrEmpty(fullPaths))
                {
                    // Locate the first item based on full path
                    object firstItem = null;
                    foreach (var fullPath in fullPaths.Split(new char[] { '\r', '\n' }))
                    {
                        if (!string.IsNullOrEmpty(fullPath))
                        {
                            var item = targetControl.GetItemByFullPath(fullPath);
                            if (item != null)
                            {
                                firstItem = item;
                                break;
                            }
                        }
                    }

                    if (firstItem != null)
                    {
                        // Ensure that the first item is already in the target control (nav will be null if not)... if allowing drag/drop onto external
                        //   controls, you cannot use the item navigator and must rely on your own item hierarchy logic
                        var firstItemNav = targetControl.GetItemNavigator(firstItem);
                        if (firstItemNav != null)
                        {
                            // Only support a single effect (you could add support for other effects like Copy if the Ctrl key is down here)
                            if ((e.AllowedEffects & DragDropEffects.Move) == DragDropEffects.Move)
                            {
                                e.Effects = DragDropEffects.Move;
                                e.Handled = true;
                            }
                             foreach (TreeViewItem item in SolutionTreeView.Items)
                             {
                                item.IsExpanded = true;
                             }
                            switch (e.Effects)
                            {
                                case DragDropEffects.Move:
                                    // Coerce the resulting drop-area so that if dragging 'after' an item that has a next sibling, the drop area 
                                    //   becomes 'on' the item instead... can still get between the items by dragging 'before' the next sibling in this scenario
                                    if (dropArea == TreeItemDropArea.After)
                                    {
                                        var targetItemNav = targetControl.GetItemNavigator(targetItem);
                                        if ((targetItemNav != null) && (targetItemNav.GoToNextSibling()))
                                            dropArea = TreeItemDropArea.On;
                                    }

                                    return dropArea;
                            }
                        }
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            return TreeItemDropArea.None;
        }
        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        #endregion
    }
}


