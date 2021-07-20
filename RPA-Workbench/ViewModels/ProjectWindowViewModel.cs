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
using Button = ActiproSoftware.Windows.Controls.Ribbon.Controls.Button;
namespace RPA_Workbench.ViewModels
{
    public class ProjectWindowViewModel
    {
      
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


        void RelaySetup()
        {
            OpenProjectSettingsCommand = new RelayCommand(new Action<object>(OpenProjectSettings));
            OpenProjectFolderCommand = new RelayCommand(new Action<object>(OpenProjectFolder));
            DeleteFileorFolderCommand = new RelayCommand(new Action<object>(DeleteFileorFolder));
            RefreshProjectListCommand = new RelayCommand(new Action<object>(RefreshProjectList));
            OpenRenameDialogCommand = new RelayCommand(new Action<object>(OpenRenameDialog));
            SetAsMainCommand = new RelayCommand(new Action<object>(SetAsMain));
        }

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

        #endregion

        #region Methods/Functions
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
            renameDialogViewModel.ParentFolder = ProjectRootFolder;
            renameDialogViewModel.OldFileName = SelectedFileName;
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
            JsonControls jsonControls = new JsonControls();
            jsonControls.ReadJsonFile(ProjectRootFolder + "\\Project.json");
            jsonControls.DeserializeJsonObject();
            if (SelectedFileName.Contains(".xaml"))
            {
                string cleanedFileName = SelectedFileName.Remove(SelectedFileName.Length - 5);
                jsonControls.ChangeKeyString("Main", cleanedFileName);
            }
         
            RefreshSolutionList();
        }
        #endregion

        #region Events

        #endregion
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

            //show user selected path
            //SelectedFileName
            //MessageBox.Show(temp.ToString());
            string fileName = (string)((TreeViewItem)SolutionTreeView.SelectedItem).Header;
            //MessageBox.Show(ProjectRootFolder + "\\" + fileName);
            SelectedFileName = fileName;
            JsonControls jsonControls = new JsonControls();
            jsonControls.ReadJsonFile(ProjectRootFolder + "//" + "project.json");

            jsonControls.DeserializeJsonObject();
            //MessageBox.Show(jsonControls.GetKeyValue("Name"));
            if (SelectedImagePath.Contains(jsonControls.GetKeyValue("Name")))
            {
                SelectedImagePath = SelectedImagePath.Replace(jsonControls.GetKeyValue("Name"), "");
                SelectedFilePath = ProjectRootFolder + "\\" + SelectedImagePath;
            }
          
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

        public void LoadWindow(string ProjectDirectory)
        {
            ListDirectory(SolutionTreeView, ProjectDirectory);
            RefreshSolutionList();
        }

        #region Bold Main File
        void IterateThroughSolutionView()
        {
            StringBuilder l_builder = new StringBuilder();

            foreach (TreeViewItem l_item in SolutionTreeView.Items)
            {
                ProcessNodes(l_item, l_builder, 0);
            }

            //MessageBox.Show(l_builder.ToString());
        }
        private void ProcessNodes(TreeViewItem node, StringBuilder builder, int level)
        {
            JsonControls jsonControls = new JsonControls();
            jsonControls.ReadJsonFile(ProjectRootFolder + "\\Project.json");
            jsonControls.DeserializeJsonObject();
            string cleanedFileName = "";
            if (SelectedFileName != null)
            {
                cleanedFileName = SelectedFileName.Remove(SelectedFileName.Length - 5);
            }

            builder.Append(new string('\t', level) + node.Header.ToString() + Environment.NewLine);

            foreach (TreeViewItem l_innerNode in node.Items)
            {
                if (l_innerNode.Header.ToString().Contains(".xaml"))
                {
                    string _cleanedFileName = l_innerNode.Header.ToString().Remove(l_innerNode.Header.ToString().Length - 5);
                    if (jsonControls.GetKeyValue("Main") == _cleanedFileName)
                    {
                        l_innerNode.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        l_innerNode.FontWeight = FontWeights.Normal;
                    }
                    ProcessNodes(l_innerNode, builder, level + 1);
                }
               
            }
        }
        #endregion
        public void RefreshSolutionList()
        {
            ListDirectory(SolutionTreeView, ProjectDirectory);
            IterateThroughSolutionView();
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
                treeView.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
            }
            catch (Exception)
            {

                throw;
            }

        }
        private static TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name };
            foreach (var directory in directoryInfo.GetDirectories())
            {
                if (directory.Name.Contains("json") || directory.Name.StartsWith("."))
                {
                    continue;
                }
                directoryNode.Items.Add(CreateDirectoryNode(directory));
            }


            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Name.Contains("json") || file.Name.StartsWith("."))
                {
                    continue;
                }
                directoryNode.Items.Add(new TreeViewItem { Header = file.Name });
            }


            return directoryNode;

        }

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
                        if (draggedItem != null)
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
                //Check whether the target item is meeting your condition
                if (_targetItem != null || _sourceItem != null)
                {
                    if (!_sourceItem.Header.ToString().Equals(_targetItem.Header.ToString()))
                    {
                        _isEqual = true;
                        RefreshSolutionList();
                        //	ListDirectory(SolutionTreeViewBox, ProjectDirectory);
                    }
                }

            }
            catch (NullReferenceException )
            {
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
                //MessageBox.Show("Moving File: " + SelectedFilePath + Environment.NewLine +
                //				"To" + Environment.NewLine +
                //				"File Destination" + FileDestination);
                foreach (TreeViewItem item in _sourceItem.Items)
                {
                    addChild(item, item1);
                }
                File.Move(SelectedFilePath, FileDestination);

            }
            else //If Not Moving to Root Directory/Parent
            {
                string FileToMove = ProjectDirectory + "\\" + _sourceItem.Header;
                string FileDestination = ProjectDirectory + "\\" + _targetItem.Header + "\\" + _sourceItem.Header;
                foreach (TreeViewItem item in _sourceItem.Items)
                {
                    addChild(item, item1);

                }
                File.Move(FileToMove, FileDestination);
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


