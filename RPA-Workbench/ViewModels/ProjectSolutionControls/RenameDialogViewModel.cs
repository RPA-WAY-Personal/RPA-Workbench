using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using RPA_Workbench.Views;
using System.Windows;
using RPA_Workbench.Utilities;

namespace RPA_Workbench.ViewModels.ProjectSolutionControls
{
    public class RenameDialogViewModel
    {
        Views.ProjectSolutionControls.RenameDialog renameDialogLocal;
        public RenameDialogViewModel()
        {
            RelaySetup();
        }
        public RenameDialogViewModel(Views.ProjectSolutionControls.RenameDialog renameDialog = null)
        {
            renameDialogLocal = renameDialog;
            RelaySetup();
        }
        void RelaySetup()
        {
            RenameCommand = new RelayCommand(new Action<object>(RenameProject));
            CloseWindowCommand = new RelayCommand(new Action<object>(CloseWindow));
        }

        #region Icommands
        ICommand iRename;
        public ICommand RenameCommand {
            get { return iRename; }
            set { iRename = value; }
        }

        ICommand iCloseWindow;
        public ICommand CloseWindowCommand
        {
            get { return iCloseWindow; }
            set { iCloseWindow = value; }
        }
        #endregion

        #region Methods/Functions
        void RenameProject(object parameters)
        {
            string NewFileNameFullPath = "";
            string OldFileNameFullPath = "";

         

            if (OldFileNameFullPath == ProjectRootFolder + "\\" + OldFileName)
            {
                if (OldFileName.Contains("\\"))
                {
                    OldFileName = OldFileName.Replace("\\", "");
                }
                OldFileNameFullPath = ProjectRootFolder + "\\" + OldFileName;
            }
            else
            {
                OldFileNameFullPath = ProjectRootFolder + OldFileName;
            }

           

            if (NewFileName.Contains(".xaml")) // If the user inserts .xaml with the name, it will remove it
            {
                NewFileName = NewFileName.Replace(".xaml", "");
            }

            if (NewFileNameFullPath == ProjectRootFolder + "\\" + NewFileName)
            {
                NewFileNameFullPath = ProjectRootFolder + "\\" + NewFileName + ".xaml";
            }
            else
            {
                OldFileName = Path.GetDirectoryName(OldFileNameFullPath);
                NewFileNameFullPath = OldFileName + "\\" + NewFileName + ".xaml";
            }



            if (File.Exists(ParentFolder + "\\" + NewFileName + ".xaml") == true)
            {
                var messageBoxResult = CustomControls.Views.CustomMessageBox.Show("Path already exists", "File with same name exits, choose another name",
                CustomControls.Views.CustomMessageBox.MessageBoxButtons.OK);
            }
            else
            {
              //  NewFileNameFullPath = ProjectRootFolder + ParentFolder + NewFileName + ".xaml";
                MessageBox.Show("Old Name: " + OldFileNameFullPath);
                MessageBox.Show("New Name: " + NewFileNameFullPath);
                File.Move(OldFileNameFullPath, NewFileNameFullPath);
                renameDialogLocal.Close();
            }
           

        }

        void CloseWindow(object parameters) {
            renameDialogLocal.Close();
        }
        #endregion

        #region Public Methods/Functions

        #endregion

        #region Public Properties
        string _projectRootFolder;
        public string ProjectRootFolder
        {
            get { return _projectRootFolder; }
            set { _projectRootFolder = value; }
        }
        string _parentFolder;
        public string ParentFolder
        {
            get { return _parentFolder; }
            set { _parentFolder = value; }
        }

        string _oldFilename;
        public string OldFileName {
            get { return _oldFilename; }
            set { _oldFilename = value; }
        }

        string _newFilename;
        public string NewFileName {
            get { return _newFilename; }
            set { _newFilename = value; }
        }
        #endregion
    }
}
